using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.DirectX;
using Microsoft.DirectX.DirectInput;

namespace KidFansChannel.InputCore
{
	public class GamePadInput : IDisposable
	{
		public class GamePadInstance
		{
			public string Name;

			public bool ForceFeedback;

			public Guid Guid;
		}

		public const int ButtonNumber = 16;

		private Device directInputDevice;

		private bool[] buttonPressed = new bool[16];

		private EffectObject effectObject;

		public List<GamePadInstance> GetGamePadList()
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0050: Unknown result type (might be due to invalid IL or missing references)
			//IL_0057: Expected Obj, but got Unknown
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			List<GamePadInstance> pads = new List<GamePadInstance>();
			DeviceList devices = Manager.GetDevices((DeviceClass)4, (EnumDevicesFlags)1);
			foreach (DeviceInstance item in devices)
			{
				DeviceInstance deviceInstance = item;
				GamePadInstance gamePadInstance = new GamePadInstance();
				gamePadInstance.Name = deviceInstance.InstanceName;
				gamePadInstance.Guid = deviceInstance.InstanceGuid;
				Device device = new Device(deviceInstance.InstanceGuid);
				DeviceCaps caps = device.Caps;
				gamePadInstance.ForceFeedback = caps.ForceFeedback;
				device.Dispose();
				pads.Add(gamePadInstance);
			}
			return pads;
		}

		public void Initialize(GamePadInstance gamePadInstance, Form form)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected Obj, but got Unknown
			//IL_0057: Unknown result type (might be due to invalid IL or missing references)
			//IL_005c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_00df: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e4: Unknown result type (might be due to invalid IL or missing references)
			//IL_017a: Unknown result type (might be due to invalid IL or missing references)
			//IL_017f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_0225: Unknown result type (might be due to invalid IL or missing references)
			//IL_022d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0237: Expected Obj, but got Unknown
			directInputDevice = new Device(gamePadInstance.Guid);
			if (!(directInputDevice != (Device)null))
			{
				return;
			}
			try
			{
				directInputDevice.SetDataFormat((DeviceDataFormat)2);
				directInputDevice.SetCooperativeLevel((Control)form, (CooperativeLevelFlags)6);
				foreach (DeviceObjectInstance @object in directInputDevice.Objects)
				{
					DeviceObjectInstance devObj = @object;
					if ((devObj.ObjectId & 3) != 0)
					{
						directInputDevice.Properties.SetRange((ParameterHow)2, devObj.ObjectId, new InputRange(-1000, 1000));
					}
				}
				if (gamePadInstance.ForceFeedback)
				{
					int[] axes = null;
					foreach (DeviceObjectInstance object2 in directInputDevice.GetObjects((DeviceObjectTypeFlags)3))
					{
						DeviceObjectInstance objInfo = object2;
						if ((objInfo.Flags & 1) != 0)
						{
							if (axes != null)
							{
								int[] expanded = new int[axes.Length + 1];
								axes.CopyTo(expanded, 0);
								axes = expanded;
							}
							else
							{
								axes = new int[1];
							}
							axes[axes.Length - 1] = objInfo.Offset;
							if (2 == axes.Length)
							{
								break;
							}
						}
					}
					foreach (EffectInformation effect in directInputDevice.GetEffects((EffectType)0))
					{
						EffectInformation effectInfo = effect;
						if (DInputHelper.GetTypeCode(effectInfo.EffectType) == 1)
						{
							Effect effectParams = new Effect();
							effectParams.SetDirection(new int[axes.Length]);
							effectParams.SetAxes(new int[axes.Length]);
							effectParams.EffectType = (EffectType)1;
							effectParams.ConditionStruct = new Condition[axes.Length];
							effectParams.Duration = -1;
							effectParams.Gain = 10000;
							effectParams.SamplePeriod = 0;
							effectParams.TriggerButton = -1;
							effectParams.TriggerRepeatInterval = -1;
							effectParams.Flags = (EffectFlags)18;
							effectParams.Constant.Magnitude = 5000;
							effectParams.SetAxes(axes);
							effectObject = new EffectObject(effectInfo.EffectGuid, effectParams, directInputDevice);
						}
					}
				}
				else
				{
					effectObject = null;
				}
				directInputDevice.Acquire();
			}
			catch
			{
			}
		}

		~GamePadInput()
		{
			Dispose();
		}

		public void Update()
		{
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			if (!(directInputDevice != (Device)null))
			{
				return;
			}
			try
			{
				directInputDevice.Poll();
			}
			catch (DirectXException)
			{
				while (true)
				{
					try
					{
						directInputDevice.Acquire();
						break;
					}
					catch (InputLostException)
					{
					}
					catch (InputException)
					{
						break;
					}
				}
			}
			JoystickState currentJoystickState;
			try
			{
				currentJoystickState = directInputDevice.CurrentJoystickState;
			}
			catch (InputException)
			{
				return;
			}
			byte[] buttons = currentJoystickState.GetButtons();
			for (int i = 0; i < 12; i++)
			{
				if (i < buttons.Length)
				{
					buttonPressed[i] = (buttons[i] & 0x80) == 128;
				}
				else
				{
					buttonPressed[i] = false;
				}
			}
			for (int j = 12; j < 16; j++)
			{
				buttonPressed[j] = false;
			}
			int[] pointOfView = currentJoystickState.GetPointOfView();
			int povAngle = ((pointOfView.Length != 0) ? pointOfView[0] : (-1));
			int x = currentJoystickState.X;
			int y = currentJoystickState.Y;
			if ((x < -500 && x < -Math.Abs(y)) || povAngle == 27000 || povAngle == 31500 || povAngle == 22500)
			{
				buttonPressed[12] = true;
			}
			else if ((x > 500 && x > Math.Abs(y)) || povAngle == 9000 || povAngle == 13500 || povAngle == 4500)
			{
				buttonPressed[13] = true;
			}
			else if ((y < -500 && y <= -Math.Abs(x)) || povAngle == 0 || povAngle == 4500 || povAngle == 31500)
			{
				buttonPressed[14] = true;
			}
			else if ((y > 500 && y >= Math.Abs(x)) || povAngle == 18000 || povAngle == 22500 || povAngle == 13500)
			{
				buttonPressed[15] = true;
			}
		}

		public bool GetButtonState(int index)
		{
			if (index < 0 || index >= 16)
			{
				throw new ArgumentOutOfRangeException();
			}
			return buttonPressed[index];
		}

		public void StartShock()
		{
			if (effectObject != (EffectObject)null)
			{
				try
				{
					effectObject.Start(1);
				}
				catch (Exception)
				{
					// 设备不支持力反馈时静默跳过，不影响正常操作
				}
			}
		}

		public void StopShock()
		{
			if (effectObject != (EffectObject)null)
			{
				try
				{
					effectObject.Stop();
				}
				catch (Exception)
				{
				}
			}
		}

		public void Dispose()
		{
			if (!(directInputDevice != (Device)null))
			{
				return;
			}
			try
			{
				if ((EffectObject)null != effectObject)
				{
					try
				{
					effectObject.Stop();
				}
				catch (Exception)
				{
				}
					effectObject = null;
				}
				directInputDevice.Unacquire();
				directInputDevice.Properties.AutoCenter = true;
				directInputDevice.Dispose();
				directInputDevice = null;
			}
			catch (Exception)
			{
			}
		}
	}
}
