using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using DirectShowLib;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.VideoCore
{
	[ClassInterface(ClassInterfaceType.None)]
	[ComVisible(true)]
	public class Allocator : IVMRSurfaceAllocator9, IVMRImagePresenter9, IDisposable
	{
		private const int E_FAIL = -2147467259;

		private const int D3DERR_INVALIDCALL = -2005530516;

		private const int DxMagicNumber = -759872593;

		private Device device;

		private AdapterInformation adapterInfo;

		private DeviceCreationParameters creationParameters;

		private Surface videoSurface;

		private IntPtr unmanagedSurface = IntPtr.Zero;

		private Texture privateTexture;

		private Surface privateSurface;

		private bool needCopy;

		private Size textureSize;

		private Size videoSize;

		private IVMRSurfaceAllocatorNotify9 vmrSurfaceAllocatorNotify;

		private bool disposed;

		public Texture Texture
		{
			get
			{
				return privateTexture;
			}
		}

		public Size TextureSize
		{
			get
			{
				return textureSize;
			}
		}

		public Size VideoSize
		{
			get
			{
				return videoSize;
			}
		}

		public Allocator(Device dev)
		{
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_003d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			AdapterListCollection adapters = Manager.Adapters;
			DeviceCreationParameters creationParams = dev.CreationParameters;
			adapterInfo = adapters[creationParams.AdapterOrdinal];
			device = dev;
			creationParameters = device.CreationParameters;
		}

		~Allocator()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (!disposed)
			{
				DeleteSurfaces();
				disposed = true;
			}
		}

		private void DeleteSurfaces()
		{
			lock (this)
			{
				if (privateTexture != (Texture)null)
				{
					privateTexture.Dispose();
					privateTexture = null;
				}
				if ((Resource)(object)privateSurface != (Resource)null)
				{
					privateSurface.Dispose();
					privateSurface = null;
				}
				if ((Resource)(object)videoSurface != (Resource)null)
				{
					videoSurface.Dispose();
					videoSurface = null;
				}
			}
		}

		private string FourCCToStr(int fcc)
		{
			byte[] fccBytes = new byte[4];
			if (fcc < 100)
			{
				return fcc.ToString();
			}
			for (int i = 0; i < 4; i++)
			{
				fccBytes[i] = (byte)(fcc & 0xFF);
				fcc >>= 8;
			}
			return Encoding.ASCII.GetString(fccBytes);
		}

		public int InitializeDevice(IntPtr dwUserID, ref VMR9AllocationInfo lpAllocInfo, ref int lpNumBuffers)
		{
			//IL_0247: Expected Obj, but got Unknown
			//IL_00aa: Unknown result type (might be due to invalid IL or missing references)
			//IL_00af: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
			//IL_00b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_0023: Unknown result type (might be due to invalid IL or missing references)
			//IL_0034: Unknown result type (might be due to invalid IL or missing references)
			//IL_0039: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ce: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01d5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01f5: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fa: Unknown result type (might be due to invalid IL or missing references)
			//IL_01fe: Unknown result type (might be due to invalid IL or missing references)
			//IL_0204: Unknown result type (might be due to invalid IL or missing references)
			//IL_020e: Expected Obj, but got Unknown
			//IL_012f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0135: Unknown result type (might be due to invalid IL or missing references)
			//IL_0136: Unknown result type (might be due to invalid IL or missing references)
			//IL_0195: Unknown result type (might be due to invalid IL or missing references)
			//IL_019a: Unknown result type (might be due to invalid IL or missing references)
			//IL_019e: Unknown result type (might be due to invalid IL or missing references)
			//IL_01a4: Unknown result type (might be due to invalid IL or missing references)
			//IL_01ae: Expected Obj, but got Unknown
			int hr = 0;
			if (lpAllocInfo.Format > 808464432)
			{
				int adapterOrdinal = creationParameters.AdapterOrdinal;
				DeviceType deviceType = creationParameters.DeviceType;
				int format = lpAllocInfo.Format;
				DisplayMode currentDisplayMode = adapterInfo.CurrentDisplayMode;
				if (!Manager.CheckDeviceFormatConversion(adapterOrdinal, deviceType, (Format)format, currentDisplayMode.Format))
				{
					return -2005530516;
				}
			}
			try
			{
				IntPtr objectByValue = device.GetObjectByValue(-759872593);
				IntPtr adapterMonitor = Manager.GetAdapterMonitor(adapterInfo.Adapter);
				hr = vmrSurfaceAllocatorNotify.SetD3DDevice(objectByValue, adapterMonitor);
				DsError.ThrowExceptionForHR(hr);
				videoSize = new Size(lpAllocInfo.dwWidth, lpAllocInfo.dwHeight);
				int pow2Width = 1;
				int pow2Height = 1;
				Caps deviceCaps = device.DeviceCaps;
				TextureCaps textureCaps = deviceCaps.TextureCaps;
				if (textureCaps.SupportsPower2)
				{
					while (pow2Width < lpAllocInfo.dwWidth)
					{
						pow2Width <<= 1;
					}
					while (pow2Height < lpAllocInfo.dwHeight)
					{
						pow2Height <<= 1;
					}
					lpAllocInfo.dwWidth = pow2Width;
					lpAllocInfo.dwHeight = pow2Height;
				}
				textureSize = new Size(lpAllocInfo.dwWidth, lpAllocInfo.dwHeight);
				DeleteSurfaces();
				if (lpAllocInfo.Format > 808464432)
				{
					lpAllocInfo.dwFlags = (VMR9SurfaceAllocationFlags)((int)lpAllocInfo.dwFlags | 8);
					videoSurface = device.CreateOffscreenPlainSurface(lpAllocInfo.dwWidth, lpAllocInfo.dwHeight, (Format)lpAllocInfo.Format, (Pool)lpAllocInfo.Pool);
					unmanagedSurface = videoSurface.GetObjectByValue(-759872593);
					Device dev = device;
					int dwWidth = lpAllocInfo.dwWidth;
					int dwHeight = lpAllocInfo.dwHeight;
					DisplayMode currentDisplayMode2 = adapterInfo.CurrentDisplayMode;
					privateTexture = new Texture(dev, dwWidth, dwHeight, 1, (Usage)1, currentDisplayMode2.Format, (Pool)0);
					privateSurface = privateTexture.GetSurfaceLevel(0);
					needCopy = true;
				}
				else
				{
					lpAllocInfo.dwFlags = (VMR9SurfaceAllocationFlags)((int)lpAllocInfo.dwFlags | 4);
					Device dev2 = device;
					int dwWidth2 = lpAllocInfo.dwWidth;
					int dwHeight2 = lpAllocInfo.dwHeight;
					DisplayMode currentDisplayMode3 = adapterInfo.CurrentDisplayMode;
					privateTexture = new Texture(dev2, dwWidth2, dwHeight2, 1, (Usage)1, currentDisplayMode3.Format, (Pool)0);
					privateSurface = privateTexture.GetSurfaceLevel(0);
					unmanagedSurface = privateSurface.GetObjectByValue(-759872593);
					needCopy = false;
				}
				lpNumBuffers = 1;
			}
			catch (DirectXException ex)
			{
				DirectXException ex2 = ex;
				return ex2.ErrorCode;
			}
			catch
			{
				return -2147467259;
			}
			return 0;
		}

		public int TerminateDevice(IntPtr dwID)
		{
			DeleteSurfaces();
			return 0;
		}

		public int GetSurface(IntPtr dwUserID, int SurfaceIndex, int SurfaceFlags, out IntPtr lplpSurface)
		{
			lplpSurface = IntPtr.Zero;
			if (SurfaceIndex >= 1)
			{
				return -2147467259;
			}
			lock (this)
			{
				lplpSurface = unmanagedSurface;
				Marshal.AddRef(lplpSurface);
				return 0;
			}
		}

		public int AdviseNotify(IVMRSurfaceAllocatorNotify9 lpIVMRSurfAllocNotify)
		{
			//IL_002b: Unknown result type (might be due to invalid IL or missing references)
			//IL_0030: Unknown result type (might be due to invalid IL or missing references)
			lock (this)
			{
				vmrSurfaceAllocatorNotify = lpIVMRSurfAllocNotify;
				IntPtr objectByValue = device.GetObjectByValue(-759872593);
				AdapterListCollection adapters = Manager.Adapters;
				DeviceCreationParameters creationParams = device.CreationParameters;
				IntPtr adapterMonitor = Manager.GetAdapterMonitor(adapters[creationParams.AdapterOrdinal].Adapter);
				return vmrSurfaceAllocatorNotify.SetD3DDevice(objectByValue, adapterMonitor);
			}
		}

		public int StartPresenting(IntPtr dwUserID)
		{
			lock (this)
			{
				if (device == (Device)null)
				{
					return -2147467259;
				}
				return 0;
			}
		}

		public int StopPresenting(IntPtr dwUserID)
		{
			return 0;
		}

		/// <summary>每帧呈现完成时触发，供上层在 VMR9 交出画面后做一次环境处理。</summary>
		public EventHandler Render;

		public int PresentImage(IntPtr dwUserID, ref VMR9PresentationInfo lpPresInfo)
		{
			//IL_0051: Expected Obj, but got Unknown
			lock (this)
			{
				try
				{
					if (needCopy)
					{
						device.StretchRectangle(videoSurface, new Rectangle(Point.Empty, videoSize), privateSurface, new Rectangle(Point.Empty, videoSize), (TextureFilter)0);
					}
				}
				catch (DirectXException ex)
				{
					DirectXException ex2 = ex;
					return ex2.ErrorCode;
				}
				catch
				{
					return -2147467259;
				}
				if (Render != null)
				{
					Render(this, EventArgs.Empty);
				}
				return 0;
			}
		}
	}
}
