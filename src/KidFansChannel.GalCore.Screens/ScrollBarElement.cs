using System;
using System.Drawing;
using System.Windows.Forms;

namespace KidFansChannel.GalCore.Screens
{
	public abstract class ScrollBarElement : Element
	{
		/// <summary>指针落在滚动条的哪个部位，决定按下的动作。</summary>
		public enum ScrollBarArea
		{
			None,
			Up,
			Block,
			Down
		}

		private float _start;

		private float _length;

		private int _size;

		/// <summary>按下时指针相对元素的位置，用来算拖动位移。</summary>
		private Point dragStartLocation = Point.Empty;

		/// <summary>按下瞬间的 Start，拖动时以它为基准。</summary>
		private float dragStartValue;

		public abstract bool DataReady { get; }

		public float Start
		{
			get
			{
				return _start;
			}
			set
			{
				if (_start != value)
				{
					_start = value;
					UpdateBar();
				}
				else
				{
					_start = value;
				}
			}
		}

		public float Length
		{
			get
			{
				return _length;
			}
			set
			{
				if (_length != value)
				{
					if (value <= 1f)
					{
						_length = value;
					}
					else
					{
						_length = 1f;
					}
					UpdateBar();
				}
				else
				{
					_length = value;
				}
			}
		}

		public virtual int Size
		{
			get
			{
				return _size;
			}
			set
			{
				if (_size != value)
				{
					_size = value;
					UpdateBar();
				}
				else
				{
					_size = value;
				}
			}
		}

		/// <summary>Start 因拖动或翻页变化时触发。</summary>
		public event EventHandler ValueChanged;

		public ScrollBarElement()
		{
			CanDrag = true;
			DragStart += ScrollBarElement_DragStart;
			DragEnd += ScrollBarElement_DragEnd;
			DragMove += ScrollBarElement_DragMove;
		}

		public abstract void InitData();

		public abstract void FinalData();

		public abstract void Init();

		public virtual void Final()
		{
			_start = (_length = (_size = 0));
		}

		protected abstract void UpdateBar();

		/// <summary>按下：点上半段就翻一页向下，下半段向上，拖块则进入拖动。</summary>
		private void ScrollBarElement_DragStart(object sender, MouseEventArgs e)
		{
			switch (GetArea(e.Location))
			{
			default:
				return;
			case ScrollBarArea.Up:
				_start -= _length;
				if (_start < 0f)
				{
					_start = 0f;
				}
				break;
			case ScrollBarArea.Down:
				_start += _length;
				if (_start + _length > 1f)
				{
					_start = 1f - _length;
				}
				break;
			case ScrollBarArea.Block:
				Drag();
				dragStartLocation = e.Location;
				dragStartValue = _start;
				break;
			case ScrollBarArea.None:
				return;
			}
			if (ValueChanged != null)
			{
				ValueChanged(this, EventArgs.Empty);
			}
			UpdateBar();
		}

		/// <summary>松开：清掉拖动基准。</summary>
		private void ScrollBarElement_DragEnd(object sender, MouseEventArgs e)
		{
			dragStartLocation = Point.Empty;
			dragStartValue = 0f;
		}

		/// <summary>拖动中：按位移换算新的 Start 并夹在 0..1-Length 之间。</summary>
		private void ScrollBarElement_DragMove(object sender, MouseEventArgs e)
		{
			Point distance = new Point(e.Location.X - dragStartLocation.X, e.Location.Y - dragStartLocation.Y);
			_start = dragStartValue + DragValue(distance);
			if (_start < 0f)
			{
				_start = 0f;
			}
			else if (_start + _length > 1f)
			{
				_start = 1f - _length;
			}
			if (ValueChanged != null)
			{
				ValueChanged(this, EventArgs.Empty);
			}
			UpdateBar();
		}

		/// <summary>判断指针位于滚动条的哪个部位。</summary>
		public abstract ScrollBarArea GetArea(Point location);

		/// <summary>把像素位移换算成 Start 的增量。</summary>
		public abstract float DragValue(Point distance);
	}
}
