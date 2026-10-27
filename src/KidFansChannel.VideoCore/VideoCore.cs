using System;
using System.Drawing;
using System.Runtime.InteropServices;
using DirectShowLib;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.VideoCore
{
	public class VideoCore
	{
		public const int WM_GRAPHNOTIFY = 16507;

		private IFilterGraph2 graphBuilder;

		private IBaseFilter vmr9;

		private Allocator allocator;

		private Device _device;

		private IntPtr _hwnd;

		private bool _isPlaying;

		public Texture Texture
		{
			get
			{
				return allocator.Texture;
			}
		}

		public SizeF UV
		{
			get
			{
				return new SizeF((float)allocator.VideoSize.Width / (float)allocator.TextureSize.Width, (float)allocator.VideoSize.Height / (float)allocator.TextureSize.Height);
			}
		}

		public bool IsPlaying
		{
			get
			{
				return _isPlaying;
			}
		}

		public double Length
		{
			get
			{
				if (graphBuilder != null)
				{
					IFilterGraph2 graph = graphBuilder;
					double result = default(double);
					((IMediaPosition)((graph is IMediaPosition) ? graph : null)).get_CurrentPosition(out result);
					return result;
				}
				return 0.0;
			}
		}

		public double Position
		{
			get
			{
				if (graphBuilder != null)
				{
					IFilterGraph2 graph = graphBuilder;
					double result = default(double);
					((IMediaPosition)((graph is IMediaPosition) ? graph : null)).get_CurrentPosition(out result);
					return result;
				}
				return 0.0;
			}
		}

		public event EventHandler Stopping;

		public event EventHandler Stopped;

		public void Initial(Device device, IntPtr hwnd)
		{
			//IL_0011: Unknown result type (might be due to invalid IL or missing references)
			//IL_0017: Expected Obj, but got Unknown
			//IL_0016: Unknown result type (might be due to invalid IL or missing references)
			//IL_0020: Expected Obj, but got Unknown
			//IL_0026: Unknown result type (might be due to invalid IL or missing references)
			//IL_002c: Expected Obj, but got Unknown
			//IL_005f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0065: Expected Obj, but got Unknown
			//IL_0096: Unknown result type (might be due to invalid IL or missing references)
			//IL_009c: Expected Obj, but got Unknown
			int hr = 0;
			_device = device;
			_hwnd = hwnd;
			vmr9 = (IBaseFilter)new VideoMixingRenderer9();
			IVMRFilterConfig9 filterConfig = (IVMRFilterConfig9)vmr9;
			hr = filterConfig.SetRenderingMode((VMR9Mode)4);
			DsError.ThrowExceptionForHR(hr);
			hr = filterConfig.SetNumberOfStreams(1);
			DsError.ThrowExceptionForHR(hr);
			allocator = new Allocator(_device);
			// 把每帧的呈现回调接出来，派生类可以在这里做环境处理
			allocator.Render = (EventHandler)Delegate.Combine(allocator.Render, new EventHandler(OnRender));
			IVMRSurfaceAllocatorNotify9 allocNotify = (IVMRSurfaceAllocatorNotify9)vmr9;
			hr = allocNotify.AdviseSurfaceAllocator(IntPtr.Zero, (IVMRSurfaceAllocator9)(object)allocator);
			DsError.ThrowExceptionForHR(hr);
			hr = allocator.AdviseNotify(allocNotify);
			DsError.ThrowExceptionForHR(hr);
			IVMRMixerControl9 mixer = (IVMRMixerControl9)vmr9;
			hr = mixer.SetMixingPrefs((VMR9MixerPrefs)8213);
			DsError.ThrowExceptionForHR(hr);
		}

		public void Final()
		{
			Stop();
			if (allocator != null)
			{
				allocator.Dispose();
				allocator = null;
			}
			if (vmr9 != null)
			{
				Marshal.ReleaseComObject(vmr9);
				vmr9 = null;
			}
		}

		public virtual void Play(string fileName)
		{
			//IL_0003: Unknown result type (might be due to invalid IL or missing references)
			//IL_0009: Expected Obj, but got Unknown
			//IL_0008: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Expected Obj, but got Unknown
			int hr = 0;
			graphBuilder = (IFilterGraph2)new FilterGraph();
			IFilterGraph2 graph = graphBuilder;
			hr = ((IMediaEventEx)((graph is IMediaEventEx) ? graph : null)).SetNotifyWindow(_hwnd, 16507, IntPtr.Zero);
			DsError.ThrowExceptionForHR(hr);
			hr = graphBuilder.AddFilter(vmr9, "Video Mixing Renderer 9");
			DsError.ThrowExceptionForHR(hr);
			hr = graphBuilder.RenderFile(fileName, (string)null);
			DsError.ThrowExceptionForHR(hr);
			IFilterGraph2 runGraph = graphBuilder;
			hr = ((IMediaControl)((runGraph is IMediaControl) ? runGraph : null)).Run();
			DsError.ThrowExceptionForHR(hr);
			_isPlaying = true;
		}

		public virtual void Stop()
		{
			if (graphBuilder != null)
			{
				IFilterGraph2 graph = graphBuilder;
				((IMediaEventEx)((graph is IMediaEventEx) ? graph : null)).SetNotifyWindow(IntPtr.Zero, 0, IntPtr.Zero);
				IFilterGraph2 readyGraph = graphBuilder;
				((IMediaControl)((readyGraph is IMediaControl) ? readyGraph : null)).StopWhenReady();
				IFilterGraph2 stopGraph = graphBuilder;
				((IMediaControl)((stopGraph is IMediaControl) ? stopGraph : null)).Stop();
				Marshal.ReleaseComObject(graphBuilder);
				graphBuilder = null;
			}
			_isPlaying = false;
			if (Stopped != null)
			{
				Stopped(this, EventArgs.Empty);
			}
		}

		public void HandleGraphicEvent()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_000c: Expected Obj, but got Unknown
			//IL_0014: Unknown result type (might be due to invalid IL or missing references)
			//IL_001d: Unknown result type (might be due to invalid IL or missing references)
			//IL_001f: Invalid comparison between Unknown and I4
			IMediaEventEx eventEx = (IMediaEventEx)graphBuilder;
			if (eventEx == null)
			{
				return;
			}
			EventCode eventCode = default(EventCode);
			IntPtr eventParam1 = default(IntPtr);
			IntPtr eventParam2 = default(IntPtr);
			while (eventEx.GetEvent(out eventCode, out eventParam1, out eventParam2, 0) == 0)
			{
				eventEx.FreeEventParams(eventCode, eventParam1, eventParam2);
				if ((int)eventCode == 1 && Stopping != null)
				{
					Stopping(graphBuilder, EventArgs.Empty);
				}
				if (graphBuilder == null)
				{
					break;
				}
			}
		}

		/// <summary>每帧画面呈现后的回调，派生类可覆盖。</summary>
		protected virtual void OnRender(object sender, EventArgs e)
		{
		}
	}

}
