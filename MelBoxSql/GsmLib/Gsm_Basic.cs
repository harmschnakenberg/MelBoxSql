using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace GsmLib
{
    public partial class Gsm
    {
        #region Events
        
        #endregion

        #region Fields
        public SerialPort Port;
        public AutoResetEvent receiveNow;
        #endregion

        #region Properties
        /// <summary>
        /// Alle angeschlossenen COM-Ports
        /// </summary>
        public List<string> AvailableComPorts { get; } = System.IO.Ports.SerialPort.GetPortNames().ToList();

        public string CurrentComPortName { get; set; } = Properties.Settings.Default.ComPort;

        #endregion

        #region Methods

        #region Basic COM
        public Gsm()
        {
            int n = 0;
            while (!CheckComPort() && n < 3)
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11011901, string.Format("Verbindungsversuch {0}/3 an {1}", ++n, Port.PortName)));
                Thread.Sleep(3000);
            }
        }

        //Prüft, ob der ComPort bereit ist 
        public bool CheckComPort()
        {
            if (Port == null || !Port.IsOpen)
            {
                ConnectPort();
            }

            if (Port != null && Port.IsOpen)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Verbindet den COM-Port
        /// </summary>
        private void ConnectPort()
        {            
            #region richtigen COM-Port ermitteln
            if (AvailableComPorts.Count < 1)
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11011512, "Es sind keine COM-Ports vorhanden"));
            }

            if (!AvailableComPorts.Contains(CurrentComPortName))
            {
                CurrentComPortName = AvailableComPorts.LastOrDefault();
            }
            #endregion

            OnRaiseGsmSystemEvent(new GsmEventArgs(11051108, string.Format("Öffne Port {0}...", CurrentComPortName)));

            #region Wenn Port bereits vebunden ist, trennen
            if (Port != null && Port.IsOpen)
            {
                ClosePort();
            }
            #endregion

            #region Verbinde ComPort
            receiveNow = new AutoResetEvent(false);
            SerialPort port = new SerialPort();

            try
            {
                port.PortName = CurrentComPortName;                     //COM1
                port.BaudRate = Properties.Settings.Default.Baudrate;   //9600
                port.DataBits = Properties.Settings.Default.DataBits;   //8
                port.StopBits = StopBits.One;                           //1
                port.Parity = Parity.None;                              //None
                port.ReadTimeout = 300;                                 //300
                port.WriteTimeout = 300;                                //300
                port.Encoding = Encoding.GetEncoding("iso-8859-1");
                port.DataReceived += new SerialDataReceivedEventHandler(Port_DataReceived);
                port.Open();
                port.DtrEnable = true;
                port.RtsEnable = true;
            }
            catch (Exception ex)
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11011514, string.Format("COM-Port {0} konnte nicht verbunden werden. \r\n{1}\r\n{2}", CurrentComPortName, ex.GetType(), ex.Message)));
            }

            Port = port;
            #endregion
        }

        //Close Port
        public void ClosePort()
        {
            if (Port == null) return;

            OnRaiseGsmSystemEvent(new GsmEventArgs(11011917, "Port " + Port.PortName + " wird geschlossen.\r\n"));
            try
            {
                Port.Close();
                Port.DataReceived -= new SerialDataReceivedEventHandler(Port_DataReceived);
                Port.Dispose();
                Port = null;
                Thread.Sleep(5000);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        // Send AT Command
        public string SendATCommand(string command)
        {
            if (!CheckComPort())
            {
                OnRaiseGsmSystemEvent(new GsmEventArgs(11011708, "Port nicht bereit für SendATCommand()"));
                return null;
            }

            try
            {
                Port.DiscardOutBuffer();
                Port.DiscardInBuffer();
                receiveNow.Reset();
                Port.Write(command + "\r");
                OnRaiseGsmSentEvent(new GsmEventArgs(11051045, command));

                string input = ReadResponse();

                OnRaiseGsmRecEvent(new GsmEventArgs(11051044, input));

                if ((input.Length == 0) || ((!input.EndsWith("\r\n> ")) && (!input.EndsWith("\r\nOK\r\n"))))
                {
                    OnRaiseGsmSystemEvent(new GsmEventArgs(11021909, "Fehlerhaft Empfangen:\n\r" + input));
                    //throw new ApplicationException("No success message was received.");
                }
  
                return input;
            }
            catch (ApplicationException)
            {
                //"No Data recieved from phone."
                return null;
            }
            catch (System.IO.IOException io_ex)
            {
                //Ein nicht vorhandenes Gerät...
                OnRaiseGsmSystemEvent(new GsmEventArgs(11021909, io_ex.Message));
                Thread.Sleep(3000);
                return SendATCommand(command);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Receive data from port
        public void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (e.EventType == SerialData.Chars)
                {                  
                    receiveNow.Set();
                    //EXPERIMENT:
                    //ReadResponse();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //lese Antwort auf SendATCommand()
        public string ReadResponse()
        {
            string serialPortData = string.Empty;
            try
            {
                do
                {
                    if (receiveNow.WaitOne(300, false))
                    {
                        string data = Port.ReadExisting();
                        serialPortData += data;
                    }
                    else
                    {
                        if (serialPortData.Length > 0)
                            throw new ApplicationException("Response received is incomplete.");
                        else
                            throw new ApplicationException("No data received from phone.");
                    }
                }
                while (!serialPortData.EndsWith("\r\nOK\r\n") && !serialPortData.EndsWith("\r\n> ") && !serialPortData.EndsWith("\r\nERROR\r\n"));
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return serialPortData;
        }

        #endregion



        #endregion
    }



}