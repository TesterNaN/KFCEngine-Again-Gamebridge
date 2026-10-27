using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using KidFansChannel.GalCore;
using KidFansChannel.GameCore;
using Microsoft.DirectX.DirectInput;
using Microsoft.DirectX.DirectSound;

namespace KidFansChannel.GameSetting
{
	public class Setting : Form
	{
		private string language = Thread.CurrentThread.CurrentUICulture.Name;

		private GameOptions gameOptions = new GameOptions();

		private IContainer components;

		private TabControl tcClass;

		private TabPage tpGraphics;

		private Label lblGraphicsDevice;

		private CheckBox ckbFullScreen;

		private ComboBox cbbResolution;

		private ComboBox cbbGraphicsDevice;

		private Label lblResolution;

		private TabPage tpSound;

		private TabPage pbInput;

		private TabPage ptMisc;

		private System.Windows.Forms.Button btnCancel;

		private System.Windows.Forms.Button btnOk;

		private System.Windows.Forms.Button btnCustomResoultion;

		private Label lblSoundDevice;

		private ComboBox cbbSoundDevice;

		private Label lblInputDevice;

		private ComboBox cbbInputDevice;

		private GroupBox gpbShock;

		private RadioButton rdbShockBoth;

		private RadioButton rdbShockWindow;

		private RadioButton rdbShockGamepad;

		private RadioButton rdbShockNone;

		private CheckBox ckbFontDefault;

		private System.Windows.Forms.Button btnFont;

		private FontDialog fontDialog;

		protected Label lblLanguage;

		protected ComboBox cbbLanguage;

		private Label lblCaution;

		protected int GraphicsDevice
		{
			get
			{
				return cbbGraphicsDevice.SelectedIndex;
			}
			set
			{
				if (value < cbbGraphicsDevice.Items.Count)
				{
					cbbGraphicsDevice.SelectedIndex = value;
				}
				else
				{
					cbbGraphicsDevice.SelectedIndex = 0;
				}
			}
		}

		protected Resolution Resolution
		{
			get
			{
				return (Resolution)cbbResolution.SelectedItem;
			}
			set
			{
				bool flag = false;
				for (int i = 0; i < cbbResolution.Items.Count; i++)
				{
					if ((Resolution)cbbResolution.Items[i] == value)
					{
						cbbResolution.SelectedIndex = i;
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					cbbResolution.Items.Add(value);
					cbbResolution.SelectedItem = value;
				}
			}
		}

		protected bool FullScreen
		{
			get
			{
				return ckbFullScreen.Checked;
			}
			set
			{
				ckbFullScreen.Checked = value;
			}
		}

		protected int SoundDevice
		{
			get
			{
				return cbbSoundDevice.SelectedIndex;
			}
			set
			{
				if (value < cbbSoundDevice.Items.Count)
				{
					cbbSoundDevice.SelectedIndex = value;
				}
				else
				{
					cbbSoundDevice.SelectedIndex = 0;
				}
			}
		}

		protected int InputDevice
		{
			get
			{
				if (cbbInputDevice.Items.Count == 0)
				{
					return 0;
				}
				return cbbInputDevice.SelectedIndex;
			}
			set
			{
				if (value < cbbInputDevice.Items.Count)
				{
					cbbInputDevice.SelectedIndex = value;
				}
				else if (cbbInputDevice.Items.Count > 0)
				{
					cbbInputDevice.SelectedIndex = 0;
				}
			}
		}

		protected GameOptions.ShockMethod Shock
		{
			get
			{
				if (rdbShockNone.Checked)
				{
					return GameOptions.ShockMethod.None;
				}
				if (rdbShockGamepad.Checked)
				{
					return GameOptions.ShockMethod.Gamepad;
				}
				if (rdbShockWindow.Checked)
				{
					return GameOptions.ShockMethod.Window;
				}
				return GameOptions.ShockMethod.GamepadAndWindow;
			}
			set
			{
				switch (value)
				{
				case GameOptions.ShockMethod.None:
					rdbShockNone.Checked = true;
					break;
				case GameOptions.ShockMethod.Gamepad:
					rdbShockGamepad.Checked = true;
					break;
				case GameOptions.ShockMethod.Window:
					rdbShockWindow.Checked = true;
					break;
				case GameOptions.ShockMethod.GamepadAndWindow:
					rdbShockBoth.Checked = true;
					break;
				}
			}
		}

		protected string GameFont
		{
			get
			{
				if (ckbFontDefault.Checked)
				{
					return "@default";
				}
				return fontDialog.Font.FontFamily.Name;
			}
			set
			{
				if (value == "@default")
				{
					ckbFontDefault.Checked = true;
				}
				else
				{
					fontDialog.Font = new Font(value, 11f);
				}
			}
		}

		protected string Language
		{
			get
			{
				return (string)cbbLanguage.SelectedItem;
			}
			set
			{
				cbbLanguage.SelectedItem = value;
			}
		}

		public Setting()
		{
			XmlDocument xmlDocument = new XmlDocument();
			xmlDocument.Load("language.xml");
			XmlNode documentElement = xmlDocument.DocumentElement;
			if (documentElement.Name != "language")
			{
				throw new InvalidDataException();
			}
			IEnumerator enumerator = documentElement.ChildNodes.GetEnumerator();
			try
			{
				if (enumerator.MoveNext())
				{
					XmlNode xmlNode = (XmlNode)enumerator.Current;
					if (xmlNode.Name != "item" || !xmlNode.HasChildNodes || xmlNode.FirstChild.Name != "name" || !xmlNode.FirstChild.HasChildNodes || xmlNode.FirstChild.ChildNodes.Count != 1 || xmlNode.FirstChild.FirstChild.NodeType != XmlNodeType.Text)
					{
						throw new InvalidDataException();
					}
					language = xmlNode.FirstChild.FirstChild.Value;
				}
			}
			finally
			{
				IDisposable disposable = enumerator as IDisposable;
				if (disposable != null)
				{
					disposable.Dispose();
				}
			}
			try
			{
				Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
			}
			catch (Exception)
			{
			}
			InitializeComponent();
			gameOptions.SettingFolder = GetSettingFolder();
			gameOptions.Load();
			InitGraphics();
			InitSound();
			InitInput();
			InitMisc();
		}

		protected virtual string GetSettingFolder()
		{
			return string.Empty;
		}

		protected void InitGraphics()
		{
			AddGraphicsDevice();
			AddResolutions();
			GraphicsDevice = gameOptions.GraphicsDevice;
			Resolution = gameOptions.Resolution;
			FullScreen = gameOptions.FullScreen;
		}

		protected void AddGraphicsDevice()
		{
			cbbGraphicsDevice.Items.Clear();
			Enumeration.Enumerate();
			List<EnumAdapterInformation> adapterInformationList = Enumeration.AdapterInformationList;
			foreach (EnumAdapterInformation item in adapterInformationList)
			{
				cbbGraphicsDevice.Items.Add(item.UniqueDescription);
			}
			if (cbbGraphicsDevice.Items.Count == 0)
			{
				MessageBox.Show("No Graphics Device can be found!");
				Application.Exit();
			}
			cbbGraphicsDevice.SelectedIndex = 0;
		}

		protected virtual void AddResolutions()
		{
			cbbResolution.Items.Clear();
			cbbResolution.Items.Add(new Resolution(new Size(800, 600), new Size(4, 3), new Rectangle(0, 0, 800, 600), 1f, Rectangle.Empty, new Rectangle(0, 0, 800, 40)));
			cbbResolution.Items.Add(new Resolution(new Size(960, 720), new Size(4, 3), new Rectangle(0, 0, 960, 720), 1.2f, Rectangle.Empty, new Rectangle(0, 0, 960, 48)));
			cbbResolution.Items.Add(new Resolution(new Size(1024, 768), new Size(4, 3), new Rectangle(0, 0, 1024, 768), 1.28f, Rectangle.Empty, new Rectangle(0, 0, 1024, 52)));
			cbbResolution.Items.Add(new Resolution(new Size(1152, 864), new Size(4, 3), new Rectangle(0, 0, 1152, 864), 1.44f, Rectangle.Empty, new Rectangle(0, 0, 1152, 58)));
			cbbResolution.Items.Add(new Resolution(new Size(1280, 960), new Size(4, 3), new Rectangle(0, 0, 1280, 960), 1.6f, Rectangle.Empty, new Rectangle(0, 0, 1280, 64)));
			cbbResolution.Items.Add(new Resolution(new Size(960, 600), new Size(16, 10), new Rectangle(80, 0, 800, 600), 1f, Rectangle.Empty, new Rectangle(80, 0, 800, 40)));
			cbbResolution.Items.Add(new Resolution(new Size(1280, 800), new Size(16, 10), new Rectangle(108, 1, 1064, 798), 1.33f, Rectangle.Empty, new Rectangle(108, 0, 1064, 54)));
			cbbResolution.Items.Add(new Resolution(new Size(1440, 900), new Size(16, 10), new Rectangle(120, 0, 1200, 900), 1.5f, Rectangle.Empty, new Rectangle(120, 0, 1200, 60)));
			cbbResolution.Items.Add(new Resolution(new Size(1088, 612), new Size(16, 9), new Rectangle(136, 0, 816, 612), 1.02f, Rectangle.Empty, new Rectangle(136, 0, 816, 40)));
			cbbResolution.Items.Add(new Resolution(new Size(1176, 664), new Size(16, 9), new Rectangle(146, 0, 884, 663), 1.105f, Rectangle.Empty, new Rectangle(146, 0, 884, 44)));
			cbbResolution.Items.Add(new Resolution(new Size(1280, 720), new Size(16, 9), new Rectangle(160, 0, 960, 720), 1.2f, Rectangle.Empty, new Rectangle(160, 0, 960, 48)));
			cbbResolution.Items.Add(new Resolution(new Size(1360, 768), new Size(16, 9), new Rectangle(168, 0, 1024, 768), 1.28f, Rectangle.Empty, new Rectangle(168, 0, 1024, 52)));
			cbbResolution.Items.Add(new Resolution(new Size(1280, 1024), new Size(5, 4), new Rectangle(0, 32, 1280, 960), 1.6f, Rectangle.Empty, new Rectangle(0, 32, 1280, 64)));
			cbbResolution.Items.Add(new Resolution(new Size(1280, 768), new Size(5, 3), new Rectangle(128, 0, 1024, 768), 1.28f, Rectangle.Empty, new Rectangle(128, 0, 1024, 52)));
			cbbResolution.Items.Add(new Resolution(new Size(960, 600), new Size(16, 10), new Rectangle(0, 0, 800, 600), 1f, new Rectangle(800, 0, 160, 400), new Rectangle(800, 400, 160, 200)));
			cbbResolution.Items.Add(new Resolution(new Size(1280, 800), new Size(16, 10), new Rectangle(0, 1, 1064, 798), 1.33f, new Rectangle(1064, 1, 216, 532), new Rectangle(1064, 533, 216, 266)));
			cbbResolution.Items.Add(new Resolution(new Size(1440, 900), new Size(16, 10), new Rectangle(0, 0, 1200, 900), 1.5f, new Rectangle(1200, 0, 240, 600), new Rectangle(1200, 600, 240, 300)));
			cbbResolution.Items.Add(new Resolution(new Size(1088, 612), new Size(16, 9), new Rectangle(0, 0, 816, 612), 1.02f, new Rectangle(816, 0, 272, 408), new Rectangle(816, 408, 272, 204)));
			cbbResolution.Items.Add(new Resolution(new Size(1176, 664), new Size(16, 9), new Rectangle(0, 0, 884, 663), 1.105f, new Rectangle(884, 0, 292, 442), new Rectangle(884, 442, 292, 221)));
			cbbResolution.Items.Add(new Resolution(new Size(1280, 720), new Size(16, 9), new Rectangle(0, 0, 960, 720), 1.2f, new Rectangle(960, 0, 320, 480), new Rectangle(960, 480, 320, 240)));
			cbbResolution.Items.Add(new Resolution(new Size(1360, 768), new Size(16, 9), new Rectangle(0, 0, 1024, 768), 1.28f, new Rectangle(1024, 0, 336, 512), new Rectangle(1024, 512, 336, 256)));
			cbbResolution.Items.Add(new Resolution(new Size(1280, 1024), new Size(5, 4), new Rectangle(0, 0, 1280, 960), 1.6f, Rectangle.Empty, new Rectangle(0, 960, 1280, 64)));
			cbbResolution.Items.Add(new Resolution(new Size(1280, 768), new Size(5, 3), new Rectangle(0, 0, 1024, 768), 1.28f, new Rectangle(1024, 0, 256, 512), new Rectangle(1024, 512, 256, 256)));
			cbbResolution.SelectedIndex = 0;
		}

		private void cbbGraphicsDevice_MeasureItem(object sender, MeasureItemEventArgs e)
		{
			e.ItemHeight <<= 1;
		}

		private void cbbGraphicsDevice_DrawItem(object sender, DrawItemEventArgs e)
		{
			e.DrawBackground();
			if (e.Index >= 0)
			{
				e.Graphics.DrawString((string)((ComboBox)sender).Items[e.Index], e.Font, new SolidBrush(e.ForeColor), e.Bounds);
			}
		}

		private void cbbResolution_MeasureItem(object sender, MeasureItemEventArgs e)
		{
			e.ItemHeight <<= 1;
		}

		private void cbbResolution_DrawItem(object sender, DrawItemEventArgs e)
		{
			e.DrawBackground();
			if (e.Index < 0)
			{
				return;
			}
			Resolution resolution = (Resolution)((ComboBox)sender).Items[e.Index];
			string s = string.Format("{0}*{1}\n({2}:{3})", resolution.Size.Width, resolution.Size.Height, resolution.Proportion.Width, resolution.Proportion.Height);
			e.Graphics.DrawString(s, e.Font, new SolidBrush(e.ForeColor), e.Bounds);
			Rectangle rect = default(Rectangle);
			rect.Height = e.Bounds.Height;
			double num = (double)e.Bounds.Height / (double)resolution.Size.Height;
			rect.Width = (int)((double)resolution.Size.Width * num + 0.5);
			rect.X = e.Bounds.X + e.Bounds.Width - rect.Width;
			rect.Y = e.Bounds.Y;
			e.Graphics.FillRectangle(new SolidBrush(Color.Black), rect);
			Rectangle rect2 = default(Rectangle);
			rect2.X = (int)((double)resolution.Bounds.X * num + (double)rect.X + 0.5);
			rect2.Y = (int)((double)resolution.Bounds.Y * num + (double)rect.Y + 0.5);
			rect2.Width = (int)((double)resolution.Bounds.Width * num + 0.5);
			rect2.Height = (int)((double)resolution.Bounds.Height * num + 0.5);
			e.Graphics.FillRectangle(new SolidBrush(Color.Gray), rect2);
			if (resolution.StateArea != Rectangle.Empty)
			{
				Rectangle rectangle = default(Rectangle);
				rectangle.X = (int)((double)resolution.StateArea.X * num + (double)rect.X + 0.5);
				rectangle.Y = (int)((double)resolution.StateArea.Y * num + (double)rect.Y + 0.5);
				rectangle.Width = (int)((double)resolution.StateArea.Width * num + 0.5);
				rectangle.Height = (int)((double)resolution.StateArea.Height * num + 0.5);
				if (rectangle.Width / 2 != 0)
				{
					e.Graphics.FillRectangle(new LinearGradientBrush(new Point(rectangle.X, rectangle.Y), new Point(rectangle.X + rectangle.Width / 2, rectangle.Y), Color.White, Color.Transparent), rectangle.X, rectangle.Y, rectangle.Width / 2, rectangle.Height);
					e.Graphics.FillRectangle(new LinearGradientBrush(new Point(rectangle.X + rectangle.Width, rectangle.Y), new Point(rectangle.X + rectangle.Width / 2 - 1, rectangle.Y), Color.White, Color.Transparent), rectangle.X + rectangle.Width / 2, rectangle.Y, rectangle.Width / 2, rectangle.Height);
				}
			}
			if (resolution.CommentArea != Rectangle.Empty)
			{
				Rectangle rect3 = default(Rectangle);
				rect3.X = (int)((double)resolution.CommentArea.X * num + (double)rect.X + 0.5);
				rect3.Y = (int)((double)resolution.CommentArea.Y * num + (double)rect.Y + 0.5);
				rect3.Width = (int)((double)resolution.CommentArea.Width * num + 0.5);
				rect3.Height = (int)((double)resolution.CommentArea.Height * num + 0.5);
				if (rect3.Height != 0)
				{
					e.Graphics.FillRectangle(new LinearGradientBrush(rect3, Color.Transparent, Color.White, LinearGradientMode.Vertical), rect3);
				}
			}
		}

		private void btnCustomResoultion_Click(object sender, EventArgs e)
		{
			CustomResolutionDialog customResolutionDialog = new CustomResolutionDialog();
			customResolutionDialog.Resolution = Resolution;
			if (customResolutionDialog.ShowDialog() == DialogResult.OK)
			{
				Resolution = customResolutionDialog.Resolution;
			}
		}

		protected void InitSound()
		{
			AddSoundDevice();
			SoundDevice = gameOptions.SoundDevice;
		}

		protected void AddSoundDevice()
		{
			cbbSoundDevice.Items.Clear();
			DevicesCollection devicesCollection = new DevicesCollection();
			foreach (DeviceInformation item in devicesCollection)
			{
				cbbSoundDevice.Items.Add(item.Description);
			}
			if (cbbSoundDevice.Items.Count == 0)
			{
				MessageBox.Show("No Sound Device can be found!");
				Application.Exit();
			}
			cbbSoundDevice.SelectedIndex = 0;
		}

		protected void InitInput()
		{
			AddInputDevice();
			InputDevice = gameOptions.InputDevice;
			Shock = gameOptions.Shock;
		}

		protected void AddInputDevice()
		{
			cbbInputDevice.Items.Clear();
			DeviceList devices = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
			foreach (DeviceInstance item in devices)
			{
				cbbInputDevice.Items.Add(item.InstanceName);
			}
			if (cbbInputDevice.Items.Count > 0)
			{
				cbbInputDevice.SelectedIndex = 0;
			}
		}

		protected void InitMisc()
		{
			AddLanguage();
			Language = gameOptions.Language;
			GameFont = gameOptions.Font;
		}

		protected void AddLanguage()
		{
			cbbLanguage.Items.Clear();
			AddLanguageList();
			cbbLanguage.SelectedIndex = 0;
		}

		protected virtual void AddLanguageList()
		{
			AddOneLanguage(language);
		}

		protected void AddOneLanguage(string language)
		{
			cbbLanguage.Items.Add(language);
		}

		private void ckbFontDefault_CheckedChanged(object sender, EventArgs e)
		{
			btnFont.Enabled = !((CheckBox)sender).Checked;
		}

		private void btnFont_Click(object sender, EventArgs e)
		{
			fontDialog.ShowDialog();
		}

		private void btnOk_Click(object sender, EventArgs e)
		{
			gameOptions.GraphicsDevice = GraphicsDevice;
			gameOptions.Resolution = Resolution;
			gameOptions.FullScreen = FullScreen;
			gameOptions.SoundDevice = SoundDevice;
			gameOptions.InputDevice = InputDevice;
			gameOptions.Shock = Shock;
			gameOptions.Language = Language;
			gameOptions.Font = GameFont;
			gameOptions.Save();
			Application.Exit();
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			Application.Exit();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(KidFansChannel.GameSetting.Setting));
			this.tcClass = new System.Windows.Forms.TabControl();
			this.tpGraphics = new System.Windows.Forms.TabPage();
			this.lblCaution = new System.Windows.Forms.Label();
			this.btnCustomResoultion = new System.Windows.Forms.Button();
			this.lblGraphicsDevice = new System.Windows.Forms.Label();
			this.ckbFullScreen = new System.Windows.Forms.CheckBox();
			this.cbbResolution = new System.Windows.Forms.ComboBox();
			this.cbbGraphicsDevice = new System.Windows.Forms.ComboBox();
			this.lblResolution = new System.Windows.Forms.Label();
			this.tpSound = new System.Windows.Forms.TabPage();
			this.lblSoundDevice = new System.Windows.Forms.Label();
			this.cbbSoundDevice = new System.Windows.Forms.ComboBox();
			this.pbInput = new System.Windows.Forms.TabPage();
			this.gpbShock = new System.Windows.Forms.GroupBox();
			this.rdbShockBoth = new System.Windows.Forms.RadioButton();
			this.rdbShockWindow = new System.Windows.Forms.RadioButton();
			this.rdbShockGamepad = new System.Windows.Forms.RadioButton();
			this.rdbShockNone = new System.Windows.Forms.RadioButton();
			this.lblInputDevice = new System.Windows.Forms.Label();
			this.cbbInputDevice = new System.Windows.Forms.ComboBox();
			this.ptMisc = new System.Windows.Forms.TabPage();
			this.lblLanguage = new System.Windows.Forms.Label();
			this.cbbLanguage = new System.Windows.Forms.ComboBox();
			this.btnFont = new System.Windows.Forms.Button();
			this.ckbFontDefault = new System.Windows.Forms.CheckBox();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.fontDialog = new System.Windows.Forms.FontDialog();
			this.tcClass.SuspendLayout();
			this.tpGraphics.SuspendLayout();
			this.tpSound.SuspendLayout();
			this.pbInput.SuspendLayout();
			this.gpbShock.SuspendLayout();
			this.ptMisc.SuspendLayout();
			base.SuspendLayout();
			this.tcClass.AccessibleDescription = null;
			this.tcClass.AccessibleName = null;
			resources.ApplyResources(this.tcClass, "tcClass");
			this.tcClass.BackgroundImage = null;
			this.tcClass.Controls.Add(this.tpGraphics);
			this.tcClass.Controls.Add(this.tpSound);
			this.tcClass.Controls.Add(this.pbInput);
			this.tcClass.Controls.Add(this.ptMisc);
			this.tcClass.Font = null;
			this.tcClass.Name = "tcClass";
			this.tcClass.SelectedIndex = 0;
			this.tpGraphics.AccessibleDescription = null;
			this.tpGraphics.AccessibleName = null;
			resources.ApplyResources(this.tpGraphics, "tpGraphics");
			this.tpGraphics.BackgroundImage = null;
			this.tpGraphics.Controls.Add(this.lblCaution);
			this.tpGraphics.Controls.Add(this.btnCustomResoultion);
			this.tpGraphics.Controls.Add(this.lblGraphicsDevice);
			this.tpGraphics.Controls.Add(this.ckbFullScreen);
			this.tpGraphics.Controls.Add(this.cbbResolution);
			this.tpGraphics.Controls.Add(this.cbbGraphicsDevice);
			this.tpGraphics.Controls.Add(this.lblResolution);
			this.tpGraphics.Font = null;
			this.tpGraphics.Name = "tpGraphics";
			this.tpGraphics.UseVisualStyleBackColor = true;
			this.lblCaution.AccessibleDescription = null;
			this.lblCaution.AccessibleName = null;
			resources.ApplyResources(this.lblCaution, "lblCaution");
			this.lblCaution.Font = null;
			this.lblCaution.ForeColor = System.Drawing.Color.Red;
			this.lblCaution.Name = "lblCaution";
			this.btnCustomResoultion.AccessibleDescription = null;
			this.btnCustomResoultion.AccessibleName = null;
			resources.ApplyResources(this.btnCustomResoultion, "btnCustomResoultion");
			this.btnCustomResoultion.BackgroundImage = null;
			this.btnCustomResoultion.Font = null;
			this.btnCustomResoultion.Name = "btnCustomResoultion";
			this.btnCustomResoultion.UseVisualStyleBackColor = true;
			this.btnCustomResoultion.Click += new System.EventHandler(this.btnCustomResoultion_Click);
			this.lblGraphicsDevice.AccessibleDescription = null;
			this.lblGraphicsDevice.AccessibleName = null;
			resources.ApplyResources(this.lblGraphicsDevice, "lblGraphicsDevice");
			this.lblGraphicsDevice.Font = null;
			this.lblGraphicsDevice.Name = "lblGraphicsDevice";
			this.ckbFullScreen.AccessibleDescription = null;
			this.ckbFullScreen.AccessibleName = null;
			resources.ApplyResources(this.ckbFullScreen, "ckbFullScreen");
			this.ckbFullScreen.BackgroundImage = null;
			this.ckbFullScreen.Font = null;
			this.ckbFullScreen.Name = "ckbFullScreen";
			this.ckbFullScreen.UseVisualStyleBackColor = true;
			this.cbbResolution.AccessibleDescription = null;
			this.cbbResolution.AccessibleName = null;
			resources.ApplyResources(this.cbbResolution, "cbbResolution");
			this.cbbResolution.BackgroundImage = null;
			this.cbbResolution.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.cbbResolution.DropDownHeight = 212;
			this.cbbResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbbResolution.Font = null;
			this.cbbResolution.Name = "cbbResolution";
			this.cbbResolution.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cbbResolution_DrawItem);
			this.cbbResolution.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.cbbResolution_MeasureItem);
			this.cbbGraphicsDevice.AccessibleDescription = null;
			this.cbbGraphicsDevice.AccessibleName = null;
			resources.ApplyResources(this.cbbGraphicsDevice, "cbbGraphicsDevice");
			this.cbbGraphicsDevice.BackgroundImage = null;
			this.cbbGraphicsDevice.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawVariable;
			this.cbbGraphicsDevice.DropDownHeight = 212;
			this.cbbGraphicsDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbbGraphicsDevice.Font = null;
			this.cbbGraphicsDevice.Name = "cbbGraphicsDevice";
			this.cbbGraphicsDevice.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.cbbGraphicsDevice_DrawItem);
			this.cbbGraphicsDevice.MeasureItem += new System.Windows.Forms.MeasureItemEventHandler(this.cbbGraphicsDevice_MeasureItem);
			this.lblResolution.AccessibleDescription = null;
			this.lblResolution.AccessibleName = null;
			resources.ApplyResources(this.lblResolution, "lblResolution");
			this.lblResolution.Font = null;
			this.lblResolution.Name = "lblResolution";
			this.tpSound.AccessibleDescription = null;
			this.tpSound.AccessibleName = null;
			resources.ApplyResources(this.tpSound, "tpSound");
			this.tpSound.BackgroundImage = null;
			this.tpSound.Controls.Add(this.lblSoundDevice);
			this.tpSound.Controls.Add(this.cbbSoundDevice);
			this.tpSound.Font = null;
			this.tpSound.Name = "tpSound";
			this.tpSound.UseVisualStyleBackColor = true;
			this.lblSoundDevice.AccessibleDescription = null;
			this.lblSoundDevice.AccessibleName = null;
			resources.ApplyResources(this.lblSoundDevice, "lblSoundDevice");
			this.lblSoundDevice.Font = null;
			this.lblSoundDevice.Name = "lblSoundDevice";
			this.cbbSoundDevice.AccessibleDescription = null;
			this.cbbSoundDevice.AccessibleName = null;
			resources.ApplyResources(this.cbbSoundDevice, "cbbSoundDevice");
			this.cbbSoundDevice.BackgroundImage = null;
			this.cbbSoundDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbbSoundDevice.Font = null;
			this.cbbSoundDevice.Name = "cbbSoundDevice";
			this.pbInput.AccessibleDescription = null;
			this.pbInput.AccessibleName = null;
			resources.ApplyResources(this.pbInput, "pbInput");
			this.pbInput.BackgroundImage = null;
			this.pbInput.Controls.Add(this.gpbShock);
			this.pbInput.Controls.Add(this.lblInputDevice);
			this.pbInput.Controls.Add(this.cbbInputDevice);
			this.pbInput.Font = null;
			this.pbInput.Name = "pbInput";
			this.pbInput.UseVisualStyleBackColor = true;
			this.gpbShock.AccessibleDescription = null;
			this.gpbShock.AccessibleName = null;
			resources.ApplyResources(this.gpbShock, "gpbShock");
			this.gpbShock.BackgroundImage = null;
			this.gpbShock.Controls.Add(this.rdbShockBoth);
			this.gpbShock.Controls.Add(this.rdbShockWindow);
			this.gpbShock.Controls.Add(this.rdbShockGamepad);
			this.gpbShock.Controls.Add(this.rdbShockNone);
			this.gpbShock.Font = null;
			this.gpbShock.Name = "gpbShock";
			this.gpbShock.TabStop = false;
			this.rdbShockBoth.AccessibleDescription = null;
			this.rdbShockBoth.AccessibleName = null;
			resources.ApplyResources(this.rdbShockBoth, "rdbShockBoth");
			this.rdbShockBoth.BackgroundImage = null;
			this.rdbShockBoth.Font = null;
			this.rdbShockBoth.Name = "rdbShockBoth";
			this.rdbShockBoth.TabStop = true;
			this.rdbShockBoth.UseVisualStyleBackColor = true;
			this.rdbShockWindow.AccessibleDescription = null;
			this.rdbShockWindow.AccessibleName = null;
			resources.ApplyResources(this.rdbShockWindow, "rdbShockWindow");
			this.rdbShockWindow.BackgroundImage = null;
			this.rdbShockWindow.Font = null;
			this.rdbShockWindow.Name = "rdbShockWindow";
			this.rdbShockWindow.TabStop = true;
			this.rdbShockWindow.UseVisualStyleBackColor = true;
			this.rdbShockGamepad.AccessibleDescription = null;
			this.rdbShockGamepad.AccessibleName = null;
			resources.ApplyResources(this.rdbShockGamepad, "rdbShockGamepad");
			this.rdbShockGamepad.BackgroundImage = null;
			this.rdbShockGamepad.Font = null;
			this.rdbShockGamepad.Name = "rdbShockGamepad";
			this.rdbShockGamepad.TabStop = true;
			this.rdbShockGamepad.UseVisualStyleBackColor = true;
			this.rdbShockNone.AccessibleDescription = null;
			this.rdbShockNone.AccessibleName = null;
			resources.ApplyResources(this.rdbShockNone, "rdbShockNone");
			this.rdbShockNone.BackgroundImage = null;
			this.rdbShockNone.Font = null;
			this.rdbShockNone.Name = "rdbShockNone";
			this.rdbShockNone.TabStop = true;
			this.rdbShockNone.UseVisualStyleBackColor = true;
			this.lblInputDevice.AccessibleDescription = null;
			this.lblInputDevice.AccessibleName = null;
			resources.ApplyResources(this.lblInputDevice, "lblInputDevice");
			this.lblInputDevice.Font = null;
			this.lblInputDevice.Name = "lblInputDevice";
			this.cbbInputDevice.AccessibleDescription = null;
			this.cbbInputDevice.AccessibleName = null;
			resources.ApplyResources(this.cbbInputDevice, "cbbInputDevice");
			this.cbbInputDevice.BackgroundImage = null;
			this.cbbInputDevice.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbbInputDevice.Font = null;
			this.cbbInputDevice.Name = "cbbInputDevice";
			this.ptMisc.AccessibleDescription = null;
			this.ptMisc.AccessibleName = null;
			resources.ApplyResources(this.ptMisc, "ptMisc");
			this.ptMisc.BackgroundImage = null;
			this.ptMisc.Controls.Add(this.lblLanguage);
			this.ptMisc.Controls.Add(this.cbbLanguage);
			this.ptMisc.Controls.Add(this.btnFont);
			this.ptMisc.Controls.Add(this.ckbFontDefault);
			this.ptMisc.Font = null;
			this.ptMisc.Name = "ptMisc";
			this.ptMisc.UseVisualStyleBackColor = true;
			this.lblLanguage.AccessibleDescription = null;
			this.lblLanguage.AccessibleName = null;
			resources.ApplyResources(this.lblLanguage, "lblLanguage");
			this.lblLanguage.Font = null;
			this.lblLanguage.Name = "lblLanguage";
			this.cbbLanguage.AccessibleDescription = null;
			this.cbbLanguage.AccessibleName = null;
			resources.ApplyResources(this.cbbLanguage, "cbbLanguage");
			this.cbbLanguage.BackgroundImage = null;
			this.cbbLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cbbLanguage.Font = null;
			this.cbbLanguage.Name = "cbbLanguage";
			this.btnFont.AccessibleDescription = null;
			this.btnFont.AccessibleName = null;
			resources.ApplyResources(this.btnFont, "btnFont");
			this.btnFont.BackgroundImage = null;
			this.btnFont.Font = null;
			this.btnFont.Name = "btnFont";
			this.btnFont.UseVisualStyleBackColor = true;
			this.btnFont.Click += new System.EventHandler(this.btnFont_Click);
			this.ckbFontDefault.AccessibleDescription = null;
			this.ckbFontDefault.AccessibleName = null;
			resources.ApplyResources(this.ckbFontDefault, "ckbFontDefault");
			this.ckbFontDefault.BackgroundImage = null;
			this.ckbFontDefault.Font = null;
			this.ckbFontDefault.Name = "ckbFontDefault";
			this.ckbFontDefault.UseVisualStyleBackColor = true;
			this.ckbFontDefault.CheckedChanged += new System.EventHandler(this.ckbFontDefault_CheckedChanged);
			this.btnCancel.AccessibleDescription = null;
			this.btnCancel.AccessibleName = null;
			resources.ApplyResources(this.btnCancel, "btnCancel");
			this.btnCancel.BackgroundImage = null;
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Font = null;
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.UseVisualStyleBackColor = true;
			this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
			this.btnOk.AccessibleDescription = null;
			this.btnOk.AccessibleName = null;
			resources.ApplyResources(this.btnOk, "btnOk");
			this.btnOk.BackgroundImage = null;
			this.btnOk.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOk.Font = null;
			this.btnOk.Name = "btnOk";
			this.btnOk.UseVisualStyleBackColor = true;
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			base.AcceptButton = this.btnOk;
			base.AccessibleDescription = null;
			base.AccessibleName = null;
			resources.ApplyResources(this, "$this");
			base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackgroundImage = null;
			base.CancelButton = this.btnCancel;
			base.Controls.Add(this.btnCancel);
			base.Controls.Add(this.btnOk);
			base.Controls.Add(this.tcClass);
			this.Font = null;
			base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			base.Icon = null;
			base.MaximizeBox = false;
			base.Name = "Setting";
			this.tcClass.ResumeLayout(false);
			this.tpGraphics.ResumeLayout(false);
			this.tpGraphics.PerformLayout();
			this.tpSound.ResumeLayout(false);
			this.tpSound.PerformLayout();
			this.pbInput.ResumeLayout(false);
			this.pbInput.PerformLayout();
			this.gpbShock.ResumeLayout(false);
			this.gpbShock.PerformLayout();
			this.ptMisc.ResumeLayout(false);
			this.ptMisc.PerformLayout();
			base.ResumeLayout(false);
		}
	}
}
