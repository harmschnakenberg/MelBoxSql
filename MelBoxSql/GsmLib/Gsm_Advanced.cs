using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GsmLib
{
	public partial class Gsm
	{

        #region Properties
        #endregion

        #region Gsm Operationen
        /// <summary>
        /// Gibt dieSignalstärke in Prozent aus.
        /// </summary>
        /// <returns></returns>
        public int GetSignalQuality()
		{
			int sig_qual = 0;
			string strResp1 = SendATCommand("AT+CSQ");
			if (strResp1 == null)
				return sig_qual;

			string pattern = @"\+CSQ: \d+,";
			string strResp2 = System.Text.RegularExpressions.Regex.Match(strResp1, pattern).Groups[0].Value;
			if (strResp2 == null)
				return sig_qual;

			int.TryParse(strResp2.Substring(6, 2), out sig_qual);

			return sig_qual * 100 / 31;
		}

		/// <summary>
		/// Prüft, ob die SIM-Karte im Mobilfunknetz registriert ist 
		/// </summary>
		public bool IsSimRegiserd()
		{
			string strResp1 = SendATCommand("AT+CREG?");
			if (strResp1 == null)
				return false;

			string pattern = @"\+CREG: \d,\d";
			string strResp2 = System.Text.RegularExpressions.Regex.Match(strResp1, pattern).Groups[0].Value;
			if (strResp2 == null)
				return false;

			int.TryParse(strResp2.Substring(7, 1), out int RegisterStatus);
			int.TryParse(strResp2.Substring(9, 1), out int AccessStatus);

			Console.WriteLine("Status >" + RegisterStatus + "<");
			Console.WriteLine("Access >" + AccessStatus + "<");

			return (strResp2 != "+CREG: 0,0");
		}
		#endregion

		#region SMS empfangen


		public void SubscribeNewSms()
		{
			try
			{
				//Text-Modus
				//SendATCommand("AT+CMGF=1");

				/*/
				+CNMI: <mode>,<mt>,<bm>,<ds>,<bfr>
				< mode >
					0 – Do not forward unsolicited result codes to the Terminal Equipment(TE) (default).
					2 - (TextMode)Indicates that new message has been received 
					3 – Forward unsolicited result codes directly to the TE.
				< mt >
					0 – No received message notifications, the modem acts as an SMS client.
					1 - If  SMS-DELIVER  is  stored  in  ME/TA,  indication  of  the  memory  location  is
						routed to the TE using unsolicited result code.
					2 – SMS - DELIVERs(except class 2 and message waiting indication group) are routed directly to the TE.
				<bm>
					2 - New CBMs are routed directly to the TE using unsolicited result code.
				<ds>
					0 – No SMS-STATUS-REPORTs are routed to the TE.
					1 – SMS-STATUS-REPORTs are routed to the TE using unsolicited result code: +CDS: <length><CR><LF><pdu>.
				<bfr>
					1 – The buffer of unsolicited result codes is cleared when<mode> 1...3 is entered
				//*/
				string command = "AT+CNMI=2,1,2,0,1";
				SendATCommand(command);
			}
			catch (Exception ex)
			{
				throw new Exception("Fehler SMS lesen\r\n" + ex.GetType() + "\r\n" + ex.Message);
			}
		}

		public void ReadMessage(string filter = "ALL")
		{
			string command = string.Format("AT+CMGL=\"{0}\"", filter);

			try
			{
				//Textmode
				SendATCommand("AT+CMGF=1");
				// Use character set "PCCP437"
				//ExecCommand(port,"AT+CSCS=\"PCCP437\"", 300, "Failed to set character set.");
				// Select SIM storage SM = SIM, MT = Mobil + SIM
				SendATCommand("AT+CPMS=\"MT\"");
				// Read the messages
				string input = SendATCommand(command);

				//TEST:
				Console.WriteLine("#INPUT: " + input);

				//Rohantwort Interpretieren
				ParseMessages(input);
			}
			catch (Exception ex)
			{
				throw new Exception("Fehler SMS lesen\r\n" + ex.GetType() + "\r\n" + ex.Message);
			}
		}

		private void ParseMessages(string input)
		{
			if (input == null) return;
			try
			{
				Regex r = new Regex(@"\+CMGL: (\d+),""(.+)"",""(.+)"",(.*),""(.+)""\r\n(.+)\r\n");
				Match m = r.Match(input);
				while (m.Success)
				{
					ShortMessageArgs msg = new ShortMessageArgs
					{
						Index = m.Groups[1].Value,
						Status = m.Groups[2].Value,
						Sender = m.Groups[3].Value,
						Alphabet = m.Groups[4].Value,
						Sent = m.Groups[5].Value,
						Message = m.Groups[6].Value
					};

					OnRaiseSmsRecievedEvent(msg);
					m = m.NextMatch();
				}
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}

        #endregion
    }
}