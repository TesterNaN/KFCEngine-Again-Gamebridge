using System;
using System.Collections.Generic;
using System.IO;

namespace KidFansChannel.DataCore
{
	/// <summary>
	/// 数据源总控：把「若干目录 + 若干 KFCD 包」当成一个统一的虚拟文件系统。
	///
	/// 一个数据名（dataName）可以对应：
	///   · 一个同名目录（散装文件）
	///   · 若干分卷包   {dataName}{00..99}.{ext}
	///   · 一个整包     {dataName}.{ext}
	/// 每种还可能带一份语言覆盖包 {dataName}{nn}.{langExt}.{ext} / {dataName}.{langExt}.{ext}，
	/// 查文件时语言包优先。
	///
	/// 查找结果通过 nowData / nowFile / nowStream 三个字段回传，
	/// 由 GetFile() 与 GetStream() 取走。
	/// </summary>
	public class DataCore : IDisposable
	{
		/// <summary>一次查找命中的来源类型。</summary>
		public enum DataType
		{
			Null,
			Package,
			File,
			Any
		}

		private List<string> directoryNames = new List<string>();

		private List<string> dataNames = new List<string>();

		/// <summary>按 dataName 分组：该名下每个来源是否是一个包（false = 目录）。</summary>
		private List<List<bool>> dataIsPackages = new List<List<bool>>();

		/// <summary>按 dataName 分组：该名下的散装目录路径。</summary>
		private List<List<string>> dataDirectories = new List<List<string>>();

		/// <summary>按 dataName 分组：包读取器（与 dataIsPackages 中为 true 的项一一对应）。</summary>
		private List<List<DataDecoder>> dataDecoders = new List<List<DataDecoder>>();

		/// <summary>与 dataDecoders 同下标：对应的语言覆盖包，没有则为 null。</summary>
		private List<List<DataDecoder>> dataDecoderWithLanguages = new List<List<DataDecoder>>();

		private bool currentDirectorySetted;

		private bool directoryFinished;

		private string _languageExt = "";

		private string _extension = "dat";

		/// <summary>外部注入的替换流：同名文件优先用内存里的这份，而不是磁盘上的。</summary>
		private List<string> replaceNames = new List<string>();

		private List<Stream> replaceStreams = new List<Stream>();

		private DataType nowData;

		private string nowFile;

		private Stream nowStream;

		public string Extension
		{
			get
			{
				return _extension;
			}
			set
			{
				_extension = value;
			}
		}

		public string LanguageExt
		{
			get
			{
				return _languageExt;
			}
			set
			{
				_languageExt = value;
			}
		}

		~DataCore()
		{
			Dispose();
		}

		public void SetCurrentDirectory(string currentDirectory)
		{
			if (currentDirectorySetted || directoryFinished)
			{
				throw new InvalidOperationException();
			}
			currentDirectorySetted = true;
			dataNames.Clear();
			directoryNames.Add(currentDirectory);
		}

		public void AddReplacePair(string name, Stream stream)
		{
			replaceNames.Add(name);
			replaceStreams.Add(stream);
		}

		public void AddDirectory(string directory)
		{
			if (!currentDirectorySetted || directoryFinished)
			{
				throw new InvalidOperationException();
			}
			if (!directoryNames.Contains(Path.GetFullPath(directory)))
			{
				directoryNames.Add(Path.GetFullPath(directory));
			}
		}

		/// <summary>
		/// 登记一个数据名，并扫描所有已加入的目录，把它名下的散装目录 / 分卷包 / 整包都挂进来。
		/// 分卷包倒序挂入（后卷优先被查到）。
		/// </summary>
		public void AddData(string dataName)
		{
			if (!currentDirectorySetted)
			{
				throw new InvalidProgramException();
			}
			directoryFinished = true;
			if (dataNames.Contains(dataName))
			{
				return;
			}
			dataNames.Add(dataName);
			int dataIndex = dataIsPackages.Count;
			dataIsPackages.Add(new List<bool>());
			dataDecoders.Add(new List<DataDecoder>());
			dataDecoderWithLanguages.Add(new List<DataDecoder>());
			dataDirectories.Add(new List<string>());

			for (int dirIndex = 0; dirIndex < directoryNames.Count; dirIndex++)
			{
				string candidatePath;
				// 1) 同名目录：当散装文件处理
				if (Directory.Exists(candidatePath = Path.Combine(directoryNames[dirIndex], dataName)))
				{
					dataIsPackages[dataIndex].Add(false);
					dataDirectories[dataIndex].Add(candidatePath);
				}

				List<DataDecoder> volumeDecoders = new List<DataDecoder>();
				List<DataDecoder> volumeLangDecoders = new List<DataDecoder>();
				string volumePath;
				string langVolumePath;

				// 2) 分卷包 {dataName}{00..99}.{ext}
				for (int volume = 0; volume < 100; volume++)
				{
					if (!File.Exists(volumePath = Path.Combine(directoryNames[dirIndex], string.Format("{0}{1:D2}.{2}", dataName, volume, _extension))) && !replaceNames.Contains(Path.GetFileName(volumePath)))
					{
						break;
					}
					try
					{
						bool volumeReplaced = replaceNames.Contains(Path.GetFileName(volumePath));
						DataDecoder volumeDecoder = volumeReplaced
							? new DataDecoder(volumePath, replaceStreams[replaceNames.IndexOf(Path.GetFileName(volumePath))])
							: new DataDecoder(volumePath);
						dataIsPackages[dataIndex].Add(true);
						volumeDecoders.Add(volumeDecoder);

						// 对应的语言覆盖包
						if (File.Exists(langVolumePath = Path.Combine(directoryNames[dirIndex], string.Format("{0}{1:D2}.{2}.{3}", dataName, volume, _languageExt, _extension))) || replaceNames.Contains(Path.GetFileName(langVolumePath)))
						{
							try
							{
								bool langReplaced = replaceNames.Contains(Path.GetFileName(langVolumePath));
								DataDecoder langDecoder = langReplaced
									? new DataDecoder(langVolumePath, replaceStreams[replaceNames.IndexOf(Path.GetFileName(langVolumePath))])
									: new DataDecoder(langVolumePath);
								volumeLangDecoders.Add(langDecoder);
							}
							catch (InvalidDataException)
							{
								volumeLangDecoders.Add(null);
							}
						}
						volumeLangDecoders.Add(null);
					}
					catch (InvalidDataException)
					{
					}
				}

				// 后卷优先：倒序挂入
				for (int volume = volumeDecoders.Count - 1; volume >= 0; volume--)
				{
					dataDecoders[dataIndex].Add(volumeDecoders[volume]);
					dataDecoderWithLanguages[dataIndex].Add(volumeLangDecoders[volume]);
				}
				volumeDecoders.Clear();
				volumeLangDecoders.Clear();

				// 3) 整包 {dataName}.{ext}
				if (!File.Exists(volumePath = Path.Combine(directoryNames[dirIndex], string.Format("{0}.{1}", dataName, _extension))) && !replaceNames.Contains(Path.GetFileName(volumePath)))
				{
					continue;
				}
				try
				{
					bool singleReplaced = replaceNames.Contains(Path.GetFileName(volumePath));
					DataDecoder singleDecoder = singleReplaced
						? new DataDecoder(volumePath, replaceStreams[replaceNames.IndexOf(Path.GetFileName(volumePath))])
						: new DataDecoder(volumePath);
					dataIsPackages[dataIndex].Add(true);
					dataDecoders[dataIndex].Add(singleDecoder);
				}
				catch (InvalidDataException)
				{
				}

				// 整包的语言覆盖
				if (File.Exists(langVolumePath = Path.Combine(directoryNames[dirIndex], string.Format("{0}.{1}.{2}", dataName, _languageExt, _extension))) || replaceNames.Contains(Path.GetFileName(langVolumePath)))
				{
					try
					{
						bool langReplaced = replaceNames.Contains(Path.GetFileName(langVolumePath));
						DataDecoder langDecoder = langReplaced
							? new DataDecoder(langVolumePath, replaceStreams[replaceNames.IndexOf(Path.GetFileName(langVolumePath))])
							: new DataDecoder(langVolumePath);
						dataDecoderWithLanguages[dataIndex].Add(langDecoder);
					}
					catch (InvalidDataException)
					{
						dataDecoderWithLanguages[dataIndex].Add(null);
					}
				}
				dataDecoderWithLanguages[dataIndex].Add(null);
			}
		}

		public DataType CheckData(string dataName, string pathName)
		{
			return CheckData(dataName, pathName, DataType.Any);
		}

		/// <summary>
		/// 在指定数据名下查找 pathName。命中时 nowData / nowFile / nowStream 被填好；
		/// 未命中则三者被清空。
		/// </summary>
		public DataType CheckData(string dataName, string pathName, DataType defaultType)
		{
			int dataIndex = dataNames.IndexOf(dataName);
			if (dataIndex == -1)
			{
				nowData = DataType.Null;
				nowFile = null;
				if (nowStream != null)
				{
					nowStream.Close();
				}
				nowStream = null;
				return DataType.Null;
			}
			bool found = false;
			switch (defaultType)
			{
			case DataType.Package:
			{
				for (int packIdx = 0; packIdx < dataDecoders[dataIndex].Count; packIdx++)
				{
					try
					{
						if (CheckPackage(dataDecoders[dataIndex][packIdx], dataDecoderWithLanguages[dataIndex][packIdx], pathName))
						{
							found = true;
							break;
						}
					}
					catch (Exception)
					{
						// 包被删掉或读坏了：从名单里摘掉，继续试下一个
						if (!File.Exists(dataDecoders[dataIndex][packIdx].PackageName) || !File.Exists(dataDecoderWithLanguages[dataIndex][packIdx].PackageName))
						{
							RemovePackage(dataIndex, packIdx);
						}
					}
				}
				if (!found)
				{
					for (int dirIdx = 0; dirIdx < dataDirectories[dataIndex].Count; dirIdx++)
					{
						if (CheckFile(dataDirectories[dataIndex][dirIdx], pathName))
						{
							found = true;
							break;
						}
					}
				}
				if (found)
				{
					break;
				}
				goto default;
			}
			case DataType.File:
			{
				for (int dirIdx = 0; dirIdx < dataDirectories[dataIndex].Count; dirIdx++)
				{
					if (CheckFile(dataDirectories[dataIndex][dirIdx], pathName))
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					for (int packIdx = 0; packIdx < dataDecoders[dataIndex].Count; packIdx++)
					{
						try
						{
							if (CheckPackage(dataDecoders[dataIndex][packIdx], dataDecoderWithLanguages[dataIndex][packIdx], pathName))
							{
								found = true;
								break;
							}
						}
						catch (Exception)
						{
							if (!File.Exists(dataDecoders[dataIndex][packIdx].PackageName) || !File.Exists(dataDecoderWithLanguages[dataIndex][packIdx].PackageName))
							{
								RemovePackage(dataIndex, packIdx);
							}
						}
					}
				}
				if (found)
				{
					break;
				}
				goto default;
			}
			case DataType.Any:
			{
				// dataIsPackages 是混排的，包与目录各用一个游标推进
				int packIdx = 0;
				int dirIdx = 0;
				for (int i = 0; i < dataIsPackages[dataIndex].Count; i++)
				{
					if (dataIsPackages[dataIndex][i])
					{
						try
						{
							if (CheckPackage(dataDecoders[dataIndex][packIdx], dataDecoderWithLanguages[dataIndex][packIdx], pathName))
							{
								found = true;
								break;
							}
							packIdx++;
						}
						catch (Exception)
						{
							if (!File.Exists(dataDecoders[dataIndex][packIdx].PackageName) || !File.Exists(dataDecoderWithLanguages[dataIndex][packIdx].PackageName))
							{
								RemovePackage(dataIndex, packIdx);
							}
							// 该包已被摘掉，索引不前移，重新检查同一位
							i--;
						}
					}
					else
					{
						if (CheckFile(dataDirectories[dataIndex][dirIdx], pathName))
						{
							found = true;
							break;
						}
						dirIdx++;
					}
				}
				if (found)
				{
					break;
				}
				goto default;
			}
			default:
				nowData = DataType.Null;
				nowFile = null;
				if (nowStream != null)
				{
					nowStream.Close();
				}
				nowStream = null;
				break;
			}
			return nowData;
		}

		/// <summary>从名单里摘掉一个包（同时修正 dataIsPackages 的对应标记）。</summary>
		private void RemovePackage(int dataIndex, int decoderIndex)
		{
			dataDecoders[dataIndex].RemoveAt(decoderIndex);
			dataDecoderWithLanguages[dataIndex].RemoveAt(decoderIndex);
			int packSeen = 0;
			for (int i = 0; i < dataIsPackages[dataIndex].Count; i++)
			{
				if (dataIsPackages[dataIndex][i])
				{
					if (packSeen == decoderIndex)
					{
						dataIsPackages[dataIndex].RemoveAt(i);
						break;
					}
					packSeen++;
				}
			}
		}

		/// <summary>在散装目录里找文件，优先带语言后缀的那份。</summary>
		private bool CheckFile(string directory, string pathName)
		{
			string fileName = Path.GetFileName(pathName);
			string relativeDir = pathName.Substring(0, pathName.Length - fileName.Length);
			string hit;
			if (File.Exists(hit = Path.Combine(directory, string.Format("{0}{1}\\{2}", relativeDir, _languageExt, fileName))))
			{
				nowData = DataType.File;
				nowFile = hit;
				if (nowStream != null)
				{
					nowStream.Close();
				}
				nowStream = null;
				return true;
			}
			if (File.Exists(hit = Path.Combine(directory, pathName)))
			{
				nowData = DataType.File;
				nowFile = hit;
				if (nowStream != null)
				{
					nowStream.Close();
				}
				nowStream = null;
				return true;
			}
			return false;
		}

		/// <summary>在包里查，语言包优先。</summary>
		private bool CheckPackage(DataDecoder decoder, DataDecoder decoderWithLanguage, string pathName)
		{
			Stream hit;
			if (decoderWithLanguage != null && (hit = decoderWithLanguage.DecodeStream(pathName)) != null)
			{
				nowData = DataType.Package;
				nowFile = null;
				if (nowStream != null)
				{
					nowStream.Close();
				}
				nowStream = hit;
				return true;
			}
			if ((hit = decoder.DecodeStream(pathName)) != null)
			{
				nowData = DataType.Package;
				nowFile = null;
				if (nowStream != null)
				{
					nowStream.Close();
				}
				nowStream = hit;
				return true;
			}
			return false;
		}

		public string GetFile()
		{
			return nowFile;
		}

		/// <summary>取走命中流（取走后 nowStream 置空，调用方负责关闭）。</summary>
		public Stream GetStream()
		{
			Stream result = nowStream;
			nowStream = null;
			return result;
		}

		public void Dispose()
		{
			for (int i = 0; i < dataDecoders.Count; i++)
			{
				for (int j = 0; j < dataDecoders[i].Count; j++)
				{
					dataDecoders[i][j].Dispose();
				}
				dataIsPackages[i].Clear();
				dataDecoders[i].Clear();
				dataDecoderWithLanguages[i].Clear();
				dataDirectories[i].Clear();
			}
			directoryNames.Clear();
			dataNames.Clear();
			dataIsPackages.Clear();
			dataDecoders.Clear();
			dataDecoderWithLanguages.Clear();
			dataDirectories.Clear();
			replaceNames.Clear();
			replaceStreams.Clear();
		}
	}
}
