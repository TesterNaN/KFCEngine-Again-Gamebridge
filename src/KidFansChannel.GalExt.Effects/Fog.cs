using System.IO;
using System.Xml;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Commands;
using KidFansChannel.GalCore.ExtBases;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.GalExt.Effects
{
	public abstract class Fog : Effect
	{
		private int _style;

		protected int Alpha;

		protected bool Direction;

		protected int TypeNum;

		protected Picture FogPic1;

		protected Picture FogPic2;

		protected float X;

		public int Style
		{
			get
			{
				return _style;
			}
			set
			{
				_style = value;
			}
		}

		public override bool DataReady
		{
			get
			{
				if (FogPic1.IsCreated)
				{
					return FogPic2.IsCreated;
				}
				return false;
			}
		}

		public override float Layer
		{
			get
			{
				return 14f;
			}
		}

		public Fog()
		{
			Single = false;
			Lasting = false;
			SupportAnimation = false;
			Name = "fog";
		}

		public override void ConvertXml(XmlNode node, NodeInfo info)
		{
			_style = ScriptHelper.GetArgumentInteger(node, info, "style");
			if (_style >= TypeNum)
			{
				throw new ScriptArgumentException(node, info, "style", "Style's number is not defined.");
			}
			InitStyle();
		}

		public override void FinalData()
		{
			if (FogPic1 != null)
			{
				FogPic1.Dispose();
				FogPic1 = null;
			}
			if (FogPic2 != null)
			{
				FogPic2.Dispose();
				FogPic2 = null;
			}
		}

		protected abstract void InitStyle();

		public override void InitCommand(bool animation)
		{
			X = 0f;
		}

		public override void FinalCommand(bool animation)
		{
			Finished = true;
		}

		public override void Update(double appTime, float elaspedTime)
		{
			X += elaspedTime * 30f;
			while (X >= 1600f)
			{
				X -= 1600f;
			}
			if (Direction)
			{
				if (X < 800f)
				{
					FogPic1.X = (int)X;
				}
				else
				{
					FogPic1.X = (int)X - 1600;
				}
				FogPic2.X = (int)X - 800;
			}
			else
			{
				FogPic1.X = 800 - (int)X;
				if (X < 800f)
				{
					FogPic2.X = -(int)X;
				}
				else
				{
					FogPic2.X = 1600 - (int)X;
				}
			}
		}

		public override void Save(BinaryWriter bw)
		{
			bw.Write(Name);
			bw.Write(_style);
		}

		public override void Load(BinaryReader br)
		{
			_style = br.ReadInt32();
			InitStyle();
		}

		public override void ConfirmUpdate()
		{
		}

		public override void Draw()
		{
			if (ScriptCommand.Options.Graphics != GameOptions.Level.Low)
			{
				ScriptCommand.Canvas.DrawPicture(FogPic1);
				ScriptCommand.Canvas.DrawPicture(FogPic2);
			}
		}
	}
}
