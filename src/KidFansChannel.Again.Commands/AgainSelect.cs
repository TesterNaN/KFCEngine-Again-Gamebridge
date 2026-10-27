using System;
using System.Drawing;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Commands;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Commands
{
	public class AgainSelect : Select
	{
		private static int currentColorIndex;

		private static string currentColor;

		private static Picture[,] selectBacks = new Picture[2, 4];

		private static Picture selectBar;

		public AgainSelect()
		{
			Select.SelectCenterY = 250;
		}

		protected void GameFont_FontChanged(object sender, EventArgs e)
		{
			foreach (String2D itemString in ItemStrings)
			{
				if (itemString != null && itemString.Font != null)
				{
					itemString.Font = ScriptCommand.GalCore.GameFont.GetFont(20);
				}
			}
		}

		protected override void SetBackStyle()
		{
		}

		protected override void SetFontStyle(String2D itemString, bool chosen)
		{
			itemString.Font = ScriptCommand.GalCore.GameFont.GetFont(20);
			itemString.LineSize = new Size(675, 38);
			itemString.LineNum = 1;
			if (ScriptCommand.Options.SelectedShow && chosen)
			{
				itemString.DefaultColor = Color.Aqua;
			}
		}

		public override void InitStaticData()
		{
			ScriptCommand.GalCore.GameFont.FontChanged += GameFont_FontChanged;
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					selectBacks[i, j] = ScriptCommand.Data.LoadPictureInstant("system", "select" + i + "_back" + j + ".png");
					selectBacks[i, j].Center = new Point(280, i * 20 + 60);
					selectBacks[i, j].Position = new Point(400, Select.SelectCenterY);
					selectBacks[i, j].Alpha = 0;
				}
			}
			selectBar = ScriptCommand.Data.LoadPictureInstant("system", "select_bar.png");
			selectBar.Center = new Point(80, 20);
			selectBar.X = 400;
			currentColor = ScriptCommand.Options.DialogueColor;
			GetColorIndex();
		}

		public override void FinalStaticData()
		{
			for (int i = 0; i < 2; i++)
			{
				for (int j = 0; j < 4; j++)
				{
					selectBacks[i, j].Dispose();
					selectBacks[i, j] = null;
				}
			}
			selectBar.Dispose();
			selectBar = null;
		}

		protected override void Show()
		{
			base.Show();
			int rowOffset = ((ItemStrings.Count != 2) ? 1 : 0);
			selectBacks[rowOffset, 0].BeginTransform(0.1f);
			selectBacks[rowOffset, 0].AlphaTo((int)(ScriptCommand.Options.DialogueAlpha * 255f));
			selectBacks[rowOffset, 0].EndTransform();
			for (int i = 1; i < 4; i++)
			{
				selectBacks[rowOffset, i].BeginTransform(0.1f);
				selectBacks[rowOffset, i].AlphaTo(255);
				selectBacks[rowOffset, i].EndTransform();
			}
		}

		protected override void Hide()
		{
			base.Hide();
			Picture[,] backs = selectBacks;
			foreach (Picture picture in backs)
			{
				picture.BeginTransform(0.1f);
				picture.AlphaTo(0);
				picture.EndTransform();
			}
		}

		protected override void UpdateBack(float elaspedTime)
		{
			Picture[,] backs = selectBacks;
			foreach (Picture picture in backs)
			{
				picture.Update(elaspedTime);
			}
		}

		protected override void UpdateItem(float elaspedTime)
		{
			if (currentSelected == -1)
			{
				selectBar.ScaleX = 1f;
			}
			else
			{
				selectBar.ScaleX = (float)(ItemStrings[currentSelected].Width + 80) / (float)selectBar.Width;
			}
		}

		protected override bool CheckShown()
		{
			int rowOffset = ((ItemStrings.Count != 2) ? 1 : 0);
			if (selectBacks[rowOffset, 0].Time == 0f)
			{
				return selectBacks[rowOffset, 0].Alpha == (int)(ScriptCommand.Options.DialogueAlpha * 255f);
			}
			return false;
		}

		protected override bool CheckHiden()
		{
			int rowOffset = ((ItemStrings.Count != 2) ? 1 : 0);
			if (selectBacks[rowOffset, 0].Time == 0f)
			{
				return selectBacks[rowOffset, 0].Alpha == 0;
			}
			return false;
		}

		protected override void DrawBackGround()
		{
			int rowOffset = ((ItemStrings.Count != 2) ? 1 : 0);
			if (ScriptCommand.Options.DialogueColor != currentColor)
			{
				GetColorIndex();
			}
			ScriptCommand.Graphics.DrawPicture(selectBacks[rowOffset, 0]);
			ScriptCommand.Graphics.DrawPicture(selectBacks[rowOffset, currentColorIndex]);
		}

		protected override void DrawItem()
		{
			if (currentSelected >= 0)
			{
				selectBar.Y = ItemStrings[currentSelected].Location.Y + 15;
			}
			ScriptCommand.Graphics.DrawPicture(selectBar);
		}

		public override void OnItemMove(Point location)
		{
			base.OnItemMove(location);
		}

		private void GetColorIndex()
		{
			currentColor = ScriptCommand.Options.DialogueColor;
			if (currentColor == "Blue")
			{
				currentColorIndex = 1;
				return;
			}
			if (currentColor == "Green")
			{
				currentColorIndex = 2;
				return;
			}
			if (currentColor == "Pink")
			{
				currentColorIndex = 3;
				return;
			}
			throw new Exception("Unbelievable!");
		}

		public override void InitCommand()
		{
			base.InitCommand();
			if (ItemStrings.Count > 0)
			{
				GameManager.Data.LoadAndPlaySystemSound((DateTime.Now.Month == 4 && DateTime.Now.Day == 1) ? "sysse.ogg" : "se_notice.ogg");
			}
		}
	}
}
