using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace KidFansChannel.Native
{
	// Token: 0x02000003 RID: 3
	public class Invoke : IInvoke
	{
		// Token: 0x06000002 RID: 2 RVA: 0x00002050 File Offset: 0x00001050
		public void StartProgram(byte[] prog, string pwd, string[] args)
		{
			try
			{
				string text = "KID Fans Channel";
				byte[] array = new byte[24];
				byte[] array2 = new byte[8];
				for (int i = 0; i < 24; i++)
				{
					if (i < text.Length)
					{
						array[i] = (byte)text[i];
					}
					else
					{
						array[i] = 0;
					}
				}
				for (int j = 0; j < 8; j++)
				{
					if (j < pwd.Length)
					{
						array2[j] = (byte)pwd[j];
					}
					else
					{
						array2[j] = 0;
					}
				}
				TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
				ICryptoTransform cryptoTransform = tripleDESCryptoServiceProvider.CreateDecryptor(array, array2);
				byte[] array3;
				using (MemoryStream memoryStream = new MemoryStream(prog))
				{
					using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
					{
						array3 = new byte[prog.Length - 8];
						cryptoStream.Read(array3, 0, array3.Length);
					}
				}
				Assembly assembly = Assembly.Load(array3);
				string[] array4 = new string[args.Length - 1];
				for (int k = 0; k < array4.Length; k++)
				{
					array4[k] = args[k + 1];
				}
				assembly.EntryPoint.Invoke(null, new object[] { array4 });
			}
			catch (Exception ex)
			{
				if (ex.InnerException != null)
				{
					MessageBox.Show(string.Concat(new string[]
					{
						ex.Message,
						"\n",
						ex.InnerException.Message,
						"\n",
						ex.InnerException.Source,
						"\n",
						ex.InnerException.StackTrace
					}));
				}
				else
				{
					MessageBox.Show(ex.Message);
				}
			}
		}
	}
}
