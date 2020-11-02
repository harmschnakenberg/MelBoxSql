using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBoxSql
{
    public partial class MelBoxSql
    {

        internal static string GetKeyWords(string MessageContent)
        {

            char[] split = new char[] { ' ', ',', '-', '.', ':', ';' };
            string[] words = MessageContent.Split(split);

            string KeyWords = words[0].Trim();

            if (words.Length > 1) KeyWords += words[1].Trim();

            return KeyWords;
        }

        private DataTable ExecuteRead(string query, Dictionary<string, object> args)
        {
            if (string.IsNullOrEmpty(query.Trim()))
                return null;

            using (var con = new SQLiteConnection(Datasource))
            {
                con.Open();
                using (SQLiteCommand cmd = new SQLiteCommand(query, con))
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
            const string query = "SELECT Id " +
                                 "FROM Contact " +
                                 "WHERE  " +
                                 "( length(Name) > 3 AND Name = @name ) " +
                                 "OR ( Phone > 0 AND Phone = @phone ) " +
                                 "OR ( length(Email) > 5 AND Email = @email )" +
                                 "OR ( length(KeyWord) > 2 AND KeyWord = @keyWord ) ";

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
                return (int)result.Rows[0][0];
            }
        }

        /// <summary>
        /// Gibt die Id der Nachricht aus, oder erstellt sie.
        /// </summary>
        /// <param name="message">Nachricht, deren Id herausgefunden werdne soll.</param>
        /// <returns></returns>
        public uint GetMessageId(string message)
        {
            uint contendId = 0;
            const string contentQuery = "SELECT ID FROM MessageContent WHERE Content = @Content";

            var args1 = new Dictionary<string, object>
                {
                    {"@Content", message }
                };

            DataTable dt1 = ExecuteRead(contentQuery, args1);

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

            return contendId;
        }
    }
}
