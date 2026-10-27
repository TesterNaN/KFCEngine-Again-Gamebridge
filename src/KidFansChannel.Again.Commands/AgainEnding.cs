using KidFansChannel.GalCore.Commands;

namespace KidFansChannel.Again.Commands
{
	internal class AgainEnding : Ending
	{
		public override void InitCommand()
		{
			base.InitCommand();
			switch (EndingIndex)
			{
			case 0:
			{
				for (int m = 0; m < 4; m++)
				{
					ScriptCommand.GalCore.GameState.UpdateAlbum(m + 90);
				}
				for (int n = 0; n < 4; n++)
				{
					ScriptCommand.GalCore.GameState.UpdateAlbum(n + 105);
				}
				for (int i = 0; i < 2; i++)
				{
					ScriptCommand.GalCore.GameState.UpdateAlbum(i + 115);
				}
				ScriptCommand.GalCore.GameState.UpdateAlbum(121);
				ScriptCommand.GalCore.GameState.UpdateEncyOpen(8);
				ScriptCommand.GalCore.GameState.UpdateEncyOpen(13);
				ScriptCommand.GalCore.GameState.UpdateEncyOpen(34);
				ScriptCommand.GalCore.GameState.UpdateEncyOpen(35);
				break;
			}
			case 1:
			case 2:
			{
				for (int k = 0; k < 5; k++)
				{
					ScriptCommand.GalCore.GameState.UpdateAlbum(k + 94);
				}
				for (int l = 0; l < 2; l++)
				{
					ScriptCommand.GalCore.GameState.UpdateAlbum(l + 109);
				}
				ScriptCommand.GalCore.GameState.UpdateAlbum(122);
				ScriptCommand.GalCore.GameState.UpdateEncyOpen(14);
				ScriptCommand.GalCore.GameState.UpdateEncyOpen(19);
				ScriptCommand.GalCore.GameState.UpdateEncyOpen(20);
				break;
			}
			case 3:
			case 4:
			{
				for (int i = 0; i < 6; i++)
				{
					ScriptCommand.GalCore.GameState.UpdateAlbum(i + 99);
				}
				for (int j = 0; j < 9; j++)
				{
					ScriptCommand.GalCore.GameState.UpdateAlbum(j + 111);
				}
				ScriptCommand.GalCore.GameState.UpdateAlbum(120);
				ScriptCommand.GalCore.GameState.UpdateEncyOpen(27);
				ScriptCommand.GalCore.GameState.UpdateEncyOpen(29);
				ScriptCommand.GalCore.GameState.UpdateEncyOpen(48);
				ScriptCommand.GalCore.GameState.UpdateEncyOpen(66);
				ScriptCommand.GalCore.GameState.UpdateEncyOpen(67);
				break;
			}
			}
			ScriptCommand.GalCore.GameState.UpdateMusic(7);
			if (!ScriptCommand.GalCore.GameState.CheckEnding(0) || (!ScriptCommand.GalCore.GameState.CheckEnding(1) && !ScriptCommand.GalCore.GameState.CheckEnding(2)) || (!ScriptCommand.GalCore.GameState.CheckEnding(3) && !ScriptCommand.GalCore.GameState.CheckEnding(4)))
			{
				return;
			}
			for (int j = 0; j < 3; j++)
			{
				ScriptCommand.GalCore.GameState.UpdateAlbum(j + 123);
			}
			for (int k = 0; k < 3; k++)
			{
				ScriptCommand.GalCore.GameState.UpdateMusic(k + 37);
			}
			if (EndingIndex != -1 && ScriptCommand.GalCore.GameState.CheckEnding(0) && ScriptCommand.GalCore.GameState.CheckEnding(1) && ScriptCommand.GalCore.GameState.CheckEnding(4))
			{
				ScriptCommand.GalCore.SetReturnTitleStory("Congratulations", "szz00", "Congratulations", 0);
			}
			if (ScriptCommand.GalCore.GameState.CheckEnding(0) && ScriptCommand.GalCore.GameState.CheckEnding(1) && ScriptCommand.GalCore.GameState.CheckEnding(2) && ScriptCommand.GalCore.GameState.CheckEnding(3) && ScriptCommand.GalCore.GameState.CheckEnding(4))
			{
				ScriptCommand.GalCore.GameState.UpdateEncyOpen(15);
				for (int l = 0; l < 11; l++)
				{
					ScriptCommand.GalCore.GameState.UpdateEncyOpen(l + 83);
				}
			}
		}
	}
}
