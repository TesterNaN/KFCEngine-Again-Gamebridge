using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace KidFansChannel.DataCore
{
	/// <summary>
	/// KFCD 容器读取器。
	///
	/// 容器布局（所有多字节整数为小端）：
	/// <code>
	///   0x00   "KFCD"            魔数
	///   0x04   int32  version    版本
	///   0x08   int32  count      条目数
	///   0x0C   int32  (保留，恒为 0)
	///   0x10   条目表            3DES 加密，每条 64 字节：
	///            int64  offset    数据区偏移
	///            int64  length    长度；负数表示该条目已加密，取绝对值
	///            byte[48] name    文件名，以 0x00 填充到 48 字节
	/// </code>
	///
	/// 加密参数（写死在程序里）：3DES-CBC，key = "www.KID Fans Channel.net"，
	/// iv = "v2Mullen"。密钥只对条目表与标记为加密的条目生效。
	/// </summary>
	public class DataDecoder : IDisposable
	{
		private string _packageName;

		private int _version;

		private int _fileNumber;

		private List<string> fileNames = new List<string>();

		private List<long> fileOffsets = new List<long>();

		private List<long> fileLengths = new List<long>();

		/// <summary>与 fileNames 同下标：该条目的数据是否经过 3DES 加密。</summary>
		private List<bool> fileCryptos = new List<bool>();

		private string _currentDirectory;

		private static readonly string key = "www.KID Fans Channel.net";

		private static readonly string iv = "v2Mullen";

		private Stream fs;

		private ICryptoTransform ct;

		public int Version
		{
			get
			{
				return _version;
			}
		}

		public string PackageName
		{
			get
			{
				return _packageName;
			}
		}

		public int FileNumber
		{
			get
			{
				return _fileNumber;
			}
		}

		public string CurrentDirectory
		{
			get
			{
				return _currentDirectory;
			}
			set
			{
				_currentDirectory = value;
			}
		}

		public DataDecoder(string packageName)
		{
			_packageName = packageName;
			_currentDirectory = Path.GetDirectoryName(".");
			SetUpDecryptor();
			fs = new FileStream(_packageName, FileMode.Open, FileAccess.Read);
			DecodeHeader();
		}

		public DataDecoder(string packageName, Stream packageStream)
		{
			_packageName = packageName;
			_currentDirectory = Path.GetDirectoryName(".");
			SetUpDecryptor();
			fs = packageStream;
			DecodeHeader();
		}

		/// <summary>把写死的 key / iv 填成定长字节数组，建立 3DES 解密器。</summary>
		private void SetUpDecryptor()
		{
			byte[] keyBytes = new byte[24];
			byte[] ivBytes = new byte[8];
			for (int i = 0; i < 24; i++)
			{
				keyBytes[i] = (byte)key[i];
			}
			for (int j = 0; j < 8; j++)
			{
				ivBytes[j] = (byte)iv[j];
			}
			TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();
			ct = des.CreateDecryptor(keyBytes, ivBytes);
		}

		~DataDecoder()
		{
			Dispose();
		}

		/// <summary>校验魔数并读出条目表。魔数不对时抛 InvalidDataException。</summary>
		protected void DecodeHeader()
		{
			fs.Seek(0L, SeekOrigin.Begin);
			if (fs.ReadByte() == 75 && fs.ReadByte() == 70 && fs.ReadByte() == 67 && fs.ReadByte() == 68)
			{
				// "KFCD"
				_version = 0;
				for (int i = 0; i < 4; i++)
				{
					_version += fs.ReadByte() << 8 * i;
				}
				_fileNumber = 0;
				for (int j = 0; j < 4; j++)
				{
					_fileNumber += fs.ReadByte() << 8 * j;
				}
				fs.Seek(16L, SeekOrigin.Begin);
				if (_fileNumber <= 0)
				{
					return;
				}
				// 条目表整体在 3DES 加密区里，长度 = 条目数 × 64
				using (DataStream tableStream = new DataStream(fs, 16L, 16 + 64 * _fileNumber))
				{
					using (CryptoStream decrypted = new CryptoStream(tableStream, ct, CryptoStreamMode.Read))
					{
						using (BinaryReader reader = new BinaryReader(decrypted))
						{
							for (int k = 0; k < _fileNumber; k++)
							{
								fileOffsets.Add(reader.ReadInt64());
								long recordLength = reader.ReadInt64();
								if (recordLength >= 0)
								{
									fileLengths.Add(recordLength);
									fileCryptos.Add(false);
								}
								else
								{
									// 负数长度：该条目加密，绝对值才是真实长度
									fileLengths.Add(-recordLength);
									fileCryptos.Add(true);
								}
								// 文件名占 48 字节，遇 0x00 即结束，其余为填充
								StringBuilder nameBuilder = new StringBuilder();
								bool nameEnded = false;
								for (int l = 0; l < 48; l++)
								{
									byte ch = reader.ReadByte();
									if (!nameEnded)
									{
										if (ch == 0)
										{
											nameEnded = true;
										}
										else
										{
											nameBuilder.Append((char)ch);
										}
									}
								}
								fileNames.Add(nameBuilder.ToString());
							}
							return;
						}
					}
				}
			}
			throw new InvalidDataException();
		}

		public string[] GetFileList()
		{
			return fileNames.ToArray();
		}

		/// <summary>按名字取出一条的只读流；找不到返回 null。加密条目在内存中解密。</summary>
		public Stream DecodeStream(string fileName)
		{
			int index = fileNames.IndexOf(fileName);
			if (index < 0)
			{
				return null;
			}
			if (fileCryptos[index])
			{
				// 密文长度向上对齐到 8 字节（3DES 块大小）
				using (DataStream cipherStream = new DataStream(fs, fileOffsets[index], (fileLengths[index] + 8) & 0xFFFFFFF8u))
				{
					cipherStream.Seek(0L, SeekOrigin.Begin);
					using (CryptoStream decrypted = new CryptoStream(cipherStream, ct, CryptoStreamMode.Read))
					{
						byte[] plain = new byte[fileLengths[index]];
						decrypted.Read(plain, 0, plain.Length);
						MemoryStream result = new MemoryStream(plain, 0, (int)fileLengths[index]);
						result.Seek(0L, SeekOrigin.Begin);
						return result;
					}
				}
			}
			DataStream rawStream = new DataStream(fs, fileOffsets[index], fileLengths[index]);
			rawStream.Seek(0L, SeekOrigin.Begin);
			return rawStream;
		}

		/// <summary>把一条解出来写到 CurrentDirectory 下（按名字里的路径自动建目录）。</summary>
		public void DecodeFile(string fileName)
		{
			int index = fileNames.IndexOf(fileName);
			if (index < 0)
			{
				return;
			}
			byte[] data = new byte[fileLengths[index]];
			if (fileCryptos[index])
			{
				using (DataStream cipherStream = new DataStream(fs, fileOffsets[index], (fileLengths[index] + 8) & 0xFFFFFFF8u))
				{
					cipherStream.Seek(0L, SeekOrigin.Begin);
					using (CryptoStream decrypted = new CryptoStream(cipherStream, ct, CryptoStreamMode.Read))
					{
						decrypted.Read(data, 0, data.Length);
					}
				}
			}
			else
			{
				using (DataStream rawStream = new DataStream(fs, fileOffsets[index], fileLengths[index]))
				{
					rawStream.Seek(0L, SeekOrigin.Begin);
					rawStream.Read(data, 0, data.Length);
				}
			}
			string outputPath = Path.Combine(_currentDirectory, fileName);
			if (!Directory.Exists(Path.GetDirectoryName(outputPath)))
			{
				Directory.CreateDirectory(Path.GetDirectoryName(outputPath));
			}
			File.WriteAllBytes(outputPath, data);
		}

		/// <summary>把整个容器解包到磁盘。</summary>
		public void Decode()
		{
			foreach (string fileName in fileNames)
			{
				DecodeFile(fileName);
			}
		}

		public void Dispose()
		{
			fs.Close();
			fileCryptos.Clear();
			fileLengths.Clear();
			fileNames.Clear();
			fileOffsets.Clear();
		}
	}
}
