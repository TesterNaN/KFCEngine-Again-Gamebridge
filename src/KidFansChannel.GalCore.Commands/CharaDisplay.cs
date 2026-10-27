using System;
using System.Drawing;
using System.IO;
using System.Xml;
using KidFansChannel.GalCore.ExtBases;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.GalCore.Commands
{
	public class CharaDisplay : ScriptCommand, IInstanceDataUser, IDrawable, IUpdatable, ISaveable
	{
		public enum CharaTime
		{
			Undefined = -1,
			Day,
			Evening,
			Night
		}

		protected enum CharaState
		{
			Showing,
			Shown,
			Hiding,
			Hiden,
			Changing,
			Transforming
		}

		private string _charaStr;

		private Picture _chara;

		private Picture _charaBase;

		private SubPicture[] _charaParts = new SubPicture[4];

		private Picture[] _charaMouth;

		private Point _mouthLocation;

		private float _center;

		private int _id;

		private int? _x;

		private int? _y;

		private float? _rotate;

		private float? _zoom;

		private CharaTime _time;

		private int? _alpha;

		private bool _animation;

		private int _index;

		private MixPicture mixChara;

		private Picture oldChara;

		private Picture oldCharaBase;

		private Picture[] oldCharaMouth;

		private Point oldMouthLocation;

		private int oldId;

		private TransformStyle<CharaDisplay> _transformStyle;

		private bool _shocking;

		public static int MouthNum = 3;

		public static int StepX = 40;

		public static int StepY = 30;

		public static float CharaSpeed = 0.5f;

		protected CharaState State;

		protected bool IsChange;

		private CharaDisplay charaDisplay;

		private bool charaCreated;

		protected float ChangeBaseAlpha;

		private int currentMouth;

		private bool mouthOpening = true;

		private float mouthTime;

		private short[] voiceVol;

		private bool voicing;

		private float voiceTime;

		private bool loading;

		private bool saveLasting;

		private string saveCharaStr;

		private int saveX;

		private int saveY;

		private float saveRotate;

		private float saveZoom;

		private int saveTime;

		private int saveAlpha;

		private int saveIndex;

		private string saveTransfromStyle;

		private bool saveShocking;

		protected string CharaStr
		{
			get
			{
				return _charaStr;
			}
			set
			{
				_charaStr = value;
			}
		}

		public Picture Chara
		{
			get
			{
				return _chara;
			}
			protected set
			{
				_chara = value;
			}
		}

		public Picture CharaBase
		{
			get
			{
				return _charaBase;
			}
			protected set
			{
				_charaBase = value;
			}
		}

		public Picture[] CharaMouth
		{
			get
			{
				return _charaMouth;
			}
			protected set
			{
				_charaMouth = value;
			}
		}

		public Point MouthLocation
		{
			get
			{
				return _mouthLocation;
			}
			protected set
			{
				_mouthLocation = value;
			}
		}

		public float Center
		{
			get
			{
				return _center;
			}
			protected set
			{
				_center = value;
			}
		}

		public int Id
		{
			get
			{
				return _id;
			}
			protected set
			{
				_id = value;
			}
		}

		public int? X
		{
			get
			{
				return _x;
			}
			set
			{
				_x = value;
			}
		}

		public int? Y
		{
			get
			{
				return _y;
			}
			set
			{
				_y = value;
			}
		}

		public float? Rotate
		{
			get
			{
				return _rotate;
			}
			protected set
			{
				_rotate = value;
			}
		}

		public float? Zoom
		{
			get
			{
				return _zoom;
			}
			protected set
			{
				_zoom = value;
			}
		}

		public CharaTime Time
		{
			get
			{
				return _time;
			}
			protected set
			{
				_time = value;
			}
		}

		public int? Alpha
		{
			get
			{
				return _alpha;
			}
			protected set
			{
				_alpha = value;
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

		public int Index
		{
			get
			{
				return _index;
			}
			protected set
			{
				_index = value;
			}
		}

		public TransformStyle<CharaDisplay> TransformStyle
		{
			get
			{
				return _transformStyle;
			}
		}

		protected bool Shocking
		{
			get
			{
				return _shocking;
			}
		}

		public bool DataReady
		{
			get
			{
				if (MouthNum == 0)
				{
					if (!_chara.IsCreated)
					{
						return false;
					}
				}
				else
				{
					if (!_charaBase.IsCreated)
					{
						return false;
					}
					for (int i = 0; i < MouthNum; i++)
					{
						if (!_charaMouth[i].IsCreated)
						{
							return false;
						}
					}
				}
				if (!charaCreated)
				{
					if (MouthNum == 0)
					{
						_chara.Scale = new SizeF((float)BgDisplay.ScreenSize.Width / (float)_chara.Width, (float)BgDisplay.ScreenSize.Height / (float)_chara.Height);
					}
					else
					{
						_chara.Size = _charaBase.Size;
						_chara.Scale = new SizeF((float)BgDisplay.ScreenSize.Width / (float)_chara.Width, (float)BgDisplay.ScreenSize.Height / (float)_chara.Height);
					}
					charaCreated = true;
				}
				return true;
			}
		}

		public int LayerMajor
		{
			get
			{
				if (_index < 0)
				{
					return 1;
				}
				return 8;
			}
		}

		public float LayerMinor
		{
			get
			{
				return (float)_index - (IsChange ? 0.5f : 0f);
			}
		}

		public CharaDisplay()
		{
			Single = true;
			Lasting = true;
			Command = "charadisplay";
			DataName = "chara";
		}

		public override void ConvertXml(XmlNode node, NodeInfo info)
		{
			base.ConvertXml(node, info);
			_charaStr = ScriptHelper.GetArgumentString(node, info, "file");
			try
			{
				_x = (int)(ScriptHelper.GetArgumentFloat(node, info, "x") * (float)StepX + (float)(BgDisplay.ScreenSize.Width / 2));
			}
			catch (ScriptArgumentException)
			{
				_x = null;
			}
			try
			{
				_y = (int)(ScriptHelper.GetArgumentFloat(node, info, "y") * (float)StepY + (float)BgDisplay.ScreenSize.Height);
			}
			catch (ScriptArgumentException)
			{
				_y = null;
			}
			try
			{
				_rotate = (float)((double)ScriptHelper.GetArgumentFloat(node, info, "rotate") * Math.PI / 2.0);
			}
			catch (ScriptArgumentException)
			{
				_rotate = null;
			}
			try
			{
				_zoom = ScriptHelper.GetArgumentFloat(node, info, "zoom");
			}
			catch (ScriptArgumentException)
			{
				_zoom = null;
			}
			try
			{
				_alpha = ScriptHelper.GetArgumentInteger(node, info, "alpha");
			}
			catch (ScriptArgumentException)
			{
				_alpha = null;
			}
			string timeText = null;
			try
			{
				timeText = ScriptHelper.GetArgumentString(node, info, "time");
			}
			catch (ScriptArgumentException)
			{
				_time = CharaTime.Undefined;
			}
			switch (timeText)
			{
			case "day":
				_time = CharaTime.Day;
				break;
			case "evening":
				_time = CharaTime.Evening;
				break;
			case "night":
				_time = CharaTime.Night;
				break;
			default:
				throw new ScriptArgumentException(node, info, "time", "Time should be day, evening or night.");
			case null:
				break;
			}
			_animation = ScriptHelper.GetArgumentBool(node, info, "animation", true);
			_index = ScriptHelper.GetArgumentInteger(node, info, "index");
			if (_index < -1)
			{
				throw new ScriptArgumentException(node, info, "index", ArgumentType.Integer, "Index should not less than -1.");
			}
			LoadDataInfo();
			Single = _animation;
			Lasting = _animation;
		}

		protected void LoadDataInfo()
		{
			XmlDocument xmlDocument = new XmlDocument();
			_id = 0;
			_mouthLocation = Point.Empty;
			_center = 0.5f;
			if (!ScriptCommand.Data.LoadXmlDocument(DataName, _charaStr + ".xml", xmlDocument))
			{
				return;
			}
			XmlNode documentElement = xmlDocument.DocumentElement;
			if (documentElement.Name != "chara")
			{
				throw new ScriptStructureException(documentElement, new NodeInfo(), "Chara's document element should be chara.");
			}
			XmlNode node;
			if ((node = documentElement.SelectSingleNode("id")) != null)
			{
				_id = ScriptHelper.GetArgumentInteger(node, new NodeInfo(), "value", 0);
				if (_id < 0)
				{
					throw new ScriptArgumentException(node, new NodeInfo(), "value", ArgumentType.Integer, "Id should not less than 0.");
				}
			}
			if (MouthNum > 0 && (node = documentElement.SelectSingleNode("mouth")) != null)
			{
				_mouthLocation.X = ScriptHelper.GetArgumentInteger(node, new NodeInfo(), "x");
				_mouthLocation.Y = ScriptHelper.GetArgumentInteger(node, new NodeInfo(), "y");
			}
			if ((node = documentElement.SelectSingleNode("center")) != null)
			{
				_center = ScriptHelper.GetArgumentFloat(node, new NodeInfo(), "value", 0.5f);
			}
		}

		public override void InitCommand()
		{
			base.InitCommand();
			if (MouthNum > 0)
			{
				_charaParts[0] = ScriptCommand.Graphics.CreateSubPicture(_charaBase, new Rectangle(0, 0, _mouthLocation.X - _charaMouth[0].Width / 2, _charaBase.Height));
				_charaParts[1] = ScriptCommand.Graphics.CreateSubPicture(_charaBase, new Rectangle(_mouthLocation.X + _charaMouth[0].Width / 2, 0, _charaBase.Width - _mouthLocation.X - _charaMouth[0].Width / 2, _charaBase.Height));
				_charaParts[2] = ScriptCommand.Graphics.CreateSubPicture(_charaBase, new Rectangle(_mouthLocation.X - _charaMouth[0].Width / 2, 0, _charaMouth[0].Width, _mouthLocation.Y - _charaMouth[0].Height / 2));
				_charaParts[3] = ScriptCommand.Graphics.CreateSubPicture(_charaBase, new Rectangle(_mouthLocation.X - _charaMouth[0].Width / 2, _mouthLocation.Y + _charaMouth[0].Height / 2, _charaMouth[0].Width, _charaBase.Height - _mouthLocation.Y - _charaMouth[0].Height / 2));
				((BindingPicture)_chara).AddPicture(_charaParts[0], Point.Empty);
				((BindingPicture)_chara).AddPicture(_charaParts[1], new Point(_mouthLocation.X + _charaMouth[0].Width / 2, 0));
				((BindingPicture)_chara).AddPicture(_charaParts[2], new Point(_mouthLocation.X - _charaMouth[0].Width / 2, 0));
				((BindingPicture)_chara).AddPicture(_charaParts[3], new Point(_mouthLocation.X - _charaMouth[0].Width / 2, _mouthLocation.Y + _charaMouth[0].Height / 2));
				for (int i = 0; i < MouthNum; i++)
				{
					((BindingPicture)_chara).AddPicture(_charaMouth[i], new Point(_mouthLocation.X - _charaMouth[0].Width / 2, _mouthLocation.Y - _charaMouth[0].Height / 2));
				}
				for (int j = 5; j < MouthNum + 4; j++)
				{
					((BindingPicture)_chara).SetVisible(j, false);
				}
			}
			_chara.Center = new Point((int)(_center * (float)_chara.Width), _chara.Height);
			bool found = false;
			for (int i = ScriptCommand.CommandList.Count - 1; i >= 0; i--)
			{
				ScriptCommand scriptCommand = ScriptCommand.CommandList[i];
				if (scriptCommand is CharaDisplay && scriptCommand != this && ((CharaDisplay)scriptCommand)._index == _index)
				{
					charaDisplay = (CharaDisplay)scriptCommand;
					found = true;
					break;
				}
				if (scriptCommand is Scene)
				{
					break;
				}
			}
			if (!found)
			{
				if (!_x.HasValue)
				{
					_x = BgDisplay.ScreenSize.Width / 2;
				}
				if (!_y.HasValue)
				{
					_y = BgDisplay.ScreenSize.Height;
				}
				if (!_rotate.HasValue)
				{
					_rotate = 0f;
				}
				if (!_zoom.HasValue)
				{
					_zoom = 1f;
				}
				if (!_alpha.HasValue)
				{
					_alpha = 255;
				}
				if (_time == CharaTime.Undefined)
				{
					_time = CharaTime.Day;
				}
			}
			else
			{
				_time = charaDisplay._time;
			}
			if (_x.HasValue && _y.HasValue)
			{
				_chara.X = _x.Value;
				_chara.Y = _y.Value;
			}
			if (_rotate.HasValue)
			{
				_chara.Rotate = _rotate.Value;
			}
			if (_zoom.HasValue)
			{
				_chara.Scale = new SizeF(_zoom.Value * (float)BgDisplay.ScreenSize.Width / (float)_chara.Width, _zoom.Value * (float)BgDisplay.ScreenSize.Height / (float)_chara.Height);
			}
			switch (_time)
			{
			case CharaTime.Evening:
				_chara.ArgumentColor = Color.FromArgb(32, 143, 95, 40);
				break;
			case CharaTime.Night:
				_chara.ArgumentColor = Color.FromArgb(64, 51, 47, 63);
				break;
			}
			if (!found)
			{
				if (_animation)
				{
					_chara.Alpha = 0;
					ChangeBaseAlpha = 0f;
					_chara.BeginTransform(CharaSpeed);
					_chara.AlphaTo(_alpha.Value);
					_chara.EndTransform();
					State = CharaState.Showing;
				}
				else
				{
					_chara.Alpha = _alpha.Value;
					State = CharaState.Shown;
				}
			}
			else
			{
				oldChara = charaDisplay.Chara;
				if (MouthNum > 0)
				{
					oldCharaBase = charaDisplay.CharaBase;
					oldCharaMouth = charaDisplay.CharaMouth;
					oldMouthLocation = charaDisplay.MouthLocation;
				}
				oldId = charaDisplay.Id;
				ChangeBaseAlpha = 0f;
				bool highQuality = ScriptCommand.Options.Graphics == GameOptions.Level.High && !_x.HasValue && !_y.HasValue && !_rotate.HasValue && !_zoom.HasValue && !_alpha.HasValue && (MouthNum == 0 || _mouthLocation == oldMouthLocation) && _id == oldId;
				charaDisplay.Change(Lasting);
				if (highQuality)
				{
					if (MouthNum == 0)
					{
						mixChara = ScriptCommand.Graphics.CreateMixPicture(oldChara, _chara);
					}
					else
					{
						mixChara = ScriptCommand.Graphics.CreateMixPicture(oldCharaBase, _charaBase);
						mixChara.Center = new Point((int)(_center * (float)_chara.Width), _chara.Height);
						mixChara.X = charaDisplay.Chara.X;
						mixChara.Y = charaDisplay.Chara.Y;
						mixChara.Rotate = charaDisplay.Chara.Rotate;
						mixChara.Scale = charaDisplay.Chara.Scale;
						mixChara.Alpha = charaDisplay.Chara.Alpha;
					}
					mixChara.ArgumentColor = charaDisplay.Chara.ArgumentColor;
				}
				else
				{
					mixChara = null;
				}
				State = CharaState.Changing;
			}
			if (!loading)
			{
				return;
			}
			if (_transformStyle != null)
			{
				if (_shocking)
				{
					ScriptCommand.GalCore.StartShock();
				}
				_transformStyle.InitStyle();
				State = CharaState.Transforming;
			}
			loading = false;
		}

		public override void FinalCommand()
		{
			base.FinalCommand();
			if (_shocking)
			{
				ScriptCommand.GalCore.StopShock();
				_shocking = false;
			}
			FinalInstanceData();
		}

		public void Change(bool lasting)
		{
			Lasting = lasting;
			IsChange = true;
		}

		public void Hide(bool lasting, bool animation)
		{
			if (_shocking)
			{
				ScriptCommand.GalCore.StopShock();
				_shocking = false;
			}
			Lasting = lasting;
			ChangeBaseAlpha = 1f;
			if (animation)
			{
				_chara.BeginTransform(0.5f);
				_chara.AlphaTo(0);
				_chara.EndTransform();
				State = CharaState.Hiding;
			}
			else
			{
				_chara.Alpha = 0;
				State = CharaState.Hiden;
			}
		}

		public void Transform(int x, int y, float rotate, float zoom, int alpha, float speed, bool lasting)
		{
			if (_shocking)
			{
				ScriptCommand.GalCore.StopShock();
				_shocking = false;
			}
			_transformStyle = null;
			_x = x;
			_y = y;
			_rotate = rotate;
			_zoom = zoom;
			_alpha = alpha;
			if (speed == 0f)
			{
				_chara.X = x;
				_chara.Y = y;
				_chara.Rotate = rotate;
				_chara.Scale = new SizeF(zoom * (float)BgDisplay.ScreenSize.Width / (float)_chara.Width, zoom * (float)BgDisplay.ScreenSize.Height / (float)_chara.Height);
				_chara.Alpha = alpha;
				State = CharaState.Shown;
			}
			else
			{
				_chara.BeginTransform(speed);
				_chara.PositionTo(new Point(x, y));
				_chara.ScaleTo(new SizeF(zoom * (float)BgDisplay.ScreenSize.Width / (float)_chara.Width, zoom * (float)BgDisplay.ScreenSize.Height / (float)_chara.Height));
				_chara.RotateTo(rotate);
				_chara.AlphaTo(alpha);
				_chara.EndTransform();
				Lasting = lasting;
				State = CharaState.Transforming;
			}
		}

		public virtual void Transform(TransformStyle<CharaDisplay> style, bool shock)
		{
			if (shock)
			{
				_shocking = true;
				ScriptCommand.GalCore.StartShock();
			}
			else if (_shocking)
			{
				ScriptCommand.GalCore.StopShock();
				_shocking = false;
			}
			_transformStyle = style;
			_transformStyle.InitStyle();
			Lasting = _transformStyle.Lasting;
			State = CharaState.Transforming;
		}

		public void InitInstanceData()
		{
			if (MouthNum == 0)
			{
				_chara = ScriptCommand.Data.LoadPicture(DataName, _charaStr + ".png");
				return;
			}
			_charaMouth = new Picture[MouthNum];
			_chara = ScriptCommand.Graphics.CreateBindingPicture();
			_charaBase = ScriptCommand.Data.LoadPicture(DataName, _charaStr + ".png");
			for (int i = 0; i < MouthNum; i++)
			{
				_charaMouth[i] = ScriptCommand.Data.LoadPicture(DataName, _charaStr + "_" + i + ".png");
			}
		}

		public void FinalInstanceData()
		{
			if (_shocking)
			{
				ScriptCommand.GalCore.StopShock();
				_shocking = false;
			}
			if (_chara != null)
			{
				_chara.Dispose();
				_chara = null;
			}
			if (MouthNum <= 0)
			{
				return;
			}
			if (_charaBase != null)
			{
				_charaBase.Dispose();
				_charaBase = null;
			}
			for (int i = 0; i < 4; i++)
			{
				if (_charaParts[i] != null)
				{
					_charaParts[i].Dispose();
					_charaParts[i] = null;
				}
			}
			for (int j = 0; j < MouthNum; j++)
			{
				_charaMouth[j].Dispose();
				_charaMouth[j] = null;
			}
			_charaMouth = null;
		}

		public void Draw()
		{
			if (IsChange)
			{
				return;
			}
			if (State == CharaState.Changing)
			{
				if (mixChara != null)
				{
					ScriptCommand.Canvas.DrawPicture(mixChara);
					return;
				}
				ScriptCommand.Canvas.DrawPicture(oldChara);
				ScriptCommand.Canvas.DrawPicture(_chara);
			}
			else
			{
				ScriptCommand.Canvas.DrawPicture(_chara);
			}
		}

		public void Update(double appTime, float elaspedTime)
		{
			switch (State)
			{
			case CharaState.Showing:
				if (ScriptCommand.PlayModeQuicking)
				{
					_chara.Update(elaspedTime * (float)ScriptCommand.QuickFactor);
				}
				else
				{
					_chara.Update(elaspedTime);
				}
				if (_chara.Alpha == _alpha)
				{
					State = CharaState.Shown;
				}
				break;
			case CharaState.Shown:
				Lasting = false;
				if (voicing)
				{
					if (!ScriptCommand.Sound.IsPlayingVoice)
					{
						MouthStop();
					}
					else
					{
						UpdateMouth(elaspedTime);
					}
				}
				break;
			case CharaState.Hiding:
				if (ScriptCommand.PlayModeQuicking)
				{
					_chara.Update(elaspedTime * (float)ScriptCommand.QuickFactor);
				}
				else
				{
					_chara.Update(elaspedTime);
				}
				if (_chara.Alpha == 0)
				{
					State = CharaState.Hiden;
				}
				break;
			case CharaState.Hiden:
				Lasting = false;
				FinishCommand();
				break;
			case CharaState.Changing:
				if (ScriptCommand.PlayModeQuicking)
				{
					ChangeBaseAlpha += elaspedTime / CharaSpeed * (float)ScriptCommand.QuickFactor;
				}
				else
				{
					ChangeBaseAlpha += elaspedTime / CharaSpeed;
				}
				if (ChangeBaseAlpha > 1f)
				{
					ChangeBaseAlpha = 1f;
				}
				if (mixChara != null)
				{
					mixChara.X = charaDisplay.Chara.X;
					mixChara.Y = charaDisplay.Chara.Y;
					mixChara.Rotate = charaDisplay.Chara.Rotate;
					mixChara.Scale = charaDisplay.Chara.Scale;
					mixChara.Alpha = charaDisplay.Chara.Alpha;
					mixChara.MixAlpha = ChangeBaseAlpha;
				}
				else
				{
					oldChara.X = charaDisplay.Chara.X;
					oldChara.Y = charaDisplay.Chara.Y;
					oldChara.Rotate = charaDisplay.Chara.Rotate;
					oldChara.Scale = charaDisplay.Chara.Scale;
					int value = charaDisplay.Alpha.Value;
					if (!_x.HasValue || !_y.HasValue)
					{
						if (!_x.HasValue)
						{
							_chara.X = charaDisplay.Chara.X;
						}
						else
						{
							_chara.X = _x.Value;
						}
						if (!_y.HasValue)
						{
							_chara.Y = charaDisplay.Chara.Y;
						}
						else
						{
							_chara.Y = _y.Value;
						}
					}
					if (!_rotate.HasValue)
					{
						_chara.Rotate = charaDisplay.Chara.Rotate;
					}
					if (!_zoom.HasValue)
					{
						_chara.Scale = charaDisplay.Chara.Scale;
					}
					int baseAlpha = (_alpha.HasValue ? _alpha.Value : charaDisplay.Alpha.Value);
					if (ChangeBaseAlpha > 0.25f)
					{
						oldChara.Alpha = (int)(Math.Pow((double)(1f - ChangeBaseAlpha) / 0.75, 0.5) * (double)value + 0.5);
					}
					else
					{
						oldChara.Alpha = value;
					}
					if (ChangeBaseAlpha < 0.75f)
					{
						_chara.Alpha = (int)(Math.Pow((double)ChangeBaseAlpha / 0.75, 0.5) * (double)baseAlpha + 0.5);
					}
					else
					{
						_chara.Alpha = baseAlpha;
					}
				}
				if (ChangeBaseAlpha == 1f)
				{
					oldChara = null;
					oldCharaBase = null;
					oldCharaMouth = null;
					if (mixChara != null)
					{
						mixChara.Dispose();
						mixChara = null;
					}
					if (!_x.HasValue)
					{
						_x = charaDisplay.X;
					}
					if (!_y.HasValue)
					{
						_y = charaDisplay.Y;
					}
					if (!_rotate.HasValue)
					{
						_rotate = charaDisplay.Rotate;
					}
					if (!_zoom.HasValue)
					{
						_zoom = charaDisplay.Zoom;
					}
					if (!_alpha.HasValue)
					{
						_alpha = charaDisplay.Alpha;
					}
					_chara.X = _x.Value;
					_chara.Y = _y.Value;
					_chara.Rotate = _rotate.Value;
					_chara.Scale = new SizeF(_zoom.Value * (float)BgDisplay.ScreenSize.Width / (float)_chara.Width, _zoom.Value * (float)BgDisplay.ScreenSize.Height / (float)_chara.Height);
					_chara.Alpha = _alpha.Value;
					charaDisplay.FinishCommand();
					State = CharaState.Shown;
				}
				break;
			case CharaState.Transforming:
				if (_transformStyle == null)
				{
					if (ScriptCommand.PlayModeQuicking)
					{
						_chara.Update(elaspedTime * (float)ScriptCommand.QuickFactor);
					}
					else
					{
						_chara.Update(elaspedTime);
					}
					if (_chara.Time == 0f)
					{
						State = CharaState.Shown;
					}
				}
				else
				{
					_transformStyle.UpdateStyle(elaspedTime);
					if (_transformStyle.Finished)
					{
						if (_shocking)
						{
							ScriptCommand.GalCore.StopShock();
							_shocking = false;
						}
						State = CharaState.Shown;
					}
				}
				if (voicing)
				{
					UpdateMouth(elaspedTime);
				}
				break;
			}
		}

		private void UpdateMouth(float elaspedTime)
		{
			mouthTime += elaspedTime;
			voiceTime += elaspedTime;
			if (!(mouthTime > 0.125f))
			{
				return;
			}
			while (mouthTime >= 0.125f)
			{
				mouthTime -= 0.125f;
			}
			double baseAlpha = voiceTime;
			if (baseAlpha == 0.0)
			{
				ChangeMouth(0);
				return;
			}
			int volumeIndex = (int)(baseAlpha * 60.0);
			if (volumeIndex >= voiceVol.Length)
			{
				MouthStop();
			}
			else if (voiceVol[volumeIndex] > 125)
			{
				if (mouthOpening)
				{
					if (currentMouth < 2)
					{
						ChangeMouth(currentMouth + 1);
						return;
					}
					ChangeMouth(1);
					mouthOpening = false;
				}
				else if (currentMouth > 0)
				{
					ChangeMouth(currentMouth - 1);
				}
				else
				{
					ChangeMouth(1);
					mouthOpening = true;
				}
			}
			else
			{
				ChangeMouth(0);
				mouthOpening = true;
			}
		}

		private void ChangeMouth(int value)
		{
			if (currentMouth != value)
			{
				((BindingPicture)_chara).SetVisible(currentMouth + 4, false);
				currentMouth = value;
				((BindingPicture)_chara).SetVisible(currentMouth + 4, true);
			}
		}

		public void MouthPrepare(short[] voice)
		{
			ChangeMouth(0);
			mouthOpening = true;
			voiceVol = voice;
		}

		public void MouthPlay()
		{
			voicing = true;
			mouthTime = 0f;
			voiceTime = 0f;
		}

		public void MouthStop()
		{
			ChangeMouth(0);
			voiceVol = null;
			voicing = false;
		}

		public void Save(BinaryWriter bw)
		{
			bw.Write(Command);
			bw.Write(saveLasting);
			bw.Write(saveCharaStr);
			bw.Write(saveX);
			bw.Write(saveY);
			bw.Write(saveRotate);
			bw.Write(saveZoom);
			bw.Write(saveTime);
			bw.Write(saveAlpha);
			bw.Write(saveIndex);
			bw.Write(saveTransfromStyle);
			bw.Write(saveShocking);
		}

		public void Load(BinaryReader br)
		{
			Lasting = br.ReadBoolean();
			_charaStr = br.ReadString();
			_x = br.ReadInt32();
			_y = br.ReadInt32();
			_rotate = br.ReadSingle();
			_zoom = br.ReadSingle();
			_time = (CharaTime)br.ReadInt32();
			_alpha = br.ReadInt32();
			_index = br.ReadInt32();
			string text = br.ReadString();
			if (text != string.Empty)
			{
				Type type = CharaTransform.Types[text];
				_transformStyle = (TransformStyle<CharaDisplay>)type.Assembly.CreateInstance(type.FullName);
				_transformStyle.Init(this);
				loading = true;
			}
			_shocking = br.ReadBoolean();
			LoadDataInfo();
			_animation = false;
		}

		public void ConfirmUpdate()
		{
			saveLasting = Lasting;
			saveCharaStr = _charaStr;
			saveX = (_x.HasValue ? _x.Value : charaDisplay.X.Value);
			saveY = (_y.HasValue ? _y.Value : charaDisplay.Y.Value);
			saveRotate = (_rotate.HasValue ? _rotate.Value : charaDisplay.Rotate.Value);
			saveZoom = (_zoom.HasValue ? _zoom.Value : charaDisplay.Zoom.Value);
			saveTime = (int)((_time != CharaTime.Undefined) ? _time : charaDisplay.Time);
			saveAlpha = (_alpha.HasValue ? _alpha.Value : charaDisplay.Alpha.Value);
			saveIndex = _index;
			saveTransfromStyle = ((_transformStyle == null) ? string.Empty : _transformStyle.Name);
			saveShocking = _shocking;
		}
	}
}
