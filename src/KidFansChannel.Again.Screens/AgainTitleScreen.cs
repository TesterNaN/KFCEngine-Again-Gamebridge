using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using KidFansChannel.DataCore;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Screens
{
	public class AgainTitleScreen : TitleScreen
	{
		private Picture picMain;

		private Picture picTop;

		private Picture picMenuOnBase;

		private Picture[] picMenuOns;

		private Picture picMenuOffBase;

		private Picture[] picMenuOffs;

		private Picture picLogo;

		private Picture picCompany;

		private Element eleScreen;

		/// <summary>菜单项全集。注意 "Staff" 只有娱乐通零售版才有，光谱补丁版到 "Drama" 为止。</summary>
		private string[] values = new string[21]
		{
			"Null", "New Game", "Continue", "Options", "Memories", "Exit", "Last Game", "Load", "Shortcut", "Album",
			"Music", "Movie", "Clear List", "Encyclopedia", "Start", "Prologue", "Inori", "Miyabi", "Rikarin", "Drama",
			"Staff"
		};

		private bool isGameListNow;

		private List<MenuItem> MenuList = new List<MenuItem>();

		private float moveTime;

		private string command;

		public override bool DataReady
		{
			get
			{
				if (base.DataReady)
				{
					return true;
				}
				if (picMain != null && picMain.IsCreated && picLogo != null && picLogo.IsCreated && picMenuOnBase != null && picMenuOnBase.IsCreated && picMenuOffBase != null && picMenuOffBase.IsCreated && picTop != null && picTop.IsCreated && picCompany != null && picCompany.IsCreated)
				{
					for (int i = 0; i < 16; i++)
					{
						picMenuOns[i] = GameManager.Graphics.CreateSubPicture(picMenuOnBase, new Rectangle(0, 40 * i, 320, 40));
						picMenuOffs[i] = GameManager.Graphics.CreateSubPicture(picMenuOffBase, new Rectangle(0, 40 * i, 320, 40));
					}
					for (int j = 0; j < 3; j++)
					{
						picMenuOns[j + 16] = GameManager.Graphics.CreateSubPicture(picMenuOnBase, new Rectangle(0, 80 * j + 640, 320, 80));
						picMenuOffs[j + 16] = GameManager.Graphics.CreateSubPicture(picMenuOffBase, new Rectangle(0, 80 * j + 640, 320, 80));
					}
					picMenuOns[19] = GameManager.Graphics.CreateSubPicture(picMenuOnBase, new Rectangle(0, 880, 320, 40));
					picMenuOffs[19] = GameManager.Graphics.CreateSubPicture(picMenuOffBase, new Rectangle(0, 880, 320, 40));
					picMenuOns[20] = GameManager.Graphics.CreateSubPicture(picMenuOnBase, new Rectangle(0, 920, 320, 40));
					picMenuOffs[20] = GameManager.Graphics.CreateSubPicture(picMenuOffBase, new Rectangle(0, 920, 320, 40));
					for (int k = 0; k < values.Length; k++)
					{
						picMenuOns[k].CenterX = 160;
						picMenuOffs[k].CenterX = 160;
					}
					picLogo.CenterX = picLogo.Width / 2;
					picCompany.CenterX = picCompany.Width / 2;
					DataReady = true;
					return true;
				}
				return false;
			}
		}

		protected override Point GetItemPosition(int index, int level)
		{
			return new Point(0, index * (isGameListNow ? 60 : 40));
		}

		public override void InitData()
		{
			base.InitData();
			int month = DateTime.Now.Month;
			if (month <= 5 || month > 8)
			{
				picMain = GameManager.Data.LoadPicture("system", "title0.png");
			}
			else
			{
				picMain = GameManager.Data.LoadPicture("system", "title1.png");
			}
			picTop = GameManager.Graphics.CreatePicture(Color.Black);
			picTop.Size = new Size(800, 600);
			picMenuOnBase = GameManager.Data.LoadPicture("system", "title_menu0.png");
			picMenuOffBase = GameManager.Data.LoadPicture("system", "title_menu1.png");
			picLogo = GameManager.Data.LoadPicture("system", "title_logo.png");
			picCompany = GameManager.Data.LoadPicture("system", "company.png");
			picMenuOns = new Picture[values.Length];
			picMenuOffs = new Picture[values.Length];
		}

		public override void FinalData()
		{
			base.FinalData();
			for (int i = 0; i < values.Length; i++)
			{
				picMenuOns[i].Dispose();
				picMenuOffs[i].Dispose();
			}
			picMenuOnBase.Dispose();
			picMenuOffBase.Dispose();
			picLogo.Dispose();
			picCompany.Dispose();
			picMain.Dispose();
			picTop.Dispose();
		}

		public override bool InitScreen()
		{
			if (!base.InitScreen())
			{
				return false;
			}
			for (int i = 0; i < 5; i++)
			{
				MenuItem menuItem = new MenuItem();
				menuItem.Add(new Element(picMenuOffs[i + 1]), "off", true);
				menuItem.Add(new Element(picMenuOns[i + 1]), "on", false);
				menuItem.Value = values[i + 1];
				RootItem.Add(menuItem, (string)menuItem.Value);
				MenuList.Add(menuItem);
			}
			bool hasEnding = GameManager.GalCore.GameState.CheckEnding(0) || GameManager.GalCore.GameState.CheckEnding(1) || GameManager.GalCore.GameState.CheckEnding(2) || GameManager.GalCore.GameState.CheckEnding(3) || GameManager.GalCore.GameState.CheckEnding(4);
			bool[] endingFlags = new bool[3]
			{
				GameManager.GalCore.GameState.CheckChapterOpen("Inori", "SummerSeason"),
				GameManager.GalCore.GameState.CheckChapterOpen("Miyabi", "SpreadWings"),
				GameManager.GalCore.GameState.CheckChapterOpen("Rikarin", "SecretProgram")
			};
			for (int j = 0; j < (hasEnding ? 4 : 3); j++)
			{
				MenuItem menuItem2 = new MenuItem();
				menuItem2.Add(new Element(picMenuOffs[j + 16]), "off", true);
				menuItem2.Add(new Element(picMenuOns[j + 16]), "on", false);
				menuItem2.Value = values[j + 16];
				if (j < 3 && endingFlags[j])
				{
					for (int k = 0; k < 2; k++)
					{
						MenuItem menuItem3 = new MenuItem();
						menuItem3.Add(new Element(picMenuOffs[k + 14]), "off", true);
						menuItem3.Add(new Element(picMenuOns[k + 14]), "on", false);
						menuItem3.Value = values[j + 16] + values[k + 14];
						menuItem2.Add(menuItem3, (string)menuItem3.Value);
						MenuList.Add(menuItem3);
					}
					menuItem2.Value = values[j + 16];
				}
				else
				{
					menuItem2.Value = values[j + 16] + values[15];
				}
				if (j < 3)
				{
					menuItem2.Region = new Rectangle(-160, 0, 320, 60);
				}
				RootItem[0].Add(menuItem2, (string)menuItem2.Value);
				MenuList.Add(menuItem2);
			}
			for (int l = ((!File.Exists(GameManager.GameState.GetSaveFileName(90))) ? 1 : 0); l < 3; l++)
			{
				MenuItem menuItem4 = new MenuItem();
				menuItem4.Add(new Element(picMenuOffs[l + 6]), "off", true);
				menuItem4.Add(new Element(picMenuOns[l + 6]), "on", false);
				menuItem4.Value = values[l + 6];
				RootItem[1].Add(menuItem4, (string)menuItem4.Value);
				MenuList.Add(menuItem4);
			}
			bool found = false;
			for (int m = 0; m < 5; m++)
			{
				if (GameManager.GameState.CheckEnding(m))
				{
					found = true;
					break;
				}
			}
			for (int n = 0; n < 5; n++)
			{
				MenuItem menuItem5 = new MenuItem();
				if (found)
				{
					menuItem5.Add(new Element(picMenuOffs[n + 9]), "off", true);
					menuItem5.Add(new Element(picMenuOns[n + 9]), "on", false);
				}
				else
				{
					menuItem5.Add(new Element(picMenuOffs[0]), "off", true);
					menuItem5.Add(new Element(picMenuOns[0]), "on", false);
				}
				if (found)
				{
					menuItem5.Value = values[n + 9];
				}
				else
				{
					menuItem5.Value = values[0] + values[n + 9];
				}
				RootItem[3].Add(menuItem5, (string)menuItem5.Value);
				MenuList.Add(menuItem5);
			}
			Element item = new Element(picMain);
			eleScreen = new Element();
			eleScreen.Add(item, "main");
			Element element = new Element(picLogo);
			element.Position = new Point(400, 0);
			eleScreen.Add(element, "logo");
			Element element2 = new Element(picCompany);
			element2.Position = new Point(400, 540);
			eleScreen.Add(element2, "company");
			ItemElement.Position = new Point(400, 300);
			eleScreen.Add(ItemElement, "menu");
			eleScreen.Add(new Element(picTop), "top");
			RootElement = eleScreen;
			foreach (MenuItem menu in MenuList)
			{
				if (menu.Region.Height != 60)
				{
					menu.Region = new Rectangle(-160, 0, 320, 40);
				}
				menu.MouseEnter += MenuItem_MouseEnter;
				menu.MouseLeave += MenuItem_MouseLeave;
				menu.MouseClick += MenuItem_MouseClick;
				menu.MenuEvent += MenuItem_MenuEvent;
				menu.Initial += MenuItem_Initial;
			}
			bool menuReady = InitMenu();
			picMain.Alpha = 0;
			picMain.BeginTransform(1f);
			picMain.AlphaTo(255);
			picMain.EndTransform();
			picLogo.Alpha = 0;
			picCompany.Alpha = 0;
			for (int menuIndex = 0; menuIndex < values.Length; menuIndex++)
			{
				picMenuOns[menuIndex].Alpha = 0;
				picMenuOffs[menuIndex].Alpha = 0;
			}
			picTop.Alpha = 0;
			if (!menuReady)
			{
				switch (GameManager.Data.CheckData("bgm", "bgm35.ogg", KidFansChannel.DataCore.DataCore.DataType.File))
				{
				case KidFansChannel.DataCore.DataCore.DataType.File:
					GameManager.Sound.OpenBgm(GameManager.Data.GetFile(), true);
					break;
				case KidFansChannel.DataCore.DataCore.DataType.Package:
					GameManager.Sound.OpenBgm(GameManager.Data.GetStream(), true);
					break;
				default:
					return true;
				}
				GameManager.Sound.PlayBgm(0f, 6.625f);
				GameManager.Sound.VolumeBgm = 1f;
			}
			if (moveTime > 0f)
			{
				moveTime = 0f;
			}
			return true;
		}

		private void MenuItem_Initial(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			element.SetVisible("on", false);
			element.SetVisible("off", true);
		}

		private void MenuItem_MouseEnter(object sender, EventArgs e)
		{
			if (!(picTop.Time > 0f))
			{
				Element element = (Element)sender;
				element.SetVisible("on", true);
				element.SetVisible("off", false);
			}
		}

		private void MenuItem_MouseLeave(object sender, EventArgs e)
		{
			if (!(picTop.Time > 0f))
			{
				Element element = (Element)sender;
				element.SetVisible("on", false);
				element.SetVisible("off", true);
			}
		}

		private void MenuItem_MouseClick(object sender, EventArgs e)
		{
			if (picTop.Time > 0f)
			{
				return;
			}
			string text = (string)((Element)sender).Value;
			if (text.StartsWith(values[0]) || picTop.Time > 0f)
			{
				return;
			}
			switch (text)
			{
			case "InoriPrologue":
			case "MiyabiPrologue":
			case "RikarinPrologue":
			case "DramaPrologue":
			case "InoriStart":
			case "MiyabiStart":
			case "RikarinStart":
			case "Last Game":
			case "Load":
			case "Shortcut":
			case "Options":
			case "Album":
			case "Music":
			case "Movie":
			case "Clear List":
			case "Encyclopedia":
			case "Exit":
			case "Staff":
				command = text;
				picTop.BeginTransform(1f);
				picTop.AlphaTo(255);
				picTop.EndTransform();
				break;
			default:
			{
				for (int i = 0; i < ItemElement.Count; i++)
				{
					ItemElement[i]["on"].ElementPicture.BeginTransform(0.5f);
					ItemElement[i]["on"].ElementPicture.AlphaTo(0);
					ItemElement[i]["on"].ElementPicture.EndTransform();
					ItemElement[i]["off"].ElementPicture.BeginTransform(0.5f);
					ItemElement[i]["off"].ElementPicture.AlphaTo(0);
					ItemElement[i]["off"].ElementPicture.EndTransform();
				}
				break;
			}
			}
			if (text != "Exit")
			{
				GameManager.Data.LoadAndPlaySystemSound((DateTime.Now.Month == 4 && DateTime.Now.Day == 1) ? "sysse.ogg" : "se_enter.ogg");
			}
		}

		private void MenuItem_MenuEvent(object sender, MenuItem.MenuEventArg e)
		{
			Element element = (Element)sender;
			e.Enabled = element["on"].ElementPicture.Time == 0f && element["on"].ElementPicture.Alpha == 255;
		}

		public override bool FinalScreen()
		{
			if (!base.FinalScreen())
			{
				return false;
			}
			foreach (MenuItem menu in MenuList)
			{
				menu["on"].Dispose();
				menu["off"].Dispose();
			}
			MenuList.Clear();
			FinalMenu();
			eleScreen["main"].Dispose();
			eleScreen["logo"].Dispose();
			eleScreen["top"].Dispose();
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
			if (picMain.Time == 0f)
			{
				if (picLogo.Time == 0f && picLogo.Alpha == 0)
				{
					picLogo.BeginTransform(0.5f);
					picLogo.AlphaTo(255);
					picLogo.EndTransform();
					picCompany.BeginTransform(0.5f);
					picCompany.AlphaTo(255);
					picCompany.EndTransform();
				}
				else
				{
					if (picTop.Time == 0f && picTop.Alpha == 255)
					{
						if (command == "InoriPrologue")
						{
							StartNewGame("Inori");
						}
						else if (command == "MiyabiPrologue")
						{
							StartNewGame("Miyabi");
						}
						else if (command == "RikarinPrologue")
						{
							StartNewGame("Rikarin");
						}
						else if (command == "DramaPrologue")
						{
							StartNewGame("Drama");
						}
						else if (command == "InoriStart")
						{
							StartNewGame("Inori", "sin01", "SummerSeason");
						}
						else if (command == "MiyabiStart")
						{
							StartNewGame("Miyabi", "smi01", "SpreadWings");
						}
						else if (command == "RikarinStart")
						{
							StartNewGame("Rikarin", "sri01", "SecretProgram");
						}
						else if (command == "Last Game")
						{
							GameManager.GameScript.LoadStory(90);
							FinalScreen();
						}
						else if (command == "Load")
						{
							GameManager.GalCore.ShowSaveLoadScreen(false);
						}
						else if (command == "Shortcut")
						{
							GameManager.GalCore.ShowShortcutScreen();
						}
						else if (command == "Options")
						{
							GameManager.GalCore.ShowOptionsScreen();
						}
						else if (command == "Album")
						{
							GameManager.GalCore.ShowAlbumScreen();
						}
						else if (command == "Music")
						{
							GameManager.GalCore.ShowMusicScreen();
						}
						else if (command == "Movie")
						{
							GameManager.GalCore.ShowMovieScreen();
						}
						else if (command == "Clear List")
						{
							GameManager.GalCore.ShowClearListScreen();
						}
						else if (command == "Encyclopedia")
						{
							GameManager.GalCore.ShowEncyListScreen();
						}
						else if (command == "Exit")
						{
							ExitGame();
						}
						else if (command == "Staff")
						{
							GameManager.GalCore.ShowScreen(new StaffScreen());
						}
						return;
					}
					for (int i = 0; i < ItemElement.Count; i++)
					{
						if (!ItemElement[i].GetVisible("on"))
						{
							ItemElement[i]["on"].ElementPicture.Update(elaspedTime);
						}
						if (!ItemElement[i].GetVisible("off"))
						{
							ItemElement[i]["off"].ElementPicture.Update(elaspedTime);
						}
					}
					if (ItemElement.Count > 0 && ItemElement[0]["on"].ElementPicture.Time == 0f && ItemElement[0]["on"].ElementPicture.Alpha == 0)
					{
						ChangeItem();
						for (int j = 0; j < ItemElement.Count; j++)
						{
							ItemElement[j]["on"].ElementPicture.Alpha = 1;
							ItemElement[j]["on"].ElementPicture.BeginTransform(0.5f);
							ItemElement[j]["on"].ElementPicture.AlphaTo(255);
							ItemElement[j]["on"].ElementPicture.EndTransform();
							ItemElement[j]["off"].ElementPicture.Alpha = 1;
							ItemElement[j]["off"].ElementPicture.BeginTransform(0.5f);
							ItemElement[j]["off"].ElementPicture.AlphaTo(255);
							ItemElement[j]["off"].ElementPicture.EndTransform();
						}
					}
				}
			}
			if (moveTime == -1f)
			{
				moveTime = 0f;
				switch (GameManager.Data.CheckData("bgm", "bgm35.ogg", KidFansChannel.DataCore.DataCore.DataType.File))
				{
				default:
					return;
				case KidFansChannel.DataCore.DataCore.DataType.File:
					GameManager.Sound.OpenBgm(GameManager.Data.GetFile(), true);
					break;
				case KidFansChannel.DataCore.DataCore.DataType.Package:
					GameManager.Sound.OpenBgm(GameManager.Data.GetStream(), true);
					break;
				}
				GameManager.Sound.PlayBgm(0f, 6.625f);
				GameManager.Sound.VolumeBgm = 1f;
			}
			else
			{
				moveTime += elaspedTime;
			}
			if (!(moveTime > 20f))
			{
				return;
			}
			GameManager.GameState.UpdateMovie(0);
			GameManager.GameState.UpdateMusic(35);
			KidFansChannel.DataCore.DataCore.DataType dataType = GameManager.Data.CheckData("movie", "demo.wmv", KidFansChannel.DataCore.DataCore.DataType.File);
			if (dataType == KidFansChannel.DataCore.DataCore.DataType.File)
			{
				GameManager.Sound.StopBgm();
				if (GameManager.GalCore.GameVideo != null)
				{
					GameManager.GalCore.GameVideo.Play(GameManager.Data.GetFile());
				}
				moveTime = -1f;
			}
			else
			{
				moveTime = 0f;
			}
		}

		protected override void UpdateItem(MenuItem item, int level)
		{
			if ((string)item.Value == "New Game")
			{
				isGameListNow = true;
			}
			else
			{
				isGameListNow = false;
			}
			base.UpdateItem(item, level);
		}

		public override void OnKeyPress(Keys keyData)
		{
			base.OnKeyPress(keyData);
			if (moveTime > 0f)
			{
				moveTime = 0f;
			}
			if (ItemToBack != null)
			{
				for (int i = 0; i < ItemElement.Count; i++)
				{
					ItemElement[i]["on"].ElementPicture.BeginTransform(0.5f);
					ItemElement[i]["on"].ElementPicture.AlphaTo(0);
					ItemElement[i]["on"].ElementPicture.EndTransform();
					ItemElement[i]["off"].ElementPicture.BeginTransform(0.5f);
					ItemElement[i]["off"].ElementPicture.AlphaTo(0);
					ItemElement[i]["off"].ElementPicture.EndTransform();
				}
			}
		}

		/// <summary>片头动画没走完之前先吞掉鼠标，避免玩家跳过进场。</summary>
		public override void OnMouseDown(Point location)
		{
			if (moveTime > 0f)
			{
				moveTime = 0f;
			}
			if (picTop.Time == 0f && picTop.Alpha != 255)
			{
				base.OnMouseDown(location);
			}
		}

		public override void OnMouseMove(Point location)
		{
			if (moveTime > 0f)
			{
				moveTime = 0f;
			}
			if (picTop.Time == 0f && picTop.Alpha != 255)
			{
				base.OnMouseMove(location);
			}
		}

		public override void OnMouseWheel(int delta)
		{
			if (moveTime > 0f)
			{
				moveTime = 0f;
			}
			if (picTop.Time == 0f && picTop.Alpha != 255)
			{
				base.OnMouseWheel(delta);
			}
		}
	}
}
