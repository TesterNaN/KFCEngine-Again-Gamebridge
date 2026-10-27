using System.Drawing;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Commands;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Commands
{
	internal class AgainClick : Click
	{
		private float dY;

		private static Picture clickIconBase;

		private static SubPicture[] clickIcons = new SubPicture[8];

		private static AnimePicture clickIconAnime;

		public override void InitCommand()
		{
			base.InitCommand();
			dY = 0f;
			clickIconAnime.Position = new Point(0, 0);
			clickIconAnime.Position = ClickPosition;
			clickIconAnime.X -= 4;
			clickIconAnime.ResetFrame();
		}

		public override void FinalCommand()
		{
			base.FinalCommand();
		}

		public override void Update(double appTime, float elaspedTime)
		{
			base.Update(appTime, elaspedTime);
			dY += elaspedTime * 16f;
			while (dY >= 16f)
			{
				dY -= 16f;
			}
			if (dY < 4f)
			{
				clickIconAnime.Y = ClickPosition.Y - 6 + (int)(dY * 0.75f + 0.5f);
			}
			else if (dY >= 4f && dY < 12f)
			{
				clickIconAnime.Y = ClickPosition.Y - 6 - (int)(dY * 0.75f + 0.5f) + 6;
			}
			else
			{
				clickIconAnime.Y = ClickPosition.Y - 6 + (int)(dY * 0.75f + 0.5f) - 12;
			}
			clickIconAnime.Update(elaspedTime);
		}

		public override void Draw()
		{
			base.Draw();
			ScriptCommand.Graphics.DrawPicture(clickIconAnime);
		}

		public override void InitStaticData()
		{
			clickIconBase = ScriptCommand.GalCore.GameData.LoadPictureInstant("system", "click.png");
			for (int i = 0; i < 8; i++)
			{
				clickIcons[i] = ScriptCommand.GalCore.GameGraphics.CreateSubPicture(clickIconBase, new Rectangle((i & 3) * 40, (i >> 2) * 40, 40, 40));
			}
			clickIconAnime = ScriptCommand.GalCore.GameGraphics.CreateAnimePicture(new Size(40, 40));
			for (int j = 0; j < 4; j++)
			{
				clickIconAnime.AddFrame(clickIcons[j], 1f / 30f);
			}
			for (int k = 4; k < 8; k++)
			{
				clickIconAnime.AddFrame(clickIcons[k], 0.5f);
			}
		}

		public override void FinalStaticData()
		{
			base.FinalCommand();
			clickIconAnime.Dispose();
			for (int i = 0; i < 8; i++)
			{
				clickIcons[i].Dispose();
				clickIcons[i] = null;
			}
			clickIconBase.Dispose();
			clickIconBase = null;
		}

		protected override float GetAutoTime()
		{
			switch (ScriptCommand.Options.AutoSpeed)
			{
			case GameOptions.Level.High:
				return 0.5f;
			case GameOptions.Level.Mid:
				return 1f;
			case GameOptions.Level.Low:
				return 2f;
			default:
				return 1f;
			}
		}
	}
}
