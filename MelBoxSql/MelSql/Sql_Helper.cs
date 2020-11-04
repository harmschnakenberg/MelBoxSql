using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MelBox
{
    public partial class MelSql
    {
        #region enums
        /// <summary>
        /// Gibt den Weg des Empfangs / der Sendung an. 
        /// </summary>
        [Flags]
        public enum SendToWay
        {
            None = 0,   //nicht weitersenden          
            Sms = 1,    //weiterleiten per SMS
            Email = 2   //weiterleiten per Email
        }

        /// <summary>
        /// Kategorien für Logging
        /// </summary>
        public enum LogTopic
        {
            Allgemein,
            Start,
            Shutdown,
            Sms,
            Email,
            Sql
        }

        /// <summary>
        /// Priorisierung von Log-EInträgen (ggf später auch Meldungen )
        /// </summary>
        public enum LogPrio
        {
            Unknown,
            Error,
            Warning,
            Info
        }

        #endregion

        #region Methods

        /// <summary>
        /// Wandelt eine Zeitangabe DateTime in einen SQLite-konformen String
        /// </summary>
        /// <param name="dt">Zeit, die umgewandelt werden soll</param>
        /// <returns></returns>
        private static string SqlTime(DateTime dt)
        {
            return dt.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss");
        }

        /// <summary>
        /// Extrahiert die ersten beiden Worte als KeyWord aus einem SMS-Text
        /// </summary>
        /// <param name="MessageContent"></param>
        /// <returns>KeyWords</returns>
        internal static string GetKeyWords(string MessageContent)
        {

            char[] split = new char[] { ' ', ',', '-', '.', ':', ';' };
            string[] words = MessageContent.Split(split);

            string KeyWords = words[0].Trim();

            if (words.Length > 1)
            {
                KeyWords += " " + words[1].Trim();
            }

            return KeyWords;
        }

        /// <summary>
        /// Gibt True aus, wenn der übergebene String dem Muster einer Email-Adresse enstricht
        /// text@text.text
        /// </summary>
        /// <param name="mailAddress"></param>
        /// <returns>True = Muster Emailadresse erfüllt.</returns>
        internal static bool IsEmail(string mailAddress)
        {
            System.Text.RegularExpressions.Regex mailIDPattern = new System.Text.RegularExpressions.Regex(@"[\w-]+@([\w-]+\.)+[\w-]+");

            if (!string.IsNullOrEmpty(mailAddress) && mailIDPattern.IsMatch(mailAddress))
                return true;
            else
                return false;
        }
        #endregion

    }
}
