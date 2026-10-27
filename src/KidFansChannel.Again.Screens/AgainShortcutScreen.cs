using System;
using System.Drawing;
using System.Windows.Forms;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Screens
{
	public class AgainShortcutScreen : ShortcutScreen
	{
		private Picture picSelectBase;

		private Picture[] picSelects = new Picture[4];

		private Picture picMain;

		private Picture picBack;

		private String2D[] strMenus = new String2D[14];

		private String2D strInfo;

		private String2D[] strCatalogues = new String2D[3];

		private Element eleScreen;

		private int[] ups = new int[14]
		{
			13, 13, 13, 13, 0, 1, 2, 3, 7, 4,
			5, 6, 8, 10
		};

		private int[] downs = new int[14]
		{
			4, 5, 6, 7, 9, 10, 11, 8, 12, 13,
			13, 13, 13, 1
		};

		private int[] lefts = new int[14]
		{
			3, 0, 1, 2, 7, 4, 5, 6, 6, 12,
			9, 10, 11, 13
		};

		private int[] rights = new int[14]
		{
			1, 2, 3, 0, 5, 6, 7, 4, 4, 10,
			11, 12, 9, 13
		};

		private int[] xs = new int[14]
		{
			50, 230, 410, 590, 50, 230, 410, 590, 590, 50,
			230, 410, 590, 240
		};

		private int[] ys = new int[14]
		{
			110, 110, 110, 110, 210, 210, 210, 185, 235, 310,
			310, 310, 310, 390
		};

		private string[] storys = new string[3] { "Inori", "Miyabi", "Rikarin" };

		public override bool DataReady
		{
			get
			{
				if (base.DataReady)
				{
					return true;
				}
				if (picSelectBase != null && picSelectBase.IsCreated && picMain != null && picMain.IsCreated && picBack != null && picBack.IsCreated)
				{
					for (int i = 0; i < 4; i++)
					{
						picSelects[i] = GameManager.Graphics.CreateSubPicture(picSelectBase, new Rectangle(i % 2 * 20, i / 2 * 20, 20, 20));
						picSelects[i].Center = new Point((i % 2 == 0) ? 85 : (-65), (i / 2 == 0) ? 25 : (-5));
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
			picMain = GameManager.Data.LoadPicture("system", "shortcut.png");
			picBack = GameManager.Data.LoadPicture("system", "back.png");
			Font2D font = GameManager.GalCore.GameFont.GetFont(20);
			for (int i = 0; i < 14; i++)
			{
				strMenus[i] = new String2D();
				strMenus[i].Font = font;
				strMenus[i].Location = new Point(0, 5);
				strMenus[i].LineSize = new Size((i == 13) ? 320 : 160, 35);
				strMenus[i].LineNum = 1;
				strMenus[i].DefaultColor = Color.Navy;
			}
			for (int j = 0; j < 3; j++)
			{
				strCatalogues[j] = new String2D();
				strCatalogues[j].Font = font;
				strCatalogues[j].Location = new Point(0, 5);
				strCatalogues[j].LineSize = new Size(600, 35);
				strCatalogues[j].LineNum = 1;
				strCatalogues[j].DefaultColor = Color.Navy;
			}
			strInfo = new String2D();
			strInfo.Font = font;
			strInfo.Location = Point.Empty;
			strInfo.LineSize = new Size(700, 35);
			strInfo.LineNum = 2;
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
			for (int j = 0; j < 3; j++)
			{
				Element element2 = new Element(strCatalogues[j]);
				element2.Position = new Point(100, 70 + j * 100);
				strCatalogues[j].AddString(GameManager.GameScript.GetStoryName(storys[j]), ContentAlignment.TopCenter);
				element.Add(element2, "catalogue" + j);
			}
			for (int k = 0; k < 14; k++)
			{
				Element element3 = new Element(strMenus[k]);
				element3.Position = new Point(xs[k], ys[k]);
				element3.Region = new Rectangle(0, 0, (k == 13) ? 320 : 160, 40);
				element3.Value = k;
				strMenus[k].ClearString();
				if (Opens[k])
				{
					strMenus[k].AddString(Names[k], ContentAlignment.TopCenter);
				}
				else
				{
					string text = string.Empty;
					for (int l = 0; l < Names[k].Length; l++)
					{
						text += "?";
					}
					strMenus[k].AddString(text, ContentAlignment.TopCenter);
				}
				element.Add(element3, k.ToString("00"));
			}
			Element element4 = new Element(strInfo);
			element4.Position = new Point(50, 485);
			element.Add(element4, "info");
			for (int m = 0; m < 14; m++)
			{
				element[m.ToString("00")].Up = element[ups[m].ToString("00")];
				element[m.ToString("00")].Down = element[downs[m].ToString("00")];
				element[m.ToString("00")].Left = element[lefts[m].ToString("00")];
				element[m.ToString("00")].Right = element[rights[m].ToString("00")];
			}
			Element childDown = (element.ChildRight = element["00"]);
			element.ChildDown = childDown;
			Element childUp = (element.ChildLeft = element["13"]);
			element.ChildUp = childUp;
			for (int n = 0; n < 14; n++)
			{
				element[n.ToString("00")].MouseClick += Unit_MouseClick;
				element[n.ToString("00")].MouseEnter += Unit_MouseEnter;
				element[n.ToString("00")].MouseLeave += Unit_MouseLeave;
			}
			picBack.Alpha = 0;
			picBack.BeginTransform(0.5f);
			picBack.AlphaTo(255);
			picBack.EndTransform();
			RootElement.SetVisible("main", false);
			return true;
		}

		private void Unit_MouseClick(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			int shortcutId = (int)element.Value;
			if (Opens[shortcutId])
			{
				GameManager.Data.LoadAndPlaySystemSound((DateTime.Now.Month == 4 && DateTime.Now.Day == 1) ? "sysse.ogg" : "se_enter.ogg");
				StartShortcut(shortcutId);
			}
		}

		private void Unit_MouseEnter(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			for (int i = 0; i < 4; i++)
			{
				picSelects[i].BeginTransform(0.5f);
				if ((int)element.Value == 13)
				{
					picSelects[i].PositionTo(new Point((i % 2 == 0) ? (element.Position.X + 80) : (element.Position.X + 240), element.Position.Y + 20));
				}
				else
				{
					picSelects[i].PositionTo(new Point(element.Position.X + 80, element.Position.Y + 20));
				}
				picSelects[i].ScaleTo(new SizeF(1f, 1f), new SizeF(1.1f, 1.1f), 0.5f);
				picSelects[i].EndTransform();
			}
			int shortcutId = (int)element.Value;
			strInfo.ClearString();
			if (Opens[shortcutId])
			{
				strInfo.AddString(Infos[shortcutId]);
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
