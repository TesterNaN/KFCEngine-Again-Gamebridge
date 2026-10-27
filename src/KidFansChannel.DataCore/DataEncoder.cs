using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;

namespace KidFansChannel.DataCore
{
	/// <summary>
	/// KFCD 容器写入器 —— <see cref="DataDecoder"/> 的逆过程。
	///
	/// 布局与读取端约定一致：
	/// <code>
	///   0x00  "KFCD"
	///   0x04  int32  version
	///   0x08  int32  fileCount
	///   0x0C  int32  0
	///   0x10  条目表（3DES 加密，每条 64 字节）
	///   之后  各文件数据，每个按 16 字节对齐
	/// </code>
	///
	/// 注意：数据区起点是 <c>32 + 64*N</c>，比条目表结尾（<c>16 + 64*N</c>）多出 16 字节，
	/// 这是原实现留下的空档，为保持与既有归档兼容而照搬。
	/// </summary>
	public class DataEncoder
	{
		private string _packageName;

		private int _version;

		private int _fileNumber;

		private List<string> fileNames = new List<string>();

		private List<long> fileOffsets = new List<long>();

		private List<long> fileLengths = new List<long>();

		private List<bool> fileCryptos = new List<bool>();

		private string _currentDirectory;

		private static readonly string key = "www.KID Fans Channel.net";

		private static readonly string iv = "v2Mullen";

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

		public int Version
		{
			get
			{
				return _version;
			}
			set
			{
				_version = value;
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

		public DataEncoder(string packageName)
		{
			_packageName = packageName;
			_currentDirectory = Path.GetDirectoryName(".");
		}

		~DataEncoder()
		{
			fileCryptos.Clear();
			fileLengths.Clear();
			fileNames.Clear();
			fileOffsets.Clear();
		}

		/// <summary>
		/// 登记一个要打进包的文件。crypto 为 true 时该条目加密。
		/// 名字上限 47 字节（条目里固定占 48 字节，含结尾 0x00）。
		/// </summary>
		public void AddFile(string fileName, bool crypto)
		{
			if (fileName.Length >= 48)
			{
				throw new ArgumentException("fileName");
			}
			if (!File.Exists(Path.Combine(_currentDirectory, fileName)))
			{
				throw new ArgumentException("fileName");
			}
			fileNames.Add(fileName);
			fileCryptos.Add(crypto);
			_fileNumber++;
		}

		public void Encode()
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

			using (FileStream output = new FileStream(_packageName, FileMode.Create))
			{
				// 数据区起点：头部 16 + 条目表 64*N + 16 字节空档
				long dataOffset = 32 + 64 * _fileNumber;
				output.SetLength(dataOffset);
				output.Seek(dataOffset, SeekOrigin.Begin);

				// 逐条写入数据，每条按 16 字节对齐
				for (int i = 0; i < _fileNumber; i++)
				{
					byte[] fileBytes = File.ReadAllBytes(Path.Combine(_currentDirectory, fileNames[i]));
					fileOffsets.Add(dataOffset);
					fileLengths.Add(fileBytes.Length);
					// 预留足够空间：文件长度 + 3DES 填充，再向上取整到 16
					int slotSize = (fileBytes.Length + 24) & -16;
					using (DataStream slot = new DataStream(output, dataOffset, slotSize))
					{
						if (fileCryptos[i])
						{
							using (CryptoStream encryptor = new CryptoStream(slot, des.CreateEncryptor(keyBytes, ivBytes), CryptoStreamMode.Write))
							{
								encryptor.Write(fileBytes, 0, fileBytes.Length);
								encryptor.Flush();
								encryptor.FlushFinalBlock();
							}
						}
						else
						{
							slot.Write(fileBytes, 0, fileBytes.Length);
							slot.Flush();
						}
						// 槽内剩余部分补零
						int paddingSize = (int)(slotSize - slot.Position);
						for (int p = 0; p < paddingSize; p++)
						{
							slot.WriteByte(0);
						}
						slot.Flush();
					}
					dataOffset += slotSize;
				}

				// 回到文件头写魔数 / 版本 / 条目数
				output.Seek(0L, SeekOrigin.Begin);
				output.WriteByte(75);
				output.WriteByte(70);
				output.WriteByte(67);
				output.WriteByte(68);
				for (int i = 0; i < 4; i++)
				{
					output.WriteByte((byte)((_version >> 8 * i) & 0xFF));
				}
				for (int i = 0; i < 4; i++)
				{
					output.WriteByte((byte)((_fileNumber >> 8 * i) & 0xFF));
				}
				for (int i = 0; i < 4; i++)
				{
					output.WriteByte(0);
				}

				// 条目表：整体 3DES 加密后写到 0x10 起
				using (DataStream tableStream = new DataStream(output, 16L, 16 + 64 * _fileNumber))
				{
					using (CryptoStream encryptor = new CryptoStream(tableStream, des.CreateEncryptor(keyBytes, ivBytes), CryptoStreamMode.Write))
					{
						using (BinaryWriter writer = new BinaryWriter(encryptor))
						{
							for (int i = 0; i < _fileNumber; i++)
							{
								writer.Write(fileOffsets[i]);
								// 负数长度 = 该条目加密
								if (fileCryptos[i])
								{
									writer.Write(-fileLengths[i]);
								}
								else
								{
									writer.Write(fileLengths[i]);
								}
								// 名字定长 48 字节，不足补零
								for (int c = 0; c < 48; c++)
								{
									if (c < fileNames[i].Length)
									{
										writer.Write((byte)fileNames[i][c]);
									}
									else
									{
										writer.Write((byte)0);
									}
								}
							}
							writer.Flush();
						}
					}
				}
			}
		}
	}
}
