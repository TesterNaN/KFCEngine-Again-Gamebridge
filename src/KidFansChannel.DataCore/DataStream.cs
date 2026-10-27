using System;
using System.IO;

namespace KidFansChannel.DataCore
{
	/// <summary>
	/// 在一条底层流上开一个定长「窗口」。
	///
	/// Position / Seek / Length 都相对窗口起点计算，读到窗口末尾即结束，
	/// 不会越界影响底层流的其他使用者。
	///
	/// KFCD 读取器与写入器靠它把整份文件切成「条目表」和「单条数据」两块，
	/// 让 CryptoStream 只处理自己那一段，而多个 DataStream 能共用一个 FileStream。
	/// </summary>
	public class DataStream : Stream
	{
		private Stream _baseStream;

		private long _offset;

		private long _length;

		public Stream BaseStream
		{
			get
			{
				return _baseStream;
			}
		}

		public long Offset
		{
			get
			{
				return _offset;
			}
		}

		public override bool CanRead
		{
			get
			{
				return _baseStream.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return _baseStream.CanSeek;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return _baseStream.CanWrite;
			}
		}

		public override long Length
		{
			get
			{
				return _length;
			}
		}

		public override long Position
		{
			get
			{
				return _baseStream.Position - _offset;
			}
			set
			{
				_baseStream.Position = value + _offset;
			}
		}

		public DataStream(Stream baseStream, long offset, long length)
		{
			_baseStream = baseStream;
			_offset = offset;
			_length = length;
		}

		public override void Flush()
		{
			_baseStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (Position + count > _length)
			{
				count = (int)(_length - Position);
			}
			return _baseStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			switch (origin)
			{
			case SeekOrigin.Begin:
				return _baseStream.Seek(offset + _offset, SeekOrigin.Begin) - _offset;
			case SeekOrigin.Current:
				return _baseStream.Seek(offset, SeekOrigin.Current) - _offset;
			case SeekOrigin.End:
				return _baseStream.Seek(offset - _baseStream.Length + _offset + _length, SeekOrigin.End) - _offset;
			default:
				return -1L;
			}
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			_baseStream.Write(buffer, offset, count);
		}
	}
}
