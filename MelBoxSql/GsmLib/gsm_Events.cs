using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GsmLib
{
    public partial class Gsm
    {
		/// <summary>
		/// Event 'System-Ereignis'
		/// </summary>
		public event EventHandler<GsmEventArgs> RaiseGsmSystemEvent;

		/// <summary>
		/// Trigger für das Event 'System-Ereignis'
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnRaiseGsmSystemEvent(GsmEventArgs e)
		{
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.
            RaiseGsmSystemEvent?.Invoke(this, e);
        }

		/// <summary>
		/// Event 'string gesendet an COM'
		/// </summary>
		public event EventHandler<GsmEventArgs> RaiseGsmSentEvent;

		/// <summary>
		/// Triggert das Event 'string gesendet an COM'
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnRaiseGsmSentEvent(GsmEventArgs e)
		{
			RaiseGsmSentEvent?.Invoke(this, e);
		}

		/// <summary>
		/// Event 'string empfangen von COM'
		/// </summary>
		public event EventHandler<GsmEventArgs> RaiseGsmRecEvent;

		/// <summary>
		/// Triggert das Event 'string empfangen von COM'
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnRaiseGsmRecEvent(GsmEventArgs e)
		{
			RaiseGsmRecEvent?.Invoke(this, e);
		}

		/// <summary>
		/// Event SMS empfangen
		/// </summary>
		public event EventHandler<ShortMessageArgs> RaiseSmsRecievedEvent;

		/// <summary>
		/// Trigger für das Event SMS empfangen
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnRaiseSmsRecievedEvent(ShortMessageArgs e)
		{
			RaiseSmsRecievedEvent?.Invoke(this, e);
		}

	}

	// Define a class to hold custom event info
	public class GsmEventArgs : EventArgs
	{
		public GsmEventArgs(uint id, string message)
		{
			Id = id;
			Message = message;
		}

		public uint Id { get; set; }
		public string Message { get; set; }
	}

	public class ShortMessageArgs : EventArgs
	{

		#region Private Variables
		private string index;
		private string status;
		private string sender;
		private string alphabet;
		private string sent;
		private string message;
		#endregion

		#region Public Properties
		public string Index
		{
			get { return index; }
			set { index = value; }
		}
		public string Status
		{
			get { return status; }
			set { status = value; }
		}
		public string Sender
		{
			get { return sender; }
			set { sender = value; }
		}
		public string Alphabet
		{
			get { return alphabet; }
			set { alphabet = value; }
		}
		public string Sent
		{
			get { return sent; }
			set { sent = value; }
		}
		public string Message
		{
			get { return message; }
			set { message = value; }
		}
		#endregion

	}

}
