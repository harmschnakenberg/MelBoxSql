using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace MelBoxSql
{
    public partial class MelBoxSql
    {
        /// <summary>
        /// Neuer Log-Eintrag
        /// </summary>
        /// <param name="topic"></param>
        /// <param name="prio"></param>
        /// <param name="content"></param>
        public void Log(LogTopic topic, LogPrio prio, string content)
        {
            try
            {
                const string query = "INSERT INTO Log(Time, Topic, Prio, Content) VALUES (@timeStamp, @topic, @prio, @content)";

                var args = new Dictionary<string, object>
                {
                    {"@timeStamp", "CURRENT_TIMESTAMP" },
                    {"@topic", topic.ToString() },
                    {"@prio", (ushort)prio},
                    {"@content", content}
                };

                using (SqlConnection con = new SqlConnection(Datasource))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        foreach (var pair in args)
                        {
                            cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {                
                throw new Exception("Sql-Fehler Log()");
            }
        }

        /// <summary>
        /// Neuer Eintrag für Unternehmen
        /// </summary>
        /// <param name="name">Anzeigename des Unternehmens</param>
        /// <param name="address">Standortadresse</param>
        /// <param name="city">PLZ, Ort</param>
        public void InsertCompany(string name, string address, string city)
        {
            try
            {
                const string query = "INSERT INTO \"Company\" (\"Name\", \"Address\", \"City\") VALUES (@name, @address, @city );";

                var args = new Dictionary<string, object>
                {
                    {"@name", name },
                    {"@address", address},
                    {"@city", city},
                };

                using (SqlConnection con = new SqlConnection(Datasource))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        foreach (var pair in args)
                        {
                            cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                throw new Exception("Sql-Fehler InsertCompany()");
            }
        }

        /// <summary>
        /// Neuer Eintag für einen Kontakt (Sender / Empfänger) 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="companyId"></param>
        /// <param name="email"></param>
        /// <param name="phone"></param>
        /// <param name="sendWay"></param>
        public void InsertContact(string name, int companyId, string email, ulong phone, SendToWay sendWay)
        {
            try
            {
                const string query = "INSERT INTO \"Contact\" (\"Name\", \"CompanyId\", \"Email\", \"Phone\", \"SendWay\" ) VALUES (@name, @companyId, @email, @phone, @sendWay);";

                var args = new Dictionary<string, object>
                {
                    {"@name", name },                   
                    {"@companyId", companyId},
                    {"@email", email},
                    {"@phone", phone},
                    {"@sendWay", (int)sendWay},
                };

                using (SqlConnection con = new SqlConnection(Datasource))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        foreach (var pair in args)
                        {
                            cmd.Parameters.AddWithValue(pair.Key, pair.Value);
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                throw new Exception("Sql-Fehler InsertContact()");
            }
        }

        public void InsertMessage(string message, ulong phone = 0, string email = "")
        {
            try
            {
                int senderId = GetContactId("", phone, email, message);


                const string query = "INSERT INTO \"MessageContent\" (\"Content\") VALUES ('Datenbank neu erstellt.');";

                "INSERT INTO \"LogRecieved\" (\"RecieveTime\", \"FromContactId\", \"ContentId\") VALUES " +
                       "( CURRENT_TIMESTAMP, 0, 1, 1);",
            }
            catch
            {
                throw new Exception("Sql-Fehler InsertMessage()");
            }
        }
    }
}
