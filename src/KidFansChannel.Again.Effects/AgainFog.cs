using KidFansChannel.GalCore.Commands;
using KidFansChannel.GalExt.Effects;

namespace KidFansChannel.Again.Effects
{
	public class AgainFog : Fog
	{
		public AgainFog()
		{
			TypeNum = 3;
		}

		public override void InitData()
		{
			FogPic1 = ScriptCommand.Data.LoadPicture("system", "fog" + Style + "_1.png");
			FogPic2 = ScriptCommand.Data.LoadPicture("system", "fog" + Style + "_2.png");
		}

		protected override void InitStyle()
		{
			switch (Style)
			{
			case 0:
				Direction = true;
				break;
			case 1:
				Direction = false;
				break;
			case 2:
				Direction = true;
				break;
			}
			Alpha = 255;
		}
	}
}
