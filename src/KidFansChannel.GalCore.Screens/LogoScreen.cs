using KidFansChannel.DataCore;

namespace KidFansChannel.GalCore.Screens
{
	public abstract class LogoScreen : GameScreen
	{
		protected string[] Videos;

		private int currentIndex;

		public override bool DataReady
		{
			get
			{
				if (base.DataReady)
				{
					return true;
				}
				DataReady = true;
				return true;
			}
		}

		public override bool FinalScreen()
		{
			if (!base.FinalScreen())
			{
				return false;
			}
			Finished = true;
			return true;
		}

		public override void Update(float elaspedTime)
		{
			if (currentIndex == Videos.Length)
			{
				FinalScreen();
				GameManager.GalCore.ReturnTitleScreen();
				return;
			}
			KidFansChannel.DataCore.DataCore.DataType dataType = GameManager.Data.CheckData("movie", Videos[currentIndex] + ".wmv", KidFansChannel.DataCore.DataCore.DataType.File);
			if (dataType == KidFansChannel.DataCore.DataCore.DataType.File && GameManager.GalCore.GameVideo != null)
			{
				GameManager.GalCore.GameVideo.Play(GameManager.Data.GetFile());
			}
			currentIndex++;
		}
	}
}
