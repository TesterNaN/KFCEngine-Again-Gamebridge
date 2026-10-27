using System;
using System.Runtime.InteropServices;

namespace KidFansChannel.GalCore
{
	public class GameInstall
	{
		public enum GameInstallScope
		{
			NotInstalled = 1,
			CurrentUser,
			AllUsers
		}

		[DllImport("GameuxInstallHelper.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		public static extern int GenerateGUID(ref Guid guidInstanceId);

		[DllImport("GameuxInstallHelper.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "AddToGameExplorerW", ExactSpelling = true, SetLastError = true)]
		public static extern long AddToGameExplorer(string gDFBinPath, string gameInstallPath, GameInstallScope installScope, ref Guid gameInstanceGUID);

		[DllImport("GameuxInstallHelper.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "CreateTaskW", ExactSpelling = true, SetLastError = true)]
		public static extern int CreateTask(GameInstallScope installScope, ref Guid gameInstanceGUID, bool supportTask, int taskID, string taskName, string launchPath, string commandLineArgs);

		[DllImport("GameuxInstallHelper.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "SetupRichSavedGamesW", ExactSpelling = true, SetLastError = true)]
		public static extern long SetupRichSavedGames(string savedGameExtension, string launchPath, string commandLineToLaunchSavedGame);

		[DllImport("GameuxInstallHelper.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "RetrieveGUIDForApplicationW", ExactSpelling = true, SetLastError = true)]
		public static extern long RetrieveGUIDForApplication(string gDFBinPath, ref Guid gameInstanceGUID);

		[DllImport("GameuxInstallHelper.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		public static extern long RemoveFromGameExplorer(ref Guid gameInstanceGUID);

		[DllImport("GameuxInstallHelper.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
		public static extern long RemoveTasks(ref Guid gameInstanceGUID);

		[DllImport("GameuxInstallHelper.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, EntryPoint = "RemoveRichSavedGamesW", ExactSpelling = true, SetLastError = true)]
		public static extern long RemoveRichSavedGames(string savedGameExtension);
	}
}
