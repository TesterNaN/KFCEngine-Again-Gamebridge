using System;
using System.Drawing;
using KidFansChannel.GalCore;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again
{
	public class AgainState : GameState
	{
		protected void GameFont_FontChanged(object sender, EventArgs e)
		{
			if (StateString != null && StateString.Font != null)
			{
				StateString.Font = GalCore.GameFont.GetFont(18);
			}
		}

		protected override void InitDrawString()
		{
			if (!(GalCore.GameGraphics.Resolution.StateArea == Rectangle.Empty))
			{
				if (StateString == null)
				{
					GalCore.GameFont.FontChanged += GameFont_FontChanged;
					StateString = new String2D();
					StateString.Font = GalCore.GameFont.GetFont(20);
					StateString.DefaultColor = Color.White;
				}
				RectangleF targetBounds = GalCore.GameGraphics.TargetBounds;
				Rectangle stateArea = GalCore.GameGraphics.Resolution.StateArea;
				StateString.Location = new Point((int)(((float)stateArea.X - targetBounds.X) / targetBounds.Width + 0.5f), (int)(((float)stateArea.Y - targetBounds.Y) / targetBounds.Height + 0.5f));
				StateString.LineSize = new Size((int)((float)stateArea.Width / targetBounds.Width + 0.5f), 32);
				StateString.LineNum = (int)((float)stateArea.Height / targetBounds.Height + 0.5f) / 32;
			}
		}

		public override bool DrawState()
		{
			if (!base.DrawState())
			{
				return false;
			}
			StateString.Canvas = GalCore.CurrentCanvas;
			StateString.ClearString();
			StateString.AddString("\n", ContentAlignment.TopCenter);
			StateString.AddString("~ " + ((Item.Chapter == null) ? "------" : Item.Chapter) + " ~\n\n", ContentAlignment.TopCenter);
			StateString.CutLine(0, 2);
			StateString.AddString(((Item.BgName == null) ? "------" : Item.BgName) + "\n\n", ContentAlignment.TopCenter);
			StateString.CutLine(0, 4);
			StateString.AddString(((Item.BgmName == null) ? "------" : Item.BgmName) + "\n\n", ContentAlignment.TopCenter);
			StateString.CutLine(0, 6);
			StateString.AddString(Item.ReadPercentage.ToString("P") + "\n", ContentAlignment.TopLeft);
			StateString.AddString(new TimeSpan((long)Item.CurrentTime * 10000000L).ToString() + "\n", ContentAlignment.TopLeft);
			StateString.AddString(new TimeSpan((long)Item.TotalTime * 10000000L).ToString() + "\n", ContentAlignment.TopLeft);
			StateString.DrawString();
			return true;
		}

		public override void GetSaveInfo(int index, out int icon)
		{
			base.GetSaveInfo(index, out icon);
			if (icon >= 6)
			{
				icon = GalCore.GameScript.QuickIcon;
			}
		}

		public override void GetSaveInfo(int index, out int icon, out string comment, out string level, out DateTime saveTime, out Picture snap)
		{
			base.GetSaveInfo(index, out icon, out comment, out level, out saveTime, out snap);
			if (icon >= 6)
			{
				icon = GalCore.GameScript.QuickIcon;
			}
		}
	}
}
