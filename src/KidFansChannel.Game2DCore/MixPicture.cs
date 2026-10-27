using System;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.Game2DCore
{
	public class MixPicture : Picture
	{
		private Texture textureA;

		private Texture textureB;

		private float _mixAlpha;

		private float invMixAlpha;

		public float MixAlpha
		{
			get
			{
				return _mixAlpha;
			}
			set
			{
				if (value < 0f || value > 1f)
				{
					throw new ArgumentOutOfRangeException();
				}
				_mixAlpha = value;
				invMixAlpha = 1f - value;
			}
		}

		internal MixPicture(Game2DCore core, Picture pictureA, Picture pictureB)
		{
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c8: Expected Obj, but got Unknown
			if (core == null)
			{
				throw new ArgumentNullException("Core must not be null.");
			}
			if (!pictureA.GetType().Equals(typeof(Picture)) || !pictureB.GetType().Equals(typeof(Picture)))
			{
				throw new NotSupportedException("Only support basic pictures.");
			}
			if (pictureA.Size != pictureB.Size)
			{
				throw new ArgumentException("Unmatched size for two pictures");
			}
			Core = core;
			core.PictureList.Add(this);
			Size = pictureA.Size;
			Texture = new Texture(core.Device, Size.Width, Size.Height, 1, (Usage)0, (Format)21, (Pool)1);
			Textured = true;
			_mixAlpha = 0f;
			invMixAlpha = 1f;
			textureA = pictureA.Texture;
			textureB = pictureB.Texture;
			IsCreated = true;
		}

		public unsafe override void Draw(Device device, VertexBuffer vb, bool target)
		{
			uint* ptr = (uint*)Texture.LockRectangle(0, (LockFlags)0).InternalData.ToPointer();
			uint* ptr2 = (uint*)textureA.LockRectangle(0, (LockFlags)0).InternalData.ToPointer();
			uint* ptr3 = (uint*)textureB.LockRectangle(0, (LockFlags)0).InternalData.ToPointer();
			for (int i = 0; i < Height; i++)
			{
				for (int j = 0; j < Width; j++)
				{
					uint alphaA = *ptr2 >> 24;
					uint alphaB = *ptr3 >> 24;
					if (alphaA == 0 && alphaB == 0)
					{
						*ptr = 0u;
					}
					else if (alphaA == 0)
					{
						uint mixedAlphaA = (uint)((float)alphaB * _mixAlpha + 0.5f);
						*ptr = (*ptr3 & 0xFFFFFF) | (mixedAlphaA << 24);
					}
					else if (alphaB == 0)
					{
						uint mixedAlphaB = (uint)((float)alphaA * invMixAlpha + 0.5f);
						*ptr = (*ptr2 & 0xFFFFFF) | (mixedAlphaB << 24);
					}
					else if (*ptr2 == *ptr3)
					{
						*ptr = *ptr2;
					}
					else
					{
						uint outAlpha = (uint)((float)alphaA * invMixAlpha + (float)alphaB * _mixAlpha + 0.5f);
						uint outR = (uint)((float)(*ptr2 & 0xFF) * invMixAlpha + (float)(*ptr3 & 0xFF) * _mixAlpha + 0.5f);
						uint outG = (uint)((float)((*ptr2 >> 8) & 0xFF) * invMixAlpha + (float)((*ptr3 >> 8) & 0xFF) * _mixAlpha + 0.5f);
						uint outB = (uint)((float)((*ptr2 >> 16) & 0xFF) * invMixAlpha + (float)((*ptr3 >> 16) & 0xFF) * _mixAlpha + 0.5f);
						*ptr = outR | (outG << 8) | (outB << 16) | (outAlpha << 24);
					}
					ptr++;
					ptr2++;
					ptr3++;
				}
			}
			Texture.UnlockRectangle(0);
			textureA.UnlockRectangle(0);
			textureB.UnlockRectangle(0);
			base.Draw(device, vb, target);
		}
	}
}
