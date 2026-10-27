using System;
using System.Drawing;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Commands;

namespace KidFansChannel.Again.Commands
{
	internal class AgainParagraph : Paragraph
	{
		protected void GameFont_FontChanged(object sender, EventArgs e)
		{
			if (Paragraph.ShownString != null && Paragraph.ShownString.Font != null)
			{
				Paragraph.ShownString.Font = ScriptCommand.GalCore.GameFont.GetFont(20);
			}
			if (Paragraph.CurrentString != null && Paragraph.CurrentString.Font != null)
			{
				Paragraph.CurrentString.Font = ScriptCommand.GalCore.GameFont.GetFont(20);
			}
		}

		protected override float GetSpeed()
		{
			switch (ScriptCommand.Options.TextSpeed)
			{
			case GameOptions.Level.High:
				return 1000f;
			case GameOptions.Level.Mid:
				return 500f;
			case GameOptions.Level.Low:
				return 250f;
			case GameOptions.Level.None:
				return 2500f;
			default:
				return 500f;
			}
		}

		public override void InitStaticData()
		{
			ScriptCommand.GalCore.GameFont.FontChanged += GameFont_FontChanged;
			Paragraph.ShownString = new String2D();
			Paragraph.CurrentString = new String2D();
			Paragraph.ShownString.Font = ScriptCommand.GalCore.GameFont.GetFont(20);
			Paragraph.ShownString.Location = new Point(40, 80);
			Paragraph.ShownString.LineSize = new Size(720, 38);
			Paragraph.ShownString.LineNum = 12;
			Paragraph.ShownString.DrawCharCallback = DrawChar.DrawCharBorder;
			Paragraph.CurrentString.Font = ScriptCommand.GalCore.GameFont.GetFont(20);
			Paragraph.CurrentString.Location = new Point(40, 80);
			Paragraph.CurrentString.LineSize = new Size(720, 38);
			Paragraph.CurrentString.LineNum = 12;
			Paragraph.CurrentString.DrawCharCallback = DrawChar.DrawCharBorder;
		}

		public override void FinalStaticData()
		{
			base.FinalCommand();
		}
	}
}
