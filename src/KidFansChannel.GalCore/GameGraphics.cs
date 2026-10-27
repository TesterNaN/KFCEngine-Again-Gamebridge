using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using KidFansChannel.Game2DCore;
using KidFansChannel.GameCore;

namespace KidFansChannel.GalCore
{
	public class GameGraphics : KidFansChannel.Game2DCore.Game2DCore
	{
		private GalCore galCore;

		private Resolution _resolution;

		public readonly Size WindowSize = new Size(800, 600);

		public new RenderEventHandler FrameRender;

		public RenderEventHandler PeripheryRender;

		public RenderEventHandler FramePrepare;

		private Picture[] picBorder = new Picture[4];

		private bool _shocking;

		protected Point originalLocation;

		private float shockingTime;

		protected CanvasPicture CapturePicture;

		private CanvasPicture tempPicture;

		public Resolution Resolution
		{
			get
			{
				return _resolution;
			}
			set
			{
				_resolution = value;
			}
		}

		/// <summary>窗口/全屏切换完成后触发，订阅方据此重算画面布局。</summary>
		public event EventHandler FullscreenToggled;

		/// <summary>窗口模式下留给对话框的区域。全屏时改用 Resolution.CommentArea。</summary>
		public readonly Rectangle WindowComment = new Rectangle(0, 0, 800, 40);

		public event RenderEventHandler FrameCapture;

		~GameGraphics()
		{
			if (CapturePicture != null)
			{
				CapturePicture.Dispose();
			}
			if (tempPicture != null)
			{
				tempPicture.Dispose();
			}
		}

		public void SetCore(GalCore gc)
		{
			galCore = gc;
		}

		internal void GraphicsInit()
		{
			GameWindow.KeyDown += OnKeyDown;
			base.FrameRender += OnFrameRender;
			for (int i = 0; i < 4; i++)
			{
				picBorder[i] = CreatePicture(Color.Black);
			}
			PrepareBorder();
			if (!IsWindowed && FullscreenToggled != null)
			{
				FullscreenToggled(this, EventArgs.Empty);
			}
			CapturePicture = CreateCanvasPicture(WindowSize);
			tempPicture = CreateCanvasPicture(WindowSize);
		}

		private void OnFrameRender(object sender, RenderEventArgs e)
		{
			if (FramePrepare != null)
			{
				FramePrepare(sender, e);
			}
			DrawBegin();
			if (FrameRender != null)
			{
				FrameRender(sender, e);
			}
			if (galCore.GameVideo == null || !galCore.GameVideo.IsPlaying)
			{
				for (int i = 0; i < 4; i++)
				{
					DrawPicture(picBorder[i]);
				}
				if (PeripheryRender != null)
				{
					PeripheryRender(sender, e);
				}
			}
			DrawEnd();
		}

		private void OnKeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyData == (Keys.Return | Keys.Alt))
			{
				try
				{
					ToggleFullscreen();
				}
				catch
				{
					MessageBox.Show("Can't toggle full screen, wrong resolution?");
				}
			}
		}

		public override void ToggleFullscreen()
		{
			if (galCore.GameVideo == null || !galCore.GameVideo.IsPlaying)
			{
				if (_shocking && IsWindowed)
				{
					GameWindow.Location = originalLocation;
					shockingTime = 0f;
				}
				base.ToggleFullscreen();
				PrepareBorder();
				if (!IsWindowed && FullscreenToggled != null)
				{
					FullscreenToggled(this, EventArgs.Empty);
				}
			}
		}

		protected void PrepareBorder()
		{
			if (IsWindowed)
			{
				TargetBounds = new RectangleF(0f, 0f, 1f, 1f);
				FontScale = 1f;
			}
			else
			{
				TargetBounds = new RectangleF(_resolution.Bounds.X, _resolution.Bounds.Y, (float)_resolution.Bounds.Width / (float)WindowSize.Width, (float)_resolution.Bounds.Height / (float)WindowSize.Height);
				FontScale = _resolution.FontScale;
			}
			RectangleF clientBounds = GetClientBounds();
			picBorder[0].X = (int)Math.Floor(clientBounds.X);
			picBorder[0].Y = (int)Math.Floor(clientBounds.Y);
			picBorder[0].Width = (int)Math.Ceiling(clientBounds.Width);
			picBorder[0].Height = (int)Math.Ceiling(0f - clientBounds.Y);
			picBorder[1].X = (int)Math.Floor(clientBounds.X);
			picBorder[1].Y = WindowSize.Height;
			picBorder[1].Width = (int)Math.Ceiling(clientBounds.Width);
			picBorder[1].Height = (int)Math.Ceiling(clientBounds.Height - (float)WindowSize.Height);
			picBorder[2].X = (int)Math.Floor(clientBounds.X);
			picBorder[2].Y = 0;
			picBorder[2].Width = (int)Math.Ceiling(0f - clientBounds.X);
			picBorder[2].Height = WindowSize.Height;
			picBorder[3].X = WindowSize.Width;
			picBorder[3].Y = 0;
			picBorder[3].Width = (int)Math.Ceiling(clientBounds.Width - (float)WindowSize.Width);
			picBorder[3].Height = WindowSize.Height;
		}

		public void StartShock()
		{
			_shocking = true;
			if (IsWindowed)
			{
				originalLocation = GameWindow.Location;
				shockingTime = 0f;
			}
		}

		public void StopShock()
		{
			_shocking = false;
			if (IsWindowed)
			{
				GameWindow.Location = originalLocation;
			}
		}

		public void Update(double appTime, float elaspedTime)
		{
			if (_shocking && IsWindowed)
			{
				shockingTime += elaspedTime;
				while (shockingTime > 0.2f)
				{
					shockingTime -= 0.2f;
				}
				if (shockingTime < 0.025f)
				{
					GameWindow.Left = originalLocation.X - (int)(80f * shockingTime);
					GameWindow.Top = originalLocation.Y - (int)(80f * shockingTime);
				}
				else if (shockingTime < 0.075f)
				{
					GameWindow.Left = originalLocation.X - 2;
					GameWindow.Top = originalLocation.Y - 2 + (int)(80f * (shockingTime - 0.025f));
				}
				else if (shockingTime < 0.125f)
				{
					GameWindow.Left = originalLocation.X - 2 + (int)(80f * (shockingTime - 0.075f));
					GameWindow.Top = originalLocation.Y + 2 - (int)(80f * (shockingTime - 0.075f));
				}
				else if (shockingTime < 0.175f)
				{
					GameWindow.Left = originalLocation.X + 2;
					GameWindow.Top = originalLocation.Y - 2 + (int)(80f * (shockingTime - 0.125f));
				}
				else
				{
					GameWindow.Left = originalLocation.X + 2 - (int)(80f * (shockingTime - 0.175f));
					GameWindow.Top = originalLocation.Y + 2 - (int)(80f * (shockingTime - 0.175f));
				}
			}
		}

		public Stream Capture(Size size)
		{
			ICanvas currentCanvas = galCore.CurrentCanvas;
			galCore.CurrentCanvas = tempPicture;
			tempPicture.DrawBegin();
			if (FrameCapture != null)
			{
				FrameCapture(this, new RenderEventArgs(0.0, 0f));
			}
			tempPicture.DrawEnd();
			tempPicture.Scale = new SizeF((float)size.Width / (float)tempPicture.Width, (float)size.Height / (float)tempPicture.Height);
			CapturePicture.DrawBegin();
			CapturePicture.DrawPicture(tempPicture);
			CapturePicture.DrawEnd();
			galCore.CurrentCanvas = currentCanvas;
			return SaveToStream(CapturePicture, new Rectangle(Point.Empty, size));
		}
	}
}
