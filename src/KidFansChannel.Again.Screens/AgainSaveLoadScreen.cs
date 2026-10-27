using System;
using System.Drawing;
using System.Windows.Forms;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Screens
{
	public class AgainSaveLoadScreen : SaveLoadScreen
	{
		private Picture picSLIcon;

		private Picture picSLNoneIcon;

		private Picture[] picSLIcons = new Picture[6];

		private Picture picSLSelectIcon;

		private bool isSave;

		private bool back;

		private Picture picMain;

		private Picture picBack;

		private String2D strLevel;

		private String2D strTime;

		private String2D strComment;

		private Element eleScreen;

		public override bool DataReady
		{
			get
			{
				if (base.DataReady)
				{
					return true;
				}
				if (picSLIcon != null && picSLIcon.IsCreated && picMain != null && picMain.IsCreated && picBack != null && picBack.IsCreated)
				{
					picSLNoneIcon = GameManager.Graphics.CreateSubPicture(picSLIcon, new Rectangle(0, 0, 50, 50));
					for (int i = 0; i < 3; i++)
					{
						picSLIcons[i] = GameManager.Graphics.CreateSubPicture(picSLIcon, new Rectangle(50 * (i + 1), 0, 50, 50));
					}
					for (int j = 0; j < 3; j++)
					{
						picSLIcons[j + 3] = GameManager.Graphics.CreateSubPicture(picSLIcon, new Rectangle(50 * j, 50, 50, 50));
					}
					picSLSelectIcon = GameManager.Graphics.CreateSubPicture(picSLIcon, new Rectangle(150, 50, 50, 50));
					DataReady = true;
					return true;
				}
				return false;
			}
		}

		public override bool IsSave
		{
			get
			{
				return isSave;
			}
		}

		public override bool CanDrop
		{
			get
			{
				return picBack.Alpha == 255;
			}
		}

		public bool Back
		{
			get
			{
				return back;
			}
		}

		public AgainSaveLoadScreen(bool isSave, bool back)
		{
			this.isSave = isSave;
			this.back = back;
		}

		public override void InitData()
		{
			base.InitData();
			picSLIcon = GameManager.Data.LoadPicture("system", "sl_icon.png");
			picMain = GameManager.Data.LoadPicture("system", isSave ? "save.png" : "load.png");
			if (back)
			{
				picBack = GameManager.Data.LoadPicture("system", "back.png");
			}
			else
			{
				picBack = GameManager.Graphics.CreatePicture(Color.FromArgb(128, Color.Black));
			}
			Font2D font = GameManager.GalCore.GameFont.GetFont(20);
			Font2D font2 = GameManager.GalCore.GameFont.GetFont(18);
			strLevel = new String2D();
			strLevel.Font = font;
			strLevel.Location = Point.Empty;
			strLevel.LineSize = new Size(300, 38);
			strLevel.LineNum = 1;
			strTime = new String2D();
			strTime.Font = font2;
			strTime.Location = Point.Empty;
			strTime.LineSize = new Size(200, 38);
			strTime.LineNum = 1;
			strComment = new String2D();
			strComment.Font = font2;
			strComment.Location = Point.Empty;
			strComment.LineSize = new Size(500, 38);
			strComment.LineNum = 5;
		}

		public override void FinalData()
		{
			base.FinalData();
			picSLNoneIcon.Dispose();
			picSLNoneIcon = null;
			for (int i = 0; i < 6; i++)
			{
				picSLIcons[i].Dispose();
				picSLIcons[i] = null;
			}
			picSLSelectIcon.Dispose();
			picSLSelectIcon = null;
			picSLIcon.Dispose();
			picSLIcon = null;
			picMain.Dispose();
			if (picSnap != null)
			{
				picSnap.Dispose();
				picSnap = null;
			}
		}

		public override bool InitScreen()
		{
			if (!base.InitScreen())
			{
				return false;
			}
			Element item = new Element(picSLSelectIcon);
			Element[] icons = new Element[6];
			for (int i = 0; i < 6; i++)
			{
				icons[i] = new Element(picSLIcons[i]);
			}
			Element item2 = new Element(picSLNoneIcon);
			Element element = new Element();
			element.Add(item, "select", false);
			element.Add(item2, "none", false);
			for (int j = 0; j < 6; j++)
			{
				element.Add(icons[j], "icon" + j, false);
			}
			element.Region = new Rectangle(0, 0, 50, 50);
			Element item3 = new Element(picMain);
			eleScreen = new Element(picBack);
			eleScreen.Add(item3, "main");
			for (int k = 0; k < 60; k++)
			{
				Element element2 = (Element)element.Clone();
				element2.Position = new Point(25 + k % 10 * 50, 70 + k / 10 * 50);
				element2.Value = Icons[k];
				if ((int)element2.Value != -1)
				{
					element2.SetVisible("icon" + (int)element2.Value, true);
				}
				else
				{
					element2.SetVisible("none", true);
				}
				eleScreen["main"].Add(element2, "sl" + k.ToString("00"));
			}
			for (int l = 0; l < 30; l++)
			{
				Element element3 = (Element)element.Clone();
				element3.Position = new Point(525 + l % 5 * 50, 70 + l / 5 * 50);
				element3.Value = Icons[l + GameManager.GameScript.QuickIndexStart];
				if ((int)element3.Value != -1)
				{
					element3.SetVisible("icon" + (int)element3.Value, true);
				}
				else
				{
					element3.SetVisible("none", true);
				}
				eleScreen["main"].Add(element3, "slquick" + l.ToString("00"));
			}
			element.Dispose();
			eleScreen["main"].Focused = true;
			FocusedElement = eleScreen["main"];
			RootElement = eleScreen;
			for (int m = 0; m < 10; m++)
			{
				for (int n = 0; n < 6; n++)
				{
					eleScreen["main"]["sl" + (m + n * 10).ToString("00")].Left = eleScreen["main"]["sl" + ((m + 9) % 10 + n * 10).ToString("00")];
					eleScreen["main"]["sl" + (m + n * 10).ToString("00")].Right = eleScreen["main"]["sl" + ((m + 1) % 10 + n * 10).ToString("00")];
					eleScreen["main"]["sl" + (m + n * 10).ToString("00")].Up = eleScreen["main"]["sl" + (m + (n + 5) % 6 * 10).ToString("00")];
					eleScreen["main"]["sl" + (m + n * 10).ToString("00")].Down = eleScreen["main"]["sl" + (m + (n + 1) % 6 * 10).ToString("00")];
				}
			}
			if (!isSave)
			{
				for (int col = 0; col < 5; col++)
				{
					for (int row = 0; row < 6; row++)
					{
						eleScreen["main"]["slquick" + (col + row * 5).ToString("00")].Left = eleScreen["main"]["slquick" + ((col + 4) % 5 + row * 5).ToString("00")];
						eleScreen["main"]["slquick" + (col + row * 5).ToString("00")].Right = eleScreen["main"]["slquick" + ((col + 1) % 5 + row * 5).ToString("00")];
						eleScreen["main"]["slquick" + (col + row * 5).ToString("00")].Up = eleScreen["main"]["slquick" + (col + (row + 5) % 6 * 5).ToString("00")];
						eleScreen["main"]["slquick" + (col + row * 5).ToString("00")].Down = eleScreen["main"]["slquick" + (col + (row + 1) % 6 * 5).ToString("00")];
					}
				}
				for (int row = 0; row < 6; row++)
				{
					eleScreen["main"]["sl" + (row * 10).ToString("00")].Left = eleScreen["main"]["slquick" + (4 + row * 5).ToString("00")];
					eleScreen["main"]["slquick" + (row * 5).ToString("00")].Left = eleScreen["main"]["sl" + (9 + row * 10).ToString("00")];
					eleScreen["main"]["slquick" + (4 + row * 5).ToString("00")].Right = eleScreen["main"]["sl" + (row * 10).ToString("00")];
					eleScreen["main"]["sl" + (9 + row * 10).ToString("00")].Right = eleScreen["main"]["slquick" + (row * 5).ToString("00")];
				}
				Element element4 = eleScreen["main"];
				Element childLeft = (eleScreen["main"].ChildUp = eleScreen["main"]["slquick" + (GameManager.GameScript.QuickIndexEnd - GameManager.GameScript.QuickIndexStart - 1).ToString("00")]);
				element4.ChildLeft = childLeft;
			}
			else
			{
				Element element6 = eleScreen["main"];
				Element childLeft2 = (eleScreen["main"].ChildUp = eleScreen["main"]["sl" + (GameManager.GameScript.QuickIndexStart - 1).ToString("00")]);
				element6.ChildLeft = childLeft2;
			}
			Element element8 = eleScreen["main"];
			Element childRight = (eleScreen["main"].ChildDown = eleScreen["main"]["sl00"]);
			element8.ChildRight = childRight;
			for (int slot = 0; slot < 60; slot++)
			{
				eleScreen["main"]["sl" + slot.ToString("00")].MouseClick += SaveLoadUnit_MouseClick;
				eleScreen["main"]["sl" + slot.ToString("00")].MouseEnter += SaveLoadUnit_MouseEnter;
				eleScreen["main"]["sl" + slot.ToString("00")].MouseLeave += SaveLoadUnit_MouseLeave;
			}
			if (!isSave)
			{
				for (int quickSlot = 0; quickSlot < 30; quickSlot++)
				{
					eleScreen["main"]["slquick" + quickSlot.ToString("00")].MouseClick += SaveLoadUnit_MouseClick;
					eleScreen["main"]["slquick" + quickSlot.ToString("00")].MouseEnter += SaveLoadUnit_MouseEnter;
					eleScreen["main"]["slquick" + quickSlot.ToString("00")].MouseLeave += SaveLoadUnit_MouseLeave;
				}
			}
			Element element10 = new Element();
			element10.Position = new Point(522, 347);
			eleScreen["main"].Add(element10, "snap");
			Element element11 = new Element(strLevel);
			element11.Position = new Point(25, 385);
			eleScreen["main"].Add(element11, "level");
			Element element12 = new Element(strTime);
			element12.Position = new Point(325, 385);
			eleScreen["main"].Add(element12, "time");
			Element element13 = new Element(strComment);
			element13.Position = new Point(30, 425);
			eleScreen["main"].Add(element13, "comment");
			if (back)
			{
				picBack.Alpha = 0;
				picBack.BeginTransform(0.5f);
				picBack.AlphaTo(255);
				picBack.EndTransform();
				RootElement.SetVisible("main", false);
			}
			return true;
		}

		private void SaveLoadUnit_MouseClick(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			int unitIndex = -1;
			if (isSave)
			{
				if (GameManager.CurrentScript.StoryName == "Inori")
				{
					unitIndex = 0;
				}
				else if (GameManager.CurrentScript.StoryName == "Miyabi")
				{
					unitIndex = 1;
				}
				else
				{
					if (!(GameManager.CurrentScript.StoryName == "Rikarin"))
					{
						return;
					}
					unitIndex = 2;
				}
			}
			else if ((int)element.Value == -1)
			{
				return;
			}
			string elementName = eleScreen["main"].NameOf(element);
			int slotIndex = ((elementName.Length != 4) ? (int.Parse(elementName.Substring(7)) + GameManager.GameScript.QuickIndexStart) : int.Parse(elementName.Substring(2)));
			if (isSave)
			{
				if (Icons[slotIndex] >= 0)
				{
					GameManager.GalCore.ShowMessageBox(new AgainMessageBoxScreen(GameManager.GalCore.GameLanguage.ResourceManager.GetString("SaveStr"), MessageBoxButtons.YesNo, AgainSave, new object[2] { slotIndex, unitIndex }));
				}
				else
				{
					AgainSave(new object[2] { slotIndex, unitIndex });
				}
			}
			else
			{
				GameManager.GalCore.ShowMessageBox(new AgainMessageBoxScreen(GameManager.GalCore.GameLanguage.ResourceManager.GetString("LoadStr"), MessageBoxButtons.YesNo, AgainLoad, new object[1] { slotIndex }));
			}
		}

		private void AgainSave(object[] indexAndIcon)
		{
			Save((int)indexAndIcon[0], null, (int)indexAndIcon[1]);
		}

		private void AgainLoad(object[] index)
		{
			Load((int)index[0]);
			if (back)
			{
				GameManager.GalCore.HideTitleScreen();
			}
		}

		private void SaveLoadUnit_MouseEnter(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			element.SetVisible("select", true);
			if ((int)element.Value != -1)
			{
				string elementName = eleScreen["main"].NameOf(element);
				int index = ((elementName.Length != 4) ? (int.Parse(elementName.Substring(7)) + GameManager.GameScript.QuickIndexStart) : int.Parse(elementName.Substring(2)));
				GetInfo(index);
				eleScreen["main"]["snap"].ElementPicture = picSnap;
				strLevel.ClearString();
				strTime.ClearString();
				strComment.ClearString();
				if (Level != null)
				{
					string value = Level.Substring(Level.IndexOf("/") + 2) + "\n";
					strLevel.AddString(value, ContentAlignment.TopCenter);
				}
				strTime.AddString((SaveTime.Year % 100).ToString("00") + "/" + SaveTime.Month.ToString("00") + "/" + SaveTime.Day.ToString("00") + " " + SaveTime.Hour.ToString("00") + ":" + SaveTime.Minute.ToString("00"), ContentAlignment.TopRight);
				if (Comment != null)
				{
					strComment.AddString(Comment, ContentAlignment.TopLeft);
				}
			}
		}

		private void SaveLoadUnit_MouseLeave(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			element.SetVisible("select", false);
			if ((int)element.Value != -1)
			{
				eleScreen["main"]["snap"].ElementPicture = null;
				strLevel.ClearString();
				strTime.ClearString();
				strComment.ClearString();
			}
		}

		public override bool FinalScreen()
		{
			if (!base.FinalScreen())
			{
				return false;
			}
			Element element = eleScreen["main"]["sl00"];
			for (int i = 0; i < element.Count; i++)
			{
				element[i].Dispose();
			}
			for (int j = 0; j < eleScreen["main"].Count; j++)
			{
				eleScreen["main"][j].Dispose();
			}
			eleScreen["main"].Dispose();
			eleScreen.Dispose();
			eleScreen = null;
			FinalData();
			Finished = true;
			FocusedElement = null;
			RootElement = null;
			return true;
		}

		public override void Update(float elaspedTime)
		{
			base.Update(elaspedTime);
			if (picBack.Time == 0f && !eleScreen.GetVisible("main"))
			{
				if (picBack.Alpha == 255)
				{
					eleScreen.SetVisible("main", true);
				}
				else
				{
					FinalScreen();
				}
			}
		}

		public override void OnKeyPress(Keys keyData)
		{
			if (keyData == Keys.Space && back)
			{
				if (eleScreen.GetVisible("main"))
				{
					eleScreen.SetVisible("main", false);
					picBack.BeginTransform(0.5f);
					picBack.AlphaTo(0);
					picBack.EndTransform();
				}
			}
			else
			{
				base.OnKeyPress(keyData);
			}
		}
	}
}
