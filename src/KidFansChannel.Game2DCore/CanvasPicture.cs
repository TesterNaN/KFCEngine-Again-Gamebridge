using System;
using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.Game2DCore
{
	public class CanvasPicture : Picture, ICanvas
	{
		private Surface renderSurface;

		private RenderToSurface rts;

		internal CanvasPicture(Game2DCore core, Size size)
		{
			//IL_0058: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Expected Obj, but got Unknown
			//IL_007c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Expected Obj, but got Unknown
			if (core == null)
			{
				throw new ArgumentNullException("Core must not be null.");
			}
			Core = core;
			core.PictureList.Add(this);
			Size = size;
			Textured = true;
			RefreshVerts();
			rts = new RenderToSurface(core.Device, size.Width, size.Height, (Format)22, true, (DepthFormat)80);
			Texture = new Texture(core.Device, size.Width, size.Height, 1, (Usage)1, (Format)22, (Pool)0);
			renderSurface = Texture.GetSurfaceLevel(0);
			IsCreated = true;
		}

		public override void Dispose()
		{
			base.Dispose();
			if ((Resource)(object)renderSurface != (Resource)null)
			{
				renderSurface.Dispose();
				renderSurface = null;
			}
			if (rts != (RenderToSurface)null)
			{
				rts.Dispose();
				rts = null;
			}
		}

		~CanvasPicture()
		{
			Dispose();
		}

		internal void OnDeviceLost()
		{
			if (rts != (RenderToSurface)null && !rts.Disposed)
			{
				rts.Dispose();
				rts = null;
			}
			if ((Resource)(object)renderSurface != (Resource)null && !renderSurface.Disposed)
			{
				renderSurface.Dispose();
				renderSurface = null;
			}
			if (Texture != (Texture)null && !Texture.Disposed)
			{
				Texture.Dispose();
				Texture = null;
			}
		}

		internal void OnDeviceReset()
		{
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Expected Obj, but got Unknown
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Expected Obj, but got Unknown
			rts = new RenderToSurface(Core.Device, Size.Width, Size.Height, (Format)22, true, (DepthFormat)80);
			Texture = new Texture(Core.Device, Size.Width, Size.Height, 1, (Usage)1, (Format)22, (Pool)0);
			renderSurface = Texture.GetSurfaceLevel(0);
		}

		public void Save(string fileName)
		{
		}

		public void DrawBegin()
		{
			rts.BeginScene(renderSurface);
			Core.Device.Clear((ClearFlags)3, Color.Black, 1f, 0);
		}

		public void DrawPicture(Picture picture)
		{
			if (picture != null)
			{
				picture.Draw(Core.Device, Core.VB, false);
			}
		}

		public void DrawString(Font2D font, string text, int x, int y, Color color)
		{
			if (font != null)
			{
				font.DrawText(text, x, y, color, false);
			}
		}

		public void DrawLine(Line line, Vector2[] vector, Color color)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		public void DrawEnd()
		{
			rts.EndScene((Filter)1);
		}
	}
}
