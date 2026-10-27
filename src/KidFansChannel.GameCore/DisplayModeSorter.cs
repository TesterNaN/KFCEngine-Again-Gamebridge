using System.Collections.Generic;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.GameCore
{
	internal class DisplayModeSorter : IComparer<DisplayMode>
	{
		public int Compare(DisplayMode x, DisplayMode y)
		{
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0072: Unknown result type (might be due to invalid IL or missing references)
			if (x.Width > y.Width)
			{
				return 1;
			}
			if (x.Width < y.Width)
			{
				return -1;
			}
			if (x.Height > y.Height)
			{
				return 1;
			}
			if (x.Height < y.Height)
			{
				return -1;
			}
			if (x.Format > y.Format)
			{
				return 1;
			}
			if (x.Format < y.Format)
			{
				return -1;
			}
			if (x.RefreshRate > y.RefreshRate)
			{
				return 1;
			}
			if (x.RefreshRate < y.RefreshRate)
			{
				return -1;
			}
			return 0;
		}
	}
}
