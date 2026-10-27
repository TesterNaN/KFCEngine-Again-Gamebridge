using System;
using System.Xml;
using KidFansChannel.DataCore;

namespace KidFansChannel.GalCore.Commands
{
	public class Movie : ScriptCommand, IInstanceDataUser
	{
		private string _movieStr;

		private int _movieId;

		/// <summary>装载阶段就解析好的影片路径；空串表示这次拿不到数据。</summary>
		private string movieFileName;

		public string MovieStr
		{
			get
			{
				return _movieStr;
			}
			set
			{
				_movieStr = value;
			}
		}

		public int MovieId
		{
			get
			{
				return _movieId;
			}
			set
			{
				_movieId = value;
			}
		}

		public Movie()
		{
			Single = true;
			Lasting = true;
			Command = "movie";
			DataName = "movie";
		}

		public override void ConvertXml(XmlNode node, NodeInfo info)
		{
			base.ConvertXml(node, info);
			_movieStr = ScriptHelper.GetArgumentString(node, info, "file");
			LoadDataInfo();
		}

		protected void LoadDataInfo()
		{
			XmlDocument xmlDocument = new XmlDocument();
			_movieId = -1;
			if (!ScriptCommand.Data.LoadXmlDocument(DataName, _movieStr + ".xml", xmlDocument))
			{
				return;
			}
			XmlNode documentElement = xmlDocument.DocumentElement;
			if (documentElement.Name != "movie")
			{
				throw new ScriptStructureException(documentElement, new NodeInfo(), "Movie's document element should be movie.");
			}
			XmlNode node;
			if ((node = documentElement.SelectSingleNode("movie")) != null)
			{
				_movieId = ScriptHelper.GetArgumentInteger(node, new NodeInfo(), "id");
				if (_movieId < 0)
				{
					throw new ScriptArgumentException(node, new NodeInfo(), "id", ArgumentType.Integer, "Id should not less than 0.");
				}
				ScriptCommand.GameState.UpdateMovie(_movieId);
			}
		}

		public override void InitCommand()
		{
			base.InitCommand();
			if (!string.IsNullOrEmpty(movieFileName))
			{
				if (ScriptCommand.GalCore.GameVideo != null)
				{
					ScriptCommand.GalCore.GameVideo.Play(movieFileName);
					ScriptCommand.GalCore.GameVideo.Stopped += GameVideo_Stopped;
				}
				else
				{
					Lasting = false;
					FinishCommand();
				}
			}
			else
			{
				FinishCommand();
			}
		}

		/// <summary>数据是否已就绪——文件名解析完就算好了。</summary>
		public bool DataReady
		{
			get
			{
				return movieFileName != null;
			}
		}

		/// <summary>装载阶段解析影片路径。放在这里做，避免执行命令时再动 Data 的当前数据源。</summary>
		public void InitInstanceData()
		{
			KidFansChannel.DataCore.DataCore.DataType dataType = ScriptCommand.Data.CheckData(DataName, _movieStr + ".wmv", KidFansChannel.DataCore.DataCore.DataType.File);
			if (dataType == KidFansChannel.DataCore.DataCore.DataType.File)
			{
				movieFileName = ScriptCommand.Data.GetFile();
			}
			else
			{
				movieFileName = string.Empty;
			}
		}

		public void FinalInstanceData()
		{
		}

		private void GameVideo_Stopped(object sender, EventArgs e)
		{
			Lasting = false;
			FinishCommand();
		}
	}
}
