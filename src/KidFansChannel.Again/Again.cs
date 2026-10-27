using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Resources;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Records;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.InputCore;

namespace KidFansChannel.Again
{
	internal class Again
	{
		private KidFansChannel.GalCore.GalCore galCore;

		private string saveFile;

		public static string AssemblyPath;

		private bool novideo;

		protected static int Drive_CDRom;

		static Again()
		{
			Drive_CDRom = 5;
			try
			{
				AssemblyPath = Application.ExecutablePath;
			}
			catch (Exception)
			{
			}
		}

		public void Run(string[] args)
		{
			galCore = new AgainCore();
			GameManager.GalCore = galCore;
			galCore.SetGameGraphics(new GameGraphics());
			if (args.Length == 1 && File.Exists(args[0]))
			{
				saveFile = args[0];
			}
			if (args.Length == 1 && args[0] == "/novideo")
			{
				novideo = true;
			}
			InitializeAgain();
		}

		private void InitializeAgain()
		{
			Environment.CurrentDirectory = Path.GetDirectoryName(AssemblyPath);
			if (!File.Exists("system.dat"))
			{
				MessageBox.Show("System.dat not found.");
				return;
			}
			if (!File.Exists("resource.dat"))
			{
				MessageBox.Show("Resource.dat not found.");
				return;
			}
			if (!File.Exists("script.dat"))
			{
				MessageBox.Show("script.dat not found.");
				return;
			}
			if (!File.Exists("PlayOgg.dll"))
			{
				MessageBox.Show("PlayOgg.dll not found.");
				return;
			}
			if (!File.Exists("DirectShowLib-2005.dll"))
			{
				MessageBox.Show("DirectShowLib-2005.dll not found.");
				return;
			}
			if (!File.Exists("language.xml"))
			{
				MessageBox.Show("Language.xml not found.");
				return;
			}
			GameOptions gameOptions = new GameOptions();
			gameOptions.SettingFolder = "KID\\MOAgain";
			if (!File.Exists(gameOptions.GetOptionsFile()) && File.Exists("AgainSetting.exe"))
			{
				Process process = Process.Start("AgainSetting.exe");
				process.WaitForExit();
				while (!process.HasExited)
				{
				}
			}
			gameOptions.Load();
			galCore.SetGameOptions(gameOptions);
			galCore.ShowGameWindow();
			GameData gameData = new GameData();
			gameData.SetCurrentDirectory(Environment.CurrentDirectory);
			// 当前目录下五个数据包不全时，退而去找光驱：光盘卷标必须以 "MOAgain" 开头，
			// 且盘上有 MOAgain 目录并集齐五个 .dat
			if (!File.Exists("bg.dat") || !File.Exists("chara.dat") || !File.Exists("bgm.dat") || !File.Exists("se.dat") || !File.Exists("voice.dat"))
			{
				bool foundOnDisc = false;
				string[] logicalDrives = Directory.GetLogicalDrives();
				StringBuilder volumeName = new StringBuilder(256);
				StringBuilder lpFileSystemNameBuffer = new StringBuilder(256);
				for (int i = 0; i < logicalDrives.Length; i++)
				{
					int lpVolumeSerialNumber;
					int lpMaximumComponentLength;
					int lpFileSystemFlags;
					string discDataPath;
					if (GetDriveType(logicalDrives[i]) == Drive_CDRom && GetVolumeInformation(logicalDrives[i], volumeName, 256, out lpVolumeSerialNumber, out lpMaximumComponentLength, out lpFileSystemFlags, lpFileSystemNameBuffer, 256) != 0 && volumeName.ToString().StartsWith("MOAgain") && Directory.Exists(discDataPath = Path.Combine(logicalDrives[i], "MOAgain")) && File.Exists(Path.Combine(discDataPath, "bg.dat")) && File.Exists(Path.Combine(discDataPath, "chara.dat")) && File.Exists(Path.Combine(discDataPath, "bgm.dat")) && File.Exists(Path.Combine(discDataPath, "se.dat")) && File.Exists(Path.Combine(discDataPath, "voice.dat")))
					{
						gameData.AddDirectory(Path.Combine(logicalDrives[i], "MOAgain"));
						foundOnDisc = true;
						break;
					}
				}
				if (!foundOnDisc)
				{
					if (!File.Exists("bg.dat"))
					{
						MessageBox.Show("Bg.dat not found.");
						return;
					}
					if (!File.Exists("chara.dat"))
					{
						MessageBox.Show("Chara.dat not found.");
						return;
					}
				}
			}
			galCore.SetGameData(gameData);
			GameLanguage gameLanguage = new GameLanguage();
			ResourceManager resourceManager = new ResourceManager(typeof(Resource));
			gameLanguage.SetResourceManager(resourceManager);
			galCore.SetGameLanguage(gameLanguage);
			GameFont gameFont = new GameFont();
			galCore.SetGameFont(gameFont);
			GameInput gameInput = new GameInput();
			galCore.SetGameInput(gameInput);
			List<GamePadInput.GamePadInstance> gamePadList = gameInput.GetGamePadList();
			if (gamePadList.Count > gameOptions.InputDevice)
			{
				gameInput.SetGamePad(gamePadList[gameOptions.InputDevice]);
			}
			else
			{
				gameInput.SetGamePad(null);
			}
			gameOptions.Language = gameLanguage.ChangeLanguage(gameOptions.Language);
			gameOptions.Font = gameFont.ChangeFont(gameOptions.Font);
			galCore.SetGameSound(new GameSound(galCore.GameGraphics.GameWindow, gameOptions.SoundDevice));
			if (!novideo)
			{
				galCore.SetGameVideo(new GameVideo());
			}
			GameState gameState = new AgainState();
			gameState.SaveFolder = "KID\\MOAgain";
			gameState.SaveExt = "AgainSave-kfc";
			gameState.InfoExt = "AgainInfo-kfc";
			gameState.Guid = new Guid(((GuidAttribute)Attribute.GetCustomAttribute(typeof(Again).Assembly, typeof(GuidAttribute))).Value);
			gameState.GameName = "Memories Off ~Sorekara again~";
			gameState.GameSaveName = gameLanguage.ResourceManager.GetString("SaveFileStr");
			gameState.ConfigString = gameLanguage.ResourceManager.GetString("ConfigStr");
			gameState.ConfigLevel = gameLanguage.ResourceManager.GetString("LevelStr");
			gameState.ConfigComment = gameLanguage.ResourceManager.GetString("CommentStr");
			Record.AddRecordType(typeof(TextRecord));
			Record.AddRecordType(typeof(SceneRecord));
			Record.AddRecordType(typeof(ChapterRecord));
			Record.AddRecordType(typeof(SelectRecord));
			Record.AddRecordType(typeof(NullRecord));
			galCore.SetGameState(gameState);
			AgainScript againScript = new AgainScript();
			gameLanguage.ChangeLanguage(gameOptions.Language);
			Stream stream = new MemoryStream((byte[])resourceManager.GetObject("script_lang_dat"));
			gameData.AddReplacePair(string.Format("script.{0}.dat", gameLanguage.CurrentLanguage), stream);
			galCore.SetGameScript(againScript);
			if (saveFile != null)
			{
				againScript.LoadStory(saveFile);
			}
			if (againScript.CurrentGameScript == null)
			{
				if (saveFile != null)
				{
					gameState.LoadState(saveFile);
				}
				galCore.ShowLogoScreen();
			}
			MessageScreen.TextWithoutSpeakerWidth = 600;
			MessageScreen.TextWithSpeakerWidth = 480;
			MessageScreen.PageLineNum = 11;
			galCore.Initialize();
			galCore.MainLoop();
			Record.ClearRecordType();
			stream.Close();
			stream = null;
		}

		[DllImport("kernel32.dll", EntryPoint = "GetVolumeInformationA")]
		protected static extern int GetVolumeInformation(string lpRootPathName, StringBuilder lpVolumeNameBuffer, int nVolumeNameSize, out int lpVolumeSerialNumber, out int lpMaximumComponentLength, out int lpFileSystemFlags, StringBuilder lpFileSystemNameBuffer, int nFileSystemNameSize);

		[DllImport("kernel32.dll", EntryPoint = "GetDriveTypeA")]
		protected static extern int GetDriveType(string nDrive);
	}
}
