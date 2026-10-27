using System;
using KidFansChannel.GalCore.Commands;
using KidFansChannel.GalCore.ExtBases;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.GalExt.DisplayStyles
{
	public class TransLeft : DisplayStyle
	{
		public static int Width = 100;

		public float xStart;

		public float xSpeed;

		public int left;

		public int right;

		protected static byte[] values;

		private FAlphaPicture pic;

		public override bool Finished
		{
			get
			{
				return xStart == (float)BgDisplay.ScreenSize.Width;
			}
		}

		public override bool Ready
		{
			get
			{
				return true;
			}
		}

		public TransLeft()
		{
			Name = "transleft";
		}

		public override void InitStyle(float speed)
		{
			values = new byte[Width];
			for (int i = 0; i < Width; i++)
			{
				values[i] = (byte)((0.0 - Math.Sin(((double)((float)i / (float)Width) - 0.5) * Math.PI) + 1.0) * 127.5);
			}
			xStart = -Width;
			left = -Width;
			right = 0;
			xSpeed = (float)(Width + BgDisplay.ScreenSize.Width) / speed;
			pic = ScriptCommand.Graphics.CreateFAlphaPicture(Parent.Picture, Func);
		}

		public override void UpdateStyle(float elaspedTime)
		{
			xStart += elaspedTime * xSpeed;
			left = (int)xStart;
			right = (int)(xStart + (float)Width);
			if (pic != null && xStart >= (float)BgDisplay.ScreenSize.Width)
			{
				xStart = BgDisplay.ScreenSize.Width;
				pic.Dispose();
				pic = null;
			}
		}

		public override void Draw()
		{
			if (pic != null)
			{
				ScriptCommand.Graphics.DrawPicture(pic);
			}
		}

		public override void InitData()
		{
		}

		public override void FinalData()
		{
		}

		public byte Func(int x, int y)
		{
			if (x < left)
			{
				return byte.MaxValue;
			}
			if (x < right)
			{
				return values[x - left];
			}
			return 0;
		}
	}
}
