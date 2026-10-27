using System;
using Font = System.Drawing.Font;
using System.Drawing;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.Game2DCore
{
	public class Font2D : IDisposable
	{
		private Microsoft.DirectX.Direct3D.Font _font;

		private Microsoft.DirectX.Direct3D.Font _drawFont;

		private Font originalFont;

		private Game2DCore _core;

		private float lastScale;

		internal Microsoft.DirectX.Direct3D.Font Font
		{
			get
			{
				return _font;
			}
		}

		internal Microsoft.DirectX.Direct3D.Font DrawFont
		{
			get
			{
				return _drawFont;
			}
		}

		public Game2DCore Core
		{
			get
			{
				return _core;
			}
		}

		public float Size
		{
			get
			{
				return originalFont.Size;
			}
		}

		internal Font2D(Game2DCore core, Font font)
		{
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected Obj, but got Unknown
			//IL_0069: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Expected Obj, but got Unknown
			_core = core;
			core.FontList.Add(this);
			_font = new Microsoft.DirectX.Direct3D.Font(Core.Device, new Font(font.FontFamily, font.Size));
			_drawFont = new Microsoft.DirectX.Direct3D.Font(Core.Device, new Font(font.FontFamily, font.Size * Core.FontScale));
			originalFont = font;
			lastScale = core.FontScale;
		}

		public Size Measure(string s)
		{
			return _font.MeasureString((Sprite)null, s, (DrawTextFormat)0, 0).Size;
		}

		public void DrawText(string text, int x, int y, Color color, bool target)
		{
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Expected Obj, but got Unknown
			if (lastScale != Core.FontScale)
			{
				if (_drawFont != (Microsoft.DirectX.Direct3D.Font)null)
				{
					_drawFont.Dispose();
				}
				_drawFont = new Microsoft.DirectX.Direct3D.Font(Core.Device, new Font(originalFont.FontFamily, originalFont.Size * Core.FontScale));
				lastScale = Core.FontScale;
			}
			if (_drawFont != (Microsoft.DirectX.Direct3D.Font)null && !_drawFont.Disposed)
			{
				if (target)
				{
					_drawFont.DrawText((Sprite)null, text, x, y, color);
				}
				else
				{
					_font.DrawText((Sprite)null, text, x, y, color);
				}
			}
		}

		public void OnResetDevice()
		{
			if (_font != (Microsoft.DirectX.Direct3D.Font)null && !_font.Disposed)
			{
				_font.OnResetDevice();
			}
			if (_drawFont != (Microsoft.DirectX.Direct3D.Font)null && !_drawFont.Disposed)
			{
				_drawFont.OnResetDevice();
			}
		}

		public void OnLostDevice()
		{
			if (!_font.Disposed)
			{
				_font.OnLostDevice();
			}
			if (!_drawFont.Disposed)
			{
				_drawFont.OnLostDevice();
			}
		}

		public void Dispose()
		{
			if (_font != (Microsoft.DirectX.Direct3D.Font)null)
			{
				_font.Dispose();
				_font = null;
			}
			if (_drawFont != (Microsoft.DirectX.Direct3D.Font)null)
			{
				_drawFont.Dispose();
				_drawFont = null;
			}
			if (originalFont != null)
			{
				originalFont.Dispose();
				originalFont = null;
			}
			Core.FontList.Remove(this);
		}
	}
}
