using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace KidFansChannel.Again
{
	internal static class Program
	{
		[STAThread]
		private static void Main(string[] args)
		{
			try
			{
				if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Length > 1)
				{
					Application.Exit();
					return;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
			try
			{
				Application.EnableVisualStyles();
				Again again = new Again();
				again.Run(args);
			}
			catch (Exception ex2)
			{
				try
				{
					System.IO.File.WriteAllText(
						System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log"),
						ex2.ToString());
				}
				catch (Exception)
				{
				}
				MessageBox.Show(ex2.Message);
				Application.Exit();
			}
		}
	}
}
