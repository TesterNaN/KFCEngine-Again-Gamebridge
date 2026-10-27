using System;
using System.Drawing;
using System.Windows.Forms;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Screens
{
	public class AgainPauseScreen : PauseScreen
	{
		private String2D strItem;

		private String2D strValue;

		private Picture picMain;

		private Picture picBack;

		private Picture picIconBase;

		private Picture picBar;

		private Picture[,] picIcons = new Picture[8, 4];

		private Picture[] picAnimeIcon = new Picture[8];

		private String2D[] strMenuItem = new String2D[8];

		private Element eleScreen;

		public override bool DataReady
		{
			get
			{
				if (base.DataReady)
				{
					return true;
				}
				if (picMain != null && picMain.IsCreated && picBack != null && picBack.IsCreated && picIconBase != null && picIconBase.IsCreated && picBar != null && picBar.IsCreated)
				{
					for (int i = 0; i < 8; i++)
					{
						picAnimeIcon[i] = GameManager.Graphics.CreateAnimePicture(new Size(32, 32));
						for (int j = 0; j < 4; j++)
						{
							picIcons[i, j] = GameManager.Graphics.CreateSubPicture(picIconBase, new Rectangle(j * 32, i * 32, 32, 32));
							((AnimePicture)picAnimeIcon[i]).AddFrame(picIcons[i, j], 0.125f);
						}
					}
					picBar.Center = new Point(80, 20);
					picBar.Position = new Point(150, 15);
					picBar.ScaleX = 1.5f;
					DataReady = true;
					return true;
				}
				return false;
			}
		}

		public override void InitData()
		{
			base.InitData();
			picMain = GameManager.Data.LoadPicture("system", "pause.png");
			picIconBase = GameManager.Data.LoadPicture("system", "pause_icon.png");
			picBar = GameManager.Data.LoadPicture("system", "bar.png");
			picBack = GameManager.Graphics.CreatePicture(Color.FromArgb(128, Color.Black));
			Font2D font = GameManager.GalCore.GameFont.GetFont(18);
			strItem = new String2D();
			strItem.Font = font;
			strItem.Location = new Point(0, 70);
			strItem.LineSize = new Size(250, 35);
			strItem.LineNum = 6;
			strValue = new String2D();
			strValue.Font = font;
			strValue.Location = new Point(250, 70);
			strValue.LineSize = new Size(500, 35);
			strValue.LineNum = 6;
			for (int i = 0; i < 8; i++)
			{
				strMenuItem[i] = new String2D();
				strMenuItem[i].Font = font;
				strMenuItem[i].Location = new Point(50, 0);
				strMenuItem[i].LineSize = new Size(250, 35);
				strMenuItem[i].LineNum = 1;
			}
			string[] menuTexts = GameManager.GalCore.GameLanguage.ResourceManager.GetString("MenuItemStr").Split('\n');
			for (int j = 0; j < 8; j++)
			{
				strMenuItem[j].AddString(menuTexts[j]);
			}
		}

		public override void FinalData()
		{
			base.FinalData();
			picMain.Dispose();
			picBack.Dispose();
			for (int i = 0; i < 8; i++)
			{
				picAnimeIcon[i].Dispose();
			}
			for (int j = 0; j < 8; j++)
			{
				for (int k = 0; k < 4; k++)
				{
					picIcons[j, k].Dispose();
				}
			}
			picIconBase.Dispose();
			picBar.Dispose();
		}

		public override bool InitScreen()
		{
			if (!base.InitScreen())
			{
				return false;
			}
			Element element = new Element(picMain);
			eleScreen = new Element(picBack);
			element.Position = new Point(475, 235);
			eleScreen.Add(element, "main");
			eleScreen.Add(new Element(strItem), "item");
			eleScreen.Add(new Element(strValue), "value");
			Element element2 = new Element(picBar);
			for (int i = 0; i < 8; i++)
			{
				Element element3 = new Element(strMenuItem[i]);
				Element element4 = new Element();
				element4.Add(new Element(picIcons[i, 0]), "static");
				element4.Add(new Element(picAnimeIcon[i]), "anime", false);
				element4.Position = new Point(20, 0);
				element3.Add(element4, "icon");
				element3.Add((Element)element2.Clone(), "bar", false);
				element3.Position = new Point(5, 15 + i * 35);
				element3.Value = i;
				element3.Region = new Rectangle(0, 0, 300, 35);
				element.Add(element3, "menuitem" + i.ToString("00"));
			}
			element2.Dispose();
			element.Focused = true;
			element.ChildDown = element["menuitem00"];
			element.ChildUp = element["menuitem07"];
			FocusedElement = element;
			RootElement = eleScreen;
			string value = GameManager.GalCore.GameLanguage.ResourceManager.GetString("StateStr");
			string value2 = ((Item.Chapter == null) ? "------" : Item.Chapter) + "\n" + ((Item.BgName == null) ? "------" : Item.BgName) + "\n" + ((Item.BgmName == null) ? "------" : Item.BgmName) + "\n" + Item.ReadPercentage.ToString("P") + "\n" + new TimeSpan((long)Item.CurrentTime * 10000000L).ToString() + "\n" + new TimeSpan((long)Item.TotalTime * 10000000L).ToString();
			strItem.AddString(value, ContentAlignment.TopRight);
			strValue.AddString(value2, ContentAlignment.TopLeft);
			for (int j = 0; j < 7; j++)
			{
				element["menuitem" + j.ToString("00")].Down = element["menuitem" + (j + 1).ToString("00")];
				element["menuitem" + (j + 1).ToString("00")].Up = element["menuitem" + j.ToString("00")];
			}
			for (int k = 0; k < 8; k++)
			{
				element["menuitem" + k.ToString("00")].MouseClick += MenuItem_MouseClick;
				element["menuitem" + k.ToString("00")].MouseEnter += MenuItem_MouseEnter;
				element["menuitem" + k.ToString("00")].MouseLeave += MenuItem_MouseLeave;
			}
			return true;
		}

		private void MenuItem_MouseClick(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			switch ((int)element.Value)
			{
			case 0:
				GameManager.GalCore.ShowSaveLoadScreen(true);
				break;
			case 1:
				GameManager.GalCore.ShowSaveLoadScreen(false);
				break;
			case 2:
				GameManager.GameScript.QuickSave(QuickSaveType.Quick);
				FinalScreen();
				break;
			case 3:
				GameManager.GameScript.QuickLoad();
				FinalScreen();
				break;
			case 4:
				GameManager.GalCore.ShowOptionsScreen();
				break;
			case 5:
				GameManager.GalCore.ShowEncyListScreen();
				break;
			case 6:
				GameManager.GalCore.ShowMessageBox(new AgainMessageBoxScreen(GameManager.GalCore.GameLanguage.ResourceManager.GetString("ExitStr"), MessageBoxButtons.YesNo, AgainExitGame, null));
				break;
			case 7:
				FinalScreen();
				break;
			}
			GameManager.Data.LoadAndPlaySystemSound((DateTime.Now.Month == 4 && DateTime.Now.Day == 1) ? "sysse.ogg" : "se_enter.ogg");
		}

		private void AgainExitGame(object[] empty)
		{
			ExitGame();
		}

		private void MenuItem_MouseEnter(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			element.SetVisible("bar", true);
			element["icon"].SetVisible("anime", true);
			element["icon"].SetVisible("static", false);
			((AnimePicture)picAnimeIcon[(int)element.Value]).ResetFrame();
		}

		private void MenuItem_MouseLeave(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			element.SetVisible("bar", false);
			element["icon"].SetVisible("static", true);
			element["icon"].SetVisible("anime", false);
		}

		public override bool FinalScreen()
		{
			if (!base.FinalScreen())
			{
				return false;
			}
			for (int i = 0; i < eleScreen["main"].Count; i++)
			{
				for (int j = 0; j < eleScreen["main"][i]["icon"].Count; j++)
				{
					eleScreen["main"][i]["icon"][j].Dispose();
				}
				for (int k = 0; k < eleScreen["main"][i].Count; k++)
				{
					eleScreen["main"][i][k].Dispose();
				}
				eleScreen["main"][i].Dispose();
			}
			for (int l = 0; l < eleScreen.Count; l++)
			{
				eleScreen[l].Dispose();
			}
			eleScreen.Dispose();
			eleScreen = null;
			FinalData();
			Finished = true;
			FocusedElement = null;
			RootElement = null;
			return true;
		}
	}
}
