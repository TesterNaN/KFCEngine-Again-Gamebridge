using System;
using System.Drawing;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Commands;

namespace KidFansChannel.Again.Commands
{
	public class AgainShowChapter : ShowChapter
	{
		protected void GameFont_FontChanged(object sender, EventArgs e)
		{
			if (Chapter != null && Chapter.Font != null)
			{
				Chapter.Font = ScriptCommand.GalCore.GameFont.GetFont(18);
			}
		}

		protected override void SetFontStyle(String2D chapter)
		{
			chapter.Font = ScriptCommand.GalCore.GameFont.GetFont(18);
			chapter.LineSize = new Size(625, 32);
			chapter.Location = new Point(75, 30);
			chapter.DrawCharCallback = DrawChar.DrawCharBorder;
		}
	}
}
