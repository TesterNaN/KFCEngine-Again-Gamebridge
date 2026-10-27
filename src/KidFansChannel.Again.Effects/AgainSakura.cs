using System.Drawing;
using KidFansChannel.GalCore.Commands;
using KidFansChannel.GalExt.Effects;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Effects
{
	internal class AgainSakura : Sakura
	{
		private Picture PicSakura;

		private bool dataReady;

		public override bool DataReady
		{
			get
			{
				if (dataReady)
				{
					return true;
				}
				if (PicSakura.IsCreated)
				{
					for (int i = 0; i < TypeNum; i++)
					{
						for (int j = 0; j < AnimaSteps; j++)
						{
							PicSakuras[i, j] = ScriptCommand.Graphics.CreateSubPicture(PicSakura, new Rectangle(HalfSize * 2 * j, HalfSize * 2 * i, HalfSize * 2, HalfSize * 2));
							PicSakuras[i, j].Center = new Point(HalfSize, HalfSize);
						}
					}
					dataReady = true;
					return true;
				}
				return false;
			}
		}

		public AgainSakura()
		{
			TypeNum = 4;
			AnimaSteps = 4;
			HalfSize = 12;
			NumMax = 15;
			NumMin = 10;
			SpeedMax = new PointF(160f, 90f);
			ChangeMin = 1f;
			ChangeMax = 3f;
			Acc = new PointF(160f, 90f);
			SideRatio = 1.25f;
			AnimiSpeed = 0.25f;
			TypeNum = 4;
			NextMax = 0.6f;
			NextMin = 0.2f;
		}

		protected override void InitStyle()
		{
		}

		public override void InitData()
		{
			PicSakura = ScriptCommand.Data.LoadPicture("system", "sakura.png");
			PicSakuras = new Picture[TypeNum, AnimaSteps];
		}

		public override void FinalData()
		{
			Picture[,] picSakuras = PicSakuras;
			foreach (Picture picture in picSakuras)
			{
				picture.Dispose();
			}
			PicSakura.Dispose();
		}
	}
}
