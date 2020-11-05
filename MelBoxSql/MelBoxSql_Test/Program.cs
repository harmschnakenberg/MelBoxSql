using GsmLib;
using MelBox;
using System;
using System.Runtime.Remoting.Channels;
using System.Timers;

namespace MelBoxSql_Test
{
    class Program
    {
        static void Main()
        {
            MelSql sql = new MelSql();

            sql.Log(MelBox.MelSql.LogTopic.Start, MelSql.LogPrio.Info, "Dies ist der erste Manuelle Eintrag.");

            sql.UpdateBlockedMessage(1, 8, 16, 7);

            sql.UpdateCompany(1, "_UNBEKANNT2_");

            sql.UpdateContact(2, MelSql.SendToWay.Email, "", 0, "", 4915142265412);

            sql.UpdateShift(1, DateTime.Now, DateTime.Now.AddHours(1));


            //-------------------------------------------------------------------------------------------------------
            Console.WriteLine("Öffne COM-Port...");
            Gsm gsm = new Gsm();

            gsm.RaiseGsmSystemEvent += HandleGsmSystemEvent;
            gsm.RaiseGsmRecEvent += HandleGsmRecEvent;
            gsm.RaiseGsmSentEvent += HandleGsmSentEvent;
            gsm.RaiseSmsRecievedEvent += HandleSmsRecievedEvent;

            Console.WriteLine("Mobilfunktnetzerk " + (gsm.IsSimRegiserd() ? "registriert" : "kein Empfang"));

            int sig = gsm.GetSignalQuality();

            Console.WriteLine("Signalqualität " + sig + "%");

            Console.WriteLine("Abboniere neu ankommende SMS.");
            gsm.SubscribeNewSms();

            Console.WriteLine("Lese alle vorhandenen SMS:");
            gsm.ReadMessage();

            Console.WriteLine("Beliebige Taste zum beenden...");
            //Timer timer = new Timer(5000);
            //timer.Elapsed += Timer_Elapsed;
            //timer.Start();
            //while (!Console.KeyAvailable)
            //{
            //    // Infinite loop.
            //}

            //Console.WriteLine("COM-Port freigeben...");
            gsm.ClosePort();
            Console.WriteLine("beendet.");
            Console.ReadKey();

        }

        // Define what actions to take when the event is raised.
        static void HandleGsmSystemEvent(object sender, GsmEventArgs e)
        {
            Console.BackgroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(e.Id +": " + e.Message);
            Console.BackgroundColor = ConsoleColor.Black;
        }

        static void HandleGsmSentEvent(object sender, GsmEventArgs e)
        {
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(e.Id + ": " + e.Message);
            Console.BackgroundColor = ConsoleColor.Black;
        }

        static void HandleGsmRecEvent(object sender, GsmEventArgs e)
        {
            Console.BackgroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(e.Id + ": " + e.Message);
            Console.BackgroundColor = ConsoleColor.Black;
        }


        // Define what actions to take when the event is raised.
        static void HandleSmsRecievedEvent(object sender, ShortMessageArgs e)
        {
            Console.WriteLine(
                "Neue SMS:\r\n" +
                $"Index:\t\t{e.Index}\n\r" +
                $"Sent:\t\t{e.Sent}\n\r" +
                $"Sender:\t{e.Sender}\n\r" +
                $"Message:\t{e.Message}\n\r" +
                "*ENDE*");
        }

        //private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    // Use SignalTime.
        //    DateTime time = e.SignalTime;
        //    Console.WriteLine("Lese um " + time);
        //    gsm.ReadResponse()
        //}

    }
}
