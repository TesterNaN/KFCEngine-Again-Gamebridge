using System;
using System.Drawing;
using System.Windows.Forms;
using KidFansChannel.DataCore;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Screens
{
	public class AgainMovieScreen : MovieScreen
	{
		private Picture picSelectBase;

		private Picture[] picSelects = new Picture[4];

		private Picture picMenuBase;

		private Picture[] picMenus = new Picture[6];

		private Picture picMain;

		private Picture picBack;

		private Element eleScreen;

		public override bool DataReady
		{
			get
			{
				if (base.DataReady)
				{
					return true;
				}
				if (picSelectBase != null && picSelectBase.IsCreated && picMenuBase != null && picMenuBase.IsCreated && picMain != null && picMain.IsCreated && picBack != null && picBack.IsCreated)
				{
					for (int i = 0; i < 4; i++)
					{
						picSelects[i] = GameManager.Graphics.CreateSubPicture(picSelectBase, new Rectangle(i % 2 * 20, i / 2 * 20, 20, 20));
						picSelects[i].Center = new Point((i % 2 == 0) ? 100 : (-80), (i / 2 == 0) ? 75 : (-55));
					}
					for (int j = 0; j < 6; j++)
					{
						picMenus[j] = GameManager.Graphics.CreateSubPicture(picMenuBase, new Rectangle(j % 2 * 200, j / 2 * 150, 200, 150));
					}
					DataReady = true;
					return true;
				}
				return false;
			}
		}

		public override void InitData()
		{
			base.InitData();
			picSelectBase = GameManager.Data.LoadPicture("system", "select.png");
			picMenuBase = GameManager.Data.LoadPicture("system", "movie_menu.png");
			picMain = GameManager.Data.LoadPicture("system", "movie.png");
			picBack = GameManager.Data.LoadPicture("system", "movie_back.png");
		}

		public override void FinalData()
		{
			base.FinalData();
			Picture[] selectPics = picSelects;
			foreach (Picture picture in selectPics)
			{
				picture.Dispose();
			}
			picSelectBase.Dispose();
			Picture[] menuPics = picMenus;
			foreach (Picture picture2 in menuPics)
			{
				picture2.Dispose();
			}
			picMenuBase.Dispose();
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
			element.Focused = true;
			FocusedElement = element;
			RootElement = eleScreen;
			for (int i = 0; i < 4; i++)
			{
				picSelects[i].Position = new Point(picSelects[i].CenterX + ((i % 2 == 0) ? (-20) : 800), picSelects[i].CenterY + ((i / 2 == 0) ? (-20) : 600));
				element.Add(new Element(picSelects[i]), "select" + i);
			}
			for (int j = 0; j < 5; j++)
			{
				Element element2 = new Element(picMenus[Opens[j] ? j : 5]);
				element2.Position = new Point(80 + 220 * (j % 3), 130 + 200 * (j / 3));
				element2.Region = new Rectangle(0, 0, 200, 150);
				element2.Value = j;
				element.Add(element2, j.ToString("00"));
			}
			for (int k = 0; k < 5; k++)
			{
				Element element3 = element[k.ToString("00")];
				Element down = (element[k.ToString("00")].Up = element[(((k + 3) % 6 < 5) ? ((k + 3) % 6) : 4).ToString("00")]);
				element3.Down = down;
				element[k.ToString("00")].Left = element[((k < 3) ? ((k + 2) % 3) : (7 - k)).ToString("00")];
				element[k.ToString("00")].Right = element[((k < 3) ? ((k + 1) % 3) : (7 - k)).ToString("00")];
			}
			element.ChildDown = element["00"];
			element.ChildUp = element["04"];
			for (int l = 0; l < 5; l++)
			{
				element[l.ToString("00")].MouseClick += Unit_MouseClick;
				element[l.ToString("00")].MouseEnter += Unit_MouseEnter;
				element[l.ToString("00")].MouseLeave += Unit_MouseLeave;
			}
			picBack.Alpha = 0;
			picBack.BeginTransform(0.5f);
			picBack.AlphaTo(255);
			picBack.EndTransform();
			RootElement.SetVisible("main", false);
			GameManager.Sound.StopBgm();
			return true;
		}

		private void Unit_MouseClick(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			int movieId = (int)element.Value;
			if (Opens[movieId])
			{
				PlayMovie(Files[movieId]);
			}
		}

		private void Unit_MouseEnter(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			for (int i = 0; i < 4; i++)
			{
				picSelects[i].BeginTransform(0.5f);
				picSelects[i].PositionTo(new Point(element.Position.X + 100, element.Position.Y + 75));
				picSelects[i].ScaleTo(new SizeF(1f, 1f), new SizeF(1.1f, 1.1f), 0.5f);
				picSelects[i].EndTransform();
			}
		}

		private void Unit_MouseLeave(object sender, EventArgs e)
		{
			for (int i = 0; i < 4; i++)
			{
				picSelects[i].BeginTransform(1f);
				picSelects[i].PositionTo(new Point(picSelects[i].CenterX + ((i % 2 == 0) ? (-20) : 800), picSelects[i].CenterY + ((i / 2 == 0) ? (-20) : 600)));
				picSelects[i].EndTransform();
			}
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
			if (keyData == Keys.Space)
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
