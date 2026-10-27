using System;
using TransformedColoredTextured = Microsoft.DirectX.Direct3D.CustomVertex.TransformedColoredTextured;
using Font = System.Drawing.Font;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using KidFansChannel.GameCore;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.Game2DCore
{
	public class Game2DCore : KidFansChannel.GameCore.GameCore, ICanvas
	{
		internal VertexBuffer VB;

		private RectangleF _targetBounds = new RectangleF(0f, 0f, 1f, 1f);

		private float _fontScale = 1f;

		internal List<Picture> PictureList = new List<Picture>();

		internal List<Font2D> FontList = new List<Font2D>();

		private Surface snapBackBuffer;

		public RectangleF TargetBounds
		{
			get
			{
				return _targetBounds;
			}
			set
			{
				_targetBounds = value;
			}
		}

		public float FontScale
		{
			get
			{
				return _fontScale;
			}
			set
			{
				_fontScale = value;
			}
		}

		public RectangleF GetClientBounds()
		{
			RectangleF result = default(RectangleF);
			result.X = (0f - _targetBounds.X) / _targetBounds.Width;
			result.Y = (0f - _targetBounds.Y) / _targetBounds.Height;
			result.Width = (float)ClientSize.Width / _targetBounds.Width;
			result.Height = (float)ClientSize.Height / _targetBounds.Height;
			return result;
		}

		public Game2DCore()
		{
			DeviceCreated += OnDeviceCreated2D;
			DeviceReset += OnDeviceReset2D;
			DeviceLost += OnDeviceLost2D;
			Disposing += OnDisposing2D;
		}

		private void OnDeviceCreated2D(object sender, DeviceEventArgs e)
		{
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Expected Obj, but got Unknown
			VB = new VertexBuffer(typeof(TransformedColoredTextured), 4, e.Device, (Usage)0, (VertexFormats)324, (Pool)1);
		}

		private void OnDeviceReset2D(object sender, DeviceEventArgs e)
		{
			e.Device.RenderState.CullMode = (Cull)1;
			e.Device.RenderState.AlphaBlendEnable = true;
			e.Device.RenderState.SourceBlend = (Blend)5;
			e.Device.RenderState.DestinationBlend = (Blend)6;
			foreach (Font2D font in FontList)
			{
				font.OnResetDevice();
			}
			foreach (Picture picture in PictureList)
			{
				if (picture is CanvasPicture)
				{
					((CanvasPicture)picture).OnDeviceReset();
				}
			}
		}

		private void OnDeviceLost2D(object sender, EventArgs e)
		{
			foreach (Font2D font in FontList)
			{
				font.OnLostDevice();
			}
			foreach (Picture picture in PictureList)
			{
				if (picture is CanvasPicture)
				{
					((CanvasPicture)picture).OnDeviceLost();
				}
			}
		}

		private void OnDisposing2D(object sender, EventArgs e)
		{
			while (PictureList.Count > 0)
			{
				PictureList[0].Dispose();
			}
			while (FontList.Count > 0)
			{
				FontList[0].Dispose();
			}
		}

		public Picture CreatePicture(string fileName)
		{
			if (Device != (Device)null)
			{
				return new Picture(this, fileName);
			}
			return null;
		}

		public Picture CreatePicture(Stream stream)
		{
			if (Device != (Device)null)
			{
				return new Picture(this, stream);
			}
			return null;
		}

		public Picture CreatePicture(Color color)
		{
			if (Device != (Device)null)
			{
				return new Picture(this, color);
			}
			return null;
		}

		public Picture CreatePictureInstant(string fileName)
		{
			if (Device != (Device)null)
			{
				return Picture.CreateInstant(this, fileName);
			}
			return null;
		}

		public Picture CreatePictureInstant(Stream stream)
		{
			if (Device != (Device)null)
			{
				return Picture.CreateInstant(this, stream);
			}
			return null;
		}

		public CanvasPicture CreateCanvasPicture(Size size)
		{
			if (Device != (Device)null)
			{
				return new CanvasPicture(this, size);
			}
			return null;
		}

		public BindingPicture CreateBindingPicture()
		{
			if (Device != (Device)null)
			{
				return new BindingPicture(this);
			}
			return null;
		}

		public SubPicture CreateSubPicture(Picture refPicture, Rectangle rect)
		{
			if (Device != (Device)null)
			{
				return new SubPicture(this, refPicture, rect);
			}
			return null;
		}

		public MixPicture CreateMixPicture(Picture pictureA, Picture pictureB)
		{
			if (Device != (Device)null)
			{
				return new MixPicture(this, pictureA, pictureB);
			}
			return null;
		}

		public FAlphaPicture CreateFAlphaPicture(Picture pictureSource, FAlphaPicture.AlphaFunc func)
		{
			if (Device != (Device)null)
			{
				return new FAlphaPicture(this, pictureSource, func);
			}
			return null;
		}

		public AnimePicture CreateAnimePicture(Size size)
		{
			if (Device != (Device)null)
			{
				return new AnimePicture(this, size);
			}
			return null;
		}

		public FreePicture CreateFreePicture()
		{
			if (Device != (Device)null)
			{
				return new FreePicture(this);
			}
			return null;
		}

		public Font2D CreateFont(Font font)
		{
			if (Device != (Device)null)
			{
				return new Font2D(this, font);
			}
			return null;
		}

		public void Snap(string fileName)
		{
			snapBackBuffer = Device.GetBackBuffer(0, 0, (BackBufferType)0);
			new Thread(SnapThreadProc).Start(fileName);
		}

		protected void SnapThreadProc(object fileName)
		{
			SurfaceLoader.Save((string)fileName, (ImageFileFormat)3, snapBackBuffer);
			snapBackBuffer.Dispose();
			snapBackBuffer = null;
		}

		protected Stream SaveToStream(Picture picture, Rectangle rect)
		{
			return (Stream)(object)SurfaceLoader.SaveToStream((ImageFileFormat)3, picture.Texture.GetSurfaceLevel(0), rect);
		}

		public void DrawBegin()
		{
			Device.BeginScene();
			Device.Clear((ClearFlags)3, Color.Black, 1f, 0);
		}

		public void DrawPicture(Picture picture)
		{
			if (picture != null)
			{
				picture.Draw(Device, VB);
			}
		}

		public void DrawString(Font2D font, string text, int x, int y, Color color)
		{
			if (font != null)
			{
				font.DrawText(text, (int)(_targetBounds.X + (float)x * _targetBounds.Width + 0.5f), (int)(_targetBounds.Y + (float)y * _targetBounds.Height + 0.5f), color, true);
			}
		}

		public void DrawLine(Line line, Vector2[] vector, Color color)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void DrawEnd()
		{
			Device.EndScene();
		}

		protected override void OnMouseDown(object sender, MouseEventArgs e)
		{
			MouseEventArgs e2 = new MouseEventArgs(e.Button, e.Clicks, (int)(((float)e.X - _targetBounds.X) / _targetBounds.Width + 0.5f), (int)(((float)e.Y - _targetBounds.Y) / _targetBounds.Height + 0.5f), e.Delta);
			base.OnMouseDown(sender, e2);
		}

		protected override void OnMouseMove(object sender, MouseEventArgs e)
		{
			MouseEventArgs e2 = new MouseEventArgs(e.Button, e.Clicks, (int)(((float)e.X - _targetBounds.X) / _targetBounds.Width + 0.5f), (int)(((float)e.Y - _targetBounds.Y) / _targetBounds.Height + 0.5f), e.Delta);
			base.OnMouseMove(sender, e2);
		}

		protected override void OnMouseWheel(object sender, MouseEventArgs e)
		{
			MouseEventArgs e2 = new MouseEventArgs(e.Button, e.Clicks, (int)(((float)e.X - _targetBounds.X) / _targetBounds.Width + 0.5f), (int)(((float)e.Y - _targetBounds.Y) / _targetBounds.Height + 0.5f), e.Delta);
			base.OnMouseWheel(sender, e2);
		}
	}
}
