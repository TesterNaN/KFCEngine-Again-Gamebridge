using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace KidFansChannel.InputCore
{
	public class InputCore : IDisposable
	{
		public enum Direction
		{
			None,
			Up,
			UpRight,
			Right,
			DownRight,
			Down,
			DownLeft,
			Left,
			UpLeft
		}

		private KeyBoardInput keyBoard;

		private GamePadInput gamePad;

		private Dictionary<int, KeyBoardInput.Key> keyBoardMap = new Dictionary<int, KeyBoardInput.Key>();

		private Dictionary<int, int> gamePadMap = new Dictionary<int, int>();

		private List<int> indexList = new List<int>();

		private bool[] keyPress;

		private bool[] keyDown;

		private bool[] directionPress = new bool[4];

		private bool[] directionDown = new bool[4];

		public InputCore()
		{
			keyBoard = new KeyBoardInput();
			gamePad = new GamePadInput();
		}

		public List<GamePadInput.GamePadInstance> GetGamePadList()
		{
			return gamePad.GetGamePadList();
		}

		public virtual void Initialize(GamePadInput.GamePadInstance gamePadInstance, Form form)
		{
			form.Show();
			keyBoard.Initialize(form);
			if (gamePadInstance != null)
			{
				gamePad.Initialize(gamePadInstance, form);
			}
		}

		~InputCore()
		{
			Dispose();
		}

		public void Dispose()
		{
			keyBoard.Dispose();
			gamePad.Dispose();
		}

		public void StartShock()
		{
			gamePad.StartShock();
		}

		public void StopShock()
		{
			gamePad.StopShock();
		}

		public void AddMap(int index, KeyBoardInput.Key key)
		{
			if (key == KeyBoardInput.Key.Up || key == KeyBoardInput.Key.Down || key == KeyBoardInput.Key.Left || key == KeyBoardInput.Key.Right)
			{
				throw new ArgumentOutOfRangeException();
			}
			keyBoardMap.Add(index, key);
			if (!indexList.Contains(index))
			{
				indexList.Add(index);
			}
		}

		public void AddMap(int index, int button)
		{
			if (button < 0 || button >= 12)
			{
				throw new ArgumentOutOfRangeException();
			}
			gamePadMap.Add(index, button);
			if (!indexList.Contains(index))
			{
				indexList.Add(index);
			}
		}

		public void FinishMap()
		{
			keyPress = new bool[indexList.Count];
			keyDown = new bool[indexList.Count];
		}

		public void Update()
		{
			keyBoard.Update();
			gamePad.Update();
			for (int i = 0; i < indexList.Count; i++)
			{
				bool pressed = false;
				if (keyBoardMap.ContainsKey(indexList[i]) && keyBoard.GetKeyState(keyBoardMap[indexList[i]]))
				{
					pressed = true;
				}
				if (gamePadMap.ContainsKey(indexList[i]) && gamePad.GetButtonState(gamePadMap[indexList[i]]))
				{
					pressed = true;
				}
				if (pressed != keyDown[i])
				{
					keyPress[i] = pressed;
					keyDown[i] = pressed;
				}
			}
			GetDirection();
		}

		public bool CheckPress(int index)
		{
			int indexInList = indexList.IndexOf(index);
			if (indexInList == -1)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (keyPress[indexInList])
			{
				keyPress[indexInList] = false;
				return true;
			}
			return false;
		}

		public bool CheckDown(int index)
		{
			int indexInList = indexList.IndexOf(index);
			if (indexInList == -1)
			{
				throw new ArgumentOutOfRangeException();
			}
			return keyDown[indexInList];
		}

		public Direction GetDirection()
		{
			Direction[,] directionTable = new Direction[3, 3]
			{
				{
					Direction.UpLeft,
					Direction.Left,
					Direction.DownLeft
				},
				{
					Direction.Up,
					Direction.None,
					Direction.Down
				},
				{
					Direction.UpRight,
					Direction.Right,
					Direction.DownRight
				}
			};
			int horizontal = 0;
			int vertical = 0;
			bool[] raw = new bool[4]
			{
				keyBoard.GetKeyState(KeyBoardInput.Key.Left) || gamePad.GetButtonState(12),
				keyBoard.GetKeyState(KeyBoardInput.Key.Right) || gamePad.GetButtonState(13),
				keyBoard.GetKeyState(KeyBoardInput.Key.Up) || gamePad.GetButtonState(14),
				keyBoard.GetKeyState(KeyBoardInput.Key.Down) || gamePad.GetButtonState(15)
			};
			bool[] filtered = (bool[])raw.Clone();
			if (filtered[0] == filtered[1])
			{
				filtered[0] = false;
				filtered[1] = false;
			}
			if (filtered[2] == filtered[3])
			{
				filtered[2] = false;
				filtered[3] = false;
			}
			for (int i = 0; i < 4; i++)
			{
				if (directionDown[i] != filtered[i])
				{
					directionPress[i] = filtered[i];
					directionDown[i] = filtered[i];
				}
			}
			if (raw[0] == raw[1])
			{
				horizontal = 0;
			}
			else if (raw[0])
			{
				horizontal = -1;
			}
			else if (raw[1])
			{
				horizontal = 1;
			}
			if (raw[2] == raw[3])
			{
				vertical = 0;
			}
			else if (raw[2])
			{
				vertical = -1;
			}
			else if (raw[3])
			{
				vertical = 1;
			}
			return directionTable[horizontal + 1, vertical + 1];
		}

		public bool CheckDirectionPress(int direction)
		{
			if (direction < 0 || direction >= 4)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (directionPress[direction])
			{
				directionPress[direction] = false;
				return true;
			}
			return false;
		}
	}
}
