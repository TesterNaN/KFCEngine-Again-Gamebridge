using System.Collections.Generic;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.GalCore.Commands
{
	internal class EffectShadow : ScriptCommand, IDrawable, IUpdatable
	{
		private bool _local;

		private List<Picture> _pictures = new List<Picture>();

		private float _layerMinor;

		public bool Local
		{
			get
			{
				return _local;
			}
			set
			{
				_local = value;
			}
		}

		public List<Picture> Pictures
		{
			get
			{
				return _pictures;
			}
		}

		public int LayerMajor
		{
			get
			{
				return 4;
			}
		}

		public float LayerMinor
		{
			get
			{
				return _layerMinor;
			}
			set
			{
				_layerMinor = value;
			}
		}

		internal EffectShadow()
		{
			Single = false;
			Lasting = false;
			Command = "";
		}

		public override void FinishCommand()
		{
			if (!_local)
			{
				base.FinishCommand();
			}
		}

		public void Draw()
		{
			foreach (Picture picture in _pictures)
			{
				ScriptCommand.Canvas.DrawPicture(picture);
			}
		}

		public void Update(double appTime, float elaspedTime)
		{
		}
	}
}
