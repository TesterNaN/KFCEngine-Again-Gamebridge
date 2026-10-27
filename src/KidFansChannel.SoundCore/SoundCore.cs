using System;
using System.Collections.Generic;
using System.Windows.Forms;
using KidFansChannel.SoundCore.Ogg;

namespace KidFansChannel.SoundCore
{
	public class SoundCore : IDisposable
	{
		private List<SoundPlayer> _playerList = new List<SoundPlayer>();

		public List<SoundPlayer> PlayerList
		{
			get
			{
				return _playerList;
			}
		}

		public SoundCore(Form owner, int deviceIndex)
		{
			OggPlayer.Initial(owner, false, deviceIndex);
		}

		public SoundPlayer CreatePlayer(int volumeLayers)
		{
			SoundPlayer soundPlayer = new SoundPlayer(this, volumeLayers);
			_playerList.Add(soundPlayer);
			return soundPlayer;
		}

		public void Dispose()
		{
			for (int i = _playerList.Count - 1; i >= 0; i--)
			{
				_playerList[i].Dispose();
			}
			_playerList.Clear();
			OggPlayer.Final();
		}
	}
}
