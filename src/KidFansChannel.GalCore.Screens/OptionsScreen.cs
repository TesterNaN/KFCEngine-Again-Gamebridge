using System.Collections.Generic;
using System.Xml;

namespace KidFansChannel.GalCore.Screens
{
	public abstract class OptionsScreen : GameScreen
	{
		protected List<List<string>> ItemValues = new List<List<string>>();

		protected List<string> ItemTitles = new List<string>();

		protected List<XmlNodeList> ItemIntros = new List<XmlNodeList>();

		protected List<int> ItemIcon = new List<int>();

		private List<List<string>> OptionsValues = new List<List<string>>();

		private List<string> OptionsTitles = new List<string>();

		private List<string> OptionsIntros = new List<string>();

		protected GameOptions Options
		{
			get
			{
				return GameManager.GalCore.GameOptions;
			}
		}

		public override void InitData()
		{
			base.InitData();
			XmlDocument xmlDocument = new XmlDocument();
			if (!GameManager.Data.LoadXmlDocument("system", "options_info.xml", xmlDocument))
			{
				return;
			}
			XmlNode xmlNode = xmlDocument.DocumentElement.FirstChild;
			while (xmlNode != null)
			{
				if (xmlNode.Name != "item")
				{
					xmlNode = xmlNode.NextSibling;
					continue;
				}
				List<string> values = new List<string>();
				List<string> arguments = new List<string>();
				string argumentString;
				try
				{
					argumentString = ScriptHelper.GetArgumentString(xmlNode, new NodeInfo(), "value");
				}
				catch
				{
					xmlNode = xmlNode.NextSibling;
					continue;
				}
				int argumentInteger;
				try
				{
					argumentInteger = ScriptHelper.GetArgumentInteger(xmlNode, new NodeInfo(), "icon");
				}
				catch
				{
					xmlNode = xmlNode.NextSibling;
					continue;
				}
				XmlNode xmlNode2;
				if ((xmlNode2 = xmlNode.SelectSingleNode("title")) != null && xmlNode2.HasChildNodes && xmlNode2.ChildNodes.Count == 1 && xmlNode2.FirstChild.NodeType == XmlNodeType.Text)
				{
					string value = xmlNode2.FirstChild.Value;
					if ((xmlNode2 = xmlNode.SelectSingleNode("intro")) != null && xmlNode2.HasChildNodes)
					{
						XmlNodeList childNodes = xmlNode2.ChildNodes;
						if ((xmlNode2 = xmlNode.SelectSingleNode("value")) != null && xmlNode2.HasChildNodes)
						{
							xmlNode2 = xmlNode2.FirstChild;
							while (xmlNode2 != null)
							{
								if (xmlNode2.Name != "case")
								{
									xmlNode2 = xmlNode2.NextSibling;
									continue;
								}
								string argumentString2;
								try
								{
									argumentString2 = ScriptHelper.GetArgumentString(xmlNode2, new NodeInfo(), "value");
								}
								catch
								{
									xmlNode2 = xmlNode2.NextSibling;
									continue;
								}
								if (xmlNode2.HasChildNodes && xmlNode2.ChildNodes.Count == 1 && xmlNode2.FirstChild.NodeType == XmlNodeType.Text)
								{
									string value2 = xmlNode2.FirstChild.Value;
									values.Add(value2);
									arguments.Add(argumentString2);
									xmlNode2 = xmlNode2.NextSibling;
								}
								else
								{
									xmlNode2 = xmlNode2.NextSibling;
								}
							}
							OptionsTitles.Add(argumentString);
							ItemTitles.Add(value);
							OptionsValues.Add(arguments);
							ItemIcon.Add(argumentInteger);
							ItemValues.Add(values);
							ItemIntros.Add(childNodes);
							xmlNode = xmlNode.NextSibling;
						}
						else
						{
							xmlNode = xmlNode.NextSibling;
						}
					}
					else
					{
						xmlNode = xmlNode.NextSibling;
					}
				}
				else
				{
					xmlNode = xmlNode.NextSibling;
				}
			}
		}

		protected void SetOptionsValue(int item, int value)
		{
			Options.SetValue(OptionsTitles[item], OptionsValues[item][value]);
		}

		protected int GetOptionsValue(int item)
		{
			string value = Options.GetValue(OptionsTitles[item]);
			return OptionsValues[item].IndexOf(value);
		}
	}
}
