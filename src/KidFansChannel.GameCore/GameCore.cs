using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.GameCore
{
	public class GameCore : IDisposable
	{
		private delegate void DisposeDelegate();

		protected static readonly IntPtr TrueIntPtr = new IntPtr(1);

		private GameCoreData State;

		private Exception lastDisplayedMessage;

		private bool isDisposed;

		private bool showFps;

		private Microsoft.DirectX.Direct3D.Font fpsFont;

		private double nextRenderTime;

		private float nowElapsedTime;

		public EventHandler OnPause;

		public EventHandler OnResume;

		public int ExitCode
		{
			get
			{
				return State.ApplicationExitCode;
			}
		}

		public Device Device
		{
			get
			{
				return State.Device;
			}
		}

		public SurfaceDescription BackBufferSurfaceDescription
		{
			get
			{
				//IL_0006: Unknown result type (might be due to invalid IL or missing references)
				return State.BackBufferSurfaceDesc;
			}
		}

		public Window GameWindow
		{
			get
			{
				return (Window)State.Window;
			}
		}

		public Size WindowClientSize
		{
			get
			{
				return State.WindowClientSize;
			}
		}

		public Size FullScreenClientSize
		{
			get
			{
				return State.FullScreenClientSize;
			}
		}

		public Size ClientSize
		{
			get
			{
				if (!IsWindowed)
				{
					return State.FullScreenClientSize;
				}
				return State.WindowClientSize;
			}
		}

		public bool IsWindowed
		{
			get
			{
				DeviceSettings currentDeviceSettings = State.CurrentDeviceSettings;
				if (currentDeviceSettings != null && currentDeviceSettings.PresentParameters != null)
				{
					return currentDeviceSettings.PresentParameters.Windowed;
				}
				return false;
			}
		}

		public float FPS
		{
			get
			{
				return State.CurrentFrameRate;
			}
		}

		public bool ShowFps
		{
			get
			{
				return showFps;
			}
			set
			{
				showFps = value;
			}
		}

		public int CurrentFrameNumber
		{
			get
			{
				return State.CurrentFrameNumber;
			}
		}

		public bool IsShowingMsgBoxOnError
		{
			get
			{
				return State.IsShowingMsgBoxOnError;
			}
		}

		public bool WindowActive
		{
			get
			{
				return State.IsActive;
			}
		}

		/// <summary>挂在 Application.Idle 上的委托，视频播放时会临时摘下。</summary>
		public EventHandler ApplicationIdleHandler;

		/// <summary>当前是否由调用方手动驱动渲染（视频播放期间就是这样）。</summary>
		private bool isManual;

		private bool AppStillIdle
		{
			get
			{
				if (GameWindow.WindowState == FormWindowState.Minimized)
				{
					return false;
				}
				NativeMethods.Message msg;
				if (NativeMethods.PeekMessage(out msg, IntPtr.Zero, 0u, 0u, 0u))
				{
					return false;
				}
				return true;
			}
		}

		public bool Disposed
		{
			get
			{
				return isDisposed;
			}
			set
			{
				isDisposed = value;
			}
		}

		public event RenderEventHandler FrameMove;

		public event RenderEventHandler FrameRender;

		public event EventHandler Disposing;

		public event EventHandler DeviceLost;

		public event DeviceEventHandler DeviceCreated;

		public event DeviceEventHandler DeviceReset;

		public event MouseEventHandler MouseMove;

		public event MouseEventHandler MouseDown;

		public event MouseEventHandler MouseUp;

		public event MouseEventHandler MouseWheel;

		public GameCore()
		{
			State = new GameCoreData();
		}

		public void SetWndProcCallback(WndProcCallback callback)
		{
			State.WndProcFunction = callback;
		}

		public void Initialize(bool showMessageBoxOnError)
		{
			State.WasInitCalled = true;
			Device.IsUsingEventHandlers = false;
			State.IsShowingMsgBoxOnError = showMessageBoxOnError;
			try
			{
				NativeMethods.timeBeginPeriod(1u);
			}
			catch
			{
				Debugger.Log(9, string.Empty, "Could not set time period.\r\n");
			}
			GameCoreTimer.Reset();
			State.IsInited = true;
		}

		public void CreateWindow(int windowWidth, int windowHeight)
		{
			if (State.IsInsideDeviceCallback)
			{
				throw new InvalidOperationException("You cannot create a window from inside a callback.");
			}
			State.WasWindowCreateCalled = true;
			if (!State.IsInited)
			{
				if (State.WasInitCalled)
				{
					throw new InvalidOperationException("Initialize was already called and failed.");
				}
				Initialize(true);
			}
			if (State.Window == null)
			{
				Window window = new Window(this);
				HookEvents(window);
				Size clientSize = new Size(windowWidth, windowHeight);
				window.ClientSize = clientSize;
				window.Visible = false;
				window.CreateControl();
				State.WindowClientSize = window.ClientSize;
				State.WasWindowCreated = true;
				State.Window = window;
			}
		}

		public void CreateDevice(uint adapterOrdinal, bool windowed, int fullscreenWidth, int fullscreenHeight)
		{
			//IL_011d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Expected Obj, but got Unknown
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0273: Unknown result type (might be due to invalid IL or missing references)
			//IL_0278: Unknown result type (might be due to invalid IL or missing references)
			//IL_027c: Unknown result type (might be due to invalid IL or missing references)
			if (State.IsInsideDeviceCallback)
			{
				throw new InvalidOperationException("You cannot create a window from inside a callback.");
			}
			State.FullScreenClientSize = new Size(fullscreenWidth, fullscreenHeight);
			State.WasDeviceCreateCalled = true;
			if (!State.WasWindowCreated)
			{
				if (State.WasWindowCreateCalled)
				{
					throw new InvalidOperationException("CreateWindow was already called and failed.");
				}
				CreateWindow(State.WindowClientSize.Width, State.WindowClientSize.Height);
			}
			Enumeration.Enumerate();
			MatchOptions match = default(MatchOptions);
			match.AdapterOrdinal = MatchType.PreserveInput;
			match.DeviceType = MatchType.IgnoreInput;
			match.Windowed = MatchType.PreserveInput;
			match.AdapterFormat = MatchType.IgnoreInput;
			match.VertexProcessing = MatchType.IgnoreInput;
			match.Resolution = MatchType.PreserveInput;
			match.BackBufferFormat = MatchType.IgnoreInput;
			match.BackBufferCount = MatchType.IgnoreInput;
			match.MultiSample = MatchType.IgnoreInput;
			match.SwapEffect = MatchType.IgnoreInput;
			match.DepthFormat = MatchType.IgnoreInput;
			match.StencilFormat = MatchType.IgnoreInput;
			match.PresentFlags = MatchType.IgnoreInput;
			match.RefreshRate = MatchType.IgnoreInput;
			match.PresentInterval = MatchType.IgnoreInput;
			DeviceSettings deviceSettings = new DeviceSettings();
			deviceSettings.PresentParameters = new PresentParameters();
			if (Enumeration.AdapterInformationList.Count > adapterOrdinal)
			{
				deviceSettings.AdapterOrdinal = adapterOrdinal;
			}
			else
			{
				deviceSettings.AdapterOrdinal = 0u;
			}
			deviceSettings.PresentParameters.Windowed = windowed;
			deviceSettings.PresentParameters.BackBufferWidth = (windowed ? State.WindowClientSize.Width : State.FullScreenClientSize.Width);
			deviceSettings.PresentParameters.BackBufferHeight = (windowed ? State.WindowClientSize.Height : State.FullScreenClientSize.Height);
			try
			{
				deviceSettings = FindValidDeviceSettings(deviceSettings, match);
			}
			catch (Exception e)
			{
				if (deviceSettings.PresentParameters.Windowed)
				{
					DisplayErrorMessage(e);
					throw;
				}
				try
				{
					deviceSettings.PresentParameters.Windowed = true;
					deviceSettings.PresentParameters.BackBufferWidth = State.WindowClientSize.Width;
					deviceSettings.PresentParameters.BackBufferHeight = State.WindowClientSize.Height;
					deviceSettings = FindValidDeviceSettings(deviceSettings, match);
				}
				catch (Exception e2)
				{
					DisplayErrorMessage(e2);
					throw;
				}
			}
			Caps deviceCaps = Manager.GetDeviceCaps((int)deviceSettings.AdapterOrdinal, deviceSettings.DeviceType);
			ModifyDeviceSettings(deviceSettings, deviceCaps);
			GameWindow.SetScreen((int)adapterOrdinal);
			State.WindowLocation = State.Window.Location;
			ChangeDevice(deviceSettings);
		}

		private void ModifyDeviceSettings(DeviceSettings settings, Caps caps)
		{
			//IL_0002: Unknown result type (might be due to invalid IL or missing references)
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_0048: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0056: Unknown result type (might be due to invalid IL or missing references)
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			DeviceCaps deviceCaps = caps.DeviceCaps;
			if (!deviceCaps.SupportsHardwareTransformAndLight || caps.VertexShaderVersion < new Version(1, 1))
			{
				settings.BehaviorFlags = (CreateFlags)32;
			}
			else
			{
				settings.BehaviorFlags = (CreateFlags)64;
			}
			DeviceCaps deviceCaps2 = caps.DeviceCaps;
			if (deviceCaps2.SupportsPureDevice && ((int)settings.BehaviorFlags & 0x40) != 0)
			{
				settings.BehaviorFlags = (CreateFlags)((int)settings.BehaviorFlags | 0x10);
			}
		}

		public virtual void ToggleFullscreen()
		{
			Pause(true, true);
			DeviceSettings deviceSettings = State.CurrentDeviceSettings.Clone();
			deviceSettings.PresentParameters.Windowed = !deviceSettings.PresentParameters.Windowed;
			MatchOptions match = default(MatchOptions);
			match.AdapterOrdinal = MatchType.PreserveInput;
			match.DeviceType = MatchType.ClosestToInput;
			match.Windowed = MatchType.PreserveInput;
			match.AdapterFormat = MatchType.IgnoreInput;
			match.VertexProcessing = MatchType.ClosestToInput;
			match.BackBufferFormat = MatchType.IgnoreInput;
			match.BackBufferCount = MatchType.ClosestToInput;
			match.MultiSample = MatchType.ClosestToInput;
			match.SwapEffect = MatchType.ClosestToInput;
			match.DepthFormat = MatchType.ClosestToInput;
			match.StencilFormat = MatchType.ClosestToInput;
			match.PresentFlags = MatchType.ClosestToInput;
			match.RefreshRate = MatchType.IgnoreInput;
			match.PresentInterval = MatchType.IgnoreInput;
			Size size = ((!deviceSettings.PresentParameters.Windowed) ? State.FullScreenClientSize : State.WindowClientSize);
			if (size.Width > 0 && size.Height > 0)
			{
				match.Resolution = MatchType.PreserveInput;
				deviceSettings.PresentParameters.BackBufferWidth = size.Width;
				deviceSettings.PresentParameters.BackBufferHeight = size.Height;
			}
			else
			{
				match.Resolution = MatchType.IgnoreInput;
			}
			try
			{
				deviceSettings = FindValidDeviceSettings(deviceSettings, match);
				ChangeDevice(deviceSettings);
				if (IsWindowed)
				{
					GameWindow.ClientSize = State.WindowClientSize;
					GameWindow.Location = State.WindowLocation;
					GameWindow.TopMost = false;
				}
				else
				{
					State.WindowLocation = GameWindow.Location;
				}
				State.CurrentDeviceSettings = deviceSettings;
			}
			finally
			{
				Pause(false, false);
			}
		}

		private DeviceSettings FindValidDeviceSettings(DeviceSettings settings, MatchOptions match)
		{
			//IL_003a: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_007f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a3: Unknown result type (might be due to invalid IL or missing references)
			DeviceSettings settings2 = BuildOptimalDeviceSettings(settings, match);
			float bestScore = -1f;
			EnumDeviceSettingsCombo deviceCombo = new EnumDeviceSettingsCombo();
			for (int i = 0; i < Enumeration.AdapterInformationList.Count; i++)
			{
				EnumAdapterInformation enumAdapterInformation = Enumeration.AdapterInformationList[i];
				DisplayMode currentDisplayMode = Manager.Adapters[(int)enumAdapterInformation.AdapterOrdinal].CurrentDisplayMode;
				for (int j = 0; j < enumAdapterInformation.deviceInfoList.Count; j++)
				{
					EnumDeviceInformation enumDeviceInformation = enumAdapterInformation.deviceInfoList[j];
					for (int k = 0; k < enumDeviceInformation.deviceSettingsList.Count; k++)
					{
						EnumDeviceSettingsCombo enumDeviceSettingsCombo = enumDeviceInformation.deviceSettingsList[k];
						if ((!enumDeviceSettingsCombo.IsWindowed || enumDeviceSettingsCombo.AdapterFormat == currentDisplayMode.Format) && DoesDeviceComboMatchPreserveOptions(enumDeviceSettingsCombo, settings, match))
						{
							float bestCandidate = RankDeviceCombo(enumDeviceSettingsCombo, settings2, currentDisplayMode);
							if (bestCandidate > bestScore)
							{
								deviceCombo = enumDeviceSettingsCombo;
								bestScore = bestCandidate;
							}
						}
					}
				}
			}
			if (bestScore == -1f)
			{
				throw new NoCompatibleDevicesException();
			}
			return BuildValidDeviceSettings(deviceCombo, settings, match);
		}

		private DeviceSettings BuildOptimalDeviceSettings(DeviceSettings settings, MatchOptions match)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_0011: Expected Obj, but got Unknown
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0054: Unknown result type (might be due to invalid IL or missing references)
			//IL_0043: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_0116: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_018d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0217: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_02b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_031c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_034f: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b9: Unknown result type (might be due to invalid IL or missing references)
			DeviceSettings deviceSettings = new DeviceSettings();
			deviceSettings.PresentParameters = new PresentParameters();
			if (match.AdapterOrdinal == MatchType.IgnoreInput)
			{
				deviceSettings.AdapterOrdinal = 0u;
			}
			else
			{
				deviceSettings.AdapterOrdinal = settings.AdapterOrdinal;
			}
			if (match.DeviceType == MatchType.IgnoreInput)
			{
				deviceSettings.DeviceType = (DeviceType)1;
			}
			else
			{
				deviceSettings.DeviceType = settings.DeviceType;
			}
			if (match.Windowed == MatchType.IgnoreInput)
			{
				deviceSettings.PresentParameters.Windowed = true;
			}
			else
			{
				deviceSettings.PresentParameters.Windowed = settings.PresentParameters.Windowed;
			}
			if (match.AdapterFormat == MatchType.IgnoreInput)
			{
				DisplayMode currentDisplayMode = Manager.Adapters[(int)deviceSettings.AdapterOrdinal].CurrentDisplayMode;
				if (deviceSettings.PresentParameters.Windowed || ManagedUtility.GetColorChannelBits(currentDisplayMode.Format) >= 8)
				{
					deviceSettings.AdapterFormat = currentDisplayMode.Format;
				}
				else
				{
					deviceSettings.AdapterFormat = (Format)22;
				}
			}
			else
			{
				deviceSettings.AdapterFormat = settings.AdapterFormat;
			}
			if (match.VertexProcessing == MatchType.IgnoreInput)
			{
				deviceSettings.BehaviorFlags = (CreateFlags)64;
			}
			else
			{
				deviceSettings.BehaviorFlags = settings.BehaviorFlags;
			}
			if (match.Resolution == MatchType.IgnoreInput)
			{
				if (deviceSettings.PresentParameters.Windowed)
				{
					deviceSettings.PresentParameters.BackBufferWidth = State.WindowClientSize.Width;
					deviceSettings.PresentParameters.BackBufferHeight = State.WindowClientSize.Height;
				}
				else
				{
					DisplayMode currentDisplayMode2 = Manager.Adapters[(int)deviceSettings.AdapterOrdinal].CurrentDisplayMode;
					deviceSettings.PresentParameters.BackBufferWidth = currentDisplayMode2.Width;
					deviceSettings.PresentParameters.BackBufferHeight = currentDisplayMode2.Height;
				}
			}
			else
			{
				deviceSettings.PresentParameters.BackBufferWidth = settings.PresentParameters.BackBufferWidth;
				deviceSettings.PresentParameters.BackBufferHeight = settings.PresentParameters.BackBufferHeight;
			}
			if (match.BackBufferFormat == MatchType.IgnoreInput)
			{
				deviceSettings.PresentParameters.BackBufferFormat = deviceSettings.AdapterFormat;
			}
			else
			{
				deviceSettings.PresentParameters.BackBufferFormat = settings.PresentParameters.BackBufferFormat;
			}
			if (match.BackBufferCount == MatchType.IgnoreInput)
			{
				deviceSettings.PresentParameters.BackBufferCount = 2;
			}
			else
			{
				deviceSettings.PresentParameters.BackBufferCount = settings.PresentParameters.BackBufferCount;
			}
			if (match.MultiSample == MatchType.IgnoreInput)
			{
				deviceSettings.PresentParameters.MultiSampleQuality = 0;
			}
			else
			{
				deviceSettings.PresentParameters.MultiSampleQuality = settings.PresentParameters.MultiSampleQuality;
			}
			if (match.SwapEffect == MatchType.IgnoreInput)
			{
				deviceSettings.PresentParameters.SwapEffect = (SwapEffect)1;
			}
			else
			{
				deviceSettings.PresentParameters.SwapEffect = settings.PresentParameters.SwapEffect;
			}
			if (match.DepthFormat == MatchType.IgnoreInput && match.StencilFormat == MatchType.IgnoreInput)
			{
				uint colorChannelBits = ManagedUtility.GetColorChannelBits(deviceSettings.PresentParameters.BackBufferFormat);
				if (colorChannelBits >= 8)
				{
					deviceSettings.PresentParameters.AutoDepthStencilFormat = (DepthFormat)71;
				}
				else
				{
					deviceSettings.PresentParameters.AutoDepthStencilFormat = (DepthFormat)80;
				}
			}
			else
			{
				deviceSettings.PresentParameters.AutoDepthStencilFormat = settings.PresentParameters.AutoDepthStencilFormat;
			}
			if (match.PresentFlags == MatchType.IgnoreInput)
			{
				deviceSettings.PresentParameters.PresentFlag = (PresentFlag)2;
			}
			else
			{
				deviceSettings.PresentParameters.PresentFlag = settings.PresentParameters.PresentFlag;
			}
			if (match.RefreshRate == MatchType.IgnoreInput)
			{
				deviceSettings.PresentParameters.FullScreenRefreshRateInHz = 0;
			}
			else
			{
				deviceSettings.PresentParameters.FullScreenRefreshRateInHz = settings.PresentParameters.FullScreenRefreshRateInHz;
			}
			if (match.PresentInterval == MatchType.IgnoreInput)
			{
				deviceSettings.PresentParameters.PresentationInterval = (PresentInterval)int.MinValue;
			}
			else
			{
				deviceSettings.PresentParameters.PresentationInterval = settings.PresentParameters.PresentationInterval;
			}
			return deviceSettings;
		}

		private DeviceSettings BuildValidDeviceSettings(EnumDeviceSettingsCombo deviceCombo, DeviceSettings settings, MatchOptions match)
		{
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_007a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0086: Unknown result type (might be due to invalid IL or missing references)
			//IL_008b: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0053: Unknown result type (might be due to invalid IL or missing references)
			//IL_010d: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0099: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_006a: Unknown result type (might be due to invalid IL or missing references)
			//IL_006d: Unknown result type (might be due to invalid IL or missing references)
			//IL_006e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			//IL_0064: Unknown result type (might be due to invalid IL or missing references)
			//IL_014a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ae: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ba: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bb: Unknown result type (might be due to invalid IL or missing references)
			//IL_00be: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bf: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ee: Unknown result type (might be due to invalid IL or missing references)
			//IL_0209: Unknown result type (might be due to invalid IL or missing references)
			//IL_020b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0210: Unknown result type (might be due to invalid IL or missing references)
			//IL_026e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0274: Invalid comparison between Unknown and I4
			//IL_0106: Unknown result type (might be due to invalid IL or missing references)
			//IL_0109: Unknown result type (might be due to invalid IL or missing references)
			//IL_010a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0100: Unknown result type (might be due to invalid IL or missing references)
			//IL_027a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0297: Unknown result type (might be due to invalid IL or missing references)
			//IL_029c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_037b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0380: Unknown result type (might be due to invalid IL or missing references)
			//IL_0363: Unknown result type (might be due to invalid IL or missing references)
			//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a1: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ab: Invalid comparison between Unknown and I4
			//IL_0394: Unknown result type (might be due to invalid IL or missing references)
			//IL_03d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_03e8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b3: Invalid comparison between Unknown and I4
			//IL_02ea: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_0300: Unknown result type (might be due to invalid IL or missing references)
			//IL_0308: Unknown result type (might be due to invalid IL or missing references)
			//IL_030d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0313: Expected I4, but got Unknown
			//IL_0313: Unknown result type (might be due to invalid IL or missing references)
			//IL_031b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0320: Unknown result type (might be due to invalid IL or missing references)
			//IL_0326: Expected I4, but got Unknown
			//IL_03b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_03bb: Invalid comparison between Unknown and I4
			//IL_0404: Unknown result type (might be due to invalid IL or missing references)
			//IL_0409: Unknown result type (might be due to invalid IL or missing references)
			//IL_040b: Unknown result type (might be due to invalid IL or missing references)
			//IL_048e: Unknown result type (might be due to invalid IL or missing references)
			//IL_032b: Unknown result type (might be due to invalid IL or missing references)
			//IL_032d: Unknown result type (might be due to invalid IL or missing references)
			//IL_03c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_04aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_04af: Unknown result type (might be due to invalid IL or missing references)
			//IL_04b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_058c: Unknown result type (might be due to invalid IL or missing references)
			//IL_057c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0581: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_05a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_05d9: Unknown result type (might be due to invalid IL or missing references)
			//IL_05bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_05e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_05e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_05e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_05c7: Unknown result type (might be due to invalid IL or missing references)
			//IL_0719: Unknown result type (might be due to invalid IL or missing references)
			//IL_071e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0749: Unknown result type (might be due to invalid IL or missing references)
			//IL_0736: Unknown result type (might be due to invalid IL or missing references)
			//IL_0792: Unknown result type (might be due to invalid IL or missing references)
			//IL_0797: Unknown result type (might be due to invalid IL or missing references)
			//IL_079e: Unknown result type (might be due to invalid IL or missing references)
			//IL_07a3: Unknown result type (might be due to invalid IL or missing references)
			//IL_07a9: Unknown result type (might be due to invalid IL or missing references)
			//IL_07aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_07b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_07ba: Expected Obj, but got Unknown
			//IL_07e5: Unknown result type (might be due to invalid IL or missing references)
			//IL_0802: Unknown result type (might be due to invalid IL or missing references)
			//IL_081c: Unknown result type (might be due to invalid IL or missing references)
			//IL_085d: Unknown result type (might be due to invalid IL or missing references)
			//IL_086a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0889: Unknown result type (might be due to invalid IL or missing references)
			//IL_075e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0763: Unknown result type (might be due to invalid IL or missing references)
			//IL_0782: Unknown result type (might be due to invalid IL or missing references)
			//IL_077a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0677: Unknown result type (might be due to invalid IL or missing references)
			//IL_067c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0680: Unknown result type (might be due to invalid IL or missing references)
			//IL_0686: Unknown result type (might be due to invalid IL or missing references)
			DeviceSettings deviceSettings = new DeviceSettings();
			DisplayMode currentDisplayMode = Manager.Adapters[(int)deviceCombo.AdapterOrdinal].CurrentDisplayMode;
			CreateFlags createFlags = (CreateFlags)0;
			if (match.VertexProcessing == MatchType.PreserveInput)
			{
				createFlags = settings.BehaviorFlags;
			}
			else if (match.VertexProcessing == MatchType.IgnoreInput)
			{
				DeviceCaps deviceCaps = deviceCombo.deviceInformation.Caps.DeviceCaps;
				createFlags = (CreateFlags)((!deviceCaps.SupportsHardwareTransformAndLight) ? ((int)createFlags | 0x20) : ((int)createFlags | 0x40));
			}
			else
			{
				createFlags = settings.BehaviorFlags;
				DeviceCaps deviceCaps2 = deviceCombo.deviceInformation.Caps.DeviceCaps;
				if (!deviceCaps2.SupportsHardwareTransformAndLight && (((int)createFlags & 0x40) != 0 || ((int)createFlags & 0x80) != 0))
				{
					createFlags = (CreateFlags)((int)createFlags & -65);
					createFlags = (CreateFlags)((int)createFlags & -129);
					createFlags = (CreateFlags)((int)createFlags | 0x20);
				}
				if (((int)createFlags & 0x40) == 0 && ((int)createFlags & 0x80) == 0 && ((int)createFlags & 0x20) == 0)
				{
					DeviceCaps deviceCaps3 = deviceCombo.deviceInformation.Caps.DeviceCaps;
					createFlags = (CreateFlags)((!deviceCaps3.SupportsHardwareTransformAndLight) ? ((int)createFlags | 0x20) : ((int)createFlags | 0x40));
				}
			}
			DisplayMode displayMode = new DisplayMode();
			if (match.Resolution == MatchType.PreserveInput)
			{
				displayMode.Width = settings.PresentParameters.BackBufferWidth;
				displayMode.Height = settings.PresentParameters.BackBufferHeight;
			}
			else
			{
				DisplayMode requestedMode = new DisplayMode();
				if (match.Resolution == MatchType.ClosestToInput && settings.PresentParameters.BackBufferWidth != 0 && settings.PresentParameters.BackBufferWidth != 0)
				{
					requestedMode.Width = settings.PresentParameters.BackBufferWidth;
					requestedMode.Height = settings.PresentParameters.BackBufferHeight;
				}
				else if (deviceCombo.IsWindowed)
				{
					requestedMode.Width = State.WindowClientSize.Width;
					requestedMode.Height = State.WindowClientSize.Height;
				}
				else
				{
					requestedMode.Width = currentDisplayMode.Width;
					requestedMode.Height = currentDisplayMode.Height;
				}
				displayMode = FindValidResolution(deviceCombo, displayMode);
			}
			uint backBufferCount;
			if (match.BackBufferCount == MatchType.PreserveInput)
			{
				backBufferCount = (uint)settings.PresentParameters.BackBufferCount;
			}
			else if (match.BackBufferCount == MatchType.IgnoreInput)
			{
				backBufferCount = 2u;
			}
			else
			{
				backBufferCount = (uint)settings.PresentParameters.BackBufferCount;
				if (backBufferCount > 3)
				{
					backBufferCount = 3u;
				}
				if (backBufferCount < 1)
				{
					backBufferCount = 1u;
				}
			}
			MultiSampleType multiSampleType;
			uint multiSampleQuality;
			if ((int)settings.PresentParameters.SwapEffect != 1)
			{
				multiSampleType = (MultiSampleType)0;
				multiSampleQuality = 0u;
			}
			else if (match.BackBufferCount == MatchType.PreserveInput)
			{
				multiSampleType = settings.PresentParameters.MultiSample;
				multiSampleQuality = (uint)settings.PresentParameters.MultiSampleQuality;
			}
			else if (match.BackBufferCount == MatchType.IgnoreInput)
			{
				multiSampleType = (MultiSampleType)0;
				multiSampleQuality = 0u;
			}
			else if (match.BackBufferCount == MatchType.ClosestToInput)
			{
				multiSampleType = (MultiSampleType)0;
				multiSampleQuality = 0u;
				for (int i = 0; i < deviceCombo.multiSampleTypeList.Count; i++)
				{
					MultiSampleType candidateSampleType = deviceCombo.multiSampleTypeList[i];
					uint qualityLevels = (uint)deviceCombo.multiSampleQualityList[i];
					if (Math.Abs(candidateSampleType - settings.PresentParameters.MultiSample) < Math.Abs(multiSampleType - settings.PresentParameters.MultiSample))
					{
						multiSampleType = candidateSampleType;
						multiSampleQuality = (uint)Math.Min(qualityLevels - 1, settings.PresentParameters.MultiSampleQuality);
					}
				}
			}
			else
			{
				multiSampleType = (MultiSampleType)0;
				multiSampleQuality = 0u;
			}
			SwapEffect swapEffect;
			if (match.SwapEffect == MatchType.PreserveInput)
			{
				swapEffect = settings.PresentParameters.SwapEffect;
			}
			else if (match.SwapEffect == MatchType.IgnoreInput)
			{
				swapEffect = (SwapEffect)1;
			}
			else
			{
				swapEffect = settings.PresentParameters.SwapEffect;
				if ((int)swapEffect != 1 && (int)swapEffect != 2 && (int)swapEffect != 3)
				{
					swapEffect = (SwapEffect)1;
				}
			}
			int[] scores = new int[deviceCombo.depthStencilFormatList.Count];
			uint colorChannelBits = ManagedUtility.GetColorChannelBits(deviceCombo.BackBufferFormat);
			uint depthBits = ManagedUtility.GetDepthBits(settings.PresentParameters.AutoDepthStencilFormat);
			for (int j = 0; j < deviceCombo.depthStencilFormatList.Count; j++)
			{
				DepthFormat format = deviceCombo.depthStencilFormatList[j];
				uint depthBits2 = ManagedUtility.GetDepthBits(format);
				int depthScore;
				if (match.DepthFormat != MatchType.PreserveInput)
				{
					depthScore = ((match.DepthFormat != MatchType.IgnoreInput) ? Math.Abs((int)(depthBits2 - depthBits)) : Math.Abs((int)(depthBits2 - colorChannelBits * 4)));
				}
				else
				{
					depthScore = ((depthBits2 != depthBits) ? 10000 : 0);
				}
				scores[j] = depthScore;
			}
			uint stencilBits = ManagedUtility.GetStencilBits(settings.PresentParameters.AutoDepthStencilFormat);
			for (int k = 0; k < deviceCombo.depthStencilFormatList.Count; k++)
			{
				DepthFormat format2 = deviceCombo.depthStencilFormatList[k];
				int stencilScore = scores[k];
				uint stencilBits2 = ManagedUtility.GetStencilBits(format2);
				if (match.StencilFormat != MatchType.PreserveInput)
				{
					stencilScore = ((match.StencilFormat != MatchType.IgnoreInput) ? (stencilScore + Math.Abs((int)(stencilBits2 - stencilBits))) : (stencilScore + (int)stencilBits2));
				}
				else
				{
					stencilScore = ((stencilBits2 != stencilBits) ? (stencilScore + 10000) : stencilScore);
				}
				scores[k] = stencilScore;
			}
			int bestScore = 100000;
			int bestIndex = -1;
			for (int l = 0; l < scores.Length; l++)
			{
				if (scores[l] < bestScore)
				{
					bestScore = scores[l];
					bestIndex = l;
				}
			}
			DepthFormat autoDepthStencilFormat;
			bool found;
			if (bestIndex >= 0)
			{
				autoDepthStencilFormat = deviceCombo.depthStencilFormatList[bestIndex];
				found = true;
			}
			else
			{
				autoDepthStencilFormat = (DepthFormat)0;
				found = false;
			}
			PresentFlag presentFlag;
			if (match.PresentFlags == MatchType.PreserveInput)
			{
				presentFlag = settings.PresentParameters.PresentFlag;
			}
			else if (match.PresentFlags == MatchType.IgnoreInput)
			{
				presentFlag = (PresentFlag)0;
				if (found)
				{
					presentFlag = (PresentFlag)2;
				}
			}
			else
			{
				presentFlag = settings.PresentParameters.PresentFlag;
				if (found)
				{
					presentFlag = (PresentFlag)((int)presentFlag | 2);
				}
			}
			if (deviceCombo.IsWindowed)
			{
				displayMode.RefreshRate = 0;
			}
			else if (match.RefreshRate == MatchType.PreserveInput)
			{
				displayMode.RefreshRate = settings.PresentParameters.FullScreenRefreshRateInHz;
			}
			else
			{
				uint wantedRefresh = (uint)((match.RefreshRate != MatchType.ClosestToInput) ? currentDisplayMode.RefreshRate : settings.PresentParameters.FullScreenRefreshRateInHz);
				displayMode.RefreshRate = 0;
				if (wantedRefresh != 0)
				{
					int closestDiff = 100000;
					for (int m = 0; m < deviceCombo.adapterInformation.displayModeList.Count; m++)
					{
						DisplayMode mode = deviceCombo.adapterInformation.displayModeList[m];
						if (mode.Format != deviceCombo.AdapterFormat || mode.Height != displayMode.Height || mode.Width != displayMode.Width)
						{
							continue;
						}
						int refreshDiff = Math.Abs(mode.RefreshRate - (int)wantedRefresh);
						if (refreshDiff < closestDiff)
						{
							displayMode.RefreshRate = mode.RefreshRate;
							closestDiff = refreshDiff;
							if (closestDiff == 0)
							{
								break;
							}
						}
					}
				}
			}
			PresentInterval presentationInterval;
			if (match.PresentInterval == MatchType.PreserveInput)
			{
				presentationInterval = settings.PresentParameters.PresentationInterval;
			}
			else if (match.PresentInterval != MatchType.IgnoreInput)
			{
				if (deviceCombo.presentIntervalList.Contains(settings.PresentParameters.PresentationInterval))
				{
					presentationInterval = settings.PresentParameters.PresentationInterval;
				}
				else
				{
					presentationInterval = ((!deviceCombo.IsWindowed) ? ((PresentInterval)0) : ((PresentInterval)int.MinValue));
				}
			}
			else
			{
				presentationInterval = (PresentInterval)int.MinValue;
			}
			deviceSettings.AdapterOrdinal = deviceCombo.AdapterOrdinal;
			deviceSettings.DeviceType = deviceCombo.DeviceType;
			deviceSettings.AdapterFormat = deviceCombo.AdapterFormat;
			deviceSettings.BehaviorFlags = createFlags;
			deviceSettings.PresentParameters = new PresentParameters();
			deviceSettings.PresentParameters.BackBufferWidth = displayMode.Width;
			deviceSettings.PresentParameters.BackBufferHeight = displayMode.Height;
			deviceSettings.PresentParameters.BackBufferFormat = deviceCombo.BackBufferFormat;
			deviceSettings.PresentParameters.BackBufferCount = (int)backBufferCount;
			deviceSettings.PresentParameters.MultiSample = multiSampleType;
			deviceSettings.PresentParameters.MultiSampleQuality = (int)multiSampleQuality;
			deviceSettings.PresentParameters.SwapEffect = swapEffect;
			deviceSettings.PresentParameters.DeviceWindow = State.Window;
			deviceSettings.PresentParameters.Windowed = deviceCombo.IsWindowed;
			deviceSettings.PresentParameters.EnableAutoDepthStencil = found;
			deviceSettings.PresentParameters.AutoDepthStencilFormat = autoDepthStencilFormat;
			deviceSettings.PresentParameters.PresentFlag = presentFlag;
			deviceSettings.PresentParameters.FullScreenRefreshRateInHz = displayMode.RefreshRate;
			deviceSettings.PresentParameters.PresentationInterval = presentationInterval;
			deviceSettings.PresentParameters.ForceNoMultiThreadedFlag = false;
			return deviceSettings;
		}

		private bool DoesDeviceComboMatchPreserveOptions(EnumDeviceSettingsCombo deviceCombo, DeviceSettings settings, MatchOptions match)
		{
			//IL_002e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0073: Unknown result type (might be due to invalid IL or missing references)
			//IL_0079: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009a: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_015b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0166: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e9: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ef: Unknown result type (might be due to invalid IL or missing references)
			//IL_024e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0214: Unknown result type (might be due to invalid IL or missing references)
			//IL_0192: Unknown result type (might be due to invalid IL or missing references)
			//IL_0197: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_01b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_02cb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0261: Unknown result type (might be due to invalid IL or missing references)
			//IL_022a: Unknown result type (might be due to invalid IL or missing references)
			//IL_02de: Unknown result type (might be due to invalid IL or missing references)
			//IL_03ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_027d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0282: Unknown result type (might be due to invalid IL or missing references)
			//IL_0284: Unknown result type (might be due to invalid IL or missing references)
			//IL_035a: Unknown result type (might be due to invalid IL or missing references)
			//IL_035f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0363: Unknown result type (might be due to invalid IL or missing references)
			//IL_0369: Unknown result type (might be due to invalid IL or missing references)
			//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ff: Unknown result type (might be due to invalid IL or missing references)
			//IL_0301: Unknown result type (might be due to invalid IL or missing references)
			if (match.AdapterOrdinal == MatchType.PreserveInput && deviceCombo.AdapterOrdinal != settings.AdapterOrdinal)
			{
				return false;
			}
			if (match.DeviceType == MatchType.PreserveInput && deviceCombo.DeviceType != settings.DeviceType)
			{
				return false;
			}
			if (match.Windowed == MatchType.PreserveInput && deviceCombo.IsWindowed != settings.PresentParameters.Windowed)
			{
				return false;
			}
			if (match.AdapterFormat == MatchType.PreserveInput && deviceCombo.AdapterFormat != settings.AdapterFormat)
			{
				return false;
			}
			if (match.VertexProcessing == MatchType.PreserveInput && ((int)settings.BehaviorFlags & 0x40) != 0)
			{
				DeviceCaps deviceCaps = deviceCombo.deviceInformation.Caps.DeviceCaps;
				if (deviceCaps.SupportsHardwareTransformAndLight)
				{
					return false;
				}
			}
			if (match.Resolution == MatchType.PreserveInput)
			{
				bool formatSupported = false;
				for (int i = 0; i < deviceCombo.adapterInformation.displayModeList.Count; i++)
				{
					DisplayMode mode = deviceCombo.adapterInformation.displayModeList[i];
					if (mode.Format == deviceCombo.AdapterFormat && mode.Width == settings.PresentParameters.BackBufferWidth && mode.Height == settings.PresentParameters.BackBufferHeight)
					{
						formatSupported = true;
						break;
					}
				}
				if (!formatSupported)
				{
					return false;
				}
			}
			if (match.BackBufferFormat == MatchType.PreserveInput && deviceCombo.BackBufferFormat != settings.PresentParameters.BackBufferFormat)
			{
				return false;
			}
			if (match.MultiSample == MatchType.PreserveInput)
			{
				bool sampleSupported = false;
				for (int j = 0; j < deviceCombo.multiSampleTypeList.Count; j++)
				{
					MultiSampleType sampleType = deviceCombo.multiSampleTypeList[j];
					uint quality = (uint)deviceCombo.multiSampleQualityList[j];
					if (sampleType == settings.PresentParameters.MultiSample && (ulong)quality >= (ulong)settings.PresentParameters.MultiSampleQuality)
					{
						sampleSupported = true;
						break;
					}
				}
				if (!sampleSupported)
				{
					return false;
				}
			}
			if (match.DepthFormat == MatchType.PreserveInput && match.StencilFormat == MatchType.PreserveInput && (int)settings.PresentParameters.AutoDepthStencilFormat != 0 && !deviceCombo.depthStencilFormatList.Contains(settings.PresentParameters.AutoDepthStencilFormat))
			{
				return false;
			}
			if (match.DepthFormat == MatchType.PreserveInput && (int)settings.PresentParameters.AutoDepthStencilFormat != 0)
			{
				bool depthSupported = false;
				uint depthBits = ManagedUtility.GetDepthBits(settings.PresentParameters.AutoDepthStencilFormat);
				for (int k = 0; k < deviceCombo.depthStencilFormatList.Count; k++)
				{
					DepthFormat format = deviceCombo.depthStencilFormatList[k];
					uint depthBits2 = ManagedUtility.GetDepthBits(format);
					if (depthBits2 - depthBits == 0)
					{
						depthSupported = true;
					}
				}
				if (!depthSupported)
				{
					return false;
				}
			}
			if (match.StencilFormat == MatchType.PreserveInput && (int)settings.PresentParameters.AutoDepthStencilFormat != 0)
			{
				bool stencilSupported = false;
				uint stencilBits = ManagedUtility.GetStencilBits(settings.PresentParameters.AutoDepthStencilFormat);
				for (int l = 0; l < deviceCombo.depthStencilFormatList.Count; l++)
				{
					DepthFormat format2 = deviceCombo.depthStencilFormatList[l];
					uint stencilBits2 = ManagedUtility.GetStencilBits(format2);
					if (stencilBits2 - stencilBits == 0)
					{
						stencilSupported = true;
					}
				}
				if (!stencilSupported)
				{
					return false;
				}
			}
			if (match.RefreshRate == MatchType.PreserveInput)
			{
				bool refreshSupported = false;
				for (int m = 0; m < deviceCombo.adapterInformation.displayModeList.Count; m++)
				{
					DisplayMode refreshMode = deviceCombo.adapterInformation.displayModeList[m];
					if (refreshMode.Format == deviceCombo.AdapterFormat && refreshMode.RefreshRate == settings.PresentParameters.FullScreenRefreshRateInHz)
					{
						refreshSupported = true;
						break;
					}
				}
				if (!refreshSupported)
				{
					return false;
				}
			}
			if (match.PresentInterval == MatchType.PreserveInput && !deviceCombo.presentIntervalList.Contains(settings.PresentParameters.PresentationInterval))
			{
				return false;
			}
			return true;
		}

		private float RankDeviceCombo(EnumDeviceSettingsCombo deviceCombo, DeviceSettings settings, DisplayMode adapterMode)
		{
			//IL_0020: Unknown result type (might be due to invalid IL or missing references)
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Invalid comparison between Unknown and I4
			//IL_006b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0071: Unknown result type (might be due to invalid IL or missing references)
			//IL_0089: Unknown result type (might be due to invalid IL or missing references)
			//IL_0094: Unknown result type (might be due to invalid IL or missing references)
			//IL_0114: Unknown result type (might be due to invalid IL or missing references)
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_013d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0122: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00fb: Unknown result type (might be due to invalid IL or missing references)
			//IL_0102: Invalid comparison between Unknown and I4
			//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ed: Unknown result type (might be due to invalid IL or missing references)
			//IL_0163: Unknown result type (might be due to invalid IL or missing references)
			//IL_0168: Unknown result type (might be due to invalid IL or missing references)
			//IL_0196: Unknown result type (might be due to invalid IL or missing references)
			//IL_019b: Unknown result type (might be due to invalid IL or missing references)
			//IL_019f: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
			//IL_020a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0215: Unknown result type (might be due to invalid IL or missing references)
			//IL_022d: Unknown result type (might be due to invalid IL or missing references)
			//IL_023d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0276: Unknown result type (might be due to invalid IL or missing references)
			//IL_027c: Unknown result type (might be due to invalid IL or missing references)
			//IL_02a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_02ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_02bd: Unknown result type (might be due to invalid IL or missing references)
			//IL_02c5: Unknown result type (might be due to invalid IL or missing references)
			//IL_031b: Unknown result type (might be due to invalid IL or missing references)
			//IL_034a: Unknown result type (might be due to invalid IL or missing references)
			//IL_034f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0353: Unknown result type (might be due to invalid IL or missing references)
			//IL_0359: Unknown result type (might be due to invalid IL or missing references)
			//IL_03b2: Unknown result type (might be due to invalid IL or missing references)
			float score = 0f;
			if (deviceCombo.AdapterOrdinal == settings.AdapterOrdinal)
			{
				score += 1000f;
			}
			if (deviceCombo.DeviceType == settings.DeviceType)
			{
				score += 100f;
			}
			if ((int)deviceCombo.DeviceType == 1)
			{
				score += 0.1f;
			}
			if (deviceCombo.IsWindowed == settings.PresentParameters.Windowed)
			{
				score += 10f;
			}
			if (deviceCombo.AdapterFormat == settings.AdapterFormat)
			{
				score++;
			}
			else
			{
				int adapterColorDiff = Math.Abs((int)(ManagedUtility.GetColorChannelBits(deviceCombo.AdapterFormat) - ManagedUtility.GetColorChannelBits(settings.AdapterFormat)));
				float adapterColorBonus = Math.Max(0.9f - (float)adapterColorDiff * 0.2f, 0f);
				score += adapterColorBonus * 1f;
			}
			if (!deviceCombo.IsWindowed && ((ManagedUtility.GetColorChannelBits(adapterMode.Format) < 8) ? ((int)deviceCombo.AdapterFormat == 22) : (deviceCombo.AdapterFormat == adapterMode.Format)))
			{
				score += 0.1f;
			}
			if (((int)settings.BehaviorFlags & 0x40) != 0 || ((int)settings.BehaviorFlags & 0x80) != 0)
			{
				DeviceCaps deviceCaps = deviceCombo.deviceInformation.Caps.DeviceCaps;
				if (deviceCaps.SupportsHardwareTransformAndLight)
				{
					score++;
				}
			}
			DeviceCaps deviceCaps2 = deviceCombo.deviceInformation.Caps.DeviceCaps;
			if (deviceCaps2.SupportsHardwareTransformAndLight)
			{
				score += 0.1f;
			}
			bool resolutionMatched = false;
			for (int i = 0; i < deviceCombo.adapterInformation.displayModeList.Count; i++)
			{
				DisplayMode mode = deviceCombo.adapterInformation.displayModeList[i];
				if (mode.Format == deviceCombo.AdapterFormat && mode.Width == settings.PresentParameters.BackBufferWidth && mode.Height == settings.PresentParameters.BackBufferHeight)
				{
					resolutionMatched = true;
				}
			}
			if (resolutionMatched)
			{
				score++;
			}
			if (deviceCombo.BackBufferFormat == settings.PresentParameters.BackBufferFormat)
			{
				score++;
			}
			else
			{
				int depthDiff = Math.Abs((int)(ManagedUtility.GetColorChannelBits(deviceCombo.BackBufferFormat) - ManagedUtility.GetColorChannelBits(settings.PresentParameters.BackBufferFormat)));
				float sampleBonus = Math.Max(0.9f - (float)depthDiff * 0.2f, 0f);
				score += sampleBonus * 1f;
			}
			if (deviceCombo.BackBufferFormat == deviceCombo.AdapterFormat)
			{
				score += 0.1f;
			}
			bool sampleSupported = false;
			for (int j = 0; j < deviceCombo.multiSampleTypeList.Count; j++)
			{
				MultiSampleType sampleType = deviceCombo.multiSampleTypeList[j];
				uint quality = (uint)deviceCombo.multiSampleQualityList[j];
				if (sampleType == settings.PresentParameters.MultiSample && (ulong)quality >= (ulong)settings.PresentParameters.MultiSampleQuality)
				{
					sampleSupported = true;
					break;
				}
			}
			if (sampleSupported)
			{
				score++;
			}
			if (deviceCombo.depthStencilFormatList.Contains(settings.PresentParameters.AutoDepthStencilFormat))
			{
				score++;
			}
			bool refreshSupported = false;
			for (int k = 0; k < deviceCombo.adapterInformation.displayModeList.Count; k++)
			{
				DisplayMode refreshMode = deviceCombo.adapterInformation.displayModeList[k];
				if (refreshMode.Format == deviceCombo.AdapterFormat && refreshMode.RefreshRate == settings.PresentParameters.FullScreenRefreshRateInHz)
				{
					refreshSupported = true;
				}
			}
			if (refreshSupported)
			{
				score++;
			}
			if (deviceCombo.presentIntervalList.Contains(settings.PresentParameters.PresentationInterval))
			{
				score++;
			}
			return score;
		}

		private DisplayMode FindValidResolution(EnumDeviceSettingsCombo deviceCombo, DisplayMode displayMode)
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_011e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			//IL_0127: Unknown result type (might be due to invalid IL or missing references)
			//IL_012d: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_016c: Unknown result type (might be due to invalid IL or missing references)
			DisplayMode result = displayMode;
			if (deviceCombo.IsWindowed)
			{
				EnumAdapterInformation adapterInformation = Enumeration.GetAdapterInformation(deviceCombo.AdapterOrdinal);
				Screen screen = null;
				string adapterName = adapterInformation.AdapterInformation.DeviceName.ToLower();
				Screen[] allScreens = Screen.AllScreens;
				foreach (Screen screen2 in allScreens)
				{
					string screenName = screen2.DeviceName.ToLower();
					if (screenName.IndexOf("\0") > 0)
					{
						screenName = screenName.Substring(0, screenName.IndexOf("\0"));
					}
					if (screenName == adapterName)
					{
						screen = screen2;
						break;
					}
				}
				if (result.Width > screen.WorkingArea.Width)
				{
					result.Width = screen.WorkingArea.Width;
				}
				if (result.Height > screen.WorkingArea.Height)
				{
					result.Height = screen.WorkingArea.Height;
				}
			}
			else
			{
				int bestDifference = 100000;
				for (int j = 0; j < deviceCombo.adapterInformation.displayModeList.Count; j++)
				{
					DisplayMode mode = deviceCombo.adapterInformation.displayModeList[j];
					if (mode.Format != deviceCombo.AdapterFormat)
					{
						continue;
					}
					int difference = Math.Abs(mode.Width - displayMode.Width) + Math.Abs(mode.Height - displayMode.Height);
					if (difference < bestDifference)
					{
						result = displayMode;
						bestDifference = difference;
						if (bestDifference == 0)
						{
							break;
						}
					}
				}
				if (result.Width == 0)
				{
					throw new NoCompatibleDevicesException();
				}
			}
			return result;
		}

		private void ChangeDevice(DeviceSettings newDeviceSettings)
		{
			//IL_011b: Unknown result type (might be due to invalid IL or missing references)
			//IL_012c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0142: Unknown result type (might be due to invalid IL or missing references)
			//IL_0148: Expected Obj, but got Unknown
			//IL_00b0: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c7: Unknown result type (might be due to invalid IL or missing references)
			if (isDisposed)
			{
				return;
			}
			DeviceSettings deviceSettings = null;
			if (State.CurrentDeviceSettings != null)
			{
				deviceSettings = State.CurrentDeviceSettings.Clone();
			}
			Pause(true, true);
			if (newDeviceSettings.PresentParameters.Windowed && State.Window != null)
			{
				Size windowClientSize = State.WindowClientSize;
				windowClientSize = new Size(newDeviceSettings.PresentParameters.BackBufferWidth, newDeviceSettings.PresentParameters.BackBufferHeight);
				State.WindowClientSize = windowClientSize;
			}
			State.CurrentDeviceSettings = newDeviceSettings;
			if (deviceSettings != null && deviceSettings.AdapterOrdinal == newDeviceSettings.AdapterOrdinal && deviceSettings.DeviceType == newDeviceSettings.DeviceType && deviceSettings.BehaviorFlags == newDeviceSettings.BehaviorFlags)
			{
				try
				{
					ResetEnvironment();
				}
				catch
				{
					Pause(false, false);
					State.IsDeviceLost = true;
				}
			}
			else
			{
				if (deviceSettings != null)
				{
					CleanupEnvironment(false);
				}
				Device newDevice;
				try
				{
					State.Window.TopMost = true;
					newDevice = new Device((int)newDeviceSettings.AdapterOrdinal, newDeviceSettings.DeviceType, (Control)State.Window, newDeviceSettings.BehaviorFlags, new PresentParameters[1] { newDeviceSettings.PresentParameters });
					State.Window.TopMost = false;
				}
				catch (Exception inner)
				{
					Pause(false, false);
					CreatingDeviceException e = new CreatingDeviceException(inner);
					DisplayErrorMessage(e);
					throw;
				}
				newDevice.DeviceLost += OnDeviceLost;
				newDevice.DeviceReset += OnDeviceReset;
				newDevice.Disposing += OnDeviceDisposing;
				State.Device = newDevice;
				try
				{
					InitializeEnvironment();
				}
				catch
				{
					Pause(false, false);
					throw;
				}
				Enumeration.Enumerate();
				Enumeration.GetAdapterInformation(newDeviceSettings.AdapterOrdinal);
			}
			State.AdapterMonitor = Manager.GetAdapterMonitor((int)newDeviceSettings.AdapterOrdinal);
			if (!newDeviceSettings.PresentParameters.Windowed)
			{
				State.FullScreenClientSize = new Size(newDeviceSettings.PresentParameters.BackBufferWidth, newDeviceSettings.PresentParameters.BackBufferHeight);
			}
			Pause(false, false);
			State.WasDeviceCreated = true;
		}

		public void ResetEnvironment()
		{
			Device device = State.Device;
			if (device == (Device)null)
			{
				throw new InvalidOperationException("The device should not be null.");
			}
			if (DeviceLost != null)
			{
				DeviceLost(this, EventArgs.Empty);
			}
			AdjustWindowStyle(GameWindow, IsWindowed);
			try
			{
				device.Reset(new PresentParameters[1] { State.CurrentDeviceSettings.PresentParameters });
			}
			catch (Exception ex)
			{
				if (ex is MediaNotFoundException)
				{
					DisplayErrorMessage(ex as MediaNotFoundException);
				}
				else if (!(ex is DeviceLostException))
				{
					DisplayErrorMessage(new ResettingDeviceException(ex));
				}
				OnDeviceLost(null, EventArgs.Empty);
				if (!(ex is DeviceLostException))
				{
					throw;
				}
			}
		}

		public List<string> GetAdaptersList()
		{
			List<string> descriptions = new List<string>();
			Enumeration.Enumerate();
			List<EnumAdapterInformation> adapterInformationList = Enumeration.AdapterInformationList;
			foreach (EnumAdapterInformation item in adapterInformationList)
			{
				descriptions.Add(item.UniqueDescription);
			}
			return descriptions;
		}

		public List<Size> GetResolutionsList(int adapterOrdinal)
		{
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Invalid comparison between Unknown and I4
			Enumeration.Enumerate();
			if (Enumeration.AdapterInformationList.Count <= adapterOrdinal)
			{
				return null;
			}
			List<Size> resolutions = new List<Size>();
			List<DisplayMode> displayModeList = Enumeration.GetAdapterInformation((uint)adapterOrdinal).displayModeList;
			foreach (DisplayMode item2 in displayModeList)
			{
				DisplayMode current = item2;
				if ((int)current.Format == 22)
				{
					Size item = new Size(current.Width, current.Height);
					if (!resolutions.Contains(item))
					{
						resolutions.Add(item);
					}
				}
			}
			return resolutions;
		}

		public void RenderEnvironment()
		{
			//IL_026b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0270: Unknown result type (might be due to invalid IL or missing references)
			//IL_00bc: Unknown result type (might be due to invalid IL or missing references)
			//IL_00c1: Unknown result type (might be due to invalid IL or missing references)
			//IL_00cd: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_016f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0174: Unknown result type (might be due to invalid IL or missing references)
			Device device = State.Device;
			if (device == (Device)null)
			{
				return;
			}
			if (State.IsDeviceLost || State.IsRenderingPaused)
			{
				Thread.Sleep(100);
			}
			if (!State.IsActive)
			{
				Thread.Sleep(20);
			}
			if (State.IsDeviceLost && !State.IsRenderingPaused)
			{
				int cooperativeLevel = default(int);
				if (!device.CheckCooperativeLevel(out cooperativeLevel))
				{
					if (cooperativeLevel == -2005530520)
					{
						Thread.Sleep(50);
						return;
					}
					if (IsWindowed)
					{
						DisplayMode currentDisplayMode = Manager.Adapters[(int)State.CurrentDeviceSettings.AdapterOrdinal].CurrentDisplayMode;
						if (State.CurrentDeviceSettings.AdapterFormat != currentDisplayMode.Format)
						{
							MatchOptions match = default(MatchOptions);
							match.AdapterOrdinal = MatchType.PreserveInput;
							match.DeviceType = MatchType.PreserveInput;
							match.Windowed = MatchType.PreserveInput;
							match.AdapterFormat = MatchType.PreserveInput;
							match.VertexProcessing = MatchType.ClosestToInput;
							match.Resolution = MatchType.PreserveInput;
							match.BackBufferFormat = MatchType.ClosestToInput;
							match.BackBufferCount = MatchType.ClosestToInput;
							match.MultiSample = MatchType.ClosestToInput;
							match.SwapEffect = MatchType.ClosestToInput;
							match.DepthFormat = MatchType.ClosestToInput;
							match.StencilFormat = MatchType.ClosestToInput;
							match.PresentFlags = MatchType.ClosestToInput;
							match.RefreshRate = MatchType.ClosestToInput;
							match.PresentInterval = MatchType.ClosestToInput;
							DeviceSettings currentDeviceSettings = State.CurrentDeviceSettings;
							currentDeviceSettings.AdapterFormat = currentDisplayMode.Format;
							try
							{
								currentDeviceSettings = FindValidDeviceSettings(currentDeviceSettings, match);
								ChangeDevice(currentDeviceSettings);
								return;
							}
							catch
							{
								DisplayErrorMessage(new NoCompatibleDevicesException());
								Dispose();
								return;
							}
						}
					}
					try
					{
						ResetEnvironment();
					}
					catch (DeviceLostException)
					{
						Thread.Sleep(50);
						return;
					}
					catch
					{
						DeviceSettings currentDeviceSettings2 = State.CurrentDeviceSettings;
						try
						{
							ChangeDevice(currentDeviceSettings2);
						}
						catch (Exception inner)
						{
							ResettingDeviceException e = new ResettingDeviceException(inner);
							DisplayErrorMessage(e);
							Dispose();
							throw;
						}
					}
				}
				State.IsDeviceLost = false;
			}
			double time = GameCoreTimer.GetTime();
			float elapsed = (float)GameCoreTimer.GetElapsedTime();
			if (elapsed == 0f)
			{
				return;
			}
			if (time < nextRenderTime)
			{
				nowElapsedTime += elapsed;
				Thread.Sleep((int)((nextRenderTime - time) * 1000.0));
				return;
			}
			do
			{
				double scheduled = nextRenderTime;
				DisplayMode displayMode = device.DisplayMode;
				nextRenderTime = scheduled + 1.0 / (double)displayMode.RefreshRate;
			}
			while (time >= nextRenderTime);
			elapsed += nowElapsedTime;
			nowElapsedTime = 0f;
			State.CurrentTime = time;
			State.ElapsedTime = elapsed;
			HandleTimers();
			UpdateFrameStats();
			if (FrameMove != null)
			{
				FrameMove(device, new RenderEventArgs(time, elapsed));
				device = State.Device;
				if (device == (Device)null)
				{
					return;
				}
			}
			if (!State.IsRenderingPaused)
			{
				device.Clear((ClearFlags)3, Color.Black, 1f, 0);
				if (showFps && fpsFont != (Microsoft.DirectX.Direct3D.Font)null)
				{
					fpsFont.DrawText((Sprite)null, State.CurrentFrameRate.ToString("f0"), 0, 0, Color.White);
				}
				if (FrameRender != null)
				{
					FrameRender(device, new RenderEventArgs(time, elapsed));
				}
				device = State.Device;
				if (device == (Device)null)
				{
					return;
				}
			}
			if (!State.IsRenderingPaused)
			{
				try
				{
					device.Present();
				}
				catch (DeviceLostException)
				{
					State.IsDeviceLost = true;
				}
				catch (DriverInternalErrorException)
				{
					State.IsDeviceLost = true;
				}
			}
			State.CurrentFrameNumber++;
		}

		private void Shutdown()
		{
			CleanupEnvironment(true);
			if (GameWindow != null)
			{
				GameWindow.Close();
			}
		}

		public void Pause(bool pauseTime, bool pauseRendering)
		{
			State.PauseTimeCount += (pauseTime ? 1 : (-1));
			if (State.PauseTimeCount < 0)
			{
				State.PauseTimeCount = 0;
			}
			State.PauseRenderingCount += (pauseRendering ? 1 : (-1));
			if (State.PauseRenderingCount < 0)
			{
				State.PauseRenderingCount = 0;
			}
			if (State.PauseTimeCount > 0)
			{
				GameCoreTimer.Stop();
				if (OnPause != null)
				{
					OnPause(this, EventArgs.Empty);
				}
			}
			else
			{
				GameCoreTimer.Start();
				if (OnResume != null)
				{
					OnResume(this, EventArgs.Empty);
				}
			}
			State.IsRenderingPaused = State.PauseRenderingCount > 0;
			State.IsTimePaused = State.PauseTimeCount > 0;
		}

		public void DisplayErrorMessage(Exception e)
		{
			if (e == lastDisplayedMessage)
			{
				return;
			}
			lastDisplayedMessage = e;
			State.ApplicationExitCode = 1;
			bool gotMessage = true;
			if (e is ApplicationException)
			{
				if (e is NoDirect3DException)
				{
					State.ApplicationExitCode = 2;
				}
				else if (e is MediaNotFoundException)
				{
					State.ApplicationExitCode = 4;
				}
				else if (e is CreatingDeviceException)
				{
					State.ApplicationExitCode = 6;
				}
				else if (e is ResettingDeviceException)
				{
					State.ApplicationExitCode = 7;
				}
				else if (e is CreatingDeviceObjectsException)
				{
					State.ApplicationExitCode = 8;
				}
				else if (e is ResettingDeviceObjectsException)
				{
					State.ApplicationExitCode = 9;
				}
				else if (e is NoCompatibleDevicesException)
				{
					State.ApplicationExitCode = 3;
				}
				else
				{
					gotMessage = false;
				}
			}
			else
			{
				gotMessage = false;
			}
			string empty = string.Empty;
			if (gotMessage && State.IsShowingMsgBoxOnError)
			{
				empty = e.Message;
				if (e.InnerException != null)
				{
					empty = empty + "\r\n\r\n" + e.InnerException.ToString();
					lastDisplayedMessage = e.InnerException;
				}
				MessageBox.Show(empty, "DirectX Application", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
			else if (!gotMessage && State.IsShowingMsgBoxOnError)
			{
				empty = e.ToString();
				MessageBox.Show(empty, "DirectX Application", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}

		private void CleanupEnvironment(bool releaseSettings)
		{
			//IL_0024: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			Device device = State.Device;
			try
			{
				if (releaseSettings)
				{
					State.CurrentDeviceSettings = null;
				}
				State.BackBufferSurfaceDesc = new SurfaceDescription();
				State.Caps = new Caps();
				State.WasDeviceCreated = false;
			}
			finally
			{
				if (device != null)
				{
					((IDisposable)device).Dispose();
				}
			}
			State.Device = null;
		}

		private void InitializeEnvironment()
		{
			//IL_0065: Unknown result type (might be due to invalid IL or missing references)
			//IL_006f: Expected Obj, but got Unknown
			//IL_0088: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ea: Unknown result type (might be due to invalid IL or missing references)
			Device device = State.Device;
			if (device == (Device)null)
			{
				throw new InvalidOperationException("The device should not be null.");
			}
			bool succeeded = true;
			try
			{
				AdjustWindowStyle(GameWindow, IsWindowed);
				PrepareDevice(device);
				try
				{
					State.IsInsideDeviceCallback = true;
					fpsFont = new Microsoft.DirectX.Direct3D.Font(State.Device, new System.Drawing.Font(FontFamily.GenericSansSerif, 18f));
					if (DeviceCreated != null)
					{
						DeviceCreated(this, new DeviceEventArgs(device, State.BackBufferSurfaceDesc));
					}
					State.IsInsideDeviceCallback = false;
				}
				catch (MediaNotFoundException e)
				{
					DisplayErrorMessage(e);
					succeeded = false;
					throw;
				}
				catch (Exception inner)
				{
					DisplayErrorMessage(new CreatingDeviceException(inner));
					succeeded = false;
					throw;
				}
				try
				{
					State.IsInsideDeviceCallback = true;
					if (DeviceReset != null)
					{
						DeviceReset(this, new DeviceEventArgs(device, State.BackBufferSurfaceDesc));
					}
					State.IsInsideDeviceCallback = false;
				}
				catch (MediaNotFoundException e2)
				{
					DisplayErrorMessage(e2);
					succeeded = false;
					throw;
				}
				catch (Exception inner2)
				{
					DisplayErrorMessage(new CreatingDeviceException(inner2));
					succeeded = false;
					throw;
				}
			}
			finally
			{
				if (!succeeded)
				{
					CleanupEnvironment(true);
				}
			}
		}

		private void PrepareDevice(Device device)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			Surface backBuffer = device.GetBackBuffer(0, 0, (BackBufferType)0);
			try
			{
				State.BackBufferSurfaceDesc = backBuffer.Description;
			}
			finally
			{
				if (backBuffer != null)
				{
					((IDisposable)backBuffer).Dispose();
				}
			}
			State.Caps = device.DeviceCaps;
		}

		private void AdjustWindowStyle(Form window, bool isWindowed)
		{
			if (window != null)
			{
				if (isWindowed)
				{
					window.FormBorderStyle = FormBorderStyle.FixedSingle;
				}
				else
				{
					window.FormBorderStyle = FormBorderStyle.None;
				}
			}
		}

		private void UpdateFrameStats()
		{
			double absoluteTime = GameCoreTimer.GetAbsoluteTime();
			State.LastStatsUpdateFrames++;
			if (absoluteTime - State.LastStatsUpdateTime > 0.1)
			{
				float currentFrameRate = (float)((double)State.LastStatsUpdateFrames / (absoluteTime - State.LastStatsUpdateTime));
				State.CurrentFrameRate = currentFrameRate;
				State.LastStatsUpdateFrames = 0u;
				State.LastStatsUpdateTime = absoluteTime;
			}
		}

		public void ResetState()
		{
			State = new GameCoreData();
		}

		public int SetTimer(TimerCallback callbackTimer, float timeoutInSeconds)
		{
			if (callbackTimer == null)
			{
				throw new ArgumentNullException("callbackTimer", "You must pass in a valid timer callback .");
			}
			GameTimer item = default(GameTimer);
			item.Callback = callbackTimer;
			item.Countdown = timeoutInSeconds;
			item.TimeoutInSecs = timeoutInSeconds;
			item.IsEnabled = true;
			State.Timers.Add(item);
			return State.Timers.Count;
		}

		public void KillTimer(int id)
		{
			if (id < State.Timers.Count)
			{
				GameTimer value = State.Timers[id];
				value.IsEnabled = false;
				State.Timers[id] = value;
				return;
			}
			throw new ArgumentException("An invalid timer ID was passed in", "id");
		}

		private void HandleTimers()
		{
			float elapsedTime = State.ElapsedTime;
			for (int i = 0; i < State.Timers.Count; i++)
			{
				GameTimer value = State.Timers[i];
				if (value.IsEnabled)
				{
					value.Countdown -= elapsedTime;
					if (value.Countdown < 0f)
					{
						value.Callback((uint)i);
						value.Countdown = value.TimeoutInSecs;
					}
					State.Timers[i] = value;
				}
			}
		}

		public void CloseWindow()
		{
			if (GameWindow != null)
			{
				GameWindow.Close();
			}
		}

		private void OnPaint(object sender, PaintEventArgs e)
		{
			if (State.Device != (Device)null && State.IsRenderingPaused)
			{
				double currentTime = State.CurrentTime;
				float elapsedTime = State.ElapsedTime;
				if (FrameRender != null)
				{
					FrameRender(State.Device, new RenderEventArgs(currentTime, elapsedTime));
				}
				try
				{
					State.Device.Present();
				}
				catch (DeviceLostException)
				{
					State.IsDeviceLost = true;
				}
				catch (DriverInternalErrorException)
				{
					State.IsDeviceLost = true;
				}
			}
		}

		private void OnResize(object sender, EventArgs e)
		{
			Form form = sender as Form;
			if (form == null)
			{
				return;
			}
			if (form.WindowState == FormWindowState.Minimized)
			{
				State.IsMinimized = true;
				Pause(true, true);
				return;
			}
			if (State.IsMinimized)
			{
				Pause(false, false);
			}
			State.IsMinimized = false;
		}

		private void OnFormClosed(object sender, EventArgs e)
		{
			State.Window = null;
		}

		private void OnFormActivated(object sender, EventArgs e)
		{
			State.IsActive = true;
		}

		private void OnFormDeactivated(object sender, EventArgs e)
		{
			State.IsActive = false;
		}

		private void OnMenuStart(object sender, EventArgs e)
		{
			Pause(true, true);
		}

		private void OnMenuEnd(object sender, EventArgs e)
		{
			Pause(false, false);
		}

		private void HookEvents(Form form)
		{
			form.Paint += OnPaint;
			form.Resize += OnResize;
			form.Closed += OnFormClosed;
			form.Activated += OnFormActivated;
			form.Deactivate += OnFormDeactivated;
			form.MenuStart += OnMenuStart;
			form.MenuComplete += OnMenuEnd;
			form.MouseMove += OnMouseMove;
			form.MouseDown += OnMouseDown;
			form.MouseUp += OnMouseUp;
			form.MouseWheel += OnMouseWheel;
		}

		protected virtual void OnMouseMove(object sender, MouseEventArgs e)
		{
			if (MouseMove != null)
			{
				MouseMove(this, e);
			}
		}

		protected virtual void OnMouseDown(object sender, MouseEventArgs e)
		{
			if (MouseDown != null)
			{
				MouseDown(this, e);
			}
		}

		protected virtual void OnMouseUp(object sender, MouseEventArgs e)
		{
			if (MouseUp != null)
			{
				MouseUp(this, e);
			}
		}

		protected virtual void OnMouseWheel(object sender, MouseEventArgs e)
		{
			if (MouseWheel != null)
			{
				MouseWheel(this, e);
			}
		}

		internal void WindowsProcedure(ref Message m)
		{
			if (State.WndProcFunction != null)
			{
				bool NoFurtherProcessing = false;
				IntPtr result = State.WndProcFunction(m.HWnd, (NativeMethods.WindowMessage)m.Msg, m.WParam, m.LParam, ref NoFurtherProcessing);
				if (NoFurtherProcessing)
				{
					m.Result = result;
				}
			}
			switch ((NativeMethods.WindowMessage)m.Msg)
			{
			case NativeMethods.WindowMessage.PowerBroadcast:
				if (m.WParam == IntPtr.Zero)
				{
					m.Result = TrueIntPtr;
				}
				if (m.WParam.ToInt32() == 7)
				{
					GameCoreTimer.Reset();
					State.LastStatsUpdateTime = 0.0;
					m.Result = TrueIntPtr;
				}
				break;
			case NativeMethods.WindowMessage.SystemCommand:
				switch (m.WParam.ToInt32())
				{
				case 61440:
				case 61456:
				case 61488:
				case 61696:
				case 61760:
				case 61808:
					m.Result = TrueIntPtr;
					break;
				}
				break;
			}
		}

		public void MainLoop()
		{
			if (isDisposed)
			{
				return;
			}
			if (State.IsInsideDeviceCallback || State.IsInsideMainloop)
			{
				State.ApplicationExitCode = 1;
				throw new InvalidOperationException("You cannot call this method from a callback, or reenter the method.");
			}
			State.IsInsideMainloop = true;
			if (!State.WasDeviceCreated)
			{
				if (State.WasDeviceCreateCalled)
				{
					State.ApplicationExitCode = 1;
					throw new InvalidOperationException("The device was never created.");
				}
				try
				{
					CreateDevice(0u, true, State.WindowClientSize.Width, State.WindowClientSize.Height);
				}
				catch
				{
					State.ApplicationExitCode = 1;
					throw;
				}
			}
			if (!State.IsInited || !State.WasWindowCreated || !State.WasDeviceCreated)
			{
				State.ApplicationExitCode = 1;
				throw new InvalidOperationException("The framework was not initialized, cannot continue.");
			}
			ApplicationIdleHandler = OnApplicationIdle;
			Application.Idle += ApplicationIdleHandler;
			isManual = false;
			Application.Run(GameWindow);
			State.IsInsideMainloop = false;
		}

		/// <summary>摘下或挂回 idle 渲染。视频播放期间要摘掉，否则会和 DirectShow 抢渲染目标。</summary>
		public void ToggleManual(bool manual)
		{
			if (manual != isManual)
			{
				if (manual)
				{
					Application.Idle -= ApplicationIdleHandler;
				}
				else
				{
					Application.Idle += ApplicationIdleHandler;
				}
				isManual = manual;
			}
		}

		private void OnApplicationIdle(object sender, EventArgs e)
		{
			while (AppStillIdle)
			{
				RenderEnvironment();
			}
		}

		private void OnDeviceDisposing(object sender, EventArgs e)
		{
			State.IsInsideDeviceCallback = true;
			if (fpsFont != (Microsoft.DirectX.Direct3D.Font)null)
			{
				fpsFont.Dispose();
				fpsFont = null;
			}
			if (Disposing != null)
			{
				Disposing(this, e);
			}
			State.IsInsideDeviceCallback = false;
		}

		private void OnDeviceLost(object sender, EventArgs e)
		{
			State.IsInsideDeviceCallback = true;
			if (fpsFont != (Microsoft.DirectX.Direct3D.Font)null && !fpsFont.Disposed)
			{
				fpsFont.OnLostDevice();
			}
			if (DeviceLost != null)
			{
				DeviceLost(this, e);
			}
			State.IsInsideDeviceCallback = false;
		}

		private void OnDeviceReset(object sender, EventArgs e)
		{
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			Device device = (Device)((sender is Device) ? sender : null);
			State.IsInsideDeviceCallback = true;
			PrepareDevice(device);
			if (fpsFont != (Microsoft.DirectX.Direct3D.Font)null)
			{
				fpsFont.OnResetDevice();
			}
			if (DeviceReset != null)
			{
				DeviceReset(this, new DeviceEventArgs(device, State.BackBufferSurfaceDesc));
			}
			State.IsInsideDeviceCallback = false;
		}

		public void Dispose()
		{
			GC.SuppressFinalize(this);
			if (!isDisposed)
			{
				if (Disposing != null)
				{
					Disposing(this, EventArgs.Empty);
				}
				Shutdown();
			}
			isDisposed = true;
		}

		~GameCore()
		{
			Dispose();
		}
	}
}
