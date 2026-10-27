using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;

namespace KidFansChannel.GalCore
{
	public class GameOptions
	{
		public enum Level
		{
			High,
			Mid,
			Low,
			None
		}

		public enum ShockMethod
		{
			None,
			Gamepad,
			Window,
			GamepadAndWindow
		}

		private int _graphicsDevice;

		private int _soundDevice;

		private int _inputDevice;

		private Resolution _resolution = new Resolution(new Size(800, 600), new Size(4, 3), new Rectangle(0, 0, 800, 600), 1f, Rectangle.Empty, new Rectangle(0, 0, 800, 40));

		private bool _fullScreen = true;

		private Level _graphics;

		private string _dialogueColor = "Blue";

		private float _dialogueAlpha = 0.75f;

		private float _systemVolume = 0.5f;

		private float _bgmVolume = 0.5f;

		private float _seVolume = 0.5f;

		private float _voiceVolume = 0.75f;

		private bool _voiceHighlight = true;

		private bool _textSynchro = true;

		private Level _textSpeed = Level.Mid;

		private Level _autoSpeed = Level.Mid;

		private bool _quickReadOnly = true;

		private bool _selectedShow = true;

		private bool _titleShow = true;

		private bool _quickSave = true;

		private ShockMethod _shock = ShockMethod.GamepadAndWindow;

		private string _language = "en-US";

		private Dictionary<string, string> _font = new Dictionary<string, string>();

		private string _settingFolder;

		public int GraphicsDevice
		{
			get
			{
				return _graphicsDevice;
			}
			set
			{
				_graphicsDevice = value;
			}
		}

		public int SoundDevice
		{
			get
			{
				return _soundDevice;
			}
			set
			{
				_soundDevice = value;
			}
		}

		public int InputDevice
		{
			get
			{
				return _inputDevice;
			}
			set
			{
				_inputDevice = value;
			}
		}

		public Resolution Resolution
		{
			get
			{
				return _resolution;
			}
			set
			{
				_resolution = value;
			}
		}

		public bool FullScreen
		{
			get
			{
				return _fullScreen;
			}
			set
			{
				_fullScreen = value;
			}
		}

		public Level Graphics
		{
			get
			{
				return _graphics;
			}
			set
			{
				if (value == Level.None)
				{
					throw new ArgumentOutOfRangeException();
				}
				_graphics = value;
			}
		}

		public string DialogueColor
		{
			get
			{
				return _dialogueColor;
			}
			set
			{
				_dialogueColor = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public float DialogueAlpha
		{
			get
			{
				return _dialogueAlpha;
			}
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				_dialogueAlpha = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public float SystemVolume
		{
			get
			{
				return _systemVolume;
			}
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				_systemVolume = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public float BgmVolume
		{
			get
			{
				return _bgmVolume;
			}
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				_bgmVolume = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public float SeVolume
		{
			get
			{
				return _seVolume;
			}
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				_seVolume = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public float VoiceVolume
		{
			get
			{
				return _voiceVolume;
			}
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				_voiceVolume = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public bool VoiceHighlight
		{
			get
			{
				return _voiceHighlight;
			}
			set
			{
				_voiceHighlight = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public bool TextSynchro
		{
			get
			{
				return _textSynchro;
			}
			set
			{
				_textSynchro = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public Level TextSpeed
		{
			get
			{
				return _textSpeed;
			}
			set
			{
				_textSpeed = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public Level AutoSpeed
		{
			get
			{
				return _autoSpeed;
			}
			set
			{
				if (value == Level.None)
				{
					throw new ArgumentOutOfRangeException();
				}
				_autoSpeed = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public bool QuickReadOnly
		{
			get
			{
				return _quickReadOnly;
			}
			set
			{
				_quickReadOnly = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public bool SelectedShow
		{
			get
			{
				return _selectedShow;
			}
			set
			{
				_selectedShow = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public bool TitleShow
		{
			get
			{
				return _titleShow;
			}
			set
			{
				_titleShow = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public bool QuickSave
		{
			get
			{
				return _quickSave;
			}
			set
			{
				_quickSave = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public ShockMethod Shock
		{
			get
			{
				return _shock;
			}
			set
			{
				_shock = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public string Language
		{
			get
			{
				return _language;
			}
			set
			{
				_language = value;
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public string Font
		{
			get
			{
				if (_font.ContainsKey(_language))
				{
					return _font[_language];
				}
				_font.Add(_language, "@default");
				return "@default";
			}
			set
			{
				if (_font.ContainsKey(_language))
				{
					_font[_language] = value;
				}
				else
				{
					_font.Add(_language, value);
				}
				if (OptionsUpdate != null)
				{
					OptionsUpdate(this, EventArgs.Empty);
				}
			}
		}

		public string SettingFolder
		{
			get
			{
				return _settingFolder;
			}
			set
			{
				_settingFolder = value;
			}
		}

		public event EventHandler OptionsUpdate;

		public string GetOptionsFile()
		{
			if (!Directory.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _settingFolder)))
			{
				try
				{
					Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _settingFolder));
				}
				catch (Exception ex)
				{
					MessageBox.Show("An Error occured while creating options directory.\n" + ex.Message);
				}
			}
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), _settingFolder + "\\Options.xml");
		}

		protected void SaveSetting(XmlDocument doc, string name, object value)
		{
			if (doc == null)
			{
				throw new ArgumentNullException("doc");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			if (value == null)
			{
				throw new ArgumentNullException("value");
			}
			if (doc.DocumentElement.Name != "Options")
			{
				throw new InvalidDataException();
			}
			string[] array = name.Split('/');
			XmlNode xmlNode = doc.DocumentElement;
			for (int i = 0; i < array.Length; i++)
			{
				XmlNode xmlNode2 = xmlNode.SelectSingleNode(array[i]);
				xmlNode = ((xmlNode2 != null) ? xmlNode2 : xmlNode.AppendChild(doc.CreateElement(array[i])));
			}
			xmlNode.InnerText = value.ToString();
		}

		public virtual void Save()
		{
			try
			{
				XmlDocument xmlDocument = new XmlDocument();
				xmlDocument.AppendChild(xmlDocument.CreateElement("Options"));
				SaveSetting(xmlDocument, "GraphicsDevice", GraphicsDevice);
				SaveSetting(xmlDocument, "SoundDevice", SoundDevice);
				SaveSetting(xmlDocument, "InputDevice", InputDevice);
				SaveSetting(xmlDocument, "Resolution/Size/Width", Resolution.Size.Width);
				SaveSetting(xmlDocument, "Resolution/Size/Height", Resolution.Size.Height);
				SaveSetting(xmlDocument, "Resolution/Proportion/Width", Resolution.Proportion.Width);
				SaveSetting(xmlDocument, "Resolution/Proportion/Height", Resolution.Proportion.Height);
				SaveSetting(xmlDocument, "Resolution/Bounds/X", Resolution.Bounds.X);
				SaveSetting(xmlDocument, "Resolution/Bounds/Y", Resolution.Bounds.Y);
				SaveSetting(xmlDocument, "Resolution/Bounds/Width", Resolution.Bounds.Width);
				SaveSetting(xmlDocument, "Resolution/Bounds/Height", Resolution.Bounds.Height);
				SaveSetting(xmlDocument, "Resolution/FontScale", Resolution.FontScale);
				SaveSetting(xmlDocument, "Resolution/StateArea/X", Resolution.StateArea.X);
				SaveSetting(xmlDocument, "Resolution/StateArea/Y", Resolution.StateArea.Y);
				SaveSetting(xmlDocument, "Resolution/StateArea/Width", Resolution.StateArea.Width);
				SaveSetting(xmlDocument, "Resolution/StateArea/Height", Resolution.StateArea.Height);
				SaveSetting(xmlDocument, "Resolution/CommentArea/X", Resolution.CommentArea.X);
				SaveSetting(xmlDocument, "Resolution/CommentArea/Y", Resolution.CommentArea.Y);
				SaveSetting(xmlDocument, "Resolution/CommentArea/Width", Resolution.CommentArea.Width);
				SaveSetting(xmlDocument, "Resolution/CommentArea/Height", Resolution.CommentArea.Height);
				SaveSetting(xmlDocument, "FullScreen", FullScreen);
				SaveSetting(xmlDocument, "Graphics", Graphics);
				SaveSetting(xmlDocument, "DialogueColor", DialogueColor);
				SaveSetting(xmlDocument, "DialogueAlpha", DialogueAlpha);
				SaveSetting(xmlDocument, "SystemVolume", SystemVolume);
				SaveSetting(xmlDocument, "BgmVolume", BgmVolume);
				SaveSetting(xmlDocument, "SeVolume", SeVolume);
				SaveSetting(xmlDocument, "VoiceVolume", VoiceVolume);
				SaveSetting(xmlDocument, "VoiceHighlight", VoiceHighlight);
				SaveSetting(xmlDocument, "TextSynchro", TextSynchro);
				SaveSetting(xmlDocument, "TextSpeed", TextSpeed);
				SaveSetting(xmlDocument, "AutoSpeed", AutoSpeed);
				SaveSetting(xmlDocument, "QuickReadOnly", QuickReadOnly);
				SaveSetting(xmlDocument, "SelectedShow", SelectedShow);
				SaveSetting(xmlDocument, "TitleShow", TitleShow);
				SaveSetting(xmlDocument, "QuickSave", QuickSave);
				SaveSetting(xmlDocument, "Shock", Shock);
				SaveSetting(xmlDocument, "Language", Language);
				if (!_font.ContainsKey(Language))
				{
					_font.Add(Language, "@default");
				}
				foreach (KeyValuePair<string, string> item in _font)
				{
					SaveSetting(xmlDocument, "Font/" + item.Key, item.Value);
				}
				xmlDocument.Save(GetOptionsFile());
			}
			catch (Exception ex)
			{
				MessageBox.Show("An Error occured while saving options.\n" + ex.Message);
			}
		}

		protected object LoadSetting(XmlDocument doc, string name, object defaultValue)
		{
			if (doc == null)
			{
				throw new ArgumentNullException("doc");
			}
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException("name");
			}
			if (defaultValue == null)
			{
				throw new ArgumentNullException("defaultValue");
			}
			XmlNode xmlNode = doc.SelectSingleNode("Options/" + name);
			if (xmlNode == null)
			{
				return defaultValue;
			}
			string innerText = xmlNode.InnerText;
			if (defaultValue is string)
			{
				return innerText;
			}
			if (defaultValue is int)
			{
				int result;
				if (int.TryParse(innerText, out result))
				{
					return result;
				}
				return defaultValue;
			}
			if (defaultValue is float)
			{
				float result2;
				if (float.TryParse(innerText, out result2))
				{
					return result2;
				}
				return defaultValue;
			}
			if (defaultValue is bool)
			{
				bool result3;
				if (bool.TryParse(innerText, out result3))
				{
					return result3;
				}
				return defaultValue;
			}
			if (defaultValue is Enum)
			{
				try
				{
					return Enum.Parse(defaultValue.GetType(), innerText);
				}
				catch (ArgumentException)
				{
					return defaultValue;
				}
			}
			return defaultValue;
		}

		public void Load()
		{
			try
			{
				if (!File.Exists(GetOptionsFile()))
				{
					return;
				}
				XmlDocument xmlDocument = new XmlDocument();
				try
				{
					xmlDocument.Load(GetOptionsFile());
				}
				catch
				{
					return;
				}
				try
				{
					GraphicsDevice = (int)LoadSetting(xmlDocument, "GraphicsDevice", GraphicsDevice);
				}
				catch
				{
				}
				try
				{
					SoundDevice = (int)LoadSetting(xmlDocument, "SoundDevice", SoundDevice);
				}
				catch
				{
				}
				try
				{
					InputDevice = (int)LoadSetting(xmlDocument, "InputDevice", InputDevice);
				}
				catch
				{
				}
				Resolution resolution = default(Resolution);
				try
				{
					resolution.Size.Width = (int)LoadSetting(xmlDocument, "Resolution/Size/Width", Resolution.Size.Width);
				}
				catch
				{
				}
				try
				{
					resolution.Size.Height = (int)LoadSetting(xmlDocument, "Resolution/Size/Height", Resolution.Size.Height);
				}
				catch
				{
				}
				try
				{
					resolution.Proportion.Width = (int)LoadSetting(xmlDocument, "Resolution/Proportion/Width", Resolution.Proportion.Width);
				}
				catch
				{
				}
				try
				{
					resolution.Proportion.Height = (int)LoadSetting(xmlDocument, "Resolution/Proportion/Height", Resolution.Proportion.Height);
				}
				catch
				{
				}
				try
				{
					resolution.Bounds.X = (int)LoadSetting(xmlDocument, "Resolution/Bounds/X", Resolution.Bounds.X);
				}
				catch
				{
				}
				try
				{
					resolution.Bounds.Y = (int)LoadSetting(xmlDocument, "Resolution/Bounds/Y", Resolution.Bounds.Y);
				}
				catch
				{
				}
				try
				{
					resolution.Bounds.Width = (int)LoadSetting(xmlDocument, "Resolution/Bounds/Width", Resolution.Bounds.Width);
				}
				catch
				{
				}
				try
				{
					resolution.Bounds.Height = (int)LoadSetting(xmlDocument, "Resolution/Bounds/Height", Resolution.Bounds.Height);
				}
				catch
				{
				}
				try
				{
					resolution.FontScale = (float)LoadSetting(xmlDocument, "Resolution/FontScale", Resolution.FontScale);
				}
				catch
				{
				}
				try
				{
					resolution.StateArea.X = (int)LoadSetting(xmlDocument, "Resolution/StateArea/X", Resolution.StateArea.X);
				}
				catch
				{
				}
				try
				{
					resolution.StateArea.Y = (int)LoadSetting(xmlDocument, "Resolution/StateArea/Y", Resolution.StateArea.Y);
				}
				catch
				{
				}
				try
				{
					resolution.StateArea.Width = (int)LoadSetting(xmlDocument, "Resolution/StateArea/Width", Resolution.StateArea.Width);
				}
				catch
				{
				}
				try
				{
					resolution.StateArea.Height = (int)LoadSetting(xmlDocument, "Resolution/StateArea/Height", Resolution.StateArea.Height);
				}
				catch
				{
				}
				try
				{
					resolution.CommentArea.X = (int)LoadSetting(xmlDocument, "Resolution/CommentArea/X", Resolution.CommentArea.X);
				}
				catch
				{
				}
				try
				{
					resolution.CommentArea.Y = (int)LoadSetting(xmlDocument, "Resolution/CommentArea/Y", Resolution.CommentArea.Y);
				}
				catch
				{
				}
				try
				{
					resolution.CommentArea.Width = (int)LoadSetting(xmlDocument, "Resolution/CommentArea/Width", Resolution.CommentArea.Width);
				}
				catch
				{
				}
				try
				{
					resolution.CommentArea.Height = (int)LoadSetting(xmlDocument, "Resolution/CommentArea/Height", Resolution.CommentArea.Height);
				}
				catch
				{
				}
				Resolution = resolution;
				try
				{
					FullScreen = (bool)LoadSetting(xmlDocument, "FullScreen", FullScreen);
				}
				catch
				{
				}
				try
				{
					Graphics = (Level)LoadSetting(xmlDocument, "Graphics", Graphics);
				}
				catch
				{
				}
				try
				{
					DialogueColor = (string)LoadSetting(xmlDocument, "DialogueColor", DialogueColor);
				}
				catch
				{
				}
				try
				{
					DialogueAlpha = (float)LoadSetting(xmlDocument, "DialogueAlpha", DialogueAlpha);
				}
				catch
				{
				}
				try
				{
					SystemVolume = (float)LoadSetting(xmlDocument, "SystemVolume", SystemVolume);
				}
				catch
				{
				}
				try
				{
					BgmVolume = (float)LoadSetting(xmlDocument, "BgmVolume", BgmVolume);
				}
				catch
				{
				}
				try
				{
					SeVolume = (float)LoadSetting(xmlDocument, "SeVolume", SeVolume);
				}
				catch
				{
				}
				try
				{
					VoiceVolume = (float)LoadSetting(xmlDocument, "VoiceVolume", VoiceVolume);
				}
				catch
				{
				}
				try
				{
					VoiceHighlight = (bool)LoadSetting(xmlDocument, "VoiceHighlight", VoiceHighlight);
				}
				catch
				{
				}
				try
				{
					TextSynchro = (bool)LoadSetting(xmlDocument, "TextSynchro", TextSynchro);
				}
				catch
				{
				}
				try
				{
					TextSpeed = (Level)LoadSetting(xmlDocument, "TextSpeed", TextSpeed);
				}
				catch
				{
				}
				try
				{
					AutoSpeed = (Level)LoadSetting(xmlDocument, "AutoSpeed", AutoSpeed);
				}
				catch
				{
				}
				try
				{
					QuickReadOnly = (bool)LoadSetting(xmlDocument, "QuickReadOnly", QuickReadOnly);
				}
				catch
				{
				}
				try
				{
					SelectedShow = (bool)LoadSetting(xmlDocument, "SelectedShow", SelectedShow);
				}
				catch
				{
				}
				try
				{
					TitleShow = (bool)LoadSetting(xmlDocument, "TitleShow", TitleShow);
				}
				catch
				{
				}
				try
				{
					QuickSave = (bool)LoadSetting(xmlDocument, "QuickSave", QuickSave);
				}
				catch
				{
				}
				try
				{
					Shock = (ShockMethod)LoadSetting(xmlDocument, "Shock", Shock);
				}
				catch
				{
				}
				try
				{
					Language = (string)LoadSetting(xmlDocument, "Language", Language);
				}
				catch
				{
				}
				XmlNode xmlNode = xmlDocument.SelectSingleNode("Options/Font");
				if (xmlNode != null)
				{
					foreach (XmlNode childNode in xmlNode.ChildNodes)
					{
						if (childNode.NodeType != XmlNodeType.Element)
						{
							continue;
						}
						try
						{
							string value = (string)LoadSetting(xmlDocument, "Font/" + childNode.Name, "@default");
							if (_font.ContainsKey(childNode.Name))
							{
								_font[childNode.Name] = value;
							}
							else
							{
								_font.Add(childNode.Name, value);
							}
						}
						catch
						{
						}
					}
				}
				if (!_font.ContainsKey(Language))
				{
					_font.Add(Language, "@default");
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("An Error occured while loading options.\n" + ex.Message);
			}
		}

		protected Level ParseLevel(string value)
		{
			switch (value)
			{
			case "Low":
				return Level.Low;
			case "Mid":
				return Level.Mid;
			case "High":
				return Level.High;
			default:
				return Level.None;
			}
		}

		public void SetValue(string title, string value)
		{
			switch (title)
			{
			case "Graphics":
				Graphics = ParseLevel(value);
				break;
			case "DialogueColor":
				DialogueColor = value;
				break;
			case "DialogueAlpha":
				DialogueAlpha = float.Parse(value);
				break;
			case "SystemVolume":
				SystemVolume = float.Parse(value);
				break;
			case "BgmVolume":
				BgmVolume = float.Parse(value);
				break;
			case "SeVolume":
				SeVolume = float.Parse(value);
				break;
			case "VoiceVolume":
				VoiceVolume = float.Parse(value);
				break;
			case "VoiceHighlight":
				VoiceHighlight = bool.Parse(value);
				break;
			case "TextSynchro":
				TextSynchro = bool.Parse(value);
				break;
			case "TextSpeed":
				TextSpeed = ParseLevel(value);
				break;
			case "AutoSpeed":
				AutoSpeed = ParseLevel(value);
				break;
			case "QuickReadOnly":
				QuickReadOnly = bool.Parse(value);
				break;
			case "SelectedShow":
				SelectedShow = bool.Parse(value);
				break;
			case "TitleShow":
				TitleShow = bool.Parse(value);
				break;
			case "QuickSave":
				QuickSave = bool.Parse(value);
				break;
			}
		}

		protected string ToStringLevel(Level value)
		{
			switch (value)
			{
			case Level.High:
				return "High";
			case Level.Mid:
				return "Mid";
			case Level.Low:
				return "Low";
			case Level.None:
				return "None";
			default:
				return "None";
			}
		}

		public string GetValue(string title)
		{
			switch (title)
			{
			case "Graphics":
				return Graphics.ToString();
			case "DialogueColor":
				return DialogueColor.ToString();
			case "DialogueAlpha":
				return DialogueAlpha.ToString();
			case "SystemVolume":
				return SystemVolume.ToString();
			case "BgmVolume":
				return BgmVolume.ToString();
			case "SeVolume":
				return SeVolume.ToString();
			case "VoiceVolume":
				return VoiceVolume.ToString();
			case "VoiceHighlight":
				return VoiceHighlight.ToString();
			case "TextSynchro":
				return TextSynchro.ToString();
			case "TextSpeed":
				return TextSpeed.ToString();
			case "AutoSpeed":
				return AutoSpeed.ToString();
			case "QuickReadOnly":
				return QuickReadOnly.ToString();
			case "SelectedShow":
				return SelectedShow.ToString();
			case "TitleShow":
				return TitleShow.ToString();
			case "QuickSave":
				return QuickSave.ToString();
			default:
				return string.Empty;
			}
		}
	}
}
