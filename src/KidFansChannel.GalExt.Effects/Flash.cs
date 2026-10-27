using System.Drawing;
using System.IO;
using System.Xml;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Commands;
using KidFansChannel.GalCore.ExtBases;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.GalExt.Effects
{
	public class Flash : Effect
	{
		protected Picture PicFlash;

		protected static float Time = 0.5f;

		public override bool DataReady
		{
			get
			{
				return true;
			}
		}

		public override float Layer
		{
			get
			{
				return 18f;
			}
		}

		public Flash()
		{
			Single = true;
			Lasting = true;
			Name = "flash";
		}

		public override void ConvertXml(XmlNode node, NodeInfo info)
		{
		}

		public override void InitData()
		{
		}

		public override void FinalData()
		{
		}

		public override void InitCommand(bool animation)
		{
			PicFlash = ScriptCommand.Graphics.CreatePicture(Color.White);
			PicFlash.Size = BgDisplay.ScreenSize;
			PicFlash.Alpha = 255;
			PicFlash.BeginTransform(Time);
			PicFlash.AlphaTo(0);
			PicFlash.EndTransform();
		}

		public override void FinalCommand(bool animation)
		{
		}

		public override void Update(double appTime, float elaspedTime)
		{
			if (ScriptCommand.PlayModeQuicking)
			{
				PicFlash.Update(elaspedTime * (float)ScriptCommand.QuickFactor);
			}
			else
			{
				PicFlash.Update(elaspedTime);
			}
			if (PicFlash.Time == 0f)
			{
				Lasting = false;
				PicFlash.Dispose();
				Finished = true;
			}
		}

		public override void Draw()
		{
			ScriptCommand.Graphics.DrawPicture(PicFlash);
		}

		public override void Save(BinaryWriter bw)
		{
			bw.Write(Name);
		}

		public override void Load(BinaryReader br)
		{
		}

		public override void ConfirmUpdate()
		{
		}
	}
}
