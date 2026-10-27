using System;
using System.IO;
using System.Xml;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Commands;
using KidFansChannel.GalCore.ExtBases;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.GalExt.Effects
{
	public class Film : Effect
	{
		protected enum FilmIntensity
		{
			Low,
			Middle,
			High
		}

		private FilmIntensity currentIntensity;

		public Picture PicFilm;

		protected float Time;

		private Random random = new Random((int)DateTime.Now.Ticks);

		public override bool DataReady
		{
			get
			{
				return PicFilm.IsCreated;
			}
		}

		public override float Layer
		{
			get
			{
				return 5f;
			}
		}

		public Film()
		{
			Single = false;
			Lasting = false;
			Name = "film";
			SupportAnimation = false;
		}

		public override void ConvertXml(XmlNode node, NodeInfo info)
		{
		}

		public override void InitData()
		{
			PicFilm = ScriptCommand.Data.LoadPicture("system", "film.png");
			currentIntensity = FilmIntensity.Low;
		}

		public override void FinalData()
		{
			if (PicFilm != null)
			{
				PicFilm.Dispose();
				PicFilm = null;
			}
		}

		public override void InitCommand(bool animation)
		{
		}

		public override void FinalCommand(bool animation)
		{
			Finished = true;
		}

		public override void Update(double appTime, float elaspedTime)
		{
			Time += elaspedTime;
			switch (currentIntensity)
			{
			case FilmIntensity.Low:
				PicFilm.Alpha = 0;
				if (Time >= 0.6f)
				{
					currentIntensity = FilmIntensity.Middle;
					Time = 0f;
				}
				break;
			case FilmIntensity.Middle:
				PicFilm.Alpha = 192;
				if (Time >= 0.6f)
				{
					if (random.NextDouble() < 0.5)
					{
						currentIntensity = FilmIntensity.High;
					}
					else
					{
						currentIntensity = FilmIntensity.Low;
					}
					Time = 0f;
				}
				break;
			case FilmIntensity.High:
				PicFilm.Alpha = 255;
				if (Time >= 0.8f)
				{
					currentIntensity = FilmIntensity.Middle;
					Time = 0f;
				}
				break;
			}
		}

		public override void Draw()
		{
			ScriptCommand.Canvas.DrawPicture(PicFilm);
		}

		public override void Load(BinaryReader br)
		{
		}

		public override void Save(BinaryWriter bw)
		{
			bw.Write(Name);
		}

		public override void ConfirmUpdate()
		{
		}
	}
}
