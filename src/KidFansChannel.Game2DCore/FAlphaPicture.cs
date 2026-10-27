using System;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.Game2DCore
{
	public class FAlphaPicture : Picture
	{
		public delegate byte AlphaFunc(int x, int y);

		private Texture textureSrc;

		private bool read;

		protected AlphaFunc Function;

		internal FAlphaPicture(Game2DCore core, Picture pictureSource, AlphaFunc func)
		{
			//IL_005e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Expected Obj, but got Unknown
			if (core == null)
			{
				throw new ArgumentNullException("Core must not be null.");
			}
			Core = core;
			core.PictureList.Add(this);
			Size = pictureSource.Size;
			Texture = new Texture(core.Device, Size.Width, Size.Height, 1, (Usage)0, (Format)21, (Pool)1);
			Textured = true;
			textureSrc = pictureSource.Texture;
			Function = func;
			IsCreated = true;
		}

		public unsafe override void Draw(Device device, VertexBuffer vb, bool target)
		{
			if (!read)
			{
				Surface surfaceLevel = textureSrc.GetSurfaceLevel(0);
				try
				{
					Surface surfaceLevel2 = Texture.GetSurfaceLevel(0);
					try
					{
						SurfaceLoader.FromSurface(surfaceLevel2, surfaceLevel, (Filter)1, 0);
					}
					finally
					{
						if (surfaceLevel2 != null)
						{
							((IDisposable)surfaceLevel2).Dispose();
						}
					}
				}
				finally
				{
					if (surfaceLevel != null)
					{
						((IDisposable)surfaceLevel).Dispose();
					}
				}
				read = true;
			}
			byte* ptr = (byte*)Texture.LockRectangle(0, (LockFlags)0).InternalData.ToPointer();
			for (int i = 0; i < Height; i++)
			{
				for (int j = 0; j < Width; j++)
				{
					ptr++;
					ptr++;
					ptr++;
					*ptr = Function(j, i);
					ptr++;
				}
			}
			Texture.UnlockRectangle(0);
			base.Draw(device, vb, target);
		}
	}
}
