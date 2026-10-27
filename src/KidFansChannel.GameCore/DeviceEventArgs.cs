using System;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.GameCore
{
	public class DeviceEventArgs : EventArgs
	{
		public Device Device;

		public SurfaceDescription BackBufferDescription;

		public DeviceEventArgs(Device d, SurfaceDescription desc)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_000f: Unknown result type (might be due to invalid IL or missing references)
			Device = d;
			BackBufferDescription = desc;
		}
	}
}
