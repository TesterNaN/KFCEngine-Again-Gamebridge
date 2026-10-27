using System;
using System.Drawing;
using System.Windows.Forms;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Screens
{
	public class AgainOptionsScreen : OptionsScreen
	{
		private Picture picMain;

		private Picture picBack;

		private Picture picSelectBase;

		private Picture[] picSelects = new Picture[2];

		private Picture picIconBase;

		private Picture[] picIcons;

		private Element eleScreen;

		private Picture picBar;

		private String2D strIntro;

		private String2D[][] strItems;

		private AgainScrollBarElement scrollBar = new AgainScrollBarElement();

		private bool back;

		private int optionsStart;

		private int optionsLength = 7;

		public override bool DataReady
		{
			get
			{
				if (base.DataReady)
				{
					return true;
				}
				if (picMain != null && picMain.IsCreated && picBack != null && picBack.IsCreated && picSelectBase != null && picSelectBase.IsCreated && picBar != null && picBar.IsCreated && picIconBase != null && picIconBase.IsCreated && scrollBar.DataReady)
				{
					picSelects[0] = GameManager.Graphics.CreateSubPicture(picSelectBase, new Rectangle(0, 0, 20, 40));
					picSelects[1] = GameManager.Graphics.CreateSubPicture(picSelectBase, new Rectangle(20, 0, 20, 40));
					picSelects[0].CenterX = 20;
					picIcons = new Picture[ItemTitles.Count];
					for (int i = 0; i < ItemTitles.Count; i++)
					{
						picIcons[i] = GameManager.Graphics.CreateSubPicture(picIconBase, new Rectangle(ItemIcon[i] % 4 * 40, ItemIcon[i] / 4 * 40, 40, 40));
					}
					DataReady = true;
					return true;
				}
				return false;
			}
		}

		public AgainOptionsScreen(bool back)
		{
			this.back = back;
		}

		public override void InitData()
		{
			base.InitData();
			picMain = GameManager.Data.LoadPicture("system", "options.png");
			picSelectBase = GameManager.Data.LoadPicture("system", "select.png");
			picBar = GameManager.Data.LoadPicture("system", "bar.png");
			picIconBase = GameManager.Data.LoadPicture("system", "options_icon.png");
			if (back)
			{
				picBack = GameManager.Data.LoadPicture("system", "back.png");
			}
			else
			{
				picBack = GameManager.Graphics.CreatePicture(Color.FromArgb(128, Color.Black));
			}
			Font2D font = GameManager.GalCore.GameFont.GetFont(18);
			Font2D font2 = GameManager.GalCore.GameFont.GetFont(20);
			strIntro = new String2D();
			strIntro.Font = font;
			strIntro.Location = new Point(0, 0);
			strIntro.LineSize = new Size(700, 40);
			strIntro.LineNum = 4;
			strItems = new String2D[ItemValues.Count][];
			for (int i = 0; i < ItemValues.Count; i++)
			{
				strItems[i] = new String2D[ItemValues[i].Count];
				for (int j = 0; j < ItemValues[i].Count; j++)
				{
					strItems[i][j] = new String2D();
					strItems[i][j].Font = font2;
					strItems[i][j].Location = new Point(0, 0);
					strItems[i][j].LineSize = new Size(600 / ItemValues[i].Count, 40);
					strItems[i][j].LineNum = 1;
					strItems[i][j].AddString(ItemValues[i][j], ContentAlignment.TopCenter);
				}
			}
			scrollBar.InitData();
		}

		public override void FinalData()
		{
			base.FinalData();
			picMain.Dispose();
			picBack.Dispose();
			for (int i = 0; i < 2; i++)
			{
				picSelects[i].Dispose();
			}
			picSelectBase.Dispose();
			for (int j = 0; j < ItemTitles.Count; j++)
			{
				picIcons[j].Dispose();
			}
			picIconBase.Dispose();
			picBar.Dispose();
			scrollBar.FinalData();
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
			Element element2 = new Element(strIntro);
			element2.Position = new Point(50, 400);
			eleScreen["main"].Add(element2, "intro");
			for (int i = 0; i < ItemValues.Count; i++)
			{
				for (int j = 0; j < strItems[i].Length; j++)
				{
					Element element3 = new Element(strItems[i][j]);
					int slotWidth = 600 / strItems[i].Length;
					element3.Position = new Point(150 + slotWidth * j, 80 + 40 * i);
					element3.Region = new Rectangle((slotWidth - strItems[i][j].Width) / 2, 0, strItems[i][j].Width, 40);
					Element element4 = new Element(picBar);
					element4.Position = new Point((slotWidth - picBar.Width) / 2, 0);
					element3.Add(element4, "bar", false);
					Element element5 = new Element(picSelects[0]);
					element5.Position = new Point(element3.Region.Left, 0);
					element3.Add(element5, "left", false);
					Element element6 = new Element(picSelects[1]);
					element6.Position = new Point(element3.Region.Right, 0);
					element3.Add(element6, "right", false);
					eleScreen["main"].Add(element3, i.ToString("00") + j);
				}
				Element element7 = new Element(picIcons[i]);
				element7.Position = new Point(80, 80 + 40 * i);
				eleScreen["main"].Add(element7, "icon" + i.ToString("00"));
			}
			for (int k = 0; k < ItemValues.Count; k++)
			{
				for (int l = 0; l < strItems[k].Length; l++)
				{
					if (k < ItemValues.Count - 1)
					{
						int downTarget = (int)((double)((float)l * (float)(strItems[k + 1].Length - 1) / (float)(strItems[k].Length - 1)) + 0.5);
						eleScreen["main"][k.ToString("00") + l].Down = eleScreen["main"][(k + 1).ToString("00") + downTarget];
					}
					if (k > 0)
					{
						int upTarget = (int)((double)((float)l * (float)(strItems[k - 1].Length - 1) / (float)(strItems[k].Length - 1)) + 0.5);
						eleScreen["main"][k.ToString("00") + l].Up = eleScreen["main"][(k - 1).ToString("00") + upTarget];
					}
					eleScreen["main"][k.ToString("00") + l].Right = eleScreen["main"][k.ToString("00") + (l + 1) % strItems[k].Length];
					eleScreen["main"][k.ToString("00") + (l + 1) % strItems[k].Length].Left = eleScreen["main"][k.ToString("00") + l];
					eleScreen["main"][k.ToString("00") + l].Value = (k << 8) + l;
					eleScreen["main"][k.ToString("00") + l].MouseEnter += OptionsValue_MouseEnter;
					eleScreen["main"][k.ToString("00") + l].MouseLeave += OptionsValue_MouseLeave;
					eleScreen["main"][k.ToString("00") + l].MouseClick += OptionsValue_MouseClick;
				}
			}
			Element element8 = eleScreen["main"];
			Element childUp = (eleScreen["main"].ChildLeft = eleScreen["main"][(ItemValues.Count - 1).ToString("00") + (strItems[ItemValues.Count - 1].Length - 1)]);
			element8.ChildUp = childUp;
			Element element10 = eleScreen["main"];
			Element childDown = (eleScreen["main"].ChildRight = eleScreen["main"]["000"]);
			element10.ChildDown = childDown;
			scrollBar.Init();
			scrollBar.Position = new Point(760, 70);
			scrollBar.Size = 300;
			element.Add(scrollBar, "scrollbar");
			scrollBar.Length = (float)optionsLength / (float)ItemTitles.Count;
			scrollBar.ValueChanged += ScrollBar_ValueChanged;
			UpdateOptions();
			eleScreen["main"].Focused = true;
			FocusedElement = eleScreen["main"];
			RootElement = eleScreen;
			InitValue();
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

		/// <summary>拖动滚动条时按新的起始项刷新选项列表。</summary>
		private void ScrollBar_ValueChanged(object sender, EventArgs e)
		{
			int previousStart = optionsStart;
			optionsStart = (int)(((ScrollBarElement)sender).Start * (float)ItemTitles.Count + 0.5f);
			if (optionsStart < 0)
			{
				optionsStart = 0;
			}
			if (optionsStart > ItemTitles.Count - optionsLength)
			{
				optionsStart = ItemTitles.Count - optionsLength;
				if (optionsStart < 0)
				{
					optionsStart = 0;
				}
			}
			if (previousStart != optionsStart)
			{
				eleScreen["main"].ClearSelected();
				UpdateOptions();
			}
		}

		private void InitValue()
		{
			for (int i = 0; i < ItemTitles.Count; i++)
			{
				int optionsValue = GetOptionsValue(i);
				if (optionsValue != -1)
				{
					UpdateValue(i, optionsValue);
				}
			}
		}

		private void UpdateValue(int index, int value)
		{
			eleScreen["main"][index.ToString("00") + value].SetVisible("left", true);
			eleScreen["main"][index.ToString("00") + value].SetVisible("right", true);
		}

		private void OptionsValue_MouseEnter(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			int slotWidth = (int)element.Value >> 8;
			element.SetVisible("bar", true);
			if (slotWidth < optionsStart)
			{
				optionsStart = slotWidth;
				UpdateOptions();
			}
			else if (slotWidth >= optionsStart + optionsLength)
			{
				optionsStart = slotWidth - optionsLength + 1;
				UpdateOptions();
			}
			strIntro.ClearString();
			strIntro.AddString(ItemTitles[slotWidth] + "\n", ContentAlignment.TopCenter);
			strIntro.AddXmlString(ItemIntros[slotWidth], new NodeInfo(), ContentAlignment.TopLeft);
		}

		private void OptionsValue_MouseLeave(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			element.SetVisible("bar", false);
		}

		private void OptionsValue_MouseClick(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			int slotWidth = (int)element.Value >> 8;
			int value = (int)element.Value & 0xF;
			for (int i = 0; i < strItems[slotWidth].Length; i++)
			{
				eleScreen["main"][slotWidth.ToString("00") + i].SetVisible("left", false);
				eleScreen["main"][slotWidth.ToString("00") + i].SetVisible("right", false);
			}
			UpdateValue(slotWidth, value);
			SetOptionsValue(slotWidth, value);
		}

		public override bool FinalScreen()
		{
			if (!base.FinalScreen())
			{
				return false;
			}
			scrollBar.Final();
			for (int i = 0; i < eleScreen["main"].Count; i++)
			{
				eleScreen["main"][i].Dispose();
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

		public override void OnMouseWheel(int delta)
		{
			base.OnMouseWheel(delta);
			eleScreen["main"].ClearSelected();
			if (delta > 0)
			{
				optionsStart -= delta / 120;
				if (optionsStart < 0)
				{
					optionsStart = 0;
				}
				UpdateOptions();
			}
			else if (delta < 0)
			{
				optionsStart -= delta / 120;
				if (optionsStart > ItemTitles.Count - optionsLength)
				{
					optionsStart = ItemTitles.Count - optionsLength;
				}
				UpdateOptions();
			}
		}

		public void UpdateOptions()
		{
			for (int i = 0; i < optionsStart; i++)
			{
				for (int j = 0; j < ItemValues[i].Count; j++)
				{
					eleScreen["main"].SetVisible(i.ToString("00") + j, false);
				}
				eleScreen["main"].SetVisible("icon" + i.ToString("00"), false);
			}
			for (int k = 0; k < optionsLength; k++)
			{
				for (int l = 0; l < ItemValues[k + optionsStart].Count; l++)
				{
					eleScreen["main"].SetVisible((k + optionsStart).ToString("00") + l, true);
					Element element = eleScreen["main"][(k + optionsStart).ToString("00") + l];
					element.Position = new Point(element.Position.X, 80 + 40 * k);
				}
				eleScreen["main"].SetVisible("icon" + (k + optionsStart).ToString("00"), true);
				Element element2 = eleScreen["main"]["icon" + (k + optionsStart).ToString("00")];
				element2.Position = new Point(element2.Position.X, 80 + 40 * k);
			}
			for (int m = optionsStart + optionsLength; m < ItemTitles.Count; m++)
			{
				for (int n = 0; n < ItemValues[m].Count; n++)
				{
					eleScreen["main"].SetVisible(m.ToString("00") + n, false);
				}
				eleScreen["main"].SetVisible("icon" + m.ToString("00"), false);
			}
			scrollBar.Start = (float)optionsStart / (float)ItemTitles.Count;
		}
	}
}
