using System;
using System.Drawing;
using System.Windows.Forms;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Screens
{
	public class AgainAlbumListScreen : AlbumListScreen
	{
		private Picture picSelectBase;

		private Picture[] picSelects = new Picture[4];

		private Picture picMenuOnBase;

		private Picture[] picMenuOns = new Picture[4];

		private Picture picMenuOffBase;

		private Picture[] picMenuOffs = new Picture[4];

		private Picture picMain;

		private Picture[] picBacks = new Picture[4];

		private Element eleScreen;

		private int currentBack;

		private int pageToShow = -1;

		public override bool DataReady
		{
			get
			{
				if (base.DataReady)
				{
					return true;
				}
				if (picSelectBase != null && picSelectBase.IsCreated && picMenuOnBase != null && picMenuOnBase.IsCreated && picMenuOffBase != null && picMenuOffBase.IsCreated && picMain != null && picMain.IsCreated && picBacks[0] != null && picBacks[0].IsCreated && picBacks[1] != null && picBacks[1].IsCreated && picBacks[2] != null && picBacks[2].IsCreated && picBacks[3] != null && picBacks[3].IsCreated)
				{
					for (int i = 0; i < 4; i++)
					{
						picSelects[i] = GameManager.Graphics.CreateSubPicture(picSelectBase, new Rectangle(i % 2 * 20, i / 2 * 20, 20, 20));
						picSelects[i].Center = new Point((i % 2 == 0) ? 185 : (-165), (i / 2 == 0) ? 45 : (-25));
					}
					for (int j = 0; j < 4; j++)
					{
						picMenuOns[j] = GameManager.Graphics.CreateSubPicture(picMenuOnBase, new Rectangle(0, j * 80, 360, 80));
						picMenuOffs[j] = GameManager.Graphics.CreateSubPicture(picMenuOffBase, new Rectangle(0, j * 80, 360, 80));
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
			picMenuOnBase = GameManager.Data.LoadPicture("system", "album_menu0.png");
			picMenuOffBase = GameManager.Data.LoadPicture("system", "album_menu1.png");
			picMain = GameManager.Data.LoadPicture("system", "album.png");
			for (int i = 0; i < 4; i++)
			{
				picBacks[i] = GameManager.Data.LoadPicture("system", "album_back" + i + ".png");
			}
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
			Picture[] menuOnPics = picMenuOns;
			foreach (Picture picture2 in menuOnPics)
			{
				picture2.Dispose();
			}
			picMenuOnBase.Dispose();
			Picture[] menuOffPics = picMenuOffs;
			foreach (Picture picture3 in menuOffPics)
			{
				picture3.Dispose();
			}
			picMenuOnBase.Dispose();
			picMain.Dispose();
			Picture[] backPics = picBacks;
			foreach (Picture picture4 in backPics)
			{
				picture4.Dispose();
			}
		}

		public override bool InitScreen()
		{
			if (!base.InitScreen())
			{
				return false;
			}
			Element element = new Element(picMain);
			eleScreen = new Element(picBacks[0]);
			eleScreen.Add(new Element(), "back");
			eleScreen.Add(element, "main");
			element.Focused = true;
			FocusedElement = element;
			RootElement = eleScreen;
			for (int i = 0; i < 4; i++)
			{
				picSelects[i].Position = new Point(picSelects[i].CenterX + ((i % 2 == 0) ? (-20) : 800), picSelects[i].CenterY + ((i / 2 == 0) ? (-20) : 600));
				element.Add(new Element(picSelects[i]), "select" + i);
			}
			for (int j = 0; j < 4; j++)
			{
				Element element2 = new Element(Opens[j] ? picMenuOns[j] : picMenuOffs[j]);
				element2.Position = new Point(40, 100 + j * 120);
				element2.Region = new Rectangle(0, 0, 360, 80);
				element2.Value = (j + 1) % 4;
				element.Add(element2, j.ToString("00"));
			}
			for (int k = 0; k < 4; k++)
			{
				element[k.ToString("00")].Up = element[((k + 3) % 4).ToString("00")];
				element[k.ToString("00")].Down = element[((k + 1) % 4).ToString("00")];
			}
			element.ChildDown = element["00"];
			element.ChildUp = element["03"];
			for (int l = 0; l < 4; l++)
			{
				element[l.ToString("00")].MouseClick += Unit_MouseClick;
				element[l.ToString("00")].MouseEnter += Unit_MouseEnter;
				element[l.ToString("00")].MouseLeave += Unit_MouseLeave;
			}
			currentBack = 0;
			pageToShow = -1;
			picBacks[currentBack].Alpha = 0;
			picBacks[currentBack].BeginTransform(0.5f);
			picBacks[currentBack].AlphaTo(255);
			picBacks[currentBack].EndTransform();
			RootElement.SetVisible("main", false);
			return true;
		}

		private void Unit_MouseClick(object sender, EventArgs e)
		{
			if (pageToShow < 0)
			{
				Element element = (Element)sender;
				int pageIndex = (int)element.Value;
				if (Opens[(pageIndex + 3) % 4])
				{
					GameManager.Data.LoadAndPlaySystemSound((DateTime.Now.Month == 4 && DateTime.Now.Day == 1) ? "sysse.ogg" : "se_enter.ogg");
					pageToShow = (pageIndex + 3) % 4;
					eleScreen.ElementPicture = picBacks[currentBack];
					picBacks[currentBack].ResetTime();
					picBacks[currentBack].Alpha = 255;
					eleScreen.SetVisible("main", false);
					eleScreen.ElementPicture.BeginTransform(1f);
					eleScreen.ElementPicture.AlphaTo(0);
					eleScreen.ElementPicture.EndTransform();
				}
			}
		}

		private void Unit_MouseEnter(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			for (int i = 0; i < 4; i++)
			{
				picSelects[i].BeginTransform(0.5f);
				picSelects[i].PositionTo(new Point(element.Position.X + 180, element.Position.Y + 40));
				picSelects[i].ScaleTo(new SizeF(1f, 1f), new SizeF(1.1f, 1.1f), 0.5f);
				picSelects[i].EndTransform();
			}
			int pageIndex = (int)element.Value;
			if (!Opens[(pageIndex + 3) % 4])
			{
				pageIndex = 0;
			}
			if (pageIndex != currentBack)
			{
				eleScreen.ElementPicture = picBacks[currentBack];
				picBacks[currentBack].ResetTime();
				picBacks[currentBack].Alpha = 255;
				eleScreen["back"].ElementPicture = picBacks[pageIndex];
				picBacks[pageIndex].Alpha = 0;
				picBacks[pageIndex].BeginTransform(0.5f);
				picBacks[pageIndex].AlphaTo(255);
				picBacks[pageIndex].EndTransform();
				currentBack = pageIndex;
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
			if (currentBack != 0)
			{
				eleScreen.ElementPicture = picBacks[currentBack];
				picBacks[currentBack].ResetTime();
				picBacks[currentBack].Alpha = 255;
				eleScreen["back"].ElementPicture = picBacks[0];
				picBacks[0].Alpha = 0;
				picBacks[0].BeginTransform(0.5f);
				picBacks[0].AlphaTo(255);
				picBacks[0].EndTransform();
				currentBack = 0;
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
			return true;
		}

		public override void Update(float elaspedTime)
		{
			base.Update(elaspedTime);
			if (picBacks[currentBack].Time == 0f && eleScreen.ElementPicture != picBacks[currentBack])
			{
				eleScreen["back"].ElementPicture = null;
				eleScreen.ElementPicture = picBacks[currentBack];
				picBacks[currentBack].Alpha = 255;
			}
			if (eleScreen.ElementPicture.Time == 0f && !eleScreen.GetVisible("main"))
			{
				if (eleScreen.ElementPicture.Alpha == 255)
				{
					eleScreen.SetVisible("main", true);
				}
				else if (pageToShow >= 0)
				{
					GameManager.GalCore.ShowScreen(new AgainAlbumScreen(pageToShow));
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
					eleScreen.ElementPicture = picBacks[currentBack];
					picBacks[currentBack].ResetTime();
					picBacks[currentBack].Alpha = 255;
					eleScreen.SetVisible("main", false);
					eleScreen.ElementPicture.BeginTransform(1f);
					eleScreen.ElementPicture.AlphaTo(0);
					eleScreen.ElementPicture.EndTransform();
				}
			}
			else
			{
				base.OnKeyPress(keyData);
			}
		}
	}
}
