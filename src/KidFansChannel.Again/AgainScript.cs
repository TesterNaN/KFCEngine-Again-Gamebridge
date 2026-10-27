using System.Windows.Forms;
using KidFansChannel.Again.Commands;
using KidFansChannel.Again.Effects;
using KidFansChannel.Again.Screens;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Commands;
using KidFansChannel.GalExt.DisplayStyles;
using KidFansChannel.GalExt.Effects;
using KidFansChannel.GalExt.TransformStyles;

namespace KidFansChannel.Again
{
	internal class AgainScript : GameScript
	{
		public AgainScript()
		{
			QuickIcon = 3;
			TitleIcon = 4;
			SelectIcon = 5;
			QuickIndexStart = 60;
			QuickIndexEnd = 90;
		}

		protected override void PrepareTypes()
		{
			base.PrepareTypes();
			AssignTypeClass(typeof(AgainClick));
			AssignTypeClass(typeof(AgainDialogue));
			AssignTypeClass(typeof(AgainParagraph));
			AssignTypeClass(typeof(AgainSelect));
			AssignTypeClass(typeof(AgainShowChapter));
			AssignTypeClass(typeof(AgainEnding));
			CharaTransform.AddStyle(typeof(BottomWaveOneChara));
			CharaTransform.AddStyle(typeof(BottomWaveChara));
			CharaTransform.AddStyle(typeof(SmallSwingChara));
			CharaTransform.AddStyle(typeof(WaveFall1Chara));
			CharaTransform.AddStyle(typeof(WaveFall2Chara));
			CharaTransform.AddStyle(typeof(Shock1Chara));
			CharaTransform.AddStyle(typeof(Shock2Chara));
			CharaTransform.AddStyle(typeof(SwingChara));
			BgTransform.AddStyle(typeof(DropBg));
			BgTransform.AddStyle(typeof(Shock1Bg));
			BgTransform.AddStyle(typeof(Shock2Bg));
			BgTransform.AddStyle(typeof(ShockLoopBg));
			BgTransform.AddStyle(typeof(ShockLoopZoomInBg));
			BgDisplay.AddStyle(typeof(TransLeft));
			BgDisplay.AddStyle(typeof(TransRight));
			BgDisplay.AddStyle(typeof(TransUp));
			BgDisplay.AddStyle(typeof(TransDown));
			EffectStart.AddEffect(typeof(Fireworks));
			EffectStart.AddEffect(typeof(Light));
			EffectStart.AddEffect(typeof(Dark));
			EffectStart.AddEffect(typeof(AgainSun));
			EffectStart.AddEffect(typeof(Shock));
			EffectStart.AddEffect(typeof(Film));
			EffectStart.AddEffect(typeof(AgainFog));
			EffectStart.AddEffect(typeof(GoldTooth));
			EffectStart.AddEffect(typeof(Flash));
			EffectStart.AddEffect(typeof(AgainSakura));
		}

		public override string GetStoryName(string story)
		{
			switch (story)
			{
			case "Inori":
				return GalCore.GameLanguage.ResourceManager.GetString("InoriStr");
			case "Miyabi":
				return GalCore.GameLanguage.ResourceManager.GetString("MiyabiStr");
			case "Rikarin":
				return GalCore.GameLanguage.ResourceManager.GetString("RikarinStr");
			case "Congratulations":
				return GalCore.GameLanguage.ResourceManager.GetString("ConStr");
			case "Drama":
				return GalCore.GameLanguage.ResourceManager.GetString("DramaStr");
			default:
				return story;
			}
		}

		public override void QuickSave(QuickSaveType type)
		{
			if (CurrentGameScript.StoryName == "Inori" || CurrentGameScript.StoryName == "Miyabi" || CurrentGameScript.StoryName == "Rikarin")
			{
				base.QuickSave(type);
				((AgainCore)GalCore).StartQuickSaveIcon();
			}
		}

		public override void QuickLoad()
		{
			GalCore.ShowMessageBox(new AgainMessageBoxScreen(GalCore.GameLanguage.ResourceManager.GetString("LoadStr"), MessageBoxButtons.YesNo, AgainQuickLoad, null));
		}

		private void AgainQuickLoad(object[] empty)
		{
			base.QuickLoad();
		}

		public override void SaveStory(int index, string comment, int icon)
		{
			if (CurrentGameScript.StoryName == "Inori" || CurrentGameScript.StoryName == "Miyabi" || CurrentGameScript.StoryName == "Rikarin")
			{
				base.SaveStory(index, comment, icon);
			}
			GalCore.GameState.SaveState();
		}
	}
}
