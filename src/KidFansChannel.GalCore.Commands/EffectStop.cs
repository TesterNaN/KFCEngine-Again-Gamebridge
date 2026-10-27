using System.Xml;

namespace KidFansChannel.GalCore.Commands
{
	public class EffectStop : ScriptCommand
	{
		private string _typeStr;

		private bool _animation;

		private EffectStart effect;

		public string TypeStr
		{
			get
			{
				return _typeStr;
			}
			protected set
			{
				_typeStr = value;
			}
		}

		public bool Animation
		{
			get
			{
				return _animation;
			}
			protected set
			{
				_animation = value;
			}
		}

		public EffectStop()
		{
			Single = false;
			Lasting = false;
			Command = "effectstop";
		}

		public override void ConvertXml(XmlNode node, NodeInfo info)
		{
			base.ConvertXml(node, info);
			_typeStr = ScriptHelper.GetArgumentString(node, info, "type");
			if (EffectStart.Types.ContainsKey(_typeStr))
			{
				_animation = ScriptHelper.GetArgumentBool(node, info, "animation", true);
				Single = _animation;
				Lasting = _animation;
			}
		}

		public override void InitCommand()
		{
			base.InitCommand();
			foreach (ScriptCommand command in ScriptCommand.CommandList)
			{
				if (command is EffectStart && ((EffectStart)command).TypeStr == _typeStr)
				{
					effect = (EffectStart)command;
					effect.Stop(_animation);
				}
			}
			FinishCommand();
		}
	}
}
