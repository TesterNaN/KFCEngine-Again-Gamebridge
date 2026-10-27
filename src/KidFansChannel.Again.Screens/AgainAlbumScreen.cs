using System;
using System.Drawing;
using System.Windows.Forms;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Screens
{
	public class AgainAlbumScreen : AlbumScreen
	{
		private int _page;

		private Picture picSelectBase;

		private Picture[] picSelects = new Picture[4];

		private Picture picMarkBase;

		private Picture[] picMarks = new Picture[2];

		private Picture picArrowBase;

		private Picture[] picArrows = new Picture[2];

		private Picture[] picMenus = new Picture[5];

		private Picture picNone;

		private Picture picMain;

		private Picture picBack;

		private String2D strPage;

		private Element eleScreen;

		private Element eleLeft;

		private Element eleRight;

		private Picture picAlbum;

		private Picture picChange;

		private float changeTime;

		private bool[,] hasGroup = new bool[5, 16]
		{
			{
				true, true, true, true, true, true, true, true, true, true,
				true, true, true, false, false, false
			},
			{
				true, true, true, true, true, true, true, true, true, true,
				true, true, false, false, false, false
			},
			{
				true, true, true, true, true, true, true, true, true, true,
				true, true, true, false, false, false
			},
			{
				true, true, true, false, true, true, true, true, true, true,
				true, true, true, true, true, true
			},
			{
				true, true, true, false, true, true, true, false, false, false,
				false, false, false, false, false, false
			}
		};

		public int Page
		{
			get
			{
				return _page;
			}
		}

		public override bool DataReady
		{
			get
			{
				if (base.DataReady)
				{
					return true;
				}
				if (picSelectBase != null && picSelectBase.IsCreated && picMarkBase != null && picMarkBase.IsCreated && picArrowBase != null && picArrowBase.IsCreated && picNone != null && picNone.IsCreated && picMenus[0] != null && picMenus[0].IsCreated && picMenus[1] != null && picMenus[1].IsCreated && picMenus[2] != null && picMenus[2].IsCreated && picMenus[3] != null && picMenus[3].IsCreated && picMenus[4] != null && picMenus[4].IsCreated && picMain != null && picMain.IsCreated && picBack != null && picBack.IsCreated)
				{
					for (int i = 0; i < 4; i++)
					{
						picSelects[i] = GameManager.Graphics.CreateSubPicture(picSelectBase, new Rectangle(i % 2 * 20, i / 2 * 20, 20, 20));
						picSelects[i].Center = new Point((i % 2 == 0) ? 85 : (-65), (i / 2 == 0) ? 65 : (-45));
					}
					picMarks[0] = GameManager.Graphics.CreateSubPicture(picMarkBase, new Rectangle(0, 0, 20, 20));
					picMarks[1] = GameManager.Graphics.CreateSubPicture(picMarkBase, new Rectangle(20, 0, 20, 20));
					picArrows[0] = GameManager.Graphics.CreateSubPicture(picArrowBase, new Rectangle(0, 0, 40, 40));
					picArrows[1] = GameManager.Graphics.CreateSubPicture(picArrowBase, new Rectangle(40, 0, 40, 40));
					DataReady = true;
					return true;
				}
				return false;
			}
		}

		public AgainAlbumScreen(int page)
		{
			_page = page;
		}

		public override void InitData()
		{
			base.InitData();
			picSelectBase = GameManager.Data.LoadPicture("system", "select.png");
			picMarkBase = GameManager.Data.LoadPicture("system", "album_mark.png");
			picArrowBase = GameManager.Data.LoadPicture("system", "arrow.png");
			for (int i = 0; i < 5; i++)
			{
				picMenus[i] = GameManager.Data.LoadPicture("system", "album_page" + i + ".png");
			}
			picNone = GameManager.Data.LoadPicture("system", "album_none.png");
			picMain = GameManager.Data.LoadPicture("system", "album.png");
			picBack = GameManager.Data.LoadPicture("system", "album_back.png");
			Font2D font = GameManager.GalCore.GameFont.GetFont(20);
			strPage = new String2D();
			strPage.Font = font;
			strPage.Location = Point.Empty;
			strPage.LineSize = new Size(150, 35);
			strPage.LineNum = 1;
			strPage.DefaultColor = Color.Black;
		}

		public override void FinalData()
		{
			base.FinalData();
			Picture[] selects = picSelects;
			foreach (Picture picture in selects)
			{
				picture.Dispose();
			}
			picSelectBase.Dispose();
			for (int j = 0; j < 2; j++)
			{
				picMarks[j].Dispose();
			}
			picMarkBase.Dispose();
			for (int k = 0; k < 2; k++)
			{
				picArrows[k].Dispose();
			}
			picArrowBase.Dispose();
			for (int l = 0; l < 5; l++)
			{
				picMenus[l].Dispose();
			}
			picNone.Dispose();
			picMain.Dispose();
			picBack.Dispose();
		}

		public override bool InitScreen()
		{
			if (!base.InitScreen())
			{
				return false;
			}
			Element element = new Element(picMain);
			eleScreen = new Element(picBack);
			eleScreen.Add(element, "main");
			Element element2 = new Element();
			eleScreen.Add(element2, "change");
			element.Add(new Element(), "menu");
			element2.Add(new Element(), "menu");
			element.Focused = true;
			FocusedElement = element;
			RootElement = eleScreen;
			for (int i = 0; i < 4; i++)
			{
				element.Add(new Element(picSelects[i]), "select" + i);
			}
			for (int j = 0; j < 16; j++)
			{
				Element element3 = new Element();
				element3.Position = new Point(20 + j % 4 * 200, 70 + j / 4 * 130);
				element3.Region = new Rectangle(0, 0, 160, 120);
				element.Add(element3, j.ToString("00"));
				element2.Add((Element)element3.Clone(), j.ToString("00"));
				for (int k = 0; k < 5; k++)
				{
					Element element4 = new Element();
					element4.Position = new Point(20 + j % 4 * 200 + k * 20, 170 + j / 4 * 130);
					element.Add(element4, j.ToString("00") + k);
					element2.Add((Element)element4.Clone(), j.ToString("00") + k);
				}
			}
			for (int l = 0; l < 16; l++)
			{
				element[l.ToString("00")].MouseClick += Unit_MouseClick;
				element[l.ToString("00")].MouseEnter += Unit_MouseEnter;
				element[l.ToString("00")].MouseLeave += Unit_MouseLeave;
			}
			Element element5 = new Element(strPage);
			element5.Position = new Point(600, 20);
			element.Add(element5, "page");
			UpdatePage("main");
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
			eleScreen.Add(new Element(), "album", false);
			eleScreen["album"].Region = new Rectangle(0, 0, 800, 600);
			eleScreen["album"].Add(new Element(), "change");
			eleScreen["album"].MouseClick += Album_MouseClick;
			picBack.Alpha = 0;
			picBack.BeginTransform(0.5f);
			picBack.AlphaTo(255);
			picBack.EndTransform();
			RootElement.SetVisible("main", false);
			return true;
		}

		private void Album_MouseClick(object sender, EventArgs e)
		{
			if (picChange != null)
			{
				return;
			}
			int groupId = (int)((Element)sender).Value / 10;
			int itemIndex = (int)((Element)sender).Value % 10;
			int nextOpen = itemIndex;
			for (int i = itemIndex + 1; i < Opens[_page][groupId].Length; i++)
			{
				if (Opens[_page][groupId][i])
				{
					nextOpen = i;
					break;
				}
			}
			if (nextOpen == itemIndex)
			{
				picChange = picAlbum;
				picAlbum = null;
				eleScreen["album"].ElementPicture = null;
				eleScreen["album"]["change"].ElementPicture = picChange;
				picChange.Alpha = 255;
				picChange.BeginTransform(0.5f);
				picChange.AlphaTo(0);
				picChange.EndTransform();
			}
			else
			{
				GameManager.Data.LoadAndPlaySystemSound((DateTime.Now.Month == 4 && DateTime.Now.Day == 1) ? "sysse.ogg" : "se_enter.ogg");
				picChange = GameManager.Data.LoadPictureInstant("bg", Files[_page][groupId][nextOpen] + ".png");
				picChange.Scale = new SizeF(Math.Max(800f / (float)picChange.Width, 600f / (float)picChange.Height), Math.Max(800f / (float)picChange.Width, 600f / (float)picChange.Height));
				eleScreen["album"]["change"].ElementPicture = picChange;
				picChange.Alpha = 0;
				picChange.BeginTransform(0.5f);
				picChange.AlphaTo(255);
				picChange.EndTransform();
				eleScreen["album"].Value = groupId * 10 + nextOpen;
			}
		}

		private void Unit_MouseClick(object sender, EventArgs e)
		{
			if (changeTime > 0f)
			{
				return;
			}
			Element element = (Element)sender;
			int groupId = (int)element.Value;
			if (!GroupOpens[_page][groupId])
			{
				return;
			}
			GameManager.Data.LoadAndPlaySystemSound((DateTime.Now.Month == 4 && DateTime.Now.Day == 1) ? "sysse.ogg" : "se_enter.ogg");
			int firstOpen = 0;
			for (int i = 0; i < Opens[_page][groupId].Length; i++)
			{
				if (Opens[_page][groupId][i])
				{
					firstOpen = i;
					break;
				}
			}
			picChange = GameManager.Data.LoadPictureInstant("bg", Files[_page][groupId][firstOpen] + ".png");
			picChange.Scale = new SizeF(Math.Max(800f / (float)picChange.Width, 600f / (float)picChange.Height), Math.Max(800f / (float)picChange.Width, 600f / (float)picChange.Height));
			eleScreen["album"]["change"].ElementPicture = picChange;
			eleScreen.SetVisible("album", true);
			picChange.Alpha = 0;
			picChange.BeginTransform(0.5f);
			picChange.AlphaTo(255);
			picChange.EndTransform();
			eleScreen["album"].Value = groupId * 10 + firstOpen;
			FocusedElement = eleScreen;
			eleScreen.Focused = true;
			eleScreen["main"].Focused = false;
			eleScreen["album"].Selected = true;
			eleScreen["main"].SetVisible("page", false);
		}

		private void Unit_MouseEnter(object sender, EventArgs e)
		{
			if (!(changeTime > 0f))
			{
				Element element = (Element)sender;
				for (int i = 0; i < 4; i++)
				{
					picSelects[i].BeginTransform(0.5f);
					picSelects[i].PositionTo(new Point(element.Position.X + 80, element.Position.Y + 60));
					picSelects[i].ScaleTo(new SizeF(1f, 1f), new SizeF(1.1f, 1.1f), 0.5f);
					picSelects[i].EndTransform();
				}
			}
		}

		private void Unit_MouseLeave(object sender, EventArgs e)
		{
			for (int i = 0; i < 4; i++)
			{
				picSelects[i].BeginTransform(0.5f);
				picSelects[i].PositionTo(new Point(picSelects[i].CenterX + ((i % 2 == 0) ? (-20) : 800), picSelects[i].CenterY + ((i / 2 == 0) ? (-20) : 600)));
				picSelects[i].EndTransform();
			}
		}

		private void Left_MouseClick(object sender, EventArgs e)
		{
			MoveLeftScreen();
		}

		private void MoveLeftScreen()
		{
			if (!(picBack.Time > 0f) && !(changeTime > 0f))
			{
				UpdatePage("change");
				_page = (_page + 4) % 5;
				eleScreen["main"].SetVisible("left", false);
				eleScreen["main"].SetVisible("right", false);
				eleScreen.SetVisible("change", true);
				UpdatePage("main");
				eleScreen["main"].Position = new Point(-800, 0);
				eleScreen["change"].Position = new Point(0, 0);
				changeTime = 0.5f;
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
			picArrows[0].BeginTransform(1f);
			picArrows[0].PositionTo(new Point(0, 0));
			picArrows[0].EndTransform();
		}

		private void Right_MouseClick(object sender, EventArgs e)
		{
			MoveRightScreen();
		}

		private void MoveRightScreen()
		{
			if (!(picBack.Time > 0f) && !(changeTime > 0f))
			{
				UpdatePage("change");
				_page = (_page + 1) % 5;
				eleScreen["main"].SetVisible("left", false);
				eleScreen["main"].SetVisible("right", false);
				eleScreen.SetVisible("change", true);
				UpdatePage("main");
				eleScreen["main"].Position = new Point(800, 0);
				eleScreen["change"].Position = new Point(0, 0);
				changeTime = 0.5f;
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

		private void UpdatePage(string screen)
		{
			eleScreen[screen]["menu"].ElementPicture = picMenus[_page];
			strPage.ClearString();
			strPage.AddString(_page + 1 + " / 5", ContentAlignment.TopRight);
			int groupCount = 0;
			int firstOpen = -1;
			int lastOpen = 0;
			for (int i = 0; i < 16; i++)
			{
				if (hasGroup[_page, i])
				{
					if (firstOpen == -1)
					{
						firstOpen = i;
					}
					lastOpen = i;
					eleScreen[screen].SetVisible(i.ToString("00"), true);
					eleScreen[screen][i.ToString("00")].ElementPicture = (GroupOpens[_page][groupCount] ? null : picNone);
					eleScreen[screen][i.ToString("00")].Value = groupCount;
					for (int j = 0; j < Opens[_page][groupCount].Length; j++)
					{
						eleScreen[screen][i.ToString("00") + j].ElementPicture = (Opens[_page][groupCount][j] ? picMarks[0] : picMarks[1]);
					}
					for (int k = Opens[_page][groupCount].Length; k < 5; k++)
					{
						eleScreen[screen][i.ToString("00") + k].ElementPicture = null;
					}
					groupCount++;
				}
				else
				{
					eleScreen[screen].SetVisible(i.ToString("00"), false);
					for (int l = 0; l < 5; l++)
					{
						eleScreen[screen][i.ToString("00") + l].ElementPicture = null;
					}
				}
			}
			for (int m = 0; m < 16; m++)
			{
				if (hasGroup[_page, m])
				{
					int upTarget = (m + 12) % 16;
					int downTarget = (m + 4) % 16;
					int leftTarget = (m + 3) % 4 + m / 4 * 4;
					int rightTarget = (m + 1) % 4 + m / 4 * 4;
					while (!eleScreen[screen].GetVisible(upTarget.ToString("00")))
					{
						upTarget = (upTarget + 12) % 16;
					}
					eleScreen[screen][m.ToString("00")].Up = eleScreen[screen][upTarget.ToString("00")];
					while (!eleScreen[screen].GetVisible(downTarget.ToString("00")))
					{
						downTarget = (downTarget + 4) % 16;
					}
					eleScreen[screen][m.ToString("00")].Down = eleScreen[screen][downTarget.ToString("00")];
					while (!eleScreen[screen].GetVisible(leftTarget.ToString("00")))
					{
						leftTarget = (leftTarget + 3) % 4 + leftTarget / 4 * 4;
					}
					eleScreen[screen][m.ToString("00")].Left = eleScreen[screen][leftTarget.ToString("00")];
					while (!eleScreen[screen].GetVisible(rightTarget.ToString("00")))
					{
						rightTarget = (rightTarget + 1) % 4 + rightTarget / 4 * 4;
					}
					eleScreen[screen][m.ToString("00")].Right = eleScreen[screen][rightTarget.ToString("00")];
				}
			}
			for (int n = 0; n < 4; n++)
			{
				picSelects[n].Position = new Point(picSelects[n].CenterX + ((n % 2 == 0) ? (-20) : 800), picSelects[n].CenterY + ((n / 2 == 0) ? (-20) : 600));
			}
			Element element = eleScreen[screen];
			Element childRight = (eleScreen[screen].ChildDown = eleScreen[screen][firstOpen.ToString("00")]);
			element.ChildRight = childRight;
			Element element3 = eleScreen[screen];
			Element childLeft = (eleScreen[screen].ChildUp = eleScreen[screen][lastOpen.ToString("00")]);
			element3.ChildLeft = childLeft;
		}

		public override bool FinalScreen()
		{
			if (!base.FinalScreen())
			{
				return false;
			}
			for (int i = 0; i < eleScreen["main"].Count; i++)
			{
				eleScreen["main"][i].Dispose();
			}
			for (int j = 0; j < eleScreen.Count; j++)
			{
				eleScreen[j].Dispose();
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
			if (eleScreen.GetVisible("album") && picChange != null && picChange.Time == 0f)
			{
				if (picChange.Alpha == 255)
				{
					if (picAlbum != null)
					{
						picAlbum.Dispose();
					}
					picAlbum = picChange;
					picChange = null;
					eleScreen["album"].ElementPicture = picAlbum;
					eleScreen["album"]["change"].ElementPicture = null;
				}
				else
				{
					picChange.Dispose();
					picChange = null;
					eleScreen["album"]["change"].ElementPicture = null;
					eleScreen.SetVisible("album", false);
					eleScreen["album"].Selected = false;
					FocusedElement = eleScreen["main"];
					eleScreen["main"].Focused = true;
					eleScreen.Focused = false;
					eleScreen["main"][((int)eleScreen["album"].Value / 10).ToString("00")].Selected = true;
					eleScreen["main"].SetVisible("page", true);
				}
			}
			if (eleScreen.GetVisible("change"))
			{
				if (changeTime > 0f)
				{
					changeTime -= elaspedTime;
					if (changeTime < 0f)
					{
						changeTime = 0f;
					}
					if (eleScreen["main"].Position.X < 0)
					{
						eleScreen["main"].Position = new Point(-(int)(changeTime * 1600f), 0);
						eleScreen["change"].Position = new Point(-(int)(changeTime * 1600f) + 800, 0);
					}
					else
					{
						eleScreen["main"].Position = new Point((int)(changeTime * 1600f), 0);
						eleScreen["change"].Position = new Point((int)(changeTime * 1600f) - 800, 0);
					}
				}
				if (changeTime == 0f)
				{
					eleScreen["main"].SetVisible("left", true);
					eleScreen["main"].SetVisible("right", true);
					UpdatePage("main");
					eleScreen.SetVisible("change", false);
				}
			}
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
			if (keyData == Keys.Space)
			{
				if (eleScreen.GetVisible("main") && picChange == null)
				{
					if (picAlbum != null)
					{
						picChange = picAlbum;
						picAlbum = null;
						eleScreen["album"].ElementPicture = null;
						eleScreen["album"]["change"].ElementPicture = picChange;
						picChange.Alpha = 255;
						picChange.BeginTransform(0.5f);
						picChange.AlphaTo(0);
						picChange.EndTransform();
					}
					else
					{
						eleScreen.SetVisible("main", false);
						picBack.BeginTransform(0.5f);
						picBack.AlphaTo(0);
						picBack.EndTransform();
					}
				}
				return;
			}
			base.OnKeyPress(keyData);
			switch (keyData)
			{
			case Keys.Up:
				if (eleScreen.GetVisible("album") && picAlbum != null)
				{
					picAlbum.Y += 200;
					if (picAlbum.Y > 0)
					{
						picAlbum.Y = 0;
					}
				}
				break;
			case Keys.Down:
				if (eleScreen.GetVisible("album") && picAlbum != null)
				{
					picAlbum.Y -= 200;
					if ((float)picAlbum.Y + (float)picAlbum.Height * picAlbum.ScaleY < 600f)
					{
						picAlbum.Y = (int)(600f - (float)picAlbum.Height * picAlbum.ScaleY);
					}
				}
				break;
			case Keys.Prior:
				MoveLeftScreen();
				break;
			case Keys.Next:
				MoveRightScreen();
				break;
			}
		}

		public override void OnMouseWheel(int delta)
		{
			base.OnMouseWheel(delta);
			if (!eleScreen.GetVisible("album") || picChange != null)
			{
				return;
			}
			if (delta > 0)
			{
				picAlbum.Y += delta / 120 * 200;
				if (picAlbum.Y > 0)
				{
					picAlbum.Y = 0;
				}
			}
			else
			{
				picAlbum.Y += delta / 120 * 200;
				if ((float)picAlbum.Y + (float)picAlbum.Height * picAlbum.ScaleY < 600f)
				{
					picAlbum.Y = (int)(600f - (float)picAlbum.Height * picAlbum.ScaleY);
				}
			}
		}
	}
}
