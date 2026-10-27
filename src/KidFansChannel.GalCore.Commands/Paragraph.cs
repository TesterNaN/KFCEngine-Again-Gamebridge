using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml;
using KidFansChannel.DataCore;
using KidFansChannel.GalCore.Records;
using KidFansChannel.GalCore.Screens;

namespace KidFansChannel.GalCore.Commands
{
	public abstract class Paragraph : ScriptCommand, IStaticDataUser, IDrawable, IUpdatable, IResetable, IClickable, IRecordable, ISaveable
	{
		public class LinkClickArgs : EventArgs
		{
			public int Value;

			public LinkClickArgs(int value)
			{
				Value = value;
			}
		}

		public enum ShowMethod
		{
			Print,
			Alpha
		}

		public enum ParagraphState
		{
			Ready,
			Showing,
			Shown,
			Hiding,
			Hiden
		}

		private static String2D _shownString = new String2D();

		private static String2D _currentString = new String2D();

		private static int _stringNumber = 0;

		private string _voiceStr;

		private ContentAlignment _align;

		private ShowMethod _show;

		private List<int> _speakers = new List<int>();

		private int _index;

		private short[] _voiceVol;

		private XmlNodeList _paragraphContent;

		public static ParagraphState State;

		private bool HideAnimation;

		protected bool HasVoice;

		protected bool PreparedVoice;

		protected bool StoppedVoice;

		protected float StringLength;

		protected float StringProgress;

		protected float StringAlpha;

		public static float VoiceHighlightFactor = 0.25f;

		protected float AlphaSpeed = 1f;

		protected float VoiceTime;

		protected float VoiceSpeed;

		protected float VoiceProgress;

		private bool canRecord;

		private int loadNum;

		private int saveStringNumber;

		public static String2D ShownString
		{
			get
			{
				return _shownString;
			}
			protected set
			{
				_shownString = value;
				_shownString.CheckLinkRead += String_CheckLinkRead;
				_shownString.ClickLink += String_ClickLink;
			}
		}

		public static String2D CurrentString
		{
			get
			{
				return _currentString;
			}
			protected set
			{
				_currentString = value;
				_currentString.CheckLinkRead += String_CheckLinkRead;
				_currentString.ClickLink += String_ClickLink;
			}
		}

		public static int StringNumber
		{
			get
			{
				return _stringNumber;
			}
			set
			{
				_stringNumber = value;
			}
		}

		protected string VoiceStr
		{
			get
			{
				return _voiceStr;
			}
			set
			{
				_voiceStr = value;
			}
		}

		public ContentAlignment Align
		{
			get
			{
				return _align;
			}
			protected set
			{
				_align = value;
			}
		}

		public ShowMethod Show
		{
			get
			{
				return _show;
			}
			protected set
			{
				_show = value;
			}
		}

		public List<int> Speakers
		{
			get
			{
				return _speakers;
			}
		}

		public int Index
		{
			get
			{
				return _index;
			}
			protected set
			{
				_index = value;
			}
		}

		public short[] VoiceVol
		{
			get
			{
				return _voiceVol;
			}
			protected set
			{
				_voiceVol = value;
			}
		}

		public XmlNodeList ParagraphContent
		{
			get
			{
				return _paragraphContent;
			}
			protected set
			{
				_paragraphContent = value;
			}
		}

		public int LayerMajor
		{
			get
			{
				return 25;
			}
		}

		public float LayerMinor
		{
			get
			{
				return 0f;
			}
		}

		public bool ToClickState
		{
			get
			{
				return false;
			}
		}

		public static event EventHandler<LinkClickArgs> ClickLink;

		public Paragraph()
		{
			Single = true;
			Lasting = true;
			Command = "paragraph";
			DataName = "voice";
		}

		~Paragraph()
		{
			_speakers.Clear();
		}

		private static void String_CheckLinkRead(object sender, String2D.LinkArgs e)
		{
			ScriptCommand.GameState.UpdateEncyOpen(e.Value);
			e.Read = ScriptCommand.GameState.CheckEncyView(e.Value);
		}

		private static void String_ClickLink(object sender, String2D.LinkArgs e)
		{
			ScriptCommand.GalCore.ShowEncyContentScreen(e.Value);
			if (ClickLink != null)
			{
				ClickLink(null, new LinkClickArgs(e.Value));
			}
		}

		public override void ConvertXml(XmlNode node, NodeInfo info)
		{
			base.ConvertXml(node, info);
			if (!node.HasChildNodes)
			{
				_paragraphContent = null;
			}
			else
			{
				if (node.InnerXml.Contains("\n") || node.InnerXml.Contains("\r"))
				{
					throw new ScriptStructureException(node, info, "Paragraph string should be in one line.");
				}
				_paragraphContent = node.ChildNodes;
			}
			try
			{
				_voiceStr = ScriptHelper.GetArgumentString(node, info, "voice");
			}
			catch (ScriptArgumentException)
			{
			}
			switch (ScriptHelper.GetArgumentString(node, info, "align", "left"))
			{
			case "left":
				_align = ContentAlignment.TopLeft;
				break;
			case "center":
				_align = ContentAlignment.TopCenter;
				break;
			case "right":
				_align = ContentAlignment.TopRight;
				break;
			default:
				throw new ScriptArgumentException(node, info, "align", "Align should be left, center or right.");
			}
			string argumentString = ScriptHelper.GetArgumentString(node, info, "show", "print");
			if (argumentString == "print")
			{
				_show = ShowMethod.Print;
			}
			else
			{
				if (!(argumentString == "alpha"))
				{
					throw new ScriptArgumentException(node, info, "show", "Show should be print or alpha.");
				}
				_show = ShowMethod.Alpha;
			}
			string argumentString2 = ScriptHelper.GetArgumentString(node, info, "speaker", string.Empty);
			if (argumentString2 != string.Empty)
			{
				string[] parts = argumentString2.Split(';');
				string[] partsCopy = parts;
				foreach (string s in partsCopy)
				{
					try
					{
						_speakers.Add(int.Parse(s));
					}
					catch (Exception innerException)
					{
						throw new ScriptArgumentException(node, info, "chara", innerException);
					}
				}
			}
			_index = ScriptHelper.GetArgumentInteger(node, info, "index", 0);
			if (_currentString.Comment != null)
			{
				ScriptCommand.GalCore.HideComment();
			}
			_currentString.Location = new Point(_shownString.Location.X, _shownString.Location.Y + (_shownString.TotalLine - 1) * _shownString.LineSize.Height);
			_currentString.ClearString();
			if (_paragraphContent != null)
			{
				_currentString.AddXmlString(_paragraphContent, info, _align);
			}
			string trimmed = _currentString.String;
			while (trimmed.Length > 0 && trimmed[0] == '\n')
			{
				trimmed = trimmed.Substring(1);
			}
			while (trimmed.Length > 0 && trimmed[trimmed.Length - 1] == '\n')
			{
				trimmed = trimmed.Substring(0, trimmed.Length - 1);
			}
			int length;
			do
			{
				length = trimmed.Length;
				trimmed = trimmed.Replace("\n\n", "\n");
			}
			while (trimmed.Length < length);
			ScriptCommand.GameState.UpdateString(trimmed, _index);
			if (loadNum == 0)
			{
				canRecord = true;
			}
		}

		public override void InitCommand()
		{
			base.InitCommand();
			if (loadNum > 0)
			{
				XmlNode xmlNode = ScriptCommand.GameScript.CommandNode;
				XmlNode[] nodes = new XmlNode[loadNum];
				NodeInfo[] nodeInfos = new NodeInfo[loadNum];
				for (int i = 0; i < loadNum; i++)
				{
					do
					{
						xmlNode = xmlNode.PreviousSibling;
					}
					while (xmlNode != null && xmlNode.Name != Command);
					if (xmlNode == null)
					{
						throw new RuntimeException(ScriptCommand.GameScript.CommandNode, new NodeInfo(), "No enough paragraph to load.");
					}
					nodes[loadNum - i - 1] = xmlNode;
					nodeInfos[loadNum - i - 1] = ScriptCommand.GameState.CurrentGameState.Info;
				}
				for (int j = 0; j < loadNum - 1; j++)
				{
					ConvertXml(nodes[j], nodeInfos[j]);
					if (_paragraphContent != null)
					{
						_shownString.AddXmlString(_paragraphContent, nodeInfos[j], _align);
					}
				}
				ConvertXml(nodes[loadNum - 1], nodeInfos[loadNum - 1]);
				_stringNumber = loadNum;
			}
			else
			{
				_stringNumber++;
			}
			PrepareString();
			State = ParagraphState.Showing;
			foreach (ScriptCommand command in ScriptCommand.CommandList)
			{
				if (command is Paragraph && command != this)
				{
					command.FinishCommand();
				}
			}
			if (_currentString.Comment != null)
			{
				ScriptCommand.GalCore.ShowComment(_currentString.Comment);
			}
		}

		public void FinishString(bool animation)
		{
			State = ParagraphState.Hiding;
			if (HasVoice)
			{
				ScriptCommand.Sound.StopVoice();
				StoppedVoice = true;
				ScriptCommand.Sound.ChangeVolumeTempBgm(1f);
				foreach (ScriptCommand command in ScriptCommand.CommandList)
				{
					if (command is CharaDisplay && _speakers.Contains(((CharaDisplay)command).Id))
					{
						((CharaDisplay)command).MouthStop();
					}
				}
			}
			_stringNumber = 0;
			Lasting = true;
			if (_currentString.Comment != null)
			{
				ScriptCommand.GalCore.HideComment();
			}
			HideAnimation = animation;
		}

		public void Draw()
		{
			switch (State)
			{
			case ParagraphState.Showing:
				DrawShownString();
				DrawCurrentString();
				break;
			case ParagraphState.Shown:
				DrawShownString();
				break;
			case ParagraphState.Hiding:
				DrawShownString();
				break;
			case ParagraphState.Ready:
			case ParagraphState.Hiden:
				break;
			}
		}

		protected virtual void DrawShownString()
		{
			_shownString.Canvas = ScriptCommand.GalCore.CurrentCanvas;
			if (State == ParagraphState.Hiding)
			{
				_shownString.DrawString(StringAlpha);
			}
			else
			{
				_shownString.DrawString();
			}
		}

		protected virtual void DrawCurrentString()
		{
			_currentString.Canvas = ScriptCommand.GalCore.CurrentCanvas;
			switch (_show)
			{
			case ShowMethod.Print:
				if (HasVoice && ScriptCommand.Options.TextSynchro)
				{
					if (StringProgress == 1f)
					{
						_currentString.DrawString();
					}
					else
					{
						_currentString.DrawPartString(StringProgress);
					}
				}
				else if (StringLength == (float)_currentString.TotalWidth)
				{
					_currentString.DrawString();
				}
				else
				{
					_currentString.DrawSubString(StringLength);
				}
				break;
			case ShowMethod.Alpha:
				_currentString.DrawString(StringAlpha);
				break;
			}
		}

		public void Update(double appTime, float elaspedTime)
		{
			switch (State)
			{
			case ParagraphState.Ready:
				PrepareString();
				break;
			case ParagraphState.Showing:
				UpdateString(elaspedTime);
				if (CheckStringShown())
				{
					_shownString.ClearClick();
					if (_paragraphContent != null)
					{
						_shownString.AddXmlString(_paragraphContent, Info, _align);
					}
					State = ParagraphState.Shown;
					Lasting = false;
				}
				break;
			case ParagraphState.Shown:
				UpdateString(elaspedTime);
				break;
			case ParagraphState.Hiding:
				UpdateString(elaspedTime);
				if (CheckStringHiden())
				{
					ClearString();
					State = ParagraphState.Hiden;
					Lasting = false;
				}
				break;
			case ParagraphState.Hiden:
				FinishCommand();
				Lasting = false;
				State = ParagraphState.Ready;
				break;
			}
		}

		protected virtual void PrepareString()
		{
			State = ParagraphState.Showing;
			if (ScriptCommand.PlayModeQuicking && ScriptCommand.PlayMode != PlayMode.QuickOnce)
			{
				switch (_show)
				{
				case ShowMethod.Print:
					StringLength = _currentString.TotalWidth;
					StringProgress = 1f;
					break;
				case ShowMethod.Alpha:
					StringAlpha = 1f;
					StringProgress = 1f;
					break;
				}
				HasVoice = false;
				PreparedVoice = false;
				StoppedVoice = false;
				return;
			}
			switch (_show)
			{
			case ShowMethod.Print:
				StringLength = -String2D.AlphaWidth;
				StringAlpha = 1f;
				break;
			case ShowMethod.Alpha:
				StringAlpha = 0f;
				break;
			}
			if (_voiceStr != null)
			{
				switch (ScriptCommand.Data.CheckData(DataName, _voiceStr + ".ogg", KidFansChannel.DataCore.DataCore.DataType.Package))
				{
				case KidFansChannel.DataCore.DataCore.DataType.Package:
					ScriptCommand.Sound.OpenVoice(ScriptCommand.Data.GetStream());
					HasVoice = true;
					PreparedVoice = false;
					StoppedVoice = false;
					VoiceTime = 0f;
					VoiceProgress = 0f;
					break;
				case KidFansChannel.DataCore.DataCore.DataType.File:
					ScriptCommand.Sound.OpenVoice(ScriptCommand.Data.GetFile());
					HasVoice = true;
					PreparedVoice = false;
					StoppedVoice = false;
					VoiceTime = 0f;
					VoiceProgress = 0f;
					break;
				default:
					HasVoice = false;
					PreparedVoice = false;
					StoppedVoice = false;
					return;
				}
				Stream stream = null;
				switch (ScriptCommand.Data.CheckData("voice", _voiceStr + ".vol"))
				{
				case KidFansChannel.DataCore.DataCore.DataType.Package:
					stream = ScriptCommand.Data.GetStream();
					break;
				case KidFansChannel.DataCore.DataCore.DataType.File:
					stream = new FileStream(ScriptCommand.Data.GetFile(), FileMode.Open, FileAccess.Read);
					break;
				}
				if (stream == null)
				{
					return;
				}
				BinaryReader binaryReader = new BinaryReader(stream);
				_voiceVol = new short[binaryReader.BaseStream.Length >> 1];
				for (int i = 0; i < _voiceVol.Length; i++)
				{
					_voiceVol[i] = binaryReader.ReadInt16();
				}
				binaryReader.Close();
				stream.Close();
				{
					foreach (ScriptCommand currentCommand in ScriptCommand.GalCore.CurrentCommandList)
					{
						if (currentCommand is CharaDisplay && _speakers.Contains(((CharaDisplay)currentCommand).Id))
						{
							((CharaDisplay)currentCommand).MouthPrepare(_voiceVol);
						}
					}
					return;
				}
			}
			HasVoice = false;
			PreparedVoice = false;
			StoppedVoice = false;
		}

		protected virtual void UpdateString(float elaspedTime)
		{
			if (HasVoice && !StoppedVoice && PreparedVoice && VoiceTime * VoiceSpeed >= 1f)
			{
				StoppedVoice = true;
				ScriptCommand.Sound.ChangeVolumeTempBgm(1f);
			}
			switch (State)
			{
			case ParagraphState.Showing:
				if (HasVoice)
				{
					if (!PreparedVoice)
					{
						if (!ScriptCommand.Sound.PreparedVoice)
						{
							break;
						}
						ScriptCommand.Sound.PlayVoice();
						PreparedVoice = true;
						VoiceSpeed = 1f / ScriptCommand.Sound.LengthVoice;
						if (ScriptCommand.Options.VoiceHighlight)
						{
							ScriptCommand.Sound.ChangeVolumeTempBgm(VoiceHighlightFactor);
						}
						foreach (ScriptCommand currentCommand in ScriptCommand.GalCore.CurrentCommandList)
						{
							if (currentCommand is CharaDisplay && _speakers.Contains(((CharaDisplay)currentCommand).Id))
							{
								((CharaDisplay)currentCommand).MouthPlay();
							}
						}
					}
					else
					{
						VoiceTime += elaspedTime;
						VoiceProgress = VoiceTime * VoiceSpeed;
						if (VoiceProgress > 1f)
						{
							VoiceProgress = 1f;
							VoiceTime = 1f / VoiceSpeed;
						}
					}
				}
				switch (_show)
				{
				case ShowMethod.Print:
					if (HasVoice && ScriptCommand.Options.TextSynchro)
					{
						if (ScriptCommand.PlayModeQuicking)
						{
							StringProgress = 1f;
							if (ScriptCommand.PlayMode != PlayMode.QuickOnce)
							{
								ScriptCommand.Sound.StopVoice();
							}
						}
						else
						{
							StringProgress = VoiceProgress;
						}
					}
					else if (ScriptCommand.PlayModeQuicking)
					{
						StringLength = _currentString.TotalWidth;
					}
					else
					{
						StringLength += elaspedTime * GetSpeed();
						if (StringLength >= (float)_currentString.TotalWidth)
						{
							StringLength = _currentString.TotalWidth;
						}
					}
					break;
				case ShowMethod.Alpha:
					if (ScriptCommand.PlayModeQuicking)
					{
						StringAlpha = 1f;
						StringProgress = 1f;
						if (ScriptCommand.PlayMode != PlayMode.QuickOnce)
						{
							ScriptCommand.Sound.StopVoice();
						}
					}
					else
					{
						StringAlpha += elaspedTime * AlphaSpeed;
						StringProgress = VoiceProgress;
						if (StringAlpha > 1f)
						{
							StringAlpha = 1f;
						}
					}
					break;
				}
				break;
			case ParagraphState.Shown:
				if (HasVoice && PreparedVoice)
				{
					VoiceTime += elaspedTime;
				}
				break;
			case ParagraphState.Hiding:
				if (HideAnimation)
				{
					if (ScriptCommand.PlayModeQuicking)
					{
						StringAlpha = 0f;
						break;
					}
					StringAlpha -= elaspedTime * AlphaSpeed;
					if (StringAlpha < 0f)
					{
						StringAlpha = 0f;
					}
				}
				else
				{
					StringAlpha = 0f;
				}
				break;
			}
		}

		private void ClearString()
		{
			_shownString.ClearString();
			HasVoice = false;
			PreparedVoice = false;
			StoppedVoice = false;
			StringLength = -String2D.AlphaWidth;
			StringProgress = 0f;
			StringAlpha = 0f;
		}

		protected virtual bool CheckStringShown()
		{
			switch (_show)
			{
			case ShowMethod.Print:
				if (HasVoice && ScriptCommand.Options.TextSynchro)
				{
					return StringProgress == 1f;
				}
				return StringLength == (float)_currentString.TotalWidth;
			case ShowMethod.Alpha:
				if (StringAlpha == 1f)
				{
					return StringProgress == 1f;
				}
				return false;
			default:
				return true;
			}
		}

		protected virtual bool CheckStringHiden()
		{
			return StringAlpha == 0f;
		}

		protected abstract float GetSpeed();

		public abstract void InitStaticData();

		public abstract void FinalStaticData();

		public void Reset()
		{
			State = ParagraphState.Ready;
			_stringNumber = 0;
			_shownString.ClearString();
			_currentString.ClearString();
			ScriptCommand.Sound.StopVoice();
		}

		public bool OnItemClick()
		{
			return _shownString.OnItemClick();
		}

		public void OnItemMove(Point location)
		{
			_shownString.OnItemMove(location);
		}

		public Record[] GetRecord()
		{
			if (canRecord)
			{
				if (_currentString.String == string.Empty)
				{
					return null;
				}
				canRecord = false;
				TextRecord textRecord = new TextRecord();
				textRecord.Text = _currentString.String;
				while (textRecord.Text.Length > 0 && textRecord.Text[0] == '\n')
				{
					textRecord.Text = textRecord.Text.Substring(1);
				}
				while (textRecord.Text.Length > 0 && textRecord.Text[textRecord.Text.Length - 1] == '\n')
				{
					textRecord.Text = textRecord.Text.Substring(0, textRecord.Text.Length - 1);
				}
				int length;
				do
				{
					length = textRecord.Text.Length;
					textRecord.Text = textRecord.Text.Replace("\n\n", "\n");
				}
				while (textRecord.Text.Length < length);
				textRecord.Line = String2D.CalLine(textRecord.Text, _currentString.Font, MessageScreen.TextWithoutSpeakerWidth);
				textRecord.Speakers.AddRange(_speakers);
				if (_voiceStr != null)
				{
					textRecord.Voice = _voiceStr;
				}
				return new Record[1] { textRecord };
			}
			return null;
		}

		public void Save(BinaryWriter bw)
		{
			bw.Write(Command);
			bw.Write(saveStringNumber);
		}

		public void Load(BinaryReader br)
		{
			loadNum = br.ReadInt32();
		}

		public void ConfirmUpdate()
		{
			saveStringNumber = _stringNumber;
		}
	}
}
