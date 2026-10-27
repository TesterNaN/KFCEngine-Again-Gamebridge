using System.Drawing;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.Game2DCore
{
	public interface ICanvas
	{
		void DrawBegin();

		void DrawPicture(Picture picture);

		void DrawString(Font2D font, string text, int x, int y, Color color);

		void DrawLine(Line line, Vector2[] vector, Color color);

		void DrawEnd();
	}
}
