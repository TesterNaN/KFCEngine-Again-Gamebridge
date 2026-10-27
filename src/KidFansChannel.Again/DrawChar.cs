using System.Drawing;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again
{
	public static class DrawChar
	{
		public static void DrawCharBorder(Point location, char c, float alpha, Color color, ICanvas canvas, Font2D font)
		{
			Color baseColor = Color.FromArgb(255 - color.R, 255 - color.B, 255 - color.G);
			Color color2 = Color.FromArgb(color.A, baseColor);
			if (alpha == 1f)
			{
				canvas.DrawString(font, c.ToString(), location.X + 1, location.Y, color2);
				canvas.DrawString(font, c.ToString(), location.X - 1, location.Y, color2);
				canvas.DrawString(font, c.ToString(), location.X, location.Y + 1, color2);
				canvas.DrawString(font, c.ToString(), location.X, location.Y - 1, color2);
				canvas.DrawString(font, c.ToString(), location.X, location.Y, color);
			}
			else
			{
				canvas.DrawString(font, c.ToString(), location.X + 1, location.Y, Color.FromArgb((int)(alpha * alpha * (float)(int)color2.A + 0.5f), baseColor));
				canvas.DrawString(font, c.ToString(), location.X - 1, location.Y, Color.FromArgb((int)(alpha * alpha * (float)(int)color2.A + 0.5f), baseColor));
				canvas.DrawString(font, c.ToString(), location.X, location.Y + 1, Color.FromArgb((int)(alpha * alpha * (float)(int)color2.A + 0.5f), baseColor));
				canvas.DrawString(font, c.ToString(), location.X, location.Y - 1, Color.FromArgb((int)(alpha * alpha * (float)(int)color2.A + 0.5f), baseColor));
				canvas.DrawString(font, c.ToString(), location.X, location.Y, Color.FromArgb((int)(alpha * (float)(int)color.A + 0.5f), color));
			}
		}
	}
}
