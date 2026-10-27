using System;
using System.Drawing;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Commands;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Commands
{
	internal class AgainDialogue : Dialogue
	{
		private static Picture[] dialogueBacks = new Picture[4];

		private static Picture[] dialogueSpeakerBacks = new Picture[4];

		private static int currentColorIndex;

		private static string currentColor;

		private static float currentAlpha;

		private static int currentAlphaInt;

		private bool alphaChanged;

		protected void GameFont_FontChanged(object sender, EventArgs e)
		{
			if (Dialogue.String != null && Dialogue.String.Font != null)
			{
				Dialogue.String.Font = ScriptCommand.GalCore.GameFont.GetFont(20);
			}
			if (Dialogue.SpeakerString != null && Dialogue.SpeakerString.Font != null)
			{
				Dialogue.SpeakerString.Font = ScriptCommand.GalCore.GameFont.GetFont(20);
			}
		}

		protected override void ShowBack()
		{
			base.ShowBack();
			dialogueBacks[0].BeginTransform(0.1f);
			dialogueBacks[0].PositionTo(new Point(20, 440));
			dialogueBacks[0].AlphaTo(currentAlphaInt);
			dialogueBacks[0].EndTransform();
			for (int i = 1; i < 4; i++)
			{
				dialogueBacks[i].BeginTransform(0.1f);
				dialogueBacks[i].PositionTo(new Point(20, 440));
				dialogueBacks[i].AlphaTo(255);
				dialogueBacks[i].EndTransform();
			}
		}

		protected override void HideBack()
		{
			base.HideBack();
			Picture[] backs = dialogueBacks;
			foreach (Picture picture in backs)
			{
				picture.BeginTransform(0.1f);
				picture.PositionTo(new Point(20, 495));
				picture.AlphaTo(0);
				picture.EndTransform();
			}
		}

		protected override void ShowSpeakerBack()
		{
			base.ShowSpeakerBack();
			float scaleX;
			if (Dialogue.SpeakerString.Width <= 100)
			{
				scaleX = 1f;
			}
			else
			{
				scaleX = ((Dialogue.SpeakerString.Width > 200) ? 2f : 1.5f);
			}
			dialogueSpeakerBacks[0].Alpha = currentAlphaInt;
			for (int i = 1; i < 4; i++)
			{
				dialogueSpeakerBacks[i].Alpha = 255;
			}
			Picture[] backs = dialogueSpeakerBacks;
			foreach (Picture picture in backs)
			{
				picture.ScaleX = scaleX;
			}
		}

		protected override void ChangeSpeakerBack()
		{
			float scaleX;
			if (Dialogue.SpeakerString.Width <= 100)
			{
				scaleX = 1f;
			}
			else
			{
				scaleX = ((Dialogue.SpeakerString.Width > 200) ? 2f : 1.5f);
			}
			Picture[] backs = dialogueSpeakerBacks;
			foreach (Picture picture in backs)
			{
				picture.ScaleX = scaleX;
			}
		}

		protected override void HideSpeakerBack()
		{
			base.HideSpeakerBack();
			Picture[] backs = dialogueSpeakerBacks;
			foreach (Picture picture in backs)
			{
				picture.Alpha = 0;
			}
		}

		protected override void UpdateBack(float elaspedTime)
		{
			if (ScriptCommand.Options.DialogueAlpha != currentAlpha)
			{
				currentAlpha = ScriptCommand.Options.DialogueAlpha;
				currentAlphaInt = (int)(currentAlpha * 255f);
				dialogueBacks[0].Alpha = currentAlphaInt;
				alphaChanged = true;
			}
			Picture[] backs = dialogueBacks;
			foreach (Picture picture in backs)
			{
				picture.Update(elaspedTime);
			}
		}

		protected override void UpdateSpeakerBack(float elaspedTime)
		{
			if (alphaChanged)
			{
				dialogueSpeakerBacks[0].Alpha = currentAlphaInt;
				alphaChanged = false;
			}
			Picture[] backs = dialogueSpeakerBacks;
			foreach (Picture picture in backs)
			{
				picture.Update(elaspedTime);
			}
		}

		protected override bool CheckShown()
		{
			if (dialogueBacks[currentColorIndex].Time == 0f)
			{
				return dialogueBacks[currentColorIndex].Alpha == 255;
			}
			return false;
		}

		protected override bool CheckHiden()
		{
			if (dialogueBacks[currentColorIndex].Time == 0f)
			{
				return dialogueBacks[currentColorIndex].Alpha == 0;
			}
			return false;
		}

		protected override bool CheckSpeakerShown()
		{
			if (dialogueSpeakerBacks[currentColorIndex].Time == 0f)
			{
				return dialogueSpeakerBacks[currentColorIndex].Alpha == 255;
			}
			return false;
		}

		protected override bool CheckSpeakerHiden()
		{
			if (dialogueSpeakerBacks[currentColorIndex].Time == 0f)
			{
				return dialogueSpeakerBacks[currentColorIndex].Alpha == 0;
			}
			return false;
		}

		protected override void DrawBackGround()
		{
			ScriptCommand.Graphics.DrawPicture(dialogueBacks[0]);
			if (ScriptCommand.Options.DialogueColor != currentColor)
			{
				GetColorIndex();
			}
			ScriptCommand.Graphics.DrawPicture(dialogueBacks[currentColorIndex]);
		}

		protected override void DrawSpeakerBackGround()
		{
			float speakerScale;
			if (Dialogue.SpeakerString.Width <= 100)
			{
				speakerScale = 1f;
			}
			else
			{
				speakerScale = ((Dialogue.SpeakerString.Width > 200) ? 2f : 1.5f);
			}
			ScriptCommand.Graphics.DrawPicture(dialogueSpeakerBacks[0]);
			ScriptCommand.Graphics.DrawPicture(dialogueSpeakerBacks[currentColorIndex]);
			Dialogue.SpeakerString.Location = new Point((int)(-140f + 80f * speakerScale), 405);
		}

		public override void InitStaticData()
		{
			ScriptCommand.GalCore.GameFont.FontChanged += GameFont_FontChanged;
			Dialogue.String = new String2D();
			Dialogue.SpeakerString = new String2D();
			Dialogue.String.Font = ScriptCommand.GalCore.GameFont.GetFont(20);
			Dialogue.String.Location = new Point(40, 455);
			Dialogue.String.LineSize = new Size(720, 38);
			Dialogue.String.LineNum = 3;
			Dialogue.SpeakerString.Font = ScriptCommand.GalCore.GameFont.GetFont(20);
			Dialogue.SpeakerString.Location = new Point(20, 405);
			Dialogue.SpeakerString.LineSize = new Size(320, 38);
			Dialogue.SpeakerString.LineNum = 1;
			for (int i = 0; i < 4; i++)
			{
				dialogueBacks[i] = ScriptCommand.Data.LoadPictureInstant("system", "dialogue" + i + ".png");
				dialogueBacks[i].Center = new Point(0, 0);
				dialogueBacks[i].Position = new Point(20, 495);
				dialogueBacks[i].Alpha = 0;
				dialogueSpeakerBacks[i] = ScriptCommand.Data.LoadPictureInstant("system", "dialogue_speaker" + i + ".png");
				dialogueSpeakerBacks[i].Center = new Point(0, 0);
				dialogueSpeakerBacks[i].Position = new Point(20, 395);
				dialogueSpeakerBacks[i].Alpha = 0;
			}
			currentColor = ScriptCommand.Options.DialogueColor;
			GetColorIndex();
			currentAlpha = ScriptCommand.Options.DialogueAlpha;
			currentAlphaInt = (int)(currentAlpha * 255f);
		}

		public override void FinalStaticData()
		{
			base.FinalCommand();
			for (int i = 0; i < 4; i++)
			{
				dialogueBacks[i].Dispose();
				dialogueBacks[i] = null;
				dialogueSpeakerBacks[i].Dispose();
				dialogueSpeakerBacks[i] = null;
			}
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
	}
}
