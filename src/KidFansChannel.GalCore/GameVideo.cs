using System;
using System.Drawing;
using System.Threading;
using KidFansChannel.Game2DCore;
using KidFansChannel.VideoCore;

namespace KidFansChannel.GalCore
{
	public class GameVideo : KidFansChannel.VideoCore.VideoCore
	{
		private GalCore _galCore;

		private FreePicture VideoPicture;

		private bool readyToDraw;

		public void SetCore(GalCore galCore)
		{
			_galCore = galCore;
			VideoPicture = _galCore.GameGraphics.CreateFreePicture();
			VideoPicture.Size = new Size(800, 600);
			Initial(_galCore.GameGraphics.Device, _galCore.GameGraphics.GameWindow.Handle);
			Stopping += GameVideo_Stopping;
		}

		~GameVideo()
		{
			Final();
			VideoPicture.Dispose();
			VideoPicture = null;
		}

		public override void Play(string fileName)
		{
			GC.Collect();
			Thread.CurrentThread.Priority = ThreadPriority.Lowest;
			readyToDraw = false;
			// 交给 VMR9 自己推进画面，先停掉主循环的 idle 渲染
			_galCore.GameGraphics.ToggleManual(true);
			base.Play(fileName);
		}

		public override void Stop()
		{
			readyToDraw = false;
			base.Stop();
			Thread.CurrentThread.Priority = ThreadPriority.Normal;
			_galCore.GameGraphics.ToggleManual(false);
		}

		public void Update()
		{
			if (Texture == null)
			{
				return;
			}
			VideoPicture.SetTexture(Texture);
			VideoPicture.SetUV(UV);
			readyToDraw = true;
		}

		public void Draw()
		{
			if (readyToDraw)
			{
				_galCore.GameGraphics.DrawPicture(VideoPicture);
			}
			readyToDraw = false;
		}

		/// <summary>VMR9 每交出一帧后重设一次渲染环境，避免残留状态影响下一次绘制。</summary>
		protected override void OnRender(object sender, EventArgs e)
		{
			base.OnRender(sender, e);
			_galCore.GameGraphics.RenderEnvironment();
		}

		private void GameVideo_Stopping(object sender, EventArgs e)
		{
			Stop();
		}
	}
}
