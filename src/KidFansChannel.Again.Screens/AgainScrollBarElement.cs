using System.Drawing;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Screens
{
	public class AgainScrollBarElement : ScrollBarElement
	{
		private Picture picScrollBar;

		private Picture[] picScrollBarParts = new Picture[3];

		private bool _dataReady;

		public override bool DataReady
		{
			get
			{
				if (_dataReady)
				{
					return true;
				}
				if (picScrollBar != null && picScrollBar.IsCreated)
				{
					picScrollBarParts[0] = GameManager.Graphics.CreateSubPicture(picScrollBar, new Rectangle(0, 0, 15, 7));
					picScrollBarParts[1] = GameManager.Graphics.CreateSubPicture(picScrollBar, new Rectangle(0, 7, 15, 30));
					picScrollBarParts[2] = GameManager.Graphics.CreateSubPicture(picScrollBar, new Rectangle(0, 37, 15, 8));
					_dataReady = true;
					return true;
				}
				return false;
			}
		}

		/// <summary>滚动条宽度固定 15，改大小时同步调整命中区域。</summary>
		public override int Size
		{
			get
			{
				return base.Size;
			}
			set
			{
				base.Size = value;
				Region = new Rectangle(0, 0, 15, Size);
			}
		}

		public override void InitData()
		{
			picScrollBar = GameManager.Data.LoadPicture("system", "scroll_bar.png");
		}

		public override void FinalData()
		{
			for (int i = 0; i < 3; i++)
			{
				picScrollBarParts[i].Dispose();
			}
			picScrollBar.Dispose();
			_dataReady = false;
		}

		public override void Init()
		{
			ReConstruct();
			for (int i = 0; i < 3; i++)
			{
				Add(new Element(picScrollBarParts[i]), "scrollbar" + i);
			}
		}

		public override void Final()
		{
			base.Final();
			for (int i = 0; i < 3; i++)
			{
				base["scrollbar" + i].Dispose();
			}
			Dispose();
		}

		protected override void UpdateBar()
		{
			int thumbStart = (int)(Start * (float)Size + 0.5f);
			int thumbSize = ((Length != 0f) ? ((int)(Length * (float)Size + 0.5f)) : Size);
			if (thumbSize > 15)
			{
				base["scrollbar0"].Position = new Point(0, thumbStart);
				SetVisible("scrollbar1", true);
				base["scrollbar1"].Position = new Point(0, thumbStart + 7);
				base["scrollbar1"].ElementPicture.ScaleY = (float)(thumbSize - 15) / 30f;
				base["scrollbar2"].Position = new Point(0, thumbStart + thumbSize - 8);
				return;
			}
			int thumbCenter = (int)((Start + Length / 2f) * (float)Size + 0.5f);
			SetVisible("scrollbar1", false);
			if (thumbCenter < 7)
			{
				base["scrollbar0"].Position = new Point(0, 0);
				base["scrollbar2"].Position = new Point(0, 7);
			}
			else if (thumbCenter > Size - 8)
			{
				base["scrollbar0"].Position = new Point(0, Size - 15);
				base["scrollbar2"].Position = new Point(0, Size - 8);
			}
			else
			{
				base["scrollbar0"].Position = new Point(0, thumbCenter - 7);
				base["scrollbar2"].Position = new Point(0, thumbCenter);
			}
		}

		/// <summary>判断指针在滑块上方（Up）、滑块上（Block）还是滑块下方（Down）。</summary>
		public override ScrollBarArea GetArea(Point location)
		{
			int thumbStart = (int)(Start * (float)Size + 0.5f);
			int thumbSize = ((Length != 0f) ? ((int)(Length * (float)Size + 0.5f)) : Size);
			if (thumbSize < 15)
			{
				// 滑块太短时不按真实位置命中，统一按 15 像素的固定片处理
				int thumbCenter = (int)((Start + Length / 2f) * (float)Size + 0.5f);
				thumbStart = ((thumbCenter >= 7) ? ((thumbCenter <= Size - 8) ? (thumbCenter - 7) : (Size - 15)) : 0);
				thumbSize = 15;
			}
			if (location.Y < thumbStart)
			{
				return ScrollBarArea.Up;
			}
			if (location.Y < thumbStart + thumbSize)
			{
				return ScrollBarArea.Block;
			}
			return ScrollBarArea.Down;
		}

		/// <summary>纵向位移换算成 Start 的增量：整条轨道等于 1.0。</summary>
		public override float DragValue(Point distance)
		{
			return (float)distance.Y / (float)Size;
		}
	}
}
