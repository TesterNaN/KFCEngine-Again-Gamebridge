using System;
using System.Drawing;
using System.Windows.Forms;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Screens
{
	internal class AgainMessageBoxScreen : MessageBoxScreen
	{
		private Picture picMain;

		private Picture picBar;

		public override bool DataReady
		{
			get
			{
				if (base.DataReady)
				{
					return true;
				}
				if (picBar != null && picBar.IsCreated && picMain != null && picMain.IsCreated)
				{
					picMain.Position = new Point(-5, -5);
					DataReady = true;
					return true;
				}
				return false;
			}
		}

		public AgainMessageBoxScreen(string text, MessageBoxButtons buttons, MessageBoxCallback callBack, object[] callBackArgs)
			: base(text, buttons, callBack, callBackArgs)
		{
		}

		protected override void SetButtonFontStyle(String2D button)
		{
			button.Font = GameManager.GalCore.GameFont.GetFont(20);
			button.Location = Point.Empty;
			button.LineSize = new Size(160, 40);
			button.LineNum = 1;
		}

		protected override void SetTextFontStyle(String2D text)
		{
			text.Font = GameManager.GalCore.GameFont.GetFont(18);
			text.Location = Point.Empty;
			text.LineSize = new Size(350, 32);
			text.LineNum = 3;
		}

		public override void InitData()
		{
			base.InitData();
			picMain = GameManager.Data.LoadPicture("system", "messagebox.png");
			picBar = GameManager.Data.LoadPicture("system", "bar.png");
		}

		public override void FinalData()
		{
			base.FinalData();
			picMain.Dispose();
			picBar.Dispose();
		}

		public override bool InitScreen()
		{
			if (!base.InitScreen())
			{
				return false;
			}
			eleScreen.ElementPicture = picMain;
			eleScreen.Position = new Point(200, 225);
			int totalLine = eleScreen["text"].ElementString.TotalLine;
			eleScreen["text"].Position = new Point(25, 5 + (3 - totalLine) * 32 / 2);
			if (Buttons == MessageBoxButtons.OK)
			{
				eleScreen["ok"].Position = new Point(120, 105);
				eleScreen["ok"].Add(new Element(picBar), "bar", false);
				eleScreen["ok"]["bar"].Position = new Point(0, -5);
				eleScreen["ok"].Region = new Rectangle(0, 0, 160, 40);
				eleScreen["ok"].MouseEnter += Button_MouseEnter;
				eleScreen["ok"].MouseLeave += Button_MouseLeave;
			}
			else
			{
				eleScreen["ok"].Position = new Point(30, 105);
				eleScreen["ok"].Add(new Element(picBar), "bar", false);
				eleScreen["ok"]["bar"].Position = new Point(0, -5);
				eleScreen["ok"].Region = new Rectangle(0, 0, 160, 40);
				eleScreen["ok"].MouseEnter += Button_MouseEnter;
				eleScreen["ok"].MouseLeave += Button_MouseLeave;
				eleScreen["cancel"].Position = new Point(210, 105);
				eleScreen["cancel"].Add(new Element(picBar), "bar", false);
				eleScreen["cancel"]["bar"].Position = new Point(0, -5);
				eleScreen["cancel"].Region = new Rectangle(0, 0, 160, 40);
				eleScreen["cancel"].MouseEnter += Button_MouseEnter;
				eleScreen["cancel"].MouseLeave += Button_MouseLeave;
			}
			return true;
		}

		private void Button_MouseEnter(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			element.SetVisible("bar", true);
		}

		private void Button_MouseLeave(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			element.SetVisible("bar", false);
		}
	}
}
