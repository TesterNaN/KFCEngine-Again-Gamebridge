using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using KidFansChannel.GalCore.Commands;
using KidFansChannel.GalCore.Defines;

namespace KidFansChannel.GalCore
{
	public class GameInstanceScript
	{
		private GameScript parent;

		private string _storyName;

		private XmlDocument currentXmlDocument;

		private XmlNode currentChapter;

		private ChapterDefine currentChapterDefine;

		private XmlNode currentScene;

		private XmlNode nextCommand;

		private GameInstanceState state;

		private bool _newChapter;

		private List<ScriptCommand> preCommandList = new List<ScriptCommand>();

		private bool justLoad;

		private bool _stringRead;

		protected XmlNode NextStringNode;

		private bool nextProcessStringRead;

		public string StoryName
		{
			get
			{
				return _storyName;
			}
			protected set
			{
				_storyName = value;
			}
		}

		public XmlNode CommandNode
		{
			get
			{
				return nextCommand;
			}
		}

		public bool NewChapter
		{
			get
			{
				return _newChapter;
			}
			set
			{
				_newChapter = value;
			}
		}

		public bool StringRead
		{
			get
			{
				return _stringRead;
			}
		}

		internal GameInstanceScript(string storyName, bool jump, GameScript gameScript)
		{
			parent = gameScript;
			_storyName = storyName;
			parent.GalCore.GameState.InitNewState(_storyName);
			state = parent.GalCore.GameState.CurrentGameState;
			string storyPath = ProcessIndex();
			if (jump)
			{
				if (!parent.GalCore.GameData.LoadXmlDocument("script", _storyName + "\\" + storyPath + ".xml", currentXmlDocument))
				{
					throw new FileNotFoundException(null, Path.Combine("script", _storyName + "\\" + storyPath + ".xml"));
				}
				state.File = storyPath;
				state.Chapter = "";
				state.Scene = 0;
				state.Click = 0;
				XmlNode documentElement = currentXmlDocument.DocumentElement;
				if (documentElement.Name != "script")
				{
					throw new ScriptStructureException(documentElement, state.Info, "Script's document element must be script.");
				}
				XmlNode xmlNode = documentElement.FirstChild;
				while (xmlNode != null && xmlNode.NodeType == XmlNodeType.Comment)
				{
					xmlNode = xmlNode.NextSibling;
				}
				if (xmlNode == null || xmlNode.Name != "chapter")
				{
					throw new ScriptStructureException(xmlNode, state.Info, "Script should have chapter child node only.");
				}
				string argumentString = ScriptHelper.GetArgumentString(xmlNode, state.Info, "title");
				JumpScene(state.File, argumentString, 0);
			}
		}

		protected string ProcessIndex()
		{
			currentXmlDocument = new XmlDocument();
			if (!parent.GalCore.GameData.LoadXmlDocument("script", _storyName + "\\index.xml", currentXmlDocument))
			{
				throw new FileNotFoundException(null, Path.Combine("script", _storyName + "\\index.xml"));
			}
			state.File = "index";
			state.Chapter = "";
			state.Scene = 0;
			state.Click = 0;
			XmlNode documentElement = currentXmlDocument.DocumentElement;
			if (documentElement.Name != "info")
			{
				throw new ScriptStructureException(documentElement, state.Info, "Index's document element must be info.");
			}
			string argumentString = ScriptHelper.GetArgumentString(documentElement, state.Info, "script");
			foreach (XmlNode childNode in documentElement.ChildNodes)
			{
				if (childNode.Name == "definechapter")
				{
					ChapterDefine chapterDefine = new ChapterDefine();
					chapterDefine.Title = ScriptHelper.GetArgumentString(childNode, state.Info, "title");
					chapterDefine.StringNum = ScriptHelper.GetArgumentInteger(childNode, state.Info, "stringnum", 0);
					chapterDefine.File = ScriptHelper.GetArgumentString(childNode, state.Info, "script").Split(';');
					state.ChaptereDefines.Add(chapterDefine.Title, chapterDefine);
				}
				else if (childNode.Name == "definevar")
				{
					VarDefine varDefine = new VarDefine();
					varDefine.Title = ScriptHelper.GetArgumentString(childNode, state.Info, "title");
					varDefine.Value = ScriptHelper.GetArgumentInteger(childNode, state.Info, "value", 0);
					state.VarDefines.Add(varDefine.Title, varDefine);
				}
				else if (childNode.Name == "defineselect")
				{
					SelectDefine selectDefine = new SelectDefine();
					selectDefine.Title = ScriptHelper.GetArgumentString(childNode, state.Info, "title");
					selectDefine.Case.AddRange(ScriptHelper.GetArgumentString(childNode, state.Info, "case").Split(';'));
					string argumentString2 = ScriptHelper.GetArgumentString(childNode, state.Info, "value", string.Empty);
					if (argumentString2 != string.Empty)
					{
						selectDefine.Value = selectDefine.Case.IndexOf(argumentString2);
						if (selectDefine.Value == -1)
						{
							throw new ScriptArgumentException(childNode, state.Info, "value", "Value is not exist in cases.");
						}
					}
					else
					{
						selectDefine.Value = -1;
					}
					state.SelectDefines.Add(selectDefine.Title, selectDefine);
				}
				else if (childNode.NodeType != XmlNodeType.Comment)
				{
					throw new ScriptStructureException(childNode, state.Info, "Info's child must be defines.");
				}
			}
			return argumentString;
		}

		public void Jump(string script, string chapter, int scene, int click)
		{
			JumpScene(script, chapter, scene, false, false);
			if (click != state.Click)
			{
				if (click > 0)
				{
					nextCommand = currentScene.SelectSingleNode("click[" + click + "]").NextSibling;
				}
				else
				{
					nextCommand = currentScene.FirstChild;
				}
				while (nextCommand != null && nextCommand.NodeType == XmlNodeType.Comment)
				{
					nextCommand = nextCommand.NextSibling;
				}
			}
			state.Click = click;
		}

		public void JumpScene(string chapter, int scene)
		{
			JumpScene(state.File, chapter, scene);
		}

		public void JumpScene(string script, string chapter, int scene)
		{
			JumpScene(script, chapter, scene, true, false);
		}

		public void JumpScene(string script, string chapter, int scene, bool only, bool load)
		{
			bool changed = load;
			if (changed || script != state.File)
			{
				if (!parent.GalCore.GameData.LoadXmlDocument("script", _storyName + "\\" + script + ".xml", currentXmlDocument))
				{
					throw new FileNotFoundException(null, Path.Combine("script", _storyName + "\\" + script + ".xml"));
				}
				changed = true;
			}
			if (changed || chapter != state.Chapter)
			{
				currentChapter = currentXmlDocument.DocumentElement.SelectSingleNode("chapter[@title=\"" + chapter + "\"]");
				InitChapter();
				if (!load)
				{
					_newChapter = true;
				}
				changed = true;
			}
			if (changed || scene != state.Scene)
			{
				currentScene = currentChapter.SelectSingleNode("scene[" + (scene + 1) + "]");
				changed = true;
			}
			state.File = script;
			state.Chapter = chapter;
			state.Scene = scene;
			if (only)
			{
				Type type = parent.Types["scene"];
				ScriptCommand scriptCommand = (ScriptCommand)type.Assembly.CreateInstance(type.FullName);
				if (load)
				{
					((Scene)scriptCommand).SetLoad();
				}
				scriptCommand.ConvertXml(currentScene, state.Info);
				bool ready = false;
				foreach (ScriptCommand preCommand in preCommandList)
				{
					if (preCommand is Scene)
					{
						ready = true;
						break;
					}
				}
				if (!ready)
				{
					preCommandList.Add(scriptCommand);
				}
				nextCommand = currentScene.FirstChild;
				while (nextCommand != null && nextCommand.NodeType == XmlNodeType.Comment)
				{
					nextCommand = nextCommand.NextSibling;
				}
				state.Click = 0;
			}
			nextProcessStringRead = true;
		}

		public void Jump(int click, bool next)
		{
			if (state.Click == click && next)
			{
				return;
			}
			if (click > 0)
			{
				nextCommand = currentScene.SelectSingleNode("click[" + click + "]");
				if (next)
				{
					nextCommand = nextCommand.NextSibling;
					while (nextCommand != null && nextCommand.NodeType == XmlNodeType.Comment)
					{
						nextCommand = nextCommand.NextSibling;
					}
					state.Click = click;
				}
				else
				{
					state.Click = click - 1;
				}
				nextProcessStringRead = true;
				return;
			}
			throw new InvalidOperationException("Click should be greater than 0.");
		}

		public void Jump(int click)
		{
			Jump(click, true);
		}

		public void Jump(string label)
		{
			if (state.Click != 0 && !(label != ScriptHelper.GetArgumentString(currentScene.SelectSingleNode("click[" + state.Click + "]"), state.Info, "label", "")))
			{
				return;
			}
			XmlNode xmlNode = currentScene.SelectSingleNode("click[@label=\"" + label + "\"]");
			if (xmlNode == null)
			{
				throw new RuntimeException(null, state.Info, "Click label not found");
			}
			XmlNodeList xmlNodeList = currentScene.SelectNodes("click");
			for (int i = 0; i < xmlNodeList.Count; i++)
			{
				if (xmlNode == xmlNodeList[i])
				{
					state.Click = i + 1;
					break;
				}
			}
			nextCommand = xmlNode.NextSibling;
			while (nextCommand != null && nextCommand.NodeType == XmlNodeType.Comment)
			{
				nextCommand = nextCommand.NextSibling;
			}
			nextProcessStringRead = true;
		}

		public void JumpFake()
		{
			nextProcessStringRead = true;
		}

		private void InitChapter()
		{
			try
			{
				currentChapterDefine = state.ChaptereDefines[currentChapter.Attributes["title"].Value];
			}
			catch (KeyNotFoundException innerException)
			{
				throw new RuntimeException(currentChapter, state.Info, "Chapter's title cannot be found in defines.", innerException);
			}
			GC.Collect();
		}

		private void GotoNextCommand()
		{
			if (nextCommand == currentScene)
			{
				nextCommand = currentScene.FirstChild;
				while (nextCommand != null && nextCommand.NodeType == XmlNodeType.Comment)
				{
					nextCommand = nextCommand.NextSibling;
				}
			}
			else
			{
				do
				{
					nextCommand = nextCommand.NextSibling;
				}
				while (nextCommand != null && nextCommand.NodeType == XmlNodeType.Comment);
			}
			if (nextCommand != null)
			{
				return;
			}
			while (true)
			{
				currentScene = currentScene.NextSibling;
				if (currentScene != null && currentScene.NodeType == XmlNodeType.Comment)
				{
					continue;
				}
				if (currentScene != null)
				{
					if (currentScene.Name != "scene")
					{
						throw new ScriptStructureException(currentScene, state.Info, "Chapter should have scene child node only.");
					}
					state.Scene++;
					state.Click = 0;
					nextCommand = currentScene;
					return;
				}
				do
				{
					currentChapter = currentChapter.NextSibling;
				}
				while (currentChapter != null && currentChapter.NodeType == XmlNodeType.Comment);
				if (currentChapter == null)
				{
					break;
				}
				state.Chapter = ScriptHelper.GetArgumentString(currentChapter, state.Info, "title");
				state.Scene = 0;
				state.Click = 0;
				InitChapter();
				_newChapter = true;
			}
			currentScene = null;
			nextCommand = null;
			state.Chapter = "";
			state.Scene = 0;
			state.Click = 0;
		}

		public List<ScriptCommand> ProcessNextCommands()
		{
			List<ScriptCommand> commands = new List<ScriptCommand>();
			commands.AddRange(preCommandList);
			preCommandList.Clear();
			if (justLoad)
			{
				FindNextStringNode();
				nextProcessStringRead = nextCommand == NextStringNode;
				justLoad = false;
				return commands;
			}
			if (nextCommand == null)
			{
				return null;
			}
			bool changed = false;
			int parValue = 0;
			do
			{
				if (nextProcessStringRead)
				{
					FindNextStringNode();
				}
				nextProcessStringRead = nextCommand == NextStringNode;
				ScriptCommand scriptCommand = ProcessCommand(nextCommand, state.Info);
				if (scriptCommand is Click)
				{
					state.Click++;
				}
				else if (scriptCommand is Par)
				{
					parValue = ((Par)scriptCommand).Value;
					GotoNextCommand();
					break;
				}
				if (scriptCommand != null)
				{
					commands.Add(scriptCommand);
					if (scriptCommand.Single)
					{
						changed = true;
					}
				}
				GotoNextCommand();
				if (nextCommand == null)
				{
					changed = true;
				}
			}
			while (!changed);
			commands.AddRange(ProcessPar(parValue));
			return commands;
		}

		protected List<ScriptCommand> ProcessPar(int parValue)
		{
			List<ScriptCommand> commands = new List<ScriptCommand>();
			while (parValue > 0)
			{
				bool changed = false;
				if (nextProcessStringRead)
				{
					FindNextStringNode();
				}
				nextProcessStringRead = nextCommand == NextStringNode;
				ScriptCommand scriptCommand = ProcessCommand(nextCommand, state.Info);
				parValue--;
				if (scriptCommand is Click)
				{
					throw new ScriptCommandException(nextCommand, state.Info, "Click should not be within par command.");
				}
				if (scriptCommand is Wait)
				{
					if (parValue == 0)
					{
						for (int i = 0; i < commands.Count; i++)
						{
							if (!(commands[i] is Wait))
							{
								commands[i].Lasting = false;
							}
						}
					}
					else
					{
						Wait wait = (Wait)scriptCommand;
						commands.Add(scriptCommand);
						GotoNextCommand();
						if (nextProcessStringRead)
						{
							FindNextStringNode();
						}
						nextProcessStringRead = nextCommand == NextStringNode;
						scriptCommand = ProcessCommand(nextCommand, state.Info);
						parValue--;
						if (!(scriptCommand is Par))
						{
							throw new ScriptCommandException(nextCommand, state.Info, "Wait should be followed with par or at the end of command series within par command.");
						}
						if (((Par)scriptCommand).Value > parValue)
						{
							throw new ScriptCommandException(nextCommand, state.Info, "Child par value plus number of earlier commands should not be greater than parent par value.");
						}
						GotoNextCommand();
						wait.ParList = ProcessPar(((Par)scriptCommand).Value);
						parValue -= ((Par)scriptCommand).Value;
						changed = true;
					}
				}
				else if (scriptCommand is Par)
				{
					throw new ScriptCommandException(nextCommand, state.Info, "Par should follow wait within par command.");
				}
				if (scriptCommand != null && !(scriptCommand is Par))
				{
					commands.Add(scriptCommand);
				}
				if (!changed)
				{
					GotoNextCommand();
				}
				if (nextCommand == null && parValue > 0)
				{
					throw new ScriptCommandException(currentXmlDocument.DocumentElement, state.Info, "No enough commands in par series.");
				}
			}
			return commands;
		}

		protected virtual ScriptCommand ProcessCommand(XmlNode nextCommand, NodeInfo info)
		{
			if (parent.Types.ContainsKey(nextCommand.Name))
			{
				Type type = parent.Types[nextCommand.Name];
				ScriptCommand scriptCommand = (ScriptCommand)type.Assembly.CreateInstance(type.FullName);
				scriptCommand.ConvertXml(nextCommand, info);
				return scriptCommand;
			}
			return null;
		}

		public string GetChapterName(string chapter)
		{
			if (!state.ChaptereDefines.ContainsKey(chapter))
			{
				return null;
			}
			string storyPath = state.ChaptereDefines[chapter].File[0];
			XmlDocument xmlDocument = new XmlDocument();
			if (!parent.GalCore.GameData.LoadXmlDocument("script", _storyName + "\\" + storyPath + ".xml", xmlDocument))
			{
				throw new FileNotFoundException(null, Path.Combine("script", _storyName + "\\" + storyPath + ".xml"));
			}
			XmlNode xmlNode = xmlDocument.SelectSingleNode("script/chapter[@title='" + chapter + "']/scene/showchapter");
			if (xmlNode != null && xmlNode.HasChildNodes && xmlNode.ChildNodes.Count == 1 && xmlNode.ChildNodes[0].NodeType == XmlNodeType.Text)
			{
				return xmlNode.ChildNodes[0].Value;
			}
			return null;
		}

		public void LoadStory(int index)
		{
			state.LoadState(index);
			LoadStory();
		}

		public void LoadStory(string fileName)
		{
			state.LoadState(fileName);
			LoadStory();
		}

		private void LoadStory()
		{
			if (state.StoryName != _storyName)
			{
				_storyName = state.StoryName;
				parent.GalCore.GameState.InitNewState(_storyName);
				state = parent.GalCore.GameState.CurrentGameState;
				ProcessIndex();
			}
			NodeInfo nodeInfo = new NodeInfo();
			nodeInfo.File = state.Info.File;
			nodeInfo.Chapter = state.Info.Chapter;
			nodeInfo.Scene = state.Info.Scene;
			nodeInfo.Click = state.Info.Click;
			JumpScene(nodeInfo.File, nodeInfo.Chapter, nodeInfo.Scene, true, true);
			Jump(nodeInfo.Click, false);
			foreach (ISaveable currentStateCommand in state.CurrentStateCommands)
			{
				preCommandList.Add((ScriptCommand)currentStateCommand);
			}
			if (parent.Types.ContainsKey("showchapter"))
			{
				Type type = parent.Types["showchapter"];
				ScriptCommand scriptCommand = (ScriptCommand)type.Assembly.CreateInstance(type.FullName);
				scriptCommand.ConvertXml(currentChapter.SelectSingleNode("scene/showchapter"), new NodeInfo());
				((ShowChapter)scriptCommand).CanRecord = false;
				preCommandList.Add(scriptCommand);
			}
			justLoad = true;
			foreach (ScriptCommand currentCommand in parent.GalCore.CurrentCommandList)
			{
				currentCommand.FinishCommand();
				if (currentCommand is IResetable)
				{
					((IResetable)currentCommand).Reset();
				}
			}
			parent.GalCore.CurrentPlayState = PlayState.Running;
		}

		public void SaveStory(int index, string comment, int icon)
		{
			state.SaveState(index, comment, icon);
		}

		protected void FindNextStringNode()
		{
			XmlNode xmlNode = NextStringNode;
			if (xmlNode == null)
			{
				xmlNode = nextCommand;
				if (xmlNode.Name == "paragraph" || xmlNode.Name == "dialogue")
				{
					NextStringNode = xmlNode;
					_stringRead = parent.GalCore.GameState.CheckString(ScriptHelper.GetArgumentInteger(NextStringNode, new NodeInfo(), "index"));
					return;
				}
				if (xmlNode.Name == "jump")
				{
					NextStringNode = null;
					return;
				}
			}
			XmlNode xmlNode2 = ((xmlNode.Name == "scene") ? xmlNode : xmlNode.ParentNode);
			while (true)
			{
				if (xmlNode == xmlNode2)
				{
					xmlNode = xmlNode2.FirstChild;
					while (xmlNode != null && xmlNode.NodeType == XmlNodeType.Comment)
					{
						xmlNode = xmlNode.NextSibling;
					}
				}
				else
				{
					do
					{
						xmlNode = xmlNode.NextSibling;
					}
					while (xmlNode != null && xmlNode.NodeType == XmlNodeType.Comment);
				}
				if (xmlNode == null)
				{
					do
					{
						xmlNode2 = xmlNode2.NextSibling;
					}
					while (xmlNode2 != null && xmlNode2.NodeType == XmlNodeType.Comment);
					if (xmlNode2 == null)
					{
						NextStringNode = null;
						return;
					}
					if (xmlNode2.Name != "scene")
					{
						throw new ScriptStructureException(xmlNode2, new NodeInfo(), "Chapter should have scene child node only.");
					}
					xmlNode = xmlNode2;
				}
				if (xmlNode != xmlNode2)
				{
					if (xmlNode.Name == "paragraph" || xmlNode.Name == "dialogue")
					{
						NextStringNode = xmlNode;
						_stringRead = parent.GalCore.GameState.CheckString(ScriptHelper.GetArgumentInteger(NextStringNode, new NodeInfo(), "index"));
						return;
					}
					if (xmlNode.Name == "jump")
					{
						break;
					}
				}
			}
			NextStringNode = null;
			_stringRead = true;
		}
	}
}
