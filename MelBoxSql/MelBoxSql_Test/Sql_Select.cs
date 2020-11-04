using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace MelBox
{
    public partial class MelSql
    {
        private DataTable ExecuteRead(string query, Dictionary<string, object> args)
        {
            if (string.IsNullOrEmpty(query.Trim()))
                return null;

            using (var con = new SQLiteConnection(Datasource))
            {
                con.Open();
#pragma warning disable CA2100 // Review SQL queries for security vulnerabilities
                using (SQLiteCommand cmd = new SQLiteCommand(query, con))
#pragma warning restore CA2100 // Review SQL queries for security vulnerabilities
                {
                    if (args != null)
                    {
                        //set the arguments given in the query
                        foreach (var pair in args)
                        {
                            cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                        }
                    }

                    var da = new SQLiteDataAdapter(cmd);
                    var dt = new DataTable();

                    try
                    {
                        da.Fill(dt);
                    }
                    catch
                    {
                        throw new Exception("Fehler ExecuteRead()");
                    }
                    finally
                    {
                        da.Dispose();
                    }

                    return dt;
                }
            }
        }

        /// <summary>
        /// Versucht den Kontakt anhand der Telefonnummer, email-Adresse oder dem Beginn eriner Nachricht zu identifizieren
        /// </summary>
        /// <param name="phone"></param>
        /// <param name="email"></param>
        /// <param name="keyWord"></param>
        /// <returns></returns>
        public int GetContactId(string name = "", ulong phone = 0, string email = "", string message = "")
        {
            try
            {
                const string query = "SELECT Id " +
                                     "FROM Contact " +
                                     "WHERE  " +
                                     "( length(Name) > 3 AND Name = @name ) " +
                                     "OR ( Phone > 0 AND Phone = @phone ) " +
                                     "OR ( length(Email) > 0 AND Email = @email )" +
                                     "OR ( length(KeyWord) > 0 AND KeyWord = @keyWord ) ";

                var args = new Dictionary<string, object>
            {
                {"@name", name},
                {"@phone", phone},
                {"@email", email},
                {"@keyWord", GetKeyWords(message)}
            };

                DataTable result = ExecuteRead(query, args);

                if (result.Rows.Count == 0)
                {
                    if (name.Length < 3)
                        name = "_UNBEKANNT_";
                    if (email.Length < 5)
                        email = null;

                    InsertContact(name, 0, email, phone, SendToWay.None);
                    return GetContactId(name, phone, email);
                }
                else
                {
                    int.TryParse(result.Rows[0][0].ToString(), out int r);
                    return r;
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Fehler in GetContactId()\r\n" + ex.GetType().ToString() + "\r\n" + ex.Message);
            }
        }

        /// <summary>
        /// Gibt die Id der Nachricht aus, oder erstellt sie.
        /// </summary>
        /// <param name="message">Nachricht, deren Id herausgefunden werdne soll.</param>
        /// <returns></returns>
        public uint GetMessageId(string message)
        {            
            try
            {
                const string contentQuery = "SELECT ID FROM MessageContent WHERE Content = @Content";

                var args1 = new Dictionary<string, object>
                {
                    {"@Content", message }
                };

                DataTable dt1 = ExecuteRead(contentQuery, args1);
                uint contendId;

                if (dt1.Rows.Count > 0)
                {
                    //Eintrag vorhanden
                    uint.TryParse(dt1.Rows[0][0].ToString(), out contendId);
                }
                else
                {
                    //Eintrag neu erstellen
                    const string doubleQuery = "INSERT INTO MessageContent (Content) VALUES (@Content); " +
                                               "SELECT ID FROM MessageContent ORDER BY ID DESC LIMIT 1";

                    dt1 = ExecuteRead(doubleQuery, args1);

                    uint.TryParse(dt1.Rows[0][0].ToString(), out contendId);
                }

                if (contendId == 0)
                {
                    //Provisorisch:
                    throw new Exception("GetMessageId() Kontakt konnte nicht zugeordnet werden.");
                }
                return contendId;
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler in GetMessageId()\r\n" + ex.GetType().ToString() + "\r\n" + ex.Message);
            }

            
        }

    }
}
