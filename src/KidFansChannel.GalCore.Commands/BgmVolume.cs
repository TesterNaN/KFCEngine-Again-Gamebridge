using System.Xml;

namespace KidFansChannel.GalCore.Commands
{
	public class BgmVolume : ScriptCommand
	{
		private float _volume;

		private bool _animation;

		protected float Volume
		{
			get
			{
				return _volume;
			}
			set
			{
				_volume = value;
			}
		}

		public bool Animation
		{
			get
			{
				return _animation;
			}
			set
			{
				_animation = value;
			}
		}

		public BgmVolume()
		{
			Single = false;
			Lasting = false;
			Command = "bgmvolume";
		}

		public override void ConvertXml(XmlNode node, NodeInfo info)
		{
			base.ConvertXml(node, info);
			_volume = ScriptHelper.GetArgumentFloat(node, info, "volume");
			_animation = ScriptHelper.GetArgumentBool(node, info, "animation", true);
		}

		public override void InitCommand()
		{
			base.InitCommand();
			bool found = false;
			foreach (ScriptCommand command in ScriptCommand.CommandList)
			{
				if (command is BgmPlay)
				{
					((BgmPlay)command).Volume = _volume;
					if (_animation)
					{
						ScriptCommand.Sound.ChangeVolumeBgm(_volume);
					}
					else
					{
						ScriptCommand.Sound.VolumeBgm = _volume;
					}
					found = true;
					break;
				}
			}
			FinishCommand();
			if (!found)
			{
				throw new RuntimeException(Node, Info, "No bgm is playing now.");
			}
		}
	}
}
