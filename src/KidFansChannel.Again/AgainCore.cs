using System;
using System.Drawing;
using System.Windows.Forms;
using KidFansChannel.Again.Screens;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Commands;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again
{
	public class AgainCore : KidFansChannel.GalCore.GalCore
	{
		private Picture picPauseBack;

		private bool? toPauseValue = null;

		private Picture picStateIconBase;

		private Picture[,] picStateIcons = new Picture[3, 8];

		private Picture[] picAnimeStateIcon = new Picture[3];

		private bool lastAutoOrQuick;

		private Picture picQuickSaveIcon;

		private String2D strComment;

		private Picture picCommentBack;

		public AgainCore()
			: base("MOAgainStr", "MOAgainIcon")
		{
		}

		public override void Initialize()
		{
			base.Initialize();
			picStateIconBase = GameData.LoadPictureInstant("system", "state_icon.png");
			picQuickSaveIcon = GameData.LoadPictureInstant("system", "quicksave_icon.png");
			picCommentBack = GameData.LoadPictureInstant("system", "comment.png");
			for (int i = 0; i < 3; i++)
			{
				picAnimeStateIcon[i] = GameGraphics.CreateAnimePicture(new Size(40, 40));
				picAnimeStateIcon[i].Position = new Point(30, 30);
				for (int j = 0; j < 8; j++)
				{
					picStateIcons[i, j] = GameGraphics.CreateSubPicture(picStateIconBase, new Rectangle(j * 40, i * 40, 40, 40));
					((AnimePicture)picAnimeStateIcon[i]).AddFrame(picStateIcons[i, j], 0.125f);
				}
			}
			picAnimeStateIcon[2].Alpha = 0;
			picQuickSaveIcon.Position = new Point(640, 30);
			picQuickSaveIcon.Alpha = 0;
			strComment = new String2D();
			strComment.Font = GameFont.GetFont(20);
			GameGraphics.FullscreenToggled += GameGraphics_FullscreenToggled;
			// 初始布局交给同一个处理函数，避免两处逻辑走样
			GameGraphics_FullscreenToggled(this, EventArgs.Empty);
		}

		/// <summary>按当前分辨率把对话框摆到 CommentArea 位置。全屏切换后要重算。</summary>
		private void LayoutCommentToResolution()
		{
			picCommentBack.ScaleX = (float)GameGraphics.Resolution.CommentArea.Width / GameGraphics.TargetBounds.Width / 40f;
			picCommentBack.ScaleY = (float)GameGraphics.Resolution.CommentArea.Height / GameGraphics.TargetBounds.Height / 40f;
			picCommentBack.X = (int)(((float)GameGraphics.Resolution.CommentArea.X - GameGraphics.TargetBounds.X) / GameGraphics.TargetBounds.Width + 0.5f);
			picCommentBack.Y = (int)(((float)GameGraphics.Resolution.CommentArea.Y - GameGraphics.TargetBounds.Y) / GameGraphics.TargetBounds.Height + 0.5f);
			strComment.Location = new Point((int)(((float)GameGraphics.Resolution.CommentArea.X - GameGraphics.TargetBounds.X) / GameGraphics.TargetBounds.Width + 0.5f), (int)(((float)GameGraphics.Resolution.CommentArea.Y - GameGraphics.TargetBounds.Y) / GameGraphics.TargetBounds.Height + 0.5f) + 2);
			strComment.LineSize = new Size((int)((float)GameGraphics.Resolution.CommentArea.Width / GameGraphics.TargetBounds.Width + 0.5f), 40);
			strComment.LineNum = (int)((float)GameGraphics.Resolution.CommentArea.Height / GameGraphics.TargetBounds.Height / 40f);
		}

		/// <summary>全屏/窗口切换后重新摆对话框。</summary>
		private void GameGraphics_FullscreenToggled(object sender, EventArgs e)
		{
			if (GameGraphics.IsWindowed)
			{
				picCommentBack.ScaleX = (float)GameGraphics.WindowComment.Width / 40f;
				picCommentBack.ScaleY = (float)GameGraphics.WindowComment.Height / 40f;
				picCommentBack.X = GameGraphics.WindowComment.X;
				picCommentBack.Y = GameGraphics.WindowComment.Y;
				strComment.Location = new Point(GameGraphics.WindowComment.X, GameGraphics.WindowComment.Y + 2);
				strComment.LineSize = new Size(GameGraphics.WindowComment.Width, 40);
				strComment.LineNum = GameGraphics.WindowComment.Height / 40;
			}
			else
			{
				LayoutCommentToResolution();
			}
		}

		protected override void ChangePauseState(bool value)
		{
			if (!(picPauseBack.Time > 0f) || !toPauseValue.HasValue || toPauseValue.Value != value)
			{
				if (value)
				{
					Pause = true;
					picPauseBack.Alpha = 0;
					picPauseBack.BeginTransform(0.5f);
					picPauseBack.AlphaTo(128);
					picPauseBack.EndTransform();
					toPauseValue = true;
					GameSound.StopVoice();
				}
				else
				{
					picPauseBack.Alpha = 128;
					picPauseBack.BeginTransform(0.5f);
					picPauseBack.AlphaTo(0);
					picPauseBack.EndTransform();
					toPauseValue = false;
				}
			}
		}

		protected override bool UpdatePause(double appTime, float elaspedTime)
		{
			base.UpdatePause(appTime, elaspedTime);
			picPauseBack.Update(elaspedTime);
			if (picPauseBack.Time == 0f)
			{
				toPauseValue = null;
				if (picPauseBack.Alpha == 0)
				{
					Pause = false;
				}
			}
			return picPauseBack.Alpha == 128;
		}

		protected override void DrawPause()
		{
			base.DrawPause();
			CurrentCanvas.DrawPicture(picPauseBack);
		}

		protected override void PreparePause()
		{
			base.PreparePause();
			picPauseBack = GameGraphics.CreatePicture(Color.Black);
			picPauseBack.Size = BgDisplay.ScreenSize;
		}

		public override void ShowSaveLoadScreen(bool isSave)
		{
			if (!isSave || GameScript.CurrentGameScript.StoryName == "Inori" || GameScript.CurrentGameScript.StoryName == "Miyabi" || GameScript.CurrentGameScript.StoryName == "Rikarin")
			{
				GameScreen screen = new AgainSaveLoadScreen(isSave, GameScript.CurrentGameScript == null);
				ShowScreen(screen);
			}
		}

		public override void ShowMessageScreen()
		{
			GameScreen screen = new AgainMessageScreen();
			ShowScreen(screen);
		}

		public override void ShowPauseScreen()
		{
			GameScreen screen = new AgainPauseScreen();
			ShowScreen(screen);
		}

		public override void ShowOptionsScreen()
		{
			GameScreen screen = new AgainOptionsScreen(GameScript.CurrentGameScript == null);
			ShowScreen(screen);
		}

		public override void ShowTitleScreen()
		{
			GameScreen screen = new AgainTitleScreen();
			ShowScreen(screen);
		}

		public override void ShowEncyListScreen()
		{
			GameScreen screen = new AgainEncyListScreen(GameScript.CurrentGameScript == null);
			ShowScreen(screen);
		}

		public override void ShowEncyContentScreen(int index)
		{
			GameScreen screen = new AgainEncyContentScreen(index, GameScript.CurrentGameScript == null);
			ShowScreen(screen);
		}

		public override void ShowAlbumScreen()
		{
			GameScreen screen = new AgainAlbumListScreen();
			ShowScreen(screen);
		}

		public override void ShowMusicScreen()
		{
			GameScreen screen = new AgainMusicScreen();
			ShowScreen(screen);
		}

		public override void ShowMovieScreen()
		{
			GameScreen screen = new AgainMovieScreen();
			ShowScreen(screen);
		}

		public override void ShowClearListScreen()
		{
			GameScreen screen = new AgainClearListScreen();
			ShowScreen(screen);
		}

		public override void ShowShortcutScreen()
		{
			GameScreen screen = new AgainShortcutScreen();
			ShowScreen(screen);
		}

		public override void ShowLogoScreen()
		{
			GameScreen screen = new AgainLogoScreen();
			ShowScreen(screen);
		}

		protected override void UpdateStateIcon(float elapsedTime)
		{
			base.UpdateStateIcon(elapsedTime);
			if (!ToNormal)
			{
				switch (CurrentPlayMode)
				{
				case PlayMode.Auto:
					picAnimeStateIcon[0].Update(elapsedTime);
					if (!lastAutoOrQuick)
					{
						GameData.LoadAndPlaySystemSound((DateTime.Now.Month == 4 && DateTime.Now.Day == 1) ? "sysse.ogg" : "se_start.ogg");
						lastAutoOrQuick = true;
					}
					break;
				case PlayMode.Quick:
					picAnimeStateIcon[1].Update(elapsedTime);
					if (!lastAutoOrQuick)
					{
						GameData.LoadAndPlaySystemSound((DateTime.Now.Month == 4 && DateTime.Now.Day == 1) ? "sysse.ogg" : "se_start.ogg");
						lastAutoOrQuick = true;
					}
					break;
				default:
					if (lastAutoOrQuick)
					{
						GameData.LoadAndPlaySystemSound((DateTime.Now.Month == 4 && DateTime.Now.Day == 1) ? "sysse.ogg" : "se_stop.ogg");
						lastAutoOrQuick = false;
						picAnimeStateIcon[2].Alpha = 255;
						picAnimeStateIcon[2].BeginTransform(2f);
						picAnimeStateIcon[2].AlphaTo(0);
						picAnimeStateIcon[2].EndTransform();
						((AnimePicture)picAnimeStateIcon[2]).ResetFrame();
					}
					else if (picAnimeStateIcon[2].Alpha > 0)
					{
						picAnimeStateIcon[2].Update(elapsedTime);
					}
					break;
				}
			}
			else if (lastAutoOrQuick)
			{
				bool hasSelect = false;
				foreach (ScriptCommand currentCommand in CurrentCommandList)
				{
					if (currentCommand is Select)
					{
						hasSelect = true;
					}
				}
				if (!hasSelect)
				{
					GameData.LoadAndPlaySystemSound((DateTime.Now.Month == 4 && DateTime.Now.Day == 1) ? "sysse.ogg" : "se_stop.ogg");
				}
				lastAutoOrQuick = false;
				picAnimeStateIcon[2].Alpha = 255;
				picAnimeStateIcon[2].BeginTransform(2f);
				picAnimeStateIcon[2].AlphaTo(0);
				picAnimeStateIcon[2].EndTransform();
				((AnimePicture)picAnimeStateIcon[2]).ResetFrame();
			}
			else if (picAnimeStateIcon[2].Alpha > 0)
			{
				picAnimeStateIcon[2].Update(elapsedTime);
			}
			if (picQuickSaveIcon.Alpha > 0)
			{
				picQuickSaveIcon.Update(elapsedTime);
			}
		}

		protected override void DrawStateIcon()
		{
			base.DrawStateIcon();
			if (!ToNormal)
			{
				switch (CurrentPlayMode)
				{
				case PlayMode.Auto:
					GameGraphics.DrawPicture(picAnimeStateIcon[0]);
					break;
				case PlayMode.Quick:
					GameGraphics.DrawPicture(picAnimeStateIcon[1]);
					break;
				default:
					if (picAnimeStateIcon[2].Alpha > 0)
					{
						GameGraphics.DrawPicture(picAnimeStateIcon[2]);
					}
					break;
				}
			}
			else if (picAnimeStateIcon[2].Alpha > 0)
			{
				GameGraphics.DrawPicture(picAnimeStateIcon[2]);
			}
			if (picQuickSaveIcon.Alpha > 0)
			{
				GameGraphics.DrawPicture(picQuickSaveIcon);
			}
		}

		public override void ReturnTitleScreen(bool save)
		{
			if (GameScript.CurrentGameScript != null && !save)
			{
				GameState.SaveState();
			}
			base.ReturnTitleScreen(save);
			lastAutoOrQuick = false;
			if (picAnimeStateIcon[2].Alpha > 0)
			{
				picAnimeStateIcon[2].Update(picAnimeStateIcon[2].Time);
			}
			if (picQuickSaveIcon.Alpha > 0)
			{
				picQuickSaveIcon.Update(picQuickSaveIcon.Time);
			}
		}

		public void StartQuickSaveIcon()
		{
			picQuickSaveIcon.Alpha = 255;
			picQuickSaveIcon.BeginTransform(2f);
			picQuickSaveIcon.AlphaTo(0);
			picQuickSaveIcon.EndTransform();
		}

		public override void ShowComment(string comment)
		{
			base.ShowComment(comment);
			picCommentBack.Alpha = 0;
			picCommentBack.BeginTransform(0.5f);
			picCommentBack.AlphaTo(255);
			picCommentBack.EndTransform();
			strComment.ClearString();
			strComment.AddString(comment, ContentAlignment.TopCenter);
		}

		public override void HideComment()
		{
			base.HideComment();
			strComment.ClearString();
		}

		protected override void UpdateComment(float elapsedTime)
		{
			base.UpdateComment(elapsedTime);
			picCommentBack.Update(elapsedTime);
		}

		protected override void DrawComment()
		{
			base.DrawComment();
			GameGraphics.DrawPicture(picCommentBack);
			strComment.Canvas = CurrentCanvas;
			strComment.DrawString((float)picCommentBack.Alpha / 255f);
		}

		protected override void GameWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (!CanExitNow)
			{
				if (GameScript.CurrentGameScript != null)
				{
					ShowMessageBox(new AgainMessageBoxScreen(GameLanguage.ResourceManager.GetString("ExitStr"), MessageBoxButtons.YesNo, AgainFormClosing, new object[2] { sender, e }));
					e.Cancel = true;
				}
				else
				{
					CanExitNow = true;
					Application.Exit();
				}
			}
			else
			{
				if (GameScript.CurrentGameScript != null)
				{
					GameScript.SaveStory(GameScript.QuickIndexEnd, null, GameScript.QuickIcon);
				}
				else
				{
					GameState.SaveState();
				}
				GameOptions.Save();
				GameSound.Dispose();
			}
		}

		private void AgainFormClosing(object[] param)
		{
			CanExitNow = true;
			Application.Exit();
		}

		protected override void LoadDropSave(string fileName)
		{
			GameManager.GalCore.ShowMessageBox(new AgainMessageBoxScreen(GameManager.GalCore.GameLanguage.ResourceManager.GetString("LoadStr"), MessageBoxButtons.YesNo, AgainLoadDropSave, new object[1] { fileName }));
		}

		private void AgainLoadDropSave(object[] param)
		{
			bool cameFromBack = false;
			if (DropScreen != null && ((AgainSaveLoadScreen)DropScreen).Back)
			{
				cameFromBack = true;
			}
			base.LoadDropSave((string)param[0]);
			if (cameFromBack)
			{
				GameManager.GalCore.HideTitleScreen();
			}
		}
	}
}
