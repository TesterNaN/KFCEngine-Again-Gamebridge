using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.GameCore
{
	internal class ManagedUtility
	{
		private ManagedUtility()
		{
		}

		public static uint GetColorChannelBits(Format format)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected I4, but got Unknown
			switch ((int)format - 20)
			{
			case 0:
			case 1:
			case 2:
				return 8u;
			case 3:
			case 4:
			case 5:
				return 5u;
			case 6:
			case 10:
				return 4u;
			case 7:
			case 9:
				return 2u;
			case 11:
			case 15:
				return 10u;
			default:
				return 0u;
			}
		}

		public static uint GetAlphaChannelBits(Format format)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Expected I4, but got Unknown
			switch ((int)format - 20)
			{
			case 0:
			case 2:
			case 3:
			case 4:
			case 7:
			case 10:
				return 0u;
			case 1:
			case 9:
				return 8u;
			case 5:
				return 1u;
			case 6:
				return 4u;
			case 11:
			case 15:
				return 2u;
			default:
				return 0u;
			}
		}

		public static uint GetDepthBits(DepthFormat format)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected I4, but got Unknown
			switch ((int)format - 70)
			{
			case 0:
			case 10:
				return 16u;
			case 3:
				return 15u;
			case 5:
			case 7:
			case 9:
			case 13:
				return 24u;
			case 1:
			case 12:
				return 32u;
			default:
				return 0u;
			}
		}

		public static uint GetStencilBits(DepthFormat format)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0005: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Expected I4, but got Unknown
			switch ((int)format - 70)
			{
			case 0:
			case 1:
			case 7:
			case 10:
			case 12:
				return 0u;
			case 3:
				return 1u;
			case 9:
				return 4u;
			case 5:
			case 13:
				return 8u;
			default:
				return 0u;
			}
		}
	}
}
