using System;
using System.Drawing;
using System.Windows.Forms;
using KidFansChannel.DataCore;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Screens
{
	public class AgainMusicScreen : MusicScreen
	{
		private Picture picDisc;

		private Picture picMain;

		private Picture picBack;

		private Picture picBar;

		private static int LineNum = 8;

		private String2D[] strNames = new String2D[LineNum];

		private String2D[] strTimes = new String2D[LineNum];

		private String2D strName;

		private String2D strTime;

		private String2D strInfo;

		private Element eleScreen;

		private AgainScrollBarElement scrollBar = new AgainScrollBarElement();

		private int currentPlayId = -1;

		private int showId = -1;

		private float playTime;

		private int currentStartId;

		public override bool DataReady
		{
			get
			{
				if (base.DataReady)
				{
					return true;
				}
				if (picDisc != null && picDisc.IsCreated && scrollBar.DataReady && picMain != null && picMain.IsCreated && picBack != null && picBack.IsCreated && picBar != null && picBar.IsCreated)
				{
					picDisc.Center = new Point(20, 20);
					DataReady = true;
					return true;
				}
				return false;
			}
		}

		public override void InitData()
		{
			base.InitData();
			picDisc = GameManager.Data.LoadPicture("system", "music_disc.png");
			picMain = GameManager.Data.LoadPicture("system", "music.png");
			picBar = GameManager.Data.LoadPicture("system", "bar.png");
			picBack = GameManager.Data.LoadPicture("system", "music_back.png");
			Font2D font = GameManager.GalCore.GameFont.GetFont(20);
			for (int i = 0; i < LineNum; i++)
			{
				strNames[i] = new String2D();
				strNames[i].Font = font;
				strNames[i].Location = Point.Empty;
				strNames[i].LineSize = new Size(600, 35);
				strNames[i].LineNum = 1;
			}
			for (int j = 0; j < LineNum; j++)
			{
				strTimes[j] = new String2D();
				strTimes[j].Font = font;
				strTimes[j].Location = Point.Empty;
				strTimes[j].LineSize = new Size(100, 35);
				strTimes[j].LineNum = 1;
			}
			strName = new String2D();
			strName.Font = font;
			strName.Location = Point.Empty;
			strName.LineSize = new Size(600, 35);
			strName.LineNum = 1;
			strTime = new String2D();
			strTime.Font = font;
			strTime.Location = Point.Empty;
			strTime.LineSize = new Size(100, 35);
			strTime.LineNum = 1;
			strInfo = new String2D();
			strInfo.Font = font;
			strInfo.Location = Point.Empty;
			strInfo.LineSize = new Size(400, 35);
			strInfo.LineNum = 2;
			scrollBar.InitData();
		}

		public override void FinalData()
		{
			base.FinalData();
			picBack.Dispose();
			picDisc.Dispose();
			picMain.Dispose();
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
			element.Focused = true;
			FocusedElement = element;
			RootElement = eleScreen;
			Element element2 = new Element(picDisc);
			element2.Position = new Point(45, 260);
			element.Add(element2, "disc", false);
			Element element3 = new Element(picBar);
			element3.Position = new Point(0, -5);
			picBar.ScaleX = 4.5f;
			for (int i = 0; i < LineNum; i++)
			{
				Element element4 = new Element(strNames[i]);
				element4.Position = new Point(40, 0);
				Element element5 = new Element(strTimes[i]);
				element5.Position = new Point(600, 0);
				Element element6 = new Element();
				element6.Add(element3, "bar", false);
				element6.Add(element4, "name", true);
				element6.Add(element5, "time", true);
				element6.Position = new Point(25, 245 + i * 40);
				element6.Region = new Rectangle(0, 0, 720, 35);
				element.Add(element6, i.ToString("00"));
			}
			Element element7 = new Element(strName);
			element7.Position = new Point(50, 90);
			element.Add(element7, "name");
			Element element8 = new Element(strTime);
			element8.Position = new Point(650, 90);
			element.Add(element8, "time");
			Element element9 = new Element(strInfo);
			element9.Position = new Point(100, 135);
			element.Add(element9, "info");
			scrollBar.Init();
			scrollBar.Position = new Point(760, 240);
			scrollBar.Size = 330;
			element.Add(scrollBar, "scrollbar");
			scrollBar.Length = (float)LineNum / (float)Names.Length;
			scrollBar.ValueChanged += ScrollBar_ValueChanged;
			for (int j = 0; j < LineNum - 1; j++)
			{
				element[j.ToString("00")].Down = element[(j + 1).ToString("00")];
				element[(j + 1).ToString("00")].Up = element[j.ToString("00")];
			}
			element.ChildDown = element["00"];
			element.ChildUp = element[(LineNum - 1).ToString("00")];
			element.Add(new Element(), "up");
			element.Add(new Element(), "down");
			element["00"].Up = element["up"];
			element[(LineNum - 1).ToString("00")].Down = element["down"];
			for (int k = 0; k < LineNum; k++)
			{
				element[k.ToString("00")].MouseClick += Unit_MouseClick;
				element[k.ToString("00")].MouseEnter += Unit_MouseEnter;
				element[k.ToString("00")].MouseLeave += Unit_MouseLeave;
				element[k.ToString("00")].KeyPress += Unit_KeyPress;
			}
			element["up"].MouseEnter += Up_MouseEnter;
			element["down"].MouseEnter += Down_MouseEnter;
			UpdateMusic();
			picBack.Alpha = 0;
			picBack.BeginTransform(0.5f);
			picBack.AlphaTo(255);
			picBack.EndTransform();
			RootElement.SetVisible("main", false);
			GameManager.Sound.StopBgm();
			return true;
		}

		/// <summary>拖动滚动条时按新的起点刷新曲目列表。</summary>
		private void ScrollBar_ValueChanged(object sender, EventArgs e)
		{
			currentStartId = (int)(((ScrollBarElement)sender).Start * (float)Names.Length + 0.5f);
			if (currentStartId < 0)
			{
				currentStartId = 0;
			}
			if (currentStartId > Names.Length - LineNum)
			{
				currentStartId = Names.Length - LineNum;
				if (currentStartId < 0)
				{
					currentStartId = 0;
				}
			}
			UpdateMusic();
		}

		private void Unit_MouseClick(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			int musicId = (int)element.Value;
			if (Opens[musicId])
			{
				if (currentPlayId == musicId)
				{
					StopMusic();
					currentPlayId = -1;
					eleScreen["main"].SetVisible("disc", false);
				}
				else
				{
					PlayMusic(Files[musicId]);
					currentPlayId = musicId;
					eleScreen["main"].SetVisible("disc", true);
					eleScreen["main"]["disc"].Position = new Point(45, 260 + (musicId - currentStartId) * 40);
					playTime = 0f;
				}
			}
		}

		private void Unit_MouseEnter(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			element.SetVisible("bar", true);
			int musicId = (int)element.Value;
			strName.ClearString();
			strTime.ClearString();
			strInfo.ClearString();
			if (Opens[musicId])
			{
				strName.AddString((musicId + 1).ToString("00") + ". " + Names[musicId]);
				strTime.AddString(Times[musicId], ContentAlignment.TopRight);
				strInfo.AddString(Infos[musicId]);
			}
			showId = musicId;
		}

		private void Unit_MouseLeave(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			element.SetVisible("bar", false);
			showId = -1;
		}

		private void Unit_KeyPress(object sender, KeyEventArgs e)
		{
			switch (e.KeyData)
			{
			case Keys.Prior:
				currentStartId -= LineNum;
				if (currentStartId < 0)
				{
					currentStartId = 0;
				}
				UpdateMusic();
				eleScreen["main"]["00"].Selected = true;
				break;
			case Keys.Next:
				currentStartId += LineNum;
				if (currentStartId > Names.Length - LineNum)
				{
					currentStartId = Names.Length - LineNum;
				}
				UpdateMusic();
				eleScreen["main"][(LineNum - 1).ToString("00")].Selected = true;
				break;
			}
		}

		private void Up_MouseEnter(object sender, EventArgs e)
		{
			if (currentStartId > 0)
			{
				currentStartId--;
			}
			eleScreen["main"]["00"].Selected = true;
			UpdateMusic();
			eleScreen["main"]["00"].Selected = true;
		}

		private void Down_MouseEnter(object sender, EventArgs e)
		{
			if (currentStartId < Names.Length - LineNum)
			{
				currentStartId++;
			}
			eleScreen["main"][(LineNum - 1).ToString("00")].Selected = true;
			UpdateMusic();
			eleScreen["main"][(LineNum - 1).ToString("00")].Selected = true;
		}

		public override void OnMouseWheel(int delta)
		{
			base.OnMouseWheel(delta);
			if (delta > 0)
			{
				currentStartId -= delta / 120;
				if (currentStartId < 0)
				{
					currentStartId = 0;
				}
				UpdateMusic();
			}
			else if (delta < 0)
			{
				currentStartId -= delta / 120;
				if (currentStartId > Names.Length - LineNum)
				{
					currentStartId = Names.Length - LineNum;
				}
				UpdateMusic();
			}
		}

		public override bool FinalScreen()
		{
			if (!base.FinalScreen())
			{
				return false;
			}
			eleScreen["main"]["00"]["bar"].Dispose();
			for (int i = 0; i < LineNum; i++)
			{
				eleScreen["main"][i.ToString("00")]["name"].Dispose();
				eleScreen["main"][i.ToString("00")]["time"].Dispose();
			}
			for (int j = 0; j < eleScreen["main"].Count; j++)
			{
				if (eleScreen["main"][j] != scrollBar)
				{
					eleScreen["main"][j].Dispose();
				}
			}
			scrollBar.Final();
			for (int k = 0; k < eleScreen.Count; k++)
			{
				eleScreen[k].Dispose();
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
			if (currentPlayId != -1)
			{
				if (!IsMusicing)
				{
					currentPlayId = -1;
					eleScreen["main"].SetVisible("disc", false);
				}
				else
				{
					playTime += elaspedTime;
					if (currentPlayId == showId)
					{
						int musicId = (int)playTime / 60;
						int seconds = (int)playTime % 60;
						strTime.ClearString();
						strTime.AddString(musicId + ":" + seconds.ToString("00"), ContentAlignment.TopRight);
					}
				}
			}
			picDisc.Rotate += elaspedTime * 4f;
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

		protected void UpdateMusic()
		{
			bool playing = false;
			for (int i = 0; i < LineNum; i++)
			{
				int musicId = i + currentStartId;
				strNames[i].ClearString();
				strTimes[i].ClearString();
				if (Opens[musicId])
				{
					strNames[i].AddString((musicId + 1).ToString("00") + ". " + Names[musicId]);
					strTimes[i].AddString(Times[musicId], ContentAlignment.TopRight);
				}
				else
				{
					for (int j = 0; j < Names[musicId].Length; j++)
					{
						strNames[i].AddString("?");
					}
				}
				eleScreen["main"][i.ToString("00")].Value = musicId;
				if (musicId == currentPlayId)
				{
					eleScreen["main"].SetVisible("disc", true);
					eleScreen["main"]["disc"].Position = new Point(45, 260 + i * 40);
					playing = true;
				}
			}
			scrollBar.Start = (float)currentStartId / (float)Names.Length;
			if (eleScreen["main"].SelectedChild != null)
			{
				int seconds = (int)eleScreen["main"].SelectedChild.Value;
				strName.ClearString();
				strTime.ClearString();
				strInfo.ClearString();
				if (Opens[seconds])
				{
					strName.AddString((seconds + 1).ToString("00") + ". " + Names[seconds]);
					strTime.AddString(Times[seconds], ContentAlignment.TopRight);
					strInfo.AddString(Infos[seconds]);
				}
				showId = seconds;
			}
			else
			{
				showId = -1;
			}
			if (!playing)
			{
				eleScreen["main"].SetVisible("disc", false);
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
