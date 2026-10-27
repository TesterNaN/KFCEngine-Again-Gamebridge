using System.Xml;
using KidFansChannel.DataCore;

namespace KidFansChannel.GalCore.Screens
{
	public abstract class MovieScreen : GameScreen
	{
		protected string[] Files;

		protected bool[] Opens;

		public override void InitData()
		{
			base.InitData();
			XmlDocument xmlDocument = new XmlDocument();
			if (!GameManager.Data.LoadXmlDocument("system", "movie_info.xml", xmlDocument))
			{
				return;
			}
			int argumentInteger = ScriptHelper.GetArgumentInteger(xmlDocument.DocumentElement, new NodeInfo(), "num", 0);
			Files = new string[argumentInteger];
			Opens = new bool[argumentInteger];
			for (int i = 0; i < argumentInteger; i++)
			{
				Opens[i] = GameManager.GameState.CheckMovie(i);
				XmlNode xmlNode = xmlDocument.DocumentElement.SelectSingleNode("item[@id=\"" + i + "\"]");
				if (xmlNode != null)
				{
					Files[i] = ScriptHelper.GetArgumentString(xmlNode, new NodeInfo(), "file");
				}
				if (Files[i] == null)
				{
					Files[i] = string.Empty;
				}
			}
		}

		protected void PlayMovie(string movie)
		{
			KidFansChannel.DataCore.DataCore.DataType dataType = GameManager.Data.CheckData("movie", movie + ".wmv", KidFansChannel.DataCore.DataCore.DataType.File);
			if (dataType == KidFansChannel.DataCore.DataCore.DataType.File && GameManager.GalCore.GameVideo != null)
			{
				GameManager.GalCore.GameVideo.Play(GameManager.Data.GetFile());
			}
		}
	}
}
