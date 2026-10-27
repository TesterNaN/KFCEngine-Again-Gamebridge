using System;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.GameCore
{
	internal class DeviceSettings : ICloneable
	{
		public uint AdapterOrdinal;

		public DeviceType DeviceType;

		public Format AdapterFormat;

		public CreateFlags BehaviorFlags;

		public PresentParameters PresentParameters;

		public DeviceSettings Clone()
		{
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_0019: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0040: Expected Obj, but got Unknown
			DeviceSettings deviceSettings = new DeviceSettings();
			deviceSettings.AdapterOrdinal = AdapterOrdinal;
			deviceSettings.BehaviorFlags = BehaviorFlags;
			deviceSettings.DeviceType = DeviceType;
			deviceSettings.PresentParameters = (PresentParameters)PresentParameters.Clone();
			return deviceSettings;
		}

		object ICloneable.Clone()
		{
			throw new NotSupportedException("Use the strongly typed overload instead.");
		}
	}
}
