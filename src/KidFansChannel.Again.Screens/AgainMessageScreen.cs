using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using KidFansChannel.GalCore;
using KidFansChannel.GalCore.Screens;
using KidFansChannel.Game2DCore;

namespace KidFansChannel.Again.Screens
{
	public class AgainMessageScreen : MessageScreen
	{
		private Picture picVoiceBase;

		private Picture[] picVoices = new Picture[4];

		private Picture picVoiceAnime;

		private Picture picMain;

		private Picture picBack;

		private String2D[] strTextWithouts = new String2D[MessageScreen.PageLineNum];

		private String2D[] strTextWiths = new String2D[MessageScreen.PageLineNum];

		private String2D[] strSpeakers = new String2D[MessageScreen.PageLineNum];

		private Element eleScreen;

		private AgainScrollBarElement scrollBar = new AgainScrollBarElement();

		private int lastVoiceIndex = -1;

		public override bool DataReady
		{
			get
			{
				if (base.DataReady)
				{
					return true;
				}
				if (picVoiceBase != null && picVoiceBase.IsCreated && scrollBar.DataReady && picMain != null && picMain.IsCreated && picBack != null && picBack.IsCreated)
				{
					for (int i = 0; i < 4; i++)
					{
						picVoices[i] = GameManager.Graphics.CreateSubPicture(picVoiceBase, new Rectangle((i & 1) * 40, (i >> 1) * 40, 40, 40));
					}
					picVoiceAnime = GameManager.Graphics.CreateAnimePicture(new Size(40, 40));
					for (int j = 1; j < 4; j++)
					{
						((AnimePicture)picVoiceAnime).AddFrame(picVoices[j], 0.125f);
					}
					DataReady = true;
					return true;
				}
				return false;
			}
		}

		static AgainMessageScreen()
		{
			MessageDisplay.SpeakerColor.Add(Color.White);
			MessageDisplay.SpeakerColor.Add(Color.Yellow);
			MessageDisplay.SpeakerColor.Add(Color.Wheat);
			MessageDisplay.SpeakerColor.Add(Color.LightCoral);
			MessageDisplay.SpeakerColor.Add(Color.DarkOrange);
			MessageDisplay.SpeakerColor.Add(Color.SkyBlue);
			MessageDisplay.SpeakerColor.Add(Color.ForestGreen);
			MessageDisplay.SpeakerColor.Add(Color.DarkCyan);
			MessageDisplay.SpeakerColor.Add(Color.RoyalBlue);
			MessageDisplay.SpeakerColor.Add(Color.PaleGreen);
			MessageDisplay.SpeakerColor.Add(Color.PaleGoldenrod);
			MessageDisplay.SpeakerColor.Add(Color.IndianRed);
			MessageDisplay.SpeakerColor.Add(Color.LightGray);
			MessageDisplay.SpeakerColor.Add(Color.LightSalmon);
			MessageDisplay.SpeakerColor.Add(Color.DarkGoldenrod);
			MessageDisplay.SpeakerColor.Add(Color.Gray);
			MessageDisplay.SpeakerColor.Add(Color.YellowGreen);
			MessageDisplay.SpeakerColor.Add(Color.SandyBrown);
			MessageDisplay.SpeakerColor.Add(Color.PaleTurquoise);
			MessageDisplay.SpeakerColor.Add(Color.Gold);
			MessageDisplay.SpeakerColor.Add(Color.Aqua);
			MessageDisplay.SelectTrueColor = Color.Aquamarine;
			MessageDisplay.SelectFalseColor = Color.DimGray;
		}

		public override void InitData()
		{
			base.InitData();
			picVoiceBase = GameManager.Data.LoadPicture("system", "voice.png");
			picMain = GameManager.Data.LoadPicture("system", "message.png");
			picBack = GameManager.Graphics.CreatePicture(Color.FromArgb(128, Color.Black));
			Font2D font = GameManager.GalCore.GameFont.GetFont(20);
			for (int i = 0; i < MessageScreen.PageLineNum; i++)
			{
				strTextWithouts[i] = new String2D();
				strTextWithouts[i].Font = font;
				strTextWithouts[i].Location = Point.Empty;
				strTextWithouts[i].LineSize = new Size(600, 40);
				strTextWithouts[i].LineNum = 1;
			}
			for (int j = 0; j < MessageScreen.PageLineNum; j++)
			{
				strTextWiths[j] = new String2D();
				strTextWiths[j].Font = font;
				strTextWiths[j].Location = Point.Empty;
				strTextWiths[j].LineSize = new Size(480, 40);
				strTextWiths[j].LineNum = 1;
			}
			for (int k = 0; k < MessageScreen.PageLineNum; k++)
			{
				strSpeakers[k] = new String2D();
				strSpeakers[k].Font = font;
				strSpeakers[k].Location = Point.Empty;
				strSpeakers[k].LineSize = new Size(120, 40);
				strSpeakers[k].LineNum = 1;
			}
			scrollBar.InitData();
		}

		public override void FinalData()
		{
			base.FinalData();
			picBack.Dispose();
			picVoiceAnime.Dispose();
			for (int i = 0; i < 4; i++)
			{
				picVoices[i].Dispose();
			}
			picVoiceBase.Dispose();
			picMain.Dispose();
			scrollBar.FinalData();
		}

		public override bool InitScreen()
		{
			if (!base.InitScreen())
			{
				return false;
			}
			Element item = new Element(picVoices[0]);
			Element item2 = new Element(picVoices[3]);
			Element item3 = new Element(picVoiceAnime);
			Element element = new Element();
			element.Add(item, "normal", true);
			element.Add(item2, "select", false);
			element.Add(item3, "play", false);
			element.Region = new Rectangle(0, 0, 40, 40);
			Element item4 = new Element(picMain);
			eleScreen = new Element(picBack);
			eleScreen.Add(item4);
			for (int i = 0; i < MessageScreen.PageLineNum; i++)
			{
				Element element2 = (Element)element.Clone();
				element2.Position = new Point(60, 100 + i * 40);
				element2.Value = i;
				eleScreen.Add(element2, "voice" + i.ToString("00"));
			}
			element.Dispose();
			Element[] textWithoutElements = new Element[MessageScreen.PageLineNum];
			Element[] textWithElements = new Element[MessageScreen.PageLineNum];
			Element[] speakerElements = new Element[MessageScreen.PageLineNum];
			Element element3 = new Element();
			for (int j = 0; j < MessageScreen.PageLineNum; j++)
			{
				textWithoutElements[j] = new Element(strTextWithouts[j]);
				textWithElements[j] = new Element(strTextWiths[j]);
				speakerElements[j] = new Element(strSpeakers[j]);
				element3.Add(textWithoutElements[j], "textwithout" + j.ToString("00"));
				element3.Add(textWithElements[j], "textwith" + j.ToString("00"));
				element3.Add(speakerElements[j], "speaker" + j.ToString("00"));
			}
			element3.Region = new Rectangle(0, 0, 600, 440);
			element3.Position = new Point(100, 100);
			eleScreen.Add(element3, "text");
			eleScreen.Focused = true;
			element3.Selected = true;
			FocusedElement = eleScreen;
			RootElement = eleScreen;
			eleScreen.Add(new Element(), "voiceup");
			eleScreen.Add(new Element(), "voicedown");
			scrollBar.Init();
			scrollBar.Position = new Point(760, 70);
			scrollBar.Size = 500;
			eleScreen.Add(scrollBar, "scrollbar");
			scrollBar.ValueChanged += ScrollBar_ValueChanged;
			for (int k = 0; k < MessageScreen.PageLineNum; k++)
			{
				eleScreen["voice" + k.ToString("00")].Right = eleScreen["text"];
				eleScreen["voice" + k.ToString("00")].MouseClick += VoiceUnit_MouseClick;
				eleScreen["voice" + k.ToString("00")].MouseEnter += VoiceUnit_MouseEnter;
				eleScreen["voice" + k.ToString("00")].MouseLeave += VoiceUnit_MouseLeave;
			}
			eleScreen["text"].KeyPress += Text_KeyPress;
			eleScreen["voiceup"].MouseEnter += VoiceUp_MouseEnter;
			eleScreen["voicedown"].MouseEnter += VoiceDown_MouseEnter;
			UpdateMessage();
			return true;
		}

		/// <summary>拖动滚动条：把比例还原成行号，跳过去再刷新显示。</summary>
		private void ScrollBar_ValueChanged(object sender, EventArgs e)
		{
			int targetLine = (int)(((ScrollBarElement)sender).Start * (float)TotalRecordLineNum + 0.5f);
			if (targetLine < 0)
			{
				targetLine = 0;
			}
			if (targetLine > TotalRecordLineNum - MessageScreen.PageLineNum)
			{
				targetLine = TotalRecordLineNum - MessageScreen.PageLineNum;
				if (targetLine < 0)
				{
					targetLine = 0;
				}
			}
			MoveTo(targetLine);
			UpdateMessage();
		}

		private void VoiceUp_MouseEnter(object sender, EventArgs e)
		{
			if (FindVoiceUp())
			{
				FindVoiceUp();
				UpdateMessage();
				eleScreen["voice00"].Selected = true;
				return;
			}
			for (int i = 0; i < MessageScreen.PageLineNum; i++)
			{
				if (eleScreen.GetVisible("voice" + i.ToString("00")))
				{
					eleScreen["voice" + i.ToString("00")].Selected = true;
					break;
				}
			}
		}

		private void VoiceDown_MouseEnter(object sender, EventArgs e)
		{
			if (FindVoiceDown())
			{
				UpdateMessage();
				eleScreen["voice" + (Messages.Count - 1).ToString("00")].Selected = true;
				return;
			}
			for (int i = MessageScreen.PageLineNum - 1; i >= 0; i--)
			{
				if (eleScreen.GetVisible("voice" + i.ToString("00")))
				{
					eleScreen["voice" + i.ToString("00")].Selected = true;
					break;
				}
			}
		}

		private void VoiceUnit_MouseClick(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			LinkedListNode<MessageDisplay> linkedListNode;
			if (lastVoiceIndex != -1)
			{
				linkedListNode = Messages.First;
				for (int i = 0; i < MessageScreen.PageLineNum; i++)
				{
					if (linkedListNode.Value.Index == lastVoiceIndex)
					{
						eleScreen["voice" + i.ToString("00")].SetVisible("normal", true);
						eleScreen["voice" + i.ToString("00")].SetVisible("select", false);
						eleScreen["voice" + i.ToString("00")].SetVisible("play", false);
						break;
					}
					linkedListNode = linkedListNode.Next;
					if (linkedListNode == null)
					{
						break;
					}
				}
			}
			linkedListNode = Messages.First;
			for (int j = 0; j < (int)element.Value; j++)
			{
				linkedListNode = linkedListNode.Next;
			}
			PlayVoice(linkedListNode.Value.Voice);
			element.SetVisible("play", true);
			element.SetVisible("normal", false);
			element.SetVisible("select", false);
			((AnimePicture)element["play"].ElementPicture).ResetFrame();
			int voiceCount = (int)element.Value;
			linkedListNode = Messages.First;
			for (int k = 0; k < voiceCount; k++)
			{
				linkedListNode = linkedListNode.Next;
			}
			lastVoiceIndex = linkedListNode.Value.Index;
		}

		private void VoiceUnit_MouseEnter(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			if (!element.GetVisible("play"))
			{
				element.SetVisible("select", true);
				element.SetVisible("normal", false);
				element.SetVisible("play", false);
			}
		}

		private void VoiceUnit_MouseLeave(object sender, EventArgs e)
		{
			Element element = (Element)sender;
			if (!element.GetVisible("play"))
			{
				element.SetVisible("normal", true);
				element.SetVisible("select", false);
				element.SetVisible("play", false);
			}
		}

		private void Text_KeyPress(object sender, KeyEventArgs e)
		{
			switch (e.KeyData)
			{
			case Keys.Up:
				ScrollUp(1);
				UpdateMessage();
				break;
			case Keys.Down:
				ScrollDown(1);
				UpdateMessage();
				break;
			case Keys.Prior:
				PageUp();
				UpdateMessage();
				break;
			case Keys.Next:
				PageDown();
				UpdateMessage();
				break;
			}
		}

		public override void OnMouseWheel(int delta)
		{
			base.OnMouseWheel(delta);
			if (delta > 0)
			{
				ScrollUp(delta / 120);
				UpdateMessage();
			}
			else if (delta < 0)
			{
				ScrollDown(-delta / 120);
				UpdateMessage();
			}
		}

		private void UpdateMessage()
		{
			int lineIndex = 0;
			int prevLine = -1;
			foreach (MessageDisplay message in Messages)
			{
				if (message.Voice != null && message.From == 0)
				{
					eleScreen.SetVisible("voice" + lineIndex.ToString("00"), true);
					if (message.Index == lastVoiceIndex)
					{
						eleScreen["voice" + lineIndex.ToString("00")].SetVisible("normal", false);
						eleScreen["voice" + lineIndex.ToString("00")].SetVisible("select", false);
						eleScreen["voice" + lineIndex.ToString("00")].SetVisible("play", true);
					}
					else
					{
						if (eleScreen["voice" + lineIndex.ToString("00")].Selected)
						{
							eleScreen["voice" + lineIndex.ToString("00")].SetVisible("select", true);
							eleScreen["voice" + lineIndex.ToString("00")].SetVisible("normal", false);
						}
						else
						{
							eleScreen["voice" + lineIndex.ToString("00")].SetVisible("normal", true);
							eleScreen["voice" + lineIndex.ToString("00")].SetVisible("select", false);
						}
						eleScreen["voice" + lineIndex.ToString("00")].SetVisible("play", false);
					}
					if (prevLine >= 0)
					{
						eleScreen["voice" + lineIndex.ToString("00")].Up = eleScreen["voice" + prevLine.ToString("00")];
						eleScreen["voice" + prevLine.ToString("00")].Down = eleScreen["voice" + lineIndex.ToString("00")];
					}
					else
					{
						eleScreen["voice" + lineIndex.ToString("00")].Up = eleScreen["voiceup"];
						eleScreen["text"].Left = eleScreen["voice" + lineIndex.ToString("00")];
					}
					prevLine = lineIndex;
					eleScreen["voice" + lineIndex.ToString("00")].Position = new Point(60, 100 + message.LinePos * 40);
				}
				else
				{
					eleScreen.SetVisible("voice" + lineIndex.ToString("00"), false);
				}
				if (message.Speaker == null)
				{
					eleScreen["text"].SetVisible("textwithout" + lineIndex.ToString("00"), true);
					eleScreen["text"].SetVisible("textwith" + lineIndex.ToString("00"), false);
					eleScreen["text"].SetVisible("speaker" + lineIndex.ToString("00"), false);
					eleScreen["text"]["textwithout" + lineIndex.ToString("00")].Position = new Point(0, message.LinePos * 40);
					strTextWithouts[lineIndex].ClearString();
					strTextWithouts[lineIndex].AddString(message.Text, message.Align);
					strTextWithouts[lineIndex].LineNum = message.To - message.From + 1;
					strTextWithouts[lineIndex].CutLine(message.From, message.To);
					strTextWithouts[lineIndex].DefaultColor = ((message.Color == Color.Empty) ? Color.White : message.Color);
				}
				else
				{
					eleScreen["text"].SetVisible("textwith" + lineIndex.ToString("00"), true);
					eleScreen["text"].SetVisible("speaker" + lineIndex.ToString("00"), true);
					eleScreen["text"].SetVisible("textwithout" + lineIndex.ToString("00"), false);
					eleScreen["text"]["textwith" + lineIndex.ToString("00")].Position = new Point(120, message.LinePos * 40);
					eleScreen["text"]["speaker" + lineIndex.ToString("00")].Position = new Point(0, message.LinePos * 40);
					strTextWiths[lineIndex].ClearString();
					strTextWiths[lineIndex].AddString(message.Text);
					strTextWiths[lineIndex].LineNum = message.To - message.From + 1;
					strTextWiths[lineIndex].CutLine(message.From, message.To);
					strTextWiths[lineIndex].DefaultColor = ((message.Color == Color.Empty) ? Color.White : message.Color);
					strSpeakers[lineIndex].ClearString();
					strSpeakers[lineIndex].AddString(message.Speaker);
					strSpeakers[lineIndex].LineNum = message.To - message.From + 1;
					strSpeakers[lineIndex].CutLine(message.From, message.To);
					strSpeakers[lineIndex].DefaultColor = ((message.Color == Color.Empty) ? Color.White : message.Color);
				}
				lineIndex++;
			}
			for (int i = lineIndex; i < MessageScreen.PageLineNum; i++)
			{
				eleScreen.SetVisible("voice" + i.ToString("00"), false);
				eleScreen["text"].SetVisible("textwithout" + i.ToString("00"), false);
				eleScreen["text"].SetVisible("textwith" + i.ToString("00"), false);
				eleScreen["text"].SetVisible("speaker" + i.ToString("00"), false);
			}
			if (prevLine == -1)
			{
				eleScreen["text"].Left = null;
			}
			else
			{
				eleScreen["voice" + prevLine.ToString("00")].Down = eleScreen["voicedown"];
			}
			eleScreen["text"].Selected = true;
			scrollBar.Start = ScrollBarStart;
			scrollBar.Length = ScrollBarLength;
		}

		public override bool FinalScreen()
		{
			if (!base.FinalScreen())
			{
				return false;
			}
			StopVoice();
			Element element = eleScreen["voice00"];
			for (int i = 0; i < element.Count; i++)
			{
				element[i].Dispose();
			}
			for (int j = 0; j < eleScreen["text"].Count; j++)
			{
				if (eleScreen["text"][j] != scrollBar)
				{
					eleScreen["text"][j].Dispose();
				}
			}
			scrollBar.Final();
			for (int k = 0; k < eleScreen.Count; k++)
			{
				eleScreen[k].Dispose();
			}
			eleScreen.Dispose();
			eleScreen = null;
			FinalData();
			Finished = true;
			FocusedElement = null;
			RootElement = null;
			return true;
		}

		public override void Update(float elaspedTime)
		{
			if (lastVoiceIndex != -1 && !IsVoicing)
			{
				LinkedListNode<MessageDisplay> linkedListNode = Messages.First;
				for (int i = 0; i < MessageScreen.PageLineNum; i++)
				{
					if (linkedListNode.Value.Index == lastVoiceIndex)
					{
						if (eleScreen["voice" + i.ToString("00")].Selected)
						{
							eleScreen["voice" + i.ToString("00")].SetVisible("select", true);
							eleScreen["voice" + i.ToString("00")].SetVisible("normal", false);
						}
						else
						{
							eleScreen["voice" + i.ToString("00")].SetVisible("normal", true);
							eleScreen["voice" + i.ToString("00")].SetVisible("select", false);
						}
						eleScreen["voice" + i.ToString("00")].SetVisible("play", false);
						break;
					}
					linkedListNode = linkedListNode.Next;
					if (linkedListNode == null)
					{
						break;
					}
				}
				lastVoiceIndex = -1;
			}
			base.Update(elaspedTime);
		}
	}
}
