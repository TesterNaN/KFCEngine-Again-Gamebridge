using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Microsoft.DirectX.DirectSound;

namespace KidFansChannel.SoundCore.Ogg
{
	public class OggPlayer : IDisposable
	{
		private const int WaitTimeout = 5000;

		private const int bitsPerSample = 16;

		private static Device directSoundDevice;

		private static bool _globalFocus;

		private static bool initialized;

		private bool stopNow;

		private EventWaitHandle ewh = new EventWaitHandle(false, EventResetMode.ManualReset);

		private Thread playbackThread;

		private SecondaryBuffer SecBuf;

		private int bufferVolume;

		private float _length;

		private float _position;

		private float _repeatPosition = float.NaN;

		private volatile IntPtr vorbisFile = IntPtr.Zero;

		private int bufferPosition;

		private int bufferRate = 1;

		private float SecBufHoldThisManySeconds = 1.2f;

		private bool _playing;

		private object thislock = new object();

		public static bool GlobalFocus
		{
			get
			{
				return _globalFocus;
			}
			set
			{
				_globalFocus = value;
			}
		}

		public float Volume
		{
			get
			{
				return (float)Math.Pow(10.0, (float)bufferVolume / 2000f);
			}
			set
			{
				int volumeDb;
				if (value == 0f || (volumeDb = (int)(2000.0 * Math.Log10(value))) < -10000)
				{
					volumeDb = -10000;
				}
				bufferVolume = volumeDb;
				if (SecBuf != (SecondaryBuffer)null)
				{
					((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Volume = bufferVolume;
				}
			}
		}

		public float Length
		{
			get
			{
				return _length;
			}
		}

		public float Position
		{
			get
			{
				float position = _position;
				if (SecBuf != (SecondaryBuffer)null)
				{
					int playPosition = ((Microsoft.DirectX.DirectSound.Buffer)SecBuf).PlayPosition;
					position = ((playPosition < bufferPosition) ? (position - (float)(bufferPosition - playPosition) / (float)bufferRate) : (position - (SecBufHoldThisManySeconds - (float)(playPosition - bufferPosition) / (float)bufferRate)));
				}
				return position;
			}
			protected set
			{
				_position = value;
			}
		}

		public float RepeatPosition
		{
			get
			{
				return _repeatPosition;
			}
			protected set
			{
				_repeatPosition = value;
			}
		}

		public bool Playing
		{
			get
			{
				return _playing;
			}
		}

		public event EventHandler<OggPlayEventArgs> Stopped;

		static OggPlayer()
		{
		}

		public static void Initial(Control owner, bool wantGlobalFocus, int deviceIndex)
		{
			//IL_0015: Unknown result type (might be due to invalid IL or missing references)
			//IL_001b: Expected Obj, but got Unknown
			//IL_005d: Unknown result type (might be due to invalid IL or missing references)
			//IL_0062: Unknown result type (might be due to invalid IL or missing references)
			//IL_0068: Expected Obj, but got Unknown
			//IL_006d: Expected Obj, but got Unknown
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_002f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0037: Unknown result type (might be due to invalid IL or missing references)
			//IL_003c: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Expected Obj, but got Unknown
			//IL_0047: Expected Obj, but got Unknown
			if (initialized)
			{
				throw new InvalidOperationException("Already initialized.");
			}
			DevicesCollection devices = new DevicesCollection();
			if (devices.Count > deviceIndex)
			{
				DeviceInformation deviceInfo = devices[deviceIndex];
				Device device = new Device(deviceInfo.DriverGuid);
				directSoundDevice = device;
				if ((Device)null == device)
				{
					throw new Exception("Unable to create Microsoft.DirectX.DirectSound Device object.");
				}
			}
			else
			{
				Device defaultDevice = new Device(DSoundHelper.DefaultPlaybackDevice);
				directSoundDevice = defaultDevice;
				if ((Device)null == defaultDevice)
				{
					throw new Exception("Unable to create Microsoft.DirectX.DirectSound Device object.");
				}
			}
			directSoundDevice.SetCooperativeLevel(owner, (CooperativeLevel)((owner == null) ? 1 : 2));
			_globalFocus = wantGlobalFocus;
			initialized = true;
		}

		public static void Final()
		{
			Thread.Sleep(20);
			if (directSoundDevice != (Device)null)
			{
				if (!directSoundDevice.Disposed)
				{
					directSoundDevice.Dispose();
				}
				directSoundDevice = null;
			}
		}

		public OggPlayer(string fileName)
		{
			Open(fileName);
		}

		public OggPlayer(Stream stream)
		{
			byte[] fileData = new byte[stream.Length];
			using (BinaryReader binaryReader = new BinaryReader(stream))
			{
				binaryReader.Read(fileData, 0, (int)stream.Length);
			}
			Open(fileData);
			fileData = null;
		}

		protected unsafe void Open(string fileName)
		{
			if (vorbisFile != IntPtr.Zero)
			{
				return;
			}
			lock (thislock)
			{
				void* ptr = null;
				int hr = 0;
				try
				{
					if (NativeMethods.OpenFile(fileName, &ptr) != 0)
					{
						return;
					}
					_length = (float)NativeMethods.LengthTime(ptr);
				}
				catch (Exception)
				{
					throw;
				}
				vorbisFile = (IntPtr)ptr;
			}
		}

		protected unsafe void Open(byte[] oggData)
		{
			if (vorbisFile != IntPtr.Zero)
			{
				return;
			}
			lock (thislock)
			{
				void* ptr = null;
				int hr = 0;
				try
				{
					if (NativeMethods.OpenStream(oggData, oggData.Length, &ptr) != 0)
					{
						return;
					}
					_length = (float)NativeMethods.LengthTime(ptr);
				}
				catch (Exception)
				{
					throw;
				}
				vorbisFile = (IntPtr)ptr;
			}
		}

		public unsafe void Close()
		{
			lock (thislock)
			{
				if (!stopNow)
				{
					stopNow = true;
					ewh.Set();
					ewh.Reset();
				}
				playbackThread = null;
				if (vorbisFile == IntPtr.Zero)
				{
					return;
				}
				int hr = 0;
				void* vf_ptr = (void*)vorbisFile;
				vorbisFile = IntPtr.Zero;
				try
				{
					if (NativeMethods.Final(vf_ptr) != 0)
					{
						return;
					}
				}
				catch
				{
				}
				if (SecBuf != (SecondaryBuffer)null)
				{
					SecBuf.Dispose();
					SecBuf = null;
				}
			}
			GC.Collect();
		}

		public void Play(float position, float repeatPosition)
		{
			if (vorbisFile == IntPtr.Zero)
			{
				return;
			}
			if (_playing)
			{
				Stop();
				while (_playing)
				{
					Thread.Sleep(20);
				}
			}
			playbackThread = new Thread(PlayOggDecodeThreadProc);
			if (playbackThread != null)
			{
				_position = position;
				_repeatPosition = repeatPosition;
				stopNow = false;
				playbackThread.Priority = ThreadPriority.Highest;
				playbackThread.Start();
			}
		}

		public void Play()
		{
			Play(0f, float.NaN);
		}

		public void Stop()
		{
			stopNow = true;
		}

		public void Dispose()
		{
			Close();
			GC.SuppressFinalize(this);
		}

		~OggPlayer()
		{
			Dispose();
		}

		public unsafe void PlayOggDecodeThreadProc()
		{
			//IL_08b4: Expected Obj, but got Unknown
			//IL_08eb: Expected Obj, but got Unknown
			//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
			//IL_05b8: Unknown result type (might be due to invalid IL or missing references)
			//IL_05be: Expected Obj, but got Unknown
			//IL_051e: Unknown result type (might be due to invalid IL or missing references)
			//IL_0528: Expected Obj, but got Unknown
			//IL_0438: Unknown result type (might be due to invalid IL or missing references)
			//IL_043e: Expected Obj, but got Unknown
			//IL_043f: Unknown result type (might be due to invalid IL or missing references)
			//IL_0493: Unknown result type (might be due to invalid IL or missing references)
			//IL_049d: Expected Obj, but got Unknown
			_playing = true;
			if (vorbisFile == IntPtr.Zero)
			{
				_playing = false;
				return;
			}
			SecBuf = null;
			BufferDescription bufferDesc = null;
			Notify notify = null;
			MemoryStream memoryStream = null;
			AutoResetEvent autoResetEvent = new AutoResetEvent(false);
			AutoResetEvent autoResetEvent2 = new AutoResetEvent(false);
			AutoResetEvent autoResetEvent3 = new AutoResetEvent(false);
			try
			{
				int channels = 0;
				int samplingRate = 0;
				int lastChannels = 0;
				int lastSamplingRate = 0;
				int bytesPerSecond = 0;
				int blockAlign = 0;
				int written = 0;
				int bufferSize = 0;
				int writePos = 0;
				int playPos = 0;
				WaitHandle[] waitHandles = new WaitHandle[4] { autoResetEvent, autoResetEvent2, autoResetEvent3, ewh };
				bool keepDecoding = true;
				bool playbackDone = false;
				bool firstFill = true;
				byte[] decodeBuffer = new byte[4096];
				memoryStream = new MemoryStream();
				WaveFormat format = new WaveFormat();
				lock (thislock)
				{
					if (!(vorbisFile != IntPtr.Zero))
					{
						return;
					}
					NativeMethods.SeekTime((void*)vorbisFile, _position);
				}
				while (true)
				{
					if (stopNow)
					{
						if ((SecondaryBuffer)null != SecBuf)
						{
							((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Stop();
						}
						break;
					}
					int decodedBytes;
					fixed (byte* buf_out = &decodeBuffer[0])
					{
						int holeErrors = 0;
						int badLinkErrors = 0;
						lock (thislock)
						{
							if (!(vorbisFile != IntPtr.Zero))
							{
								return;
							}
							decodedBytes = NativeMethods.DecodePacket((void*)vorbisFile, buf_out, decodeBuffer.Length, 16, &channels, &samplingRate, &holeErrors, &badLinkErrors);
							_position = (float)NativeMethods.TellTime((void*)vorbisFile);
						}
					}
					if (decodedBytes == 0)
					{
						if (float.IsNaN(_repeatPosition))
						{
							if (!firstFill)
							{
								if (playPos <= writePos)
								{
									memoryStream.SetLength(bufferSize - writePos);
									memoryStream.Position = 0L;
									((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Write(writePos, (Stream)memoryStream, bufferSize - writePos, (LockFlag)0);
									memoryStream.SetLength(playPos);
									memoryStream.Position = 0L;
									((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Write(0, (Stream)memoryStream, playPos, (LockFlag)0);
								}
								else
								{
									memoryStream.SetLength(playPos - writePos);
									memoryStream.Position = 0L;
									((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Write(writePos, (Stream)memoryStream, playPos - writePos, (LockFlag)0);
								}
							}
							playbackDone = true;
						}
						else
						{
							fixed (byte* buf_out2 = &decodeBuffer[0])
							{
								int holeErrors2 = 0;
								int badLinkErrors2 = 0;
								lock (thislock)
								{
									if (!(vorbisFile != IntPtr.Zero))
									{
										return;
									}
									NativeMethods.SeekTime((void*)vorbisFile, _repeatPosition);
									decodedBytes = NativeMethods.DecodePacket((void*)vorbisFile, buf_out2, decodeBuffer.Length, 16, &channels, &samplingRate, &holeErrors2, &badLinkErrors2);
									_position = (float)NativeMethods.TellTime((void*)vorbisFile);
								}
							}
						}
					}
					if (keepDecoding && playbackDone)
					{
						if (Stopped != null)
						{
							OggPlayEventArgs e = new OggPlayEventArgs(false, "The Ogg Vorbis file or memory stream has no usable data.", false);
							Stopped(this, e);
						}
						_playing = false;
						return;
					}
					if (!keepDecoding && (channels != lastChannels || samplingRate != lastSamplingRate))
					{
						if ((SecondaryBuffer)null != SecBuf)
						{
							((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Stop();
						}
						if (Stopped != null)
						{
							OggPlayEventArgs e2 = new OggPlayEventArgs(false, "The Ogg Vorbis file has a format change and DirectSound can't handle this.", false);
							Stopped(this, e2);
						}
						_playing = false;
						return;
					}
					if (keepDecoding)
					{
						blockAlign = channels * 2;
						bytesPerSecond = samplingRate * blockAlign;
						int holdBytes = (int)((float)samplingRate * SecBufHoldThisManySeconds);
						SecBufHoldThisManySeconds = (float)holdBytes / (float)samplingRate;
						bufferRate = bytesPerSecond;
						format.AverageBytesPerSecond = bytesPerSecond;
						format.BitsPerSample = 16;
						format.BlockAlign = (short)blockAlign;
						format.Channels = (short)channels;
						format.SamplesPerSecond = samplingRate;
						format.FormatTag = (WaveFormatTag)1;
						bufferDesc = new BufferDescription();
						bufferDesc.Format = format;
						bufferSize = (bufferDesc.BufferBytes = holdBytes * blockAlign);
						bufferDesc.CanGetCurrentPosition = true;
						bufferDesc.ControlPositionNotify = true;
						bufferDesc.ControlVolume = true;
						bufferDesc.ControlPan = true;
						bufferDesc.GlobalFocus = _globalFocus;
						try
						{
							if (SecBuf == (SecondaryBuffer)null)
							{
								SecBuf = new SecondaryBuffer(bufferDesc, directSoundDevice);
							}
							((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Volume = bufferVolume;
							((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Pan = 0;
						}
						catch (ControlUnavailableException)
						{
							if (SecBuf != (SecondaryBuffer)null)
							{
								if (!SecBuf.Disposed)
								{
									SecBuf.Dispose();
								}
								SecBuf = null;
							}
						}
						if (SecBuf == (SecondaryBuffer)null)
						{
							bufferDesc.ControlVolume = false;
							bufferDesc.ControlPan = false;
							SecBuf = new SecondaryBuffer(bufferDesc, directSoundDevice);
						}
						BufferPositionNotify[] notifyPositions = new BufferPositionNotify[3];
						notifyPositions[0].Offset = 0;
						notifyPositions[0].EventNotifyHandle = autoResetEvent.SafeWaitHandle.DangerousGetHandle();
						notifyPositions[1].Offset = holdBytes / 3 * blockAlign;
						notifyPositions[1].EventNotifyHandle = autoResetEvent2.SafeWaitHandle.DangerousGetHandle();
						notifyPositions[2].Offset = holdBytes * 2 / 3 * blockAlign;
						notifyPositions[2].EventNotifyHandle = autoResetEvent3.SafeWaitHandle.DangerousGetHandle();
						notify = new Notify(SecBuf);
						notify.SetNotificationPositions(notifyPositions);
						lastChannels = channels;
						lastSamplingRate = samplingRate;
						keepDecoding = false;
					}
					else if (playbackDone)
					{
						if (firstFill)
						{
							((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Play(0, (BufferPlayFlags)1);
						}
						int wrapCount = 0;
						int playSnapshot = playPos;
						while (!stopNow)
						{
							int playPosition = ((Microsoft.DirectX.DirectSound.Buffer)SecBuf).PlayPosition;
							if (playPosition < playSnapshot)
							{
								wrapCount++;
								memoryStream.SetLength(bufferSize - playSnapshot);
								memoryStream.Position = 0L;
								((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Write(playSnapshot, (Stream)memoryStream, bufferSize - playSnapshot, (LockFlag)0);
								memoryStream.SetLength(playPosition);
								memoryStream.Position = 0L;
								((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Write(0, (Stream)memoryStream, playPosition, (LockFlag)0);
							}
							else if (playPosition > playSnapshot)
							{
								memoryStream.SetLength(playPosition - playSnapshot);
								memoryStream.Position = 0L;
								((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Write(playSnapshot, (Stream)memoryStream, playPosition - playSnapshot, (LockFlag)0);
							}
							if (playPos <= writePos)
							{
								if (playPosition >= writePos || wrapCount > 0)
								{
									break;
								}
							}
							else if ((playPosition < playPos && playPosition >= writePos) || wrapCount > 1)
							{
								break;
							}
							Thread.Sleep(20);
							playSnapshot = playPosition;
						}
						if ((SecondaryBuffer)null != SecBuf)
						{
							((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Stop();
						}
						break;
					}
					memoryStream.SetLength(0L);
					memoryStream.Write(decodeBuffer, 0, decodedBytes);
					memoryStream.Position = 0L;
					written = 0;
					if (firstFill)
					{
						int chunkLength = (int)Math.Min(memoryStream.Length, bufferSize - writePos);
						if (chunkLength > 0)
						{
							((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Write(writePos, (Stream)memoryStream, chunkLength, (LockFlag)0);
							writePos += chunkLength;
							written += chunkLength;
						}
						if (bufferSize != writePos)
						{
							continue;
						}
						firstFill = false;
						writePos = 0;
						((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Play(0, (BufferPlayFlags)1);
						Thread.Sleep(0);
					}
					while (written < memoryStream.Length && !stopNow)
					{
						int spaceLeft = 0;
						int playPosition2 = ((Microsoft.DirectX.DirectSound.Buffer)SecBuf).PlayPosition;
						int writePosition = ((Microsoft.DirectX.DirectSound.Buffer)SecBuf).WritePosition;
						if (writePos < playPosition2 && (writePosition >= playPosition2 || writePosition < writePos))
						{
							spaceLeft = playPosition2 - writePos;
						}
						else if (writePos > writePosition && writePosition >= playPosition2)
						{
							spaceLeft = bufferSize - writePos + playPosition2;
						}
						if (spaceLeft > memoryStream.Length / 2)
						{
							spaceLeft = (int)Math.Min(spaceLeft, memoryStream.Length - written);
							memoryStream.Position = written;
							((Microsoft.DirectX.DirectSound.Buffer)SecBuf).Write(writePos, (Stream)memoryStream, spaceLeft, (LockFlag)0);
							writePos = (writePos + spaceLeft) % bufferSize;
							playPos = playPosition2;
							written += spaceLeft;
						}
						else
						{
							WaitHandle.WaitAny(waitHandles, new TimeSpan(0, 0, 5), true);
						}
					}
					bufferPosition = writePos;
				}
				if (Stopped != null)
				{
					OggPlayEventArgs e3 = new OggPlayEventArgs(stopNow);
					Stopped(this, e3);
				}
				_playing = false;
			}
			catch (NoDriverException ex2)
			{
				NoDriverException ex3 = ex2;
				if (Stopped != null)
				{
					OggPlayEventArgs e4 = new OggPlayEventArgs(false, ((Exception)(object)ex3).Message, false);
					Stopped(this, e4);
				}
				_playing = false;
			}
			catch (ControlUnavailableException ex4)
			{
				ControlUnavailableException ex5 = ex4;
				if (Stopped != null)
				{
					OggPlayEventArgs e5 = new OggPlayEventArgs(false, ((Exception)(object)ex5).Message, false);
					Stopped(this, e5);
				}
				_playing = false;
			}
			catch (ArgumentException ex6)
			{
				if (Stopped != null)
				{
					OggPlayEventArgs e6 = new OggPlayEventArgs(false, ex6.Message, false);
					Stopped(this, e6);
				}
				_playing = false;
			}
			catch (Exception ex7)
			{
				if (Stopped != null)
				{
					OggPlayEventArgs e7 = new OggPlayEventArgs(false, ex7.Message, false);
					Stopped(this, e7);
				}
				_playing = false;
			}
			finally
			{
				if (autoResetEvent != null)
				{
					if (autoResetEvent.SafeWaitHandle != null)
					{
						autoResetEvent.SafeWaitHandle.Close();
					}
					autoResetEvent.Close();
					autoResetEvent = null;
				}
				if (autoResetEvent2 != null)
				{
					if (autoResetEvent2.SafeWaitHandle != null)
					{
						autoResetEvent2.SafeWaitHandle.Close();
					}
					autoResetEvent2.Close();
					autoResetEvent2 = null;
				}
				if (autoResetEvent3 != null)
				{
					if (autoResetEvent3.SafeWaitHandle != null)
					{
						autoResetEvent3.SafeWaitHandle.Close();
					}
					autoResetEvent3.Close();
					autoResetEvent3 = null;
				}
				if ((SecondaryBuffer)null != SecBuf)
				{
					if (!SecBuf.Disposed)
					{
						SecBuf.Dispose();
					}
					SecBuf = null;
				}
				if (memoryStream != null)
				{
					memoryStream.Close();
					memoryStream = null;
				}
				if (bufferDesc != null)
				{
					bufferDesc.Dispose();
					bufferDesc = null;
				}
				if ((Notify)null != notify)
				{
					if (!notify.Disposed)
					{
						notify.Dispose();
					}
					notify = null;
				}
			}
		}
	}
}
