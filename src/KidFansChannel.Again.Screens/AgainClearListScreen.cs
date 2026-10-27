using System;
using System.Drawing;
using System.Windows.Forms;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Screens
{
	public class AgainClearListScreen : ClearListScreen
	{
		private Picture picClearList;

		private Picture picEnding;

		private Picture picSceneTitle;

		private Picture picBack;

		private String2D strClearListItem;

		private String2D strClearListValue;

		private String2D strSceneTitle;

		private String2D strScenePercentage;

		private String2D strEnding;

		private Picture picArrowBase;

		private Picture[] picArrows = new Picture[2];

		private Element eleScreen;

		private Element eleLeft;

		private Element eleRight;

		private AgainScrollBarElement scrollBarScene = new AgainScrollBarElement();

		private AgainScrollBarElement scrollBarEnding = new AgainScrollBarElement();

		private int LineNum = 14;

		private int currentScreen;

		private int lastScreen = -1;

		private string[] screens = new string[3] { "clearlist", "ending", "scenetitle" };

		private int currentEndingStartId;

		private int currentSceneStartId;

		public override bool DataReady
		{
			get
			{
				if (base.DataReady)
				{
					return true;
				}
				if (picArrowBase != null && picArrowBase.IsCreated && picEnding != null && picEnding.IsCreated && picSceneTitle != null && picSceneTitle.IsCreated && picClearList != null && picClearList.IsCreated && picBack != null && picBack.IsCreated && scrollBarScene.DataReady && scrollBarEnding.DataReady)
				{
					picArrows[0] = GameManager.Graphics.CreateSubPicture(picArrowBase, new Rectangle(0, 0, 40, 40));
					picArrows[1] = GameManager.Graphics.CreateSubPicture(picArrowBase, new Rectangle(40, 0, 40, 40));
					DataReady = true;
					return true;
				}
				return false;
			}
		}

		public override void InitData()
		{
			base.InitData();
			picClearList = GameManager.Data.LoadPicture("system", "clearlist.png");
			picEnding = GameManager.Data.LoadPicture("system", "ending.png");
			picSceneTitle = GameManager.Data.LoadPicture("system", "scenetitle.png");
			picBack = GameManager.Data.LoadPicture("system", "clearlist_back.png");
			picArrowBase = GameManager.Data.LoadPicture("system", "arrow.png");
			Font2D font = GameManager.GalCore.GameFont.GetFont(20);
			strClearListItem = new String2D();
			strClearListItem.Font = font;
			strClearListItem.Location = Point.Empty;
			strClearListItem.LineSize = new Size(500, 55);
			strClearListItem.LineNum = 9;
			strClearListValue = new String2D();
			strClearListValue.Font = font;
			strClearListValue.Location = Point.Empty;
			strClearListValue.LineSize = new Size(200, 55);
			strClearListValue.LineNum = 9;
			strSceneTitle = new String2D();
			strSceneTitle.Font = font;
			strSceneTitle.Location = Point.Empty;
			strSceneTitle.LineSize = new Size(550, 35);
			strSceneTitle.LineNum = LineNum;
			strScenePercentage = new String2D();
			strScenePercentage.Font = font;
			strScenePercentage.Location = Point.Empty;
			strScenePercentage.LineSize = new Size(175, 35);
			strScenePercentage.LineNum = LineNum;
			strEnding = new String2D();
			strEnding.Font = font;
			strEnding.Location = Point.Empty;
			strEnding.LineSize = new Size(725, 35);
			strEnding.LineNum = LineNum;
			scrollBarEnding.InitData();
			scrollBarScene.InitData();
		}

		public override void FinalData()
		{
			base.FinalData();
			for (int i = 0; i < 2; i++)
			{
				picArrows[i].Dispose();
			}
			picArrowBase.Dispose();
			picClearList.Dispose();
			picEnding.Dispose();
			picSceneTitle.Dispose();
			picBack.Dispose();
			scrollBarEnding.FinalData();
			scrollBarScene.FinalData();
		}

		public override bool InitScreen()
		{
			if (!base.InitScreen())
			{
				return false;
			}
			eleScreen = new Element(picBack);
			RootElement = eleScreen;
			Element element = new Element(picClearList);
			eleScreen.Add(element, "clearlist");
			element.Focused = true;
			FocusedElement = element;
			Element element2 = new Element(strClearListItem);
			element2.Position = new Point(50, 80);
			element.Add(element2, "item");
			Element element3 = new Element(strClearListValue);
			element3.Position = new Point(550, 80);
			element.Add(element3, "value");
			InitClearList();
			eleLeft = new Element(picArrows[0]);
			eleLeft.Position = new Point(0, 290);
			eleLeft.Region = new Rectangle(0, 0, 40, 40);
			eleRight = new Element(picArrows[1]);
			eleRight.Position = new Point(760, 290);
			eleRight.Region = new Rectangle(0, 0, 40, 40);
			element.Add(eleLeft, "left");
			element.Add(eleRight, "right");
			eleLeft.MouseClick += Left_MouseClick;
			eleLeft.MouseEnter += Left_MouseEnter;
			eleLeft.MouseLeave += Left_MouseLeave;
			eleRight.MouseClick += Right_MouseClick;
			eleRight.MouseEnter += Right_MouseEnter;
			eleRight.MouseLeave += Right_MouseLeave;
			Element element4 = new Element(picEnding);
			eleScreen.Add(element4, "ending", false);
			Element element5 = new Element(strEnding);
			element5.Position = new Point(50, 75);
			element4.Add(element5, "item");
			scrollBarEnding.Init();
			scrollBarEnding.Position = new Point(760, 70);
			scrollBarEnding.Size = 500;
			element4.Add(scrollBarEnding, "scrollbar");
			scrollBarEnding.Length = (float)LineNum / (float)EndingTitles.Length;
			scrollBarEnding.ValueChanged += ScrollBarEnding_ValueChanged;
			UpdateEnding();
			Element element6 = new Element(picSceneTitle);
			eleScreen.Add(element6, "scenetitle", false);
			Element element7 = new Element(strSceneTitle);
			element7.Position = new Point(50, 75);
			element6.Add(element7, "item");
			Element element8 = new Element(strScenePercentage);
			element8.Position = new Point(550, 75);
			element6.Add(element8, "value");
			scrollBarScene.Init();
			scrollBarScene.Position = new Point(760, 70);
			scrollBarScene.Size = 500;
			element6.Add(scrollBarScene, "scrollbar");
			scrollBarScene.Length = (float)LineNum / (float)SceneTitles.Length;
			scrollBarScene.ValueChanged += ScrollBarScene_ValueChanged;
			UpdateSceneTitle();
			picBack.Alpha = 0;
			picBack.BeginTransform(0.5f);
			picBack.AlphaTo(255);
			picBack.EndTransform();
			RootElement.SetVisible("clearlist", false);
			return true;
		}

		/// <summary>拖动结局页的滚动条。</summary>
		private void ScrollBarEnding_ValueChanged(object sender, EventArgs e)
		{
			currentEndingStartId = (int)(((ScrollBarElement)sender).Start * (float)EndingTitles.Length + 0.5f);
			if (currentEndingStartId < 0)
			{
				currentEndingStartId = 0;
			}
			if (currentEndingStartId > EndingTitles.Length - LineNum)
			{
				currentEndingStartId = EndingTitles.Length - LineNum;
				if (currentEndingStartId < 0)
				{
					currentEndingStartId = 0;
				}
			}
			UpdateEnding();
		}

		/// <summary>拖动场景页的滚动条。</summary>
		private void ScrollBarScene_ValueChanged(object sender, EventArgs e)
		{
			currentSceneStartId = (int)(((ScrollBarElement)sender).Start * (float)SceneTitles.Length + 0.5f);
			if (currentSceneStartId < 0)
			{
				currentSceneStartId = 0;
			}
			if (currentSceneStartId > SceneTitles.Length - LineNum)
			{
				currentSceneStartId = SceneTitles.Length - LineNum;
				if (currentSceneStartId < 0)
				{
					currentSceneStartId = 0;
				}
			}
			UpdateSceneTitle();
		}

		private void Left_MouseClick(object sender, EventArgs e)
		{
			MoveLeftScreen();
		}

		private void MoveLeftScreen()
		{
			if (lastScreen < 0 || !(eleScreen[screens[lastScreen]].ElementPicture.Time > 0f))
			{
				lastScreen = currentScreen;
				currentScreen = (currentScreen + 1) % 3;
				eleScreen[screens[lastScreen]].ClearSelected();
				eleScreen[screens[lastScreen]].Remove(eleLeft);
				eleScreen[screens[lastScreen]].Remove(eleRight);
				for (int i = 0; i < eleScreen[screens[lastScreen]].Count; i++)
				{
					eleScreen[screens[lastScreen]].SetVisible(i, false);
				}
				eleScreen.SetVisible(screens[currentScreen], true);
				for (int j = 0; j < eleScreen[screens[currentScreen]].Count; j++)
				{
					eleScreen[screens[currentScreen]].SetVisible(j, false);
				}
				eleScreen[screens[currentScreen]].Focused = true;
				FocusedElement = eleScreen[screens[currentScreen]];
				Picture elementPicture = eleScreen[screens[lastScreen]].ElementPicture;
				elementPicture.BeginTransform(0.5f);
				elementPicture.PositionTo(new Point(800, 0));
				elementPicture.EndTransform();
				Picture elementPicture2 = eleScreen[screens[currentScreen]].ElementPicture;
				elementPicture2.Position = new Point(-800, 0);
				elementPicture2.BeginTransform(0.5f);
				elementPicture2.PositionTo(new Point(0, 0));
				elementPicture2.EndTransform();
			}
		}

		private void Left_MouseEnter(object sender, EventArgs e)
		{
			picArrows[0].BeginTransform(0f);
			picArrows[0].PositionTo(new Point(-10, 0), new Point(0, 0), 0.5f);
			picArrows[0].EndTransform();
		}

		private void Left_MouseLeave(object sender, EventArgs e)
		{
			picArrows[0].BeginTransform(0.5f);
			picArrows[0].PositionTo(new Point(0, 0));
			picArrows[0].EndTransform();
		}

		private void Right_MouseClick(object sender, EventArgs e)
		{
			MoveRightScreen();
		}

		private void MoveRightScreen()
		{
			if (lastScreen < 0 || !(eleScreen[screens[lastScreen]].ElementPicture.Time > 0f))
			{
				lastScreen = currentScreen;
				currentScreen = (currentScreen + 2) % 3;
				eleScreen[screens[lastScreen]].ClearSelected();
				eleScreen[screens[lastScreen]].Remove(eleLeft);
				eleScreen[screens[lastScreen]].Remove(eleRight);
				for (int i = 0; i < eleScreen[screens[lastScreen]].Count; i++)
				{
					eleScreen[screens[lastScreen]].SetVisible(i, false);
				}
				eleScreen.SetVisible(screens[currentScreen], true);
				for (int j = 0; j < eleScreen[screens[currentScreen]].Count; j++)
				{
					eleScreen[screens[currentScreen]].SetVisible(j, false);
				}
				eleScreen[screens[currentScreen]].Focused = true;
				FocusedElement = eleScreen[screens[currentScreen]];
				Picture elementPicture = eleScreen[screens[lastScreen]].ElementPicture;
				elementPicture.BeginTransform(0.5f);
				elementPicture.PositionTo(new Point(-800, 0));
				elementPicture.EndTransform();
				Picture elementPicture2 = eleScreen[screens[currentScreen]].ElementPicture;
				elementPicture2.Position = new Point(800, 0);
				elementPicture2.BeginTransform(0.5f);
				elementPicture2.PositionTo(new Point(0, 0));
				elementPicture2.EndTransform();
			}
		}

		private void Right_MouseEnter(object sender, EventArgs e)
		{
			picArrows[1].BeginTransform(0f);
			picArrows[1].PositionTo(new Point(10, 0), new Point(0, 0), 0.5f);
			picArrows[1].EndTransform();
		}

		private void Right_MouseLeave(object sender, EventArgs e)
		{
			picArrows[1].BeginTransform(0.5f);
			picArrows[1].PositionTo(new Point(0, 0));
			picArrows[1].EndTransform();
		}

		protected void InitClearList()
		{
			strClearListItem.AddString(GameManager.GalCore.GameLanguage.ResourceManager.GetString("ClearListStr"));
			string empty = string.Empty;
			empty = empty + GameState.CountEndings() + " / 5\n";
			empty = empty + (GameState.CountChapters() - (GameState.CheckChapterOpen("Drama", "WarningDrama") ? 1 : 0)) + " / 60\n";
			empty = empty + GameState.CountAlbums() + " / 126\n";
			empty = empty + GameState.CountMusics() + " / 40\n";
			empty = empty + GameState.CountMovies() + " / 5\n";
			empty = empty + GameState.CountEncys() + " / 100\n";
			empty = empty + ((float)GameState.CountStringsRead() / (float)GameState.CountStringsTotal()).ToString("P") + "\n";
			empty = empty + ((float)GameState.CountSelectsRead() / (float)GameState.CountSelectsTotal()).ToString("P") + "\n";
			empty += new TimeSpan((long)(int)GameState.GetTime() * 10000000L).ToString();
			strClearListValue.AddString(empty, ContentAlignment.TopRight);
		}

		public override bool FinalScreen()
		{
			if (!base.FinalScreen())
			{
				return false;
			}
			for (int i = 0; i < eleScreen["clearlist"].Count; i++)
			{
				eleScreen["clearlist"][i].Dispose();
			}
			scrollBarEnding.Final();
			for (int j = 0; j < eleScreen["ending"].Count; j++)
			{
				eleScreen["ending"][j].Dispose();
			}
			scrollBarScene.Final();
			for (int k = 0; k < eleScreen["scenetitle"].Count; k++)
			{
				eleScreen["scenetitle"][k].Dispose();
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

		public override void Update(float elaspedTime)
		{
			base.Update(elaspedTime);
			if (lastScreen >= 0 && eleScreen[screens[lastScreen]].ElementPicture.Time == 0f)
			{
				eleScreen.SetVisible(screens[lastScreen], false);
				lastScreen = -1;
				for (int i = 0; i < eleScreen[screens[currentScreen]].Count; i++)
				{
					eleScreen[screens[currentScreen]].SetVisible(i, true);
				}
				eleScreen[screens[currentScreen]].Add(eleLeft, "left");
				eleScreen[screens[currentScreen]].Add(eleRight, "right");
			}
			if (picBack.Time == 0f && !eleScreen.GetVisible(screens[currentScreen]))
			{
				if (picBack.Alpha == 255)
				{
					eleScreen.SetVisible(screens[currentScreen], true);
				}
				else
				{
					FinalScreen();
				}
			}
		}

		public override void OnKeyPress(Keys keyData)
		{
			if (keyData == Keys.Space)
			{
				if (eleScreen.GetVisible(screens[currentScreen]))
				{
					eleScreen.SetVisible(screens[currentScreen], false);
					picBack.BeginTransform(0.5f);
					picBack.AlphaTo(0);
					picBack.EndTransform();
				}
				return;
			}
			base.OnKeyPress(keyData);
			switch (keyData)
			{
			case Keys.Up:
				if (lastScreen != -1)
				{
					break;
				}
				if (currentScreen == 1)
				{
					if (currentEndingStartId > 0)
					{
						currentEndingStartId--;
					}
					UpdateEnding();
				}
				else if (currentScreen == 2)
				{
					if (currentSceneStartId > 0)
					{
						currentSceneStartId--;
					}
					UpdateSceneTitle();
				}
				break;
			case Keys.Down:
				if (lastScreen != -1)
				{
					break;
				}
				if (currentScreen == 1)
				{
					if (currentEndingStartId < EndingTitles.Length - LineNum)
					{
						currentEndingStartId++;
					}
					UpdateEnding();
				}
				else if (currentScreen == 2)
				{
					if (currentSceneStartId < SceneTitles.Length - LineNum)
					{
						currentSceneStartId++;
					}
					UpdateSceneTitle();
				}
				break;
			case Keys.Prior:
				if (lastScreen != -1)
				{
					break;
				}
				if (currentScreen == 1)
				{
					currentEndingStartId -= LineNum;
					if (currentEndingStartId < 0)
					{
						currentEndingStartId = 0;
					}
					UpdateEnding();
				}
				else if (currentScreen == 2)
				{
					currentSceneStartId -= LineNum;
					if (currentSceneStartId < 0)
					{
						currentSceneStartId = 0;
					}
					UpdateSceneTitle();
				}
				break;
			case Keys.Next:
				if (lastScreen != -1)
				{
					break;
				}
				if (currentScreen == 1)
				{
					currentEndingStartId += LineNum;
					if (currentEndingStartId > EndingTitles.Length - LineNum)
					{
						currentEndingStartId = EndingTitles.Length - LineNum;
						if (currentEndingStartId < 0)
						{
							currentEndingStartId = 0;
						}
					}
					UpdateEnding();
				}
				else
				{
					if (currentScreen != 2)
					{
						break;
					}
					currentSceneStartId += LineNum;
					if (currentSceneStartId > SceneTitles.Length - LineNum)
					{
						currentSceneStartId = SceneTitles.Length - LineNum;
						if (currentSceneStartId < 0)
						{
							currentSceneStartId = 0;
						}
					}
					UpdateSceneTitle();
				}
				break;
			case Keys.Left:
				MoveLeftScreen();
				break;
			case Keys.Right:
				MoveRightScreen();
				break;
			case Keys.End:
			case Keys.Home:
				break;
			}
		}

		public override void OnMouseWheel(int delta)
		{
			base.OnMouseWheel(delta);
			if (lastScreen != -1)
			{
				return;
			}
			if (currentScreen == 1)
			{
				if (delta > 0)
				{
					currentEndingStartId -= delta / 120;
					if (currentEndingStartId < 0)
					{
						currentEndingStartId = 0;
					}
					UpdateEnding();
				}
				else
				{
					if (delta >= 0)
					{
						return;
					}
					currentEndingStartId -= delta / 120;
					if (currentEndingStartId > EndingTitles.Length - LineNum)
					{
						currentEndingStartId = EndingTitles.Length - LineNum;
						if (currentEndingStartId < 0)
						{
							currentEndingStartId = 0;
						}
					}
					UpdateEnding();
				}
			}
			else
			{
				if (currentScreen != 2)
				{
					return;
				}
				if (delta > 0)
				{
					currentSceneStartId -= delta / 120;
					if (currentSceneStartId < 0)
					{
						currentSceneStartId = 0;
					}
					UpdateSceneTitle();
				}
				else
				{
					if (delta >= 0)
					{
						return;
					}
					currentSceneStartId -= delta / 120;
					if (currentSceneStartId > SceneTitles.Length - LineNum)
					{
						currentSceneStartId = SceneTitles.Length - LineNum;
						if (currentSceneStartId < 0)
						{
							currentSceneStartId = 0;
						}
					}
					UpdateSceneTitle();
				}
			}
		}

		protected void UpdateEnding()
		{
			string line = string.Empty;
			for (int i = 0; i < LineNum; i++)
			{
				int endingId = i + currentEndingStartId;
				if (endingId == EndingTitles.Length)
				{
					break;
				}
				line = line + (endingId + 1) + ". ";
				if (EndingOpens[endingId])
				{
					line = line + EndingTitles[endingId] + "\n";
					continue;
				}
				for (int j = 0; j < EndingTitles[endingId].Length; j++)
				{
					line += "?";
				}
				line += "\n";
			}
			strEnding.ClearString();
			strEnding.AddString(line);
			scrollBarEnding.Start = (float)currentEndingStartId / (float)EndingTitles.Length;
		}

		protected void UpdateSceneTitle()
		{
			string line = string.Empty;
			string detail = string.Empty;
			for (int i = 0; i < LineNum; i++)
			{
				int endingId = i + currentSceneStartId;
				if (endingId == SceneTitles.Length)
				{
					break;
				}
				line = line + (endingId + 1).ToString("00") + ". ";
				if (SceneOpens[endingId])
				{
					line = line + SceneTitles[endingId] + "\n";
					detail = detail + Percentages[endingId].ToString("P") + "\n";
					continue;
				}
				for (int j = 0; j < SceneTitles[endingId].Length; j++)
				{
					line += "?";
				}
				line += "\n";
				detail += "\n";
			}
			strSceneTitle.ClearString();
			strSceneTitle.AddString(line);
			strScenePercentage.ClearString();
			strScenePercentage.AddString(detail, ContentAlignment.TopRight);
			scrollBarScene.Start = (float)currentSceneStartId / (float)SceneTitles.Length;
		}
	}
}
