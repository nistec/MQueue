#region Using directives

using System;
using System.Collections.Generic;
using System.Windows.Forms;

#endregion

namespace Nistec.Messaging.UI
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
            //McLock.Lock.ValidateLock();

			Application.EnableVisualStyles();
            Application.Run(new QueueManagmentForm());
		}
	}
}