using System;
using System.Collections.Generic;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.GameCore
{
	internal sealed class Enumeration
	{
		private static bool isPostPixelShaderBlendingRequired;

		private static bool isSoftwareVertexProcessing;

		private static bool isHardwareVertexProcessing;

		private static bool isPureHardwareVertexProcessing;

		private static bool isMixedVertexProcessing;

		private static uint minimumWidth;

		private static uint maximumWidth;

		private static uint minimumHeight;

		private static uint maximumHeight;

		private static uint minimumRefresh;

		private static uint maximumRefresh;

		private static uint multisampleQualityMax;

		private static List<DepthFormat> depthStencilPossibleList;

		private static List<MultiSampleType> multiSampleTypeList;

		private static List<PresentInterval> presentIntervalList;

		private static List<EnumAdapterInformation> adapterInformationList;

		private static readonly Format[] allowedFormats;

		private static readonly Format[] backbufferFormatsArray;

		private static readonly DeviceType[] deviceTypeArray;

		public static uint MultisampleQualityMax
		{
			get
			{
				return multisampleQualityMax;
			}
			set
			{
				if (value > 65535)
				{
					multisampleQualityMax = 65535u;
				}
				else
				{
					multisampleQualityMax = value;
				}
			}
		}

		public static bool IsPostPixelShaderBlendingRequred
		{
			get
			{
				return isPostPixelShaderBlendingRequired;
			}
			set
			{
				isPostPixelShaderBlendingRequired = value;
			}
		}

		public static bool IsSoftwareVertexProcessingPossible
		{
			get
			{
				return isSoftwareVertexProcessing;
			}
			set
			{
				isSoftwareVertexProcessing = value;
			}
		}

		public static bool IsHardwareVertexProcessingPossible
		{
			get
			{
				return isHardwareVertexProcessing;
			}
			set
			{
				isHardwareVertexProcessing = value;
			}
		}

		public static bool IsPureHardwareVertexProcessingPossible
		{
			get
			{
				return isPureHardwareVertexProcessing;
			}
			set
			{
				isPureHardwareVertexProcessing = value;
			}
		}

		public static bool IsMixedVertexProcessingPossible
		{
			get
			{
				return isMixedVertexProcessing;
			}
			set
			{
				isMixedVertexProcessing = value;
			}
		}

		public static List<DepthFormat> PossibleDepthStencilFormatList
		{
			get
			{
				return depthStencilPossibleList;
			}
		}

		public static List<MultiSampleType> PossibleMultisampleTypeList
		{
			get
			{
				return multiSampleTypeList;
			}
		}

		public static List<PresentInterval> PossiblePresentIntervalsList
		{
			get
			{
				return presentIntervalList;
			}
		}

		public static List<EnumAdapterInformation> AdapterInformationList
		{
			get
			{
				return adapterInformationList;
			}
		}

		private Enumeration()
		{
		}

		static Enumeration()
		{
			isPostPixelShaderBlendingRequired = true;
			isSoftwareVertexProcessing = true;
			isHardwareVertexProcessing = true;
			isPureHardwareVertexProcessing = true;
			isMixedVertexProcessing = false;
			minimumWidth = 0u;
			maximumWidth = uint.MaxValue;
			minimumHeight = 0u;
			maximumHeight = uint.MaxValue;
			minimumRefresh = 0u;
			maximumRefresh = uint.MaxValue;
			multisampleQualityMax = 65535u;
			depthStencilPossibleList = new List<DepthFormat>();
			multiSampleTypeList = new List<MultiSampleType>();
			presentIntervalList = new List<PresentInterval>();
			adapterInformationList = new List<EnumAdapterInformation>();
			allowedFormats = new Format[4]
			{
				(Format)22,
				(Format)24,
				(Format)23,
				(Format)35
			};
			backbufferFormatsArray = new Format[6]
			{
				(Format)21,
				(Format)22,
				(Format)25,
				(Format)24,
				(Format)23,
				(Format)35
			};
			deviceTypeArray = new DeviceType[3]
			{
				(DeviceType)1,
				(DeviceType)3,
				(DeviceType)2
			};
			ResetPossibleDepthStencilFormats();
			ResetPossibleMultisampleTypeList();
			ResetPossiblePresentIntervalList();
		}

		public static void Enumerate()
		{
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Expected Obj, but got Unknown
			//IL_0049: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
			//IL_016b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0170: Unknown result type (might be due to invalid IL or missing references)
			//IL_0175: Unknown result type (might be due to invalid IL or missing references)
			//IL_0187: Unknown result type (might be due to invalid IL or missing references)
			//IL_0087: Unknown result type (might be due to invalid IL or missing references)
			//IL_008c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0107: Unknown result type (might be due to invalid IL or missing references)
			//IL_0111: Unknown result type (might be due to invalid IL or missing references)
			//IL_0123: Unknown result type (might be due to invalid IL or missing references)
			DisplayModeSorter comparer = new DisplayModeSorter();
			try
			{
				adapterInformationList.Clear();
				List<Format> formats = new List<Format>();
				foreach (AdapterInformation adapter in Manager.Adapters)
				{
					AdapterInformation adapterInfo = adapter;
					EnumAdapterInformation enumAdapterInformation = new EnumAdapterInformation();
					enumAdapterInformation.AdapterOrdinal = (uint)adapterInfo.Adapter;
					enumAdapterInformation.AdapterInformation = adapterInfo.Information;
					formats.Clear();
					for (int i = 0; i < allowedFormats.Length; i++)
					{
						foreach (DisplayMode item2 in adapterInfo.SupportedDisplayModes[allowedFormats[i]])
						{
							DisplayMode item = item2;
							if (item.Width >= minimumWidth && item.Height >= minimumHeight && item.Width <= maximumWidth && item.Height <= maximumHeight && item.RefreshRate >= minimumRefresh && item.RefreshRate <= maximumRefresh)
							{
								enumAdapterInformation.displayModeList.Add(item);
								if (!formats.Contains(item.Format))
								{
									formats.Add(item.Format);
								}
							}
						}
					}
					DisplayMode currentDisplayMode = adapterInfo.CurrentDisplayMode;
					if (!formats.Contains(currentDisplayMode.Format))
					{
						formats.Add(currentDisplayMode.Format);
					}
					enumAdapterInformation.displayModeList.Sort(comparer);
					EnumerateDevices(enumAdapterInformation, formats);
					if (enumAdapterInformation.deviceInfoList.Count > 0)
					{
						adapterInformationList.Add(enumAdapterInformation);
					}
				}
				bool matched = true;
				for (int j = 0; j < adapterInformationList.Count; j++)
				{
					for (int k = j + 1; k < adapterInformationList.Count; k++)
					{
						EnumAdapterInformation enumAdapterInformation2 = adapterInformationList[j];
						EnumAdapterInformation enumAdapterInformation3 = adapterInformationList[k];
						if (string.Compare(enumAdapterInformation2.AdapterInformation.Description, enumAdapterInformation3.AdapterInformation.Description, true) == 0)
						{
							matched = false;
							break;
						}
					}
					if (!matched)
					{
						break;
					}
				}
				for (int l = 0; l < adapterInformationList.Count; l++)
				{
					EnumAdapterInformation enumAdapterInformation4 = adapterInformationList[l];
					enumAdapterInformation4.UniqueDescription = enumAdapterInformation4.AdapterInformation.Description;
					if (!matched)
					{
						enumAdapterInformation4.UniqueDescription += string.Format(" (#{0})", enumAdapterInformation4.AdapterOrdinal);
					}
				}
			}
			catch (TypeLoadException)
			{
				throw new NoDirect3DException();
			}
		}

		private static void EnumerateDevices(EnumAdapterInformation adapterInfo, List<Format> adapterFormatList)
		{
			//IL_001b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0032: Unknown result type (might be due to invalid IL or missing references)
			DirectXException.IgnoreExceptions();
			for (uint i = 0u; (ulong)i < (ulong)deviceTypeArray.Length; i++)
			{
				EnumDeviceInformation enumDeviceInformation = new EnumDeviceInformation();
				enumDeviceInformation.DeviceType = deviceTypeArray[i];
				enumDeviceInformation.Caps = Manager.GetDeviceCaps((int)adapterInfo.AdapterOrdinal, enumDeviceInformation.DeviceType);
				EnumerateDeviceCombos(adapterInfo, enumDeviceInformation, adapterFormatList);
				if (enumDeviceInformation.deviceSettingsList.Count > 0)
				{
					adapterInfo.deviceInfoList.Add(enumDeviceInformation);
				}
			}
			DirectXException.EnableExceptions();
		}

		private static void EnumerateDeviceCombos(EnumAdapterInformation adapterInfo, EnumDeviceInformation deviceInfo, List<Format> adapterFormatList)
		{
			//IL_000e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0046: Unknown result type (might be due to invalid IL or missing references)
			//IL_004b: Unknown result type (might be due to invalid IL or missing references)
			//IL_008e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0093: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Unknown result type (might be due to invalid IL or missing references)
			//IL_00a7: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_0070: Unknown result type (might be due to invalid IL or missing references)
			//IL_0075: Unknown result type (might be due to invalid IL or missing references)
			//IL_00d6: Unknown result type (might be due to invalid IL or missing references)
			//IL_00db: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
			//IL_00e3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
			foreach (Format adapterFormat in adapterFormatList)
			{
				for (int i = 0; i < backbufferFormatsArray.Length; i++)
				{
					for (int j = 0; j < 2; j++)
					{
						bool isWindowed = j == 1;
						if ((!isWindowed && adapterInfo.displayModeList.Count == 0) || !Manager.CheckDeviceType((int)adapterInfo.AdapterOrdinal, deviceInfo.DeviceType, adapterFormat, backbufferFormatsArray[i], isWindowed) || (isPostPixelShaderBlendingRequired && !Manager.CheckDeviceFormat((int)adapterInfo.AdapterOrdinal, deviceInfo.DeviceType, adapterFormat, (Usage)524288, (ResourceType)3, backbufferFormatsArray[i])))
						{
							continue;
						}
						Caps caps = deviceInfo.Caps;
						Format backbufferFormat = backbufferFormatsArray[i];
						if (Manager.CheckDeviceFormat(caps.AdapterOrdinal, caps.DeviceType, adapterFormat, (Usage)524288, (ResourceType)3, backbufferFormat))
						{
							EnumDeviceSettingsCombo enumDeviceSettingsCombo = new EnumDeviceSettingsCombo();
							enumDeviceSettingsCombo.AdapterOrdinal = adapterInfo.AdapterOrdinal;
							enumDeviceSettingsCombo.DeviceType = deviceInfo.DeviceType;
							enumDeviceSettingsCombo.AdapterFormat = adapterFormat;
							enumDeviceSettingsCombo.BackBufferFormat = backbufferFormatsArray[i];
							enumDeviceSettingsCombo.IsWindowed = isWindowed;
							BuildDepthStencilFormatList(enumDeviceSettingsCombo);
							BuildMultiSampleTypeList(enumDeviceSettingsCombo);
							if (enumDeviceSettingsCombo.multiSampleTypeList.Count != 0)
							{
								BuildConflictList(enumDeviceSettingsCombo);
								BuildPresentIntervalList(deviceInfo, enumDeviceSettingsCombo);
								enumDeviceSettingsCombo.adapterInformation = adapterInfo;
								enumDeviceSettingsCombo.deviceInformation = deviceInfo;
								deviceInfo.deviceSettingsList.Add(enumDeviceSettingsCombo);
							}
						}
					}
				}
			}
		}

		private static void BuildDepthStencilFormatList(EnumDeviceSettingsCombo deviceCombo)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_004a: Unknown result type (might be due to invalid IL or missing references)
			//IL_004f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0060: Unknown result type (might be due to invalid IL or missing references)
			foreach (DepthFormat depthStencilPossible in depthStencilPossibleList)
			{
				if (Manager.CheckDeviceFormat((int)deviceCombo.AdapterOrdinal, deviceCombo.DeviceType, deviceCombo.AdapterFormat, (Usage)2, (ResourceType)1, depthStencilPossible) && Manager.CheckDepthStencilMatch((int)deviceCombo.AdapterOrdinal, deviceCombo.DeviceType, deviceCombo.AdapterFormat, deviceCombo.BackBufferFormat, depthStencilPossible))
				{
					deviceCombo.depthStencilFormatList.Add(depthStencilPossible);
				}
			}
		}

		private static void BuildMultiSampleTypeList(EnumDeviceSettingsCombo deviceCombo)
		{
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			int qualityLevels = default(int);
			int sampleCount = default(int);
			foreach (MultiSampleType multiSampleType in multiSampleTypeList)
			{
				if (Manager.CheckDeviceMultiSampleType((int)deviceCombo.AdapterOrdinal, deviceCombo.DeviceType, deviceCombo.BackBufferFormat, deviceCombo.IsWindowed, multiSampleType, out qualityLevels, out sampleCount))
				{
					deviceCombo.multiSampleTypeList.Add(multiSampleType);
					if (sampleCount > multisampleQualityMax + 1)
					{
						sampleCount = (int)(multisampleQualityMax + 1);
					}
					deviceCombo.multiSampleQualityList.Add(sampleCount);
				}
			}
		}

		private static void BuildConflictList(EnumDeviceSettingsCombo deviceCombo)
		{
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0031: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_005a: Unknown result type (might be due to invalid IL or missing references)
			//IL_005b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0063: Unknown result type (might be due to invalid IL or missing references)
			foreach (DepthFormat depthStencilFormat in deviceCombo.depthStencilFormatList)
			{
				foreach (MultiSampleType multiSampleType in deviceCombo.multiSampleTypeList)
				{
					if (!Manager.CheckDeviceMultiSampleType((int)deviceCombo.AdapterOrdinal, deviceCombo.DeviceType, (Format)depthStencilFormat, deviceCombo.IsWindowed, multiSampleType))
					{
						EnumDepthStencilMultisampleConflict item = default(EnumDepthStencilMultisampleConflict);
						item.DepthStencilFormat = depthStencilFormat;
						item.MultisampleType = multiSampleType;
						deviceCombo.depthStencilConflictList.Add(item);
					}
				}
			}
		}

		private static void BuildPresentIntervalList(EnumDeviceInformation deviceInfo, EnumDeviceSettingsCombo deviceCombo)
		{
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_0033: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Invalid comparison between Unknown and I4
			//IL_0051: Unknown result type (might be due to invalid IL or missing references)
			//IL_003f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0044: Unknown result type (might be due to invalid IL or missing references)
			//IL_0045: Unknown result type (might be due to invalid IL or missing references)
			//IL_0025: Unknown result type (might be due to invalid IL or missing references)
			//IL_0027: Invalid comparison between Unknown and I4
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			//IL_002e: Invalid comparison between Unknown and I4
			for (int i = 0; i < presentIntervalList.Count; i++)
			{
				PresentInterval interval = presentIntervalList[i];
				if ((!deviceCombo.IsWindowed || ((int)interval != 2 && (int)interval != 4 && (int)interval != 8)) && ((int)interval == 0 || (deviceInfo.Caps.PresentationIntervals & interval) != 0))
				{
					deviceCombo.presentIntervalList.Add(interval);
				}
			}
		}

		public static void ResetPossibleDepthStencilFormats()
		{
			depthStencilPossibleList.Clear();
			depthStencilPossibleList.AddRange(new DepthFormat[6]
			{
				(DepthFormat)80,
				(DepthFormat)73,
				(DepthFormat)77,
				(DepthFormat)75,
				(DepthFormat)79,
				(DepthFormat)71
			});
		}

		public static void ResetPossibleMultisampleTypeList()
		{
			multiSampleTypeList.Clear();
			multiSampleTypeList.AddRange(new MultiSampleType[17]
			{
				default(MultiSampleType),
				(MultiSampleType)1,
				(MultiSampleType)2,
				(MultiSampleType)3,
				(MultiSampleType)4,
				(MultiSampleType)5,
				(MultiSampleType)6,
				(MultiSampleType)7,
				(MultiSampleType)8,
				(MultiSampleType)9,
				(MultiSampleType)10,
				(MultiSampleType)11,
				(MultiSampleType)12,
				(MultiSampleType)13,
				(MultiSampleType)14,
				(MultiSampleType)15,
				(MultiSampleType)16
			});
		}

		public static void ResetPossiblePresentIntervalList()
		{
			presentIntervalList.Clear();
			presentIntervalList.AddRange(new PresentInterval[6]
			{
				(PresentInterval)int.MinValue,
				default(PresentInterval),
				(PresentInterval)1,
				(PresentInterval)2,
				(PresentInterval)4,
				(PresentInterval)8
			});
		}

		public static void SetResolutionMinMax(uint minWidth, uint minHeight, uint maxWidth, uint maxHeight)
		{
			minimumWidth = minWidth;
			minimumHeight = minHeight;
			maximumWidth = maxHeight;
			maximumWidth = maxWidth;
		}

		public static void SetRefreshMinMax(uint minRefresh, uint maxRefresh)
		{
			minimumRefresh = minRefresh;
			maximumRefresh = maxRefresh;
		}

		public static EnumAdapterInformation GetAdapterInformation(uint ordinal)
		{
			foreach (EnumAdapterInformation adapterInformation in adapterInformationList)
			{
				if (adapterInformation.AdapterOrdinal == ordinal)
				{
					return adapterInformation;
				}
			}
			return null;
		}

		public static EnumDeviceInformation GetDeviceInfo(uint ordinal, DeviceType deviceType)
		{
			//IL_0027: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Unknown result type (might be due to invalid IL or missing references)
			EnumAdapterInformation adapterInformation = GetAdapterInformation(ordinal);
			if (adapterInformation != null)
			{
				foreach (EnumDeviceInformation deviceInfo in adapterInformation.deviceInfoList)
				{
					if (deviceInfo.DeviceType == deviceType)
					{
						return deviceInfo;
					}
				}
			}
			return null;
		}

		public static EnumDeviceSettingsCombo GetDeviceSettingsCombo(uint ordinal, DeviceType deviceType, Format adapterFormat, Format backBufferFormat, bool isWindowed)
		{
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			//IL_0028: Unknown result type (might be due to invalid IL or missing references)
			//IL_002d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			EnumDeviceInformation deviceInfo = GetDeviceInfo(ordinal, deviceType);
			if (deviceInfo != null)
			{
				foreach (EnumDeviceSettingsCombo deviceSettings in deviceInfo.deviceSettingsList)
				{
					if (deviceSettings.AdapterFormat == adapterFormat && deviceSettings.BackBufferFormat == backBufferFormat && deviceSettings.IsWindowed == isWindowed)
					{
						return deviceSettings;
					}
				}
			}
			return null;
		}

		public static EnumDeviceSettingsCombo GetDeviceSettingsCombo(DeviceSettings settings)
		{
			//IL_0007: Unknown result type (might be due to invalid IL or missing references)
			//IL_000d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			return GetDeviceSettingsCombo(settings.AdapterOrdinal, settings.DeviceType, settings.AdapterFormat, settings.PresentParameters.BackBufferFormat, settings.PresentParameters.Windowed);
		}
	}
}
