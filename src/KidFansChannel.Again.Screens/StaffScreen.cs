using System.IO;
using System.Windows.Forms;
using KidFansChannel.DataCore;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;

namespace KidFansChannel.Again.Screens
{
	/// <summary>
	/// 片尾职员表。这是娱乐通零售版才有的界面，光谱 Patch01 里没有。
	///
	/// 流程很简单：进来先停掉 BGM，接着打开 bgm35.ogg 但不立刻播；
	/// 第一次 Update 时播放片尾影片（resource.dat），第二次 Update 就结束，
	/// 同时把事先准备好的 bgm35 从头播 6.625 秒——正好接上职员表音乐。
	/// </summary>
	internal class StaffScreen : GameScreen
	{
		/// <summary>片尾影片所在的包名。</summary>
		protected string Video = "resource.dat";

		/// <summary>Update 被调用了几次，用来控制“先放影片、再收尾”这两步。</summary>
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

		public override bool InitScreen()
		{
			if (!base.InitScreen())
			{
				return false;
			}
			GameManager.Sound.StopBgm();
			return true;
		}

		public override bool FinalScreen()
		{
			if (!base.FinalScreen())
			{
				return false;
			}
			Finished = true;
			switch (GameManager.Data.CheckData("bgm", "bgm35.ogg", KidFansChannel.DataCore.DataCore.DataType.File))
			{
			case KidFansChannel.DataCore.DataCore.DataType.File:
				GameManager.Sound.OpenBgm(GameManager.Data.GetFile(), true);
				break;
			case KidFansChannel.DataCore.DataCore.DataType.Package:
				GameManager.Sound.OpenBgm(GameManager.Data.GetStream(), true);
				break;
			default:
				return true;
			}
			// 跳过开头的静音段，直接进职员表旋律
			GameManager.Sound.PlayBgm(0f, 6.625f);
			GameManager.Sound.VolumeBgm = 1f;
			return true;
		}

		public override void Update(float elaspedTime)
		{
			// 第一次进来放片尾影片，第二次就收尾
			if (currentIndex == 1)
			{
				FinalScreen();
				return;
			}
			GameManager.GalCore.GameVideo.Play(Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), Video));
			currentIndex++;
		}
	}
}
