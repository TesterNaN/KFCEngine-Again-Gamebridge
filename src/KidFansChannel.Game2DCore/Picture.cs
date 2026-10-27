using System;
using TransformedColoredTextured = Microsoft.DirectX.Direct3D.CustomVertex.TransformedColoredTextured;
using System.Drawing;
using System.IO;
using System.Threading;
using Microsoft.DirectX.Direct3D;

namespace KidFansChannel.Game2DCore
{
	public class Picture : IDisposable, ICloneable
	{
		protected enum TransformType
		{
			None,
			Global,
			Local,
			Loop,
			Function
		}

		public delegate float TransformFunction(float time);

		private Size _size = Size.Empty;

		private Point _center = Point.Empty;

		private Point _position = Point.Empty;

		private SizeF _scale = new SizeF(1f, 1f);

		private float _rotate;

		private int _alpha = 255;

		private Color _argumentColor = Color.Transparent;

		private float _time;

		private PointF[] verts = new PointF[4];

		private float fAlpha = 255f;

		private int _alphaTo = 255;

		private PointF[] vertsTo = new PointF[4];

		private Texture _texture;

		private Color _color;

		private bool _textured;

		private Game2DCore _core;

		private bool _isCreated;

		private RectangleF _bounds = new RectangleF(0f, 0f, 1f, 1f);

		private bool _isTransformSetup;

		private TransformType positionType;

		private float positionTime;

		private PointF positionTemp;

		private Point positionTo;

		private Point positionLoop;

		private float positionSpeedX;

		private float positionSpeedY;

		private float positionLoopTime;

		private TransformFunction positionFunctionX;

		private TransformFunction positionFunctionY;

		private TransformType scaleType;

		private float scaleTime;

		private SizeF scaleTo;

		private SizeF scaleLoop;

		private float scaleSpeedX;

		private float scaleSpeedY;

		private float scaleLoopTime;

		private TransformFunction scaleFunctionX;

		private TransformFunction scaleFunctionY;

		private TransformType rotateType;

		private float rotateTime;

		private float rotateTo;

		private float rotateLoop;

		private float rotateSpeed;

		private float rotateLoopTime;

		private TransformFunction rotateFunction;

		private TransformType alphaType;

		private float alphaTime;

		private int alphaTo;

		private int alphaLoop;

		private float alphaSpeed;

		private float alphaLoopTime;

		private TransformFunction alphaFunction;

		private float alphaTemp;

		private bool _isSetupCircle;

		public virtual Size Size
		{
			get
			{
				return _size;
			}
			set
			{
				if (_textured)
				{
					throw new AccessViolationException("Cannot set textures' size");
				}
				_size = value;
				RefreshVerts();
			}
		}

		public virtual int Width
		{
			get
			{
				return _size.Width;
			}
			set
			{
				if (_textured)
				{
					throw new AccessViolationException("Cannot set textures' size");
				}
				_size.Width = value;
				RefreshVerts();
			}
		}

		public virtual int Height
		{
			get
			{
				return _size.Height;
			}
			set
			{
				if (_textured)
				{
					throw new AccessViolationException("Cannot set textures' size");
				}
				_size.Height = value;
				RefreshVerts();
			}
		}

		public virtual Point Center
		{
			get
			{
				return _center;
			}
			set
			{
				_center = value;
				RefreshVerts();
			}
		}

		public virtual int CenterX
		{
			get
			{
				return _center.X;
			}
			set
			{
				_center.X = value;
				RefreshVerts();
			}
		}

		public virtual int CenterY
		{
			get
			{
				return _center.Y;
			}
			set
			{
				_center.Y = value;
				RefreshVerts();
			}
		}

		public virtual Point Position
		{
			get
			{
				return _position;
			}
			set
			{
				_position = value;
				RefreshVerts();
			}
		}

		public virtual int X
		{
			get
			{
				return _position.X;
			}
			set
			{
				_position.X = value;
				RefreshVerts();
			}
		}

		public virtual int Y
		{
			get
			{
				return _position.Y;
			}
			set
			{
				_position.Y = value;
				RefreshVerts();
			}
		}

		public virtual SizeF Scale
		{
			get
			{
				return _scale;
			}
			set
			{
				_scale = value;
				RefreshVerts();
			}
		}

		public virtual float ScaleX
		{
			get
			{
				return _scale.Width;
			}
			set
			{
				_scale.Width = value;
				RefreshVerts();
			}
		}

		public virtual float ScaleY
		{
			get
			{
				return _scale.Height;
			}
			set
			{
				_scale.Height = value;
				RefreshVerts();
			}
		}

		public virtual float Rotate
		{
			get
			{
				return _rotate;
			}
			set
			{
				_rotate = value;
				RefreshVerts();
			}
		}

		public virtual int Alpha
		{
			get
			{
				return _alpha;
			}
			set
			{
				_alpha = value;
				fAlpha = value;
			}
		}

		public virtual Color ArgumentColor
		{
			get
			{
				return _argumentColor;
			}
			set
			{
				_argumentColor = value;
			}
		}

		public virtual float Time
		{
			get
			{
				return _time;
			}
		}

		internal PointF Vertex1
		{
			get
			{
				return verts[0];
			}
		}

		internal PointF Vertex2
		{
			get
			{
				return verts[1];
			}
		}

		internal PointF Vertex3
		{
			get
			{
				return verts[2];
			}
		}

		internal PointF Vertex4
		{
			get
			{
				return verts[3];
			}
		}

		public Texture Texture
		{
			get
			{
				return _texture;
			}
			protected set
			{
				_texture = value;
			}
		}

		public bool Textured
		{
			get
			{
				return _textured;
			}
			protected set
			{
				_textured = value;
			}
		}

		public Game2DCore Core
		{
			get
			{
				return _core;
			}
			protected set
			{
				_core = value;
			}
		}

		public virtual bool IsCreated
		{
			get
			{
				return _isCreated;
			}
			protected set
			{
				_isCreated = value;
			}
		}

		public RectangleF Bounds
		{
			get
			{
				return _bounds;
			}
			protected set
			{
				_bounds = value;
			}
		}

		protected bool IsTransformSetup
		{
			get
			{
				return _isTransformSetup;
			}
		}

		protected bool IsSetupCircle
		{
			get
			{
				return _isSetupCircle;
			}
			set
			{
				if (!value)
				{
					_isSetupCircle = false;
				}
			}
		}

		public virtual void BeginTransform(float time)
		{
			_time = time;
			_isTransformSetup = true;
			_isSetupCircle = true;
			positionType = TransformType.None;
			rotateType = TransformType.None;
			scaleType = TransformType.None;
			alphaType = TransformType.None;
		}

		public virtual void EndTransform()
		{
			_isTransformSetup = false;
		}

		public virtual void ResetTime()
		{
			if (_isTransformSetup)
			{
				throw new Exception("Cannot reset time inside transform setup.");
			}
			_time = float.NaN;
		}

		public virtual void PositionTo(Point position)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set position transform outside transform setup.");
			}
			positionType = TransformType.Global;
			positionTo = position;
			positionTemp.X = _position.X;
			positionTemp.Y = _position.Y;
		}

		public virtual void PositionTo(Point position, float time)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set position transform outside transform setup.");
			}
			positionType = TransformType.Local;
			positionTime = time;
			positionTo = position;
			positionTemp.X = _position.X;
			positionTemp.Y = _position.Y;
		}

		public virtual void PositionTo(Point position1, Point position2, float time)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set position transform outside transform setup.");
			}
			positionType = TransformType.Loop;
			positionLoopTime = time;
			positionTo = position1;
			positionLoop = position2;
			positionSpeedX = (float)(positionLoop.X - positionTo.X) / (time / 2f);
			positionSpeedY = (float)(positionLoop.Y - positionTo.Y) / (time / 2f);
			positionTime = (float)(_position.X - positionTo.X) / positionSpeedX;
			positionTemp.X = _position.X;
			positionTemp.Y = _position.Y;
		}

		public virtual void PositionTo(TransformFunction functionX, TransformFunction functionY)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set position transform outside transform setup.");
			}
			positionType = TransformType.Function;
			positionFunctionX = functionX;
			positionFunctionY = functionY;
			positionTime = 0f;
			positionTemp.X = _position.X;
			positionTemp.Y = _position.Y;
		}

		public virtual void ScaleTo(SizeF scale)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set scale transform outside transform setup.");
			}
			scaleType = TransformType.Global;
			scaleTo = scale;
		}

		public virtual void ScaleTo(SizeF scale, float time)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set scale transform outside transform setup.");
			}
			scaleType = TransformType.Local;
			scaleTime = time;
			scaleTo = scale;
		}

		public virtual void ScaleTo(SizeF scale1, SizeF scale2, float time)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set scale transform outside transform setup.");
			}
			scaleType = TransformType.Loop;
			scaleLoopTime = time;
			scaleTo = scale1;
			scaleLoop = scale2;
			scaleSpeedX = (scaleLoop.Width - scaleTo.Width) / (time / 2f);
			scaleSpeedY = (scaleLoop.Height - scaleTo.Height) / (time / 2f);
			scaleTime = (_scale.Width - scaleTo.Width) / scaleSpeedX;
		}

		public virtual void ScaleTo(TransformFunction functionX, TransformFunction functionY)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set scale transform outside transform setup.");
			}
			scaleType = TransformType.Function;
			scaleFunctionX = functionX;
			scaleFunctionY = functionY;
			scaleTime = 0f;
		}

		public virtual void RotateTo(float rotate)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set rotate transform outside transform setup.");
			}
			rotateType = TransformType.Global;
			rotateTo = rotate;
		}

		public virtual void RotateTo(float rotate, float time)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set rotate transform outside transform setup.");
			}
			rotateType = TransformType.Local;
			rotateTime = time;
			rotateTo = rotate;
		}

		public virtual void RotateTo(float rotate1, float rotate2, float time)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set rotate transform outside transform setup.");
			}
			rotateType = TransformType.Loop;
			rotateLoopTime = time;
			rotateTo = rotate1;
			rotateLoop = rotate2;
			rotateSpeed = (rotateLoop - rotateTo) / (time / 2f);
			rotateTime = (_rotate - rotateTo) / rotateSpeed;
		}

		public virtual void RotateTo(TransformFunction function)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set rotate transform outside transform setup.");
			}
			rotateType = TransformType.Function;
			rotateFunction = function;
			rotateTime = 0f;
		}

		public virtual void AlphaTo(int alpha)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set alpha transform outside transform setup.");
			}
			alphaType = TransformType.Global;
			alphaTo = alpha;
			alphaTemp = _alpha;
		}

		public virtual void AlphaTo(int alpha, float time)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set alpha transform outside transform setup.");
			}
			alphaType = TransformType.Local;
			alphaTime = time;
			alphaTo = alpha;
			alphaTemp = _alpha;
		}

		public virtual void AlphaTo(int alpha1, int alpha2, float time)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set alpha transform outside transform setup.");
			}
			alphaType = TransformType.Loop;
			alphaLoopTime = time;
			alphaTo = alpha1;
			alphaLoop = alpha2;
			alphaSpeed = (float)(alphaLoop - alphaTo) / (time / 2f);
			alphaTime = (float)(_alpha - alphaTo) / alphaSpeed;
			alphaTemp = _alpha;
		}

		public virtual void AlphaTo(TransformFunction function)
		{
			if (!_isTransformSetup)
			{
				throw new Exception("Cannot set alpha transform outside transform setup.");
			}
			alphaType = TransformType.Function;
			alphaFunction = function;
			alphaTime = 0f;
			alphaTemp = _alpha;
		}

		public virtual void Update(float elaspedTime)
		{
			if (_isTransformSetup)
			{
				throw new Exception("Cannot update inside transform setup.");
			}
			if (_isSetupCircle)
			{
				Update();
				_isSetupCircle = false;
				return;
			}
			bool done = false;
			if (UpdatePosition(elaspedTime))
			{
				done = true;
			}
			if (UpdateScale(elaspedTime))
			{
				done = true;
			}
			if (UpdateRotate(elaspedTime))
			{
				done = true;
			}
			UpdateAlpha(elaspedTime);
			if (_time > elaspedTime)
			{
				_time -= elaspedTime;
			}
			else if (_time > 0f)
			{
				_time = 0f;
			}
			if (done)
			{
				verts = CalculateVerts(_position, _scale, _rotate);
			}
			Update();
		}

		private bool UpdatePosition(float elaspedTime)
		{
			bool done = false;
			switch (positionType)
			{
			case TransformType.Global:
				if (_time > elaspedTime)
				{
					done = true;
					float progress = elaspedTime / _time;
					positionTemp.X += ((float)positionTo.X - positionTemp.X) * progress;
					positionTemp.Y += ((float)positionTo.Y - positionTemp.Y) * progress;
				}
				else if (_time > 0f)
				{
					done = true;
					positionTemp.X = positionTo.X;
					positionTemp.Y = positionTo.Y;
				}
				break;
			case TransformType.Local:
				if (positionTime > elaspedTime)
				{
					done = true;
					float progress = elaspedTime / positionTime;
					positionTemp.X += ((float)positionTo.X - positionTemp.X) * progress;
					positionTemp.Y += ((float)positionTo.Y - positionTemp.Y) * progress;
					positionTime -= elaspedTime;
				}
				else if (positionTime > 0f)
				{
					done = true;
					positionTemp.X = positionTo.X;
					positionTemp.Y = positionTo.Y;
					positionTime = 0f;
				}
				break;
			case TransformType.Loop:
				done = true;
				positionTime -= elaspedTime;
				while (positionTime < (0f - positionLoopTime) / 2f)
				{
					positionTime += positionLoopTime;
				}
				positionTemp.X = (float)positionTo.X + Math.Abs(positionTime) * positionSpeedX;
				positionTemp.Y = (float)positionTo.Y + Math.Abs(positionTime) * positionSpeedY;
				break;
			case TransformType.Function:
				done = true;
				positionTime += elaspedTime;
				positionTemp.X = positionFunctionX(positionTime);
				positionTemp.Y = positionFunctionY(positionTime);
				break;
			}
			if (done)
			{
				_position.X = (int)((double)positionTemp.X + 0.5);
				_position.Y = (int)((double)positionTemp.Y + 0.5);
			}
			return done;
		}

		private bool UpdateScale(float elaspedTime)
		{
			bool result = false;
			switch (scaleType)
			{
			case TransformType.Global:
				if (_time > elaspedTime)
				{
					result = true;
					float progress = elaspedTime / _time;
					_scale.Width += (scaleTo.Width - _scale.Width) * progress;
					_scale.Height += (scaleTo.Height - _scale.Height) * progress;
				}
				else if (_time > 0f)
				{
					result = true;
					_scale.Width = scaleTo.Width;
					_scale.Height = scaleTo.Height;
				}
				break;
			case TransformType.Local:
				if (scaleTime > elaspedTime)
				{
					result = true;
					float progress = elaspedTime / scaleTime;
					_scale.Width += (scaleTo.Width - _scale.Width) * progress;
					_scale.Height += (scaleTo.Height - _scale.Height) * progress;
					scaleTime -= elaspedTime;
				}
				else if (scaleTime > 0f)
				{
					result = true;
					_scale.Width = scaleTo.Width;
					_scale.Height = scaleTo.Height;
					scaleTime = 0f;
				}
				break;
			case TransformType.Loop:
				result = true;
				scaleTime -= elaspedTime;
				while (scaleTime < (0f - scaleLoopTime) / 2f)
				{
					scaleTime += scaleLoopTime;
				}
				_scale.Width = scaleTo.Width + Math.Abs(scaleTime) * scaleSpeedX;
				_scale.Height = scaleTo.Height + Math.Abs(scaleTime) * scaleSpeedY;
				break;
			case TransformType.Function:
				result = true;
				scaleTime += elaspedTime;
				_scale.Width = scaleFunctionX(scaleTime);
				_scale.Height = scaleFunctionX(scaleTime);
				break;
			}
			return result;
		}

		private bool UpdateRotate(float elaspedTime)
		{
			bool result = false;
			switch (rotateType)
			{
			case TransformType.Global:
				if (_time > elaspedTime)
				{
					result = true;
					float progress = elaspedTime / _time;
					_rotate += (rotateTo - _rotate) * progress;
				}
				else if (_time > 0f)
				{
					result = true;
					_rotate = rotateTo;
				}
				break;
			case TransformType.Local:
				if (rotateTime > elaspedTime)
				{
					result = true;
					float progress = elaspedTime / rotateTime;
					_rotate += (rotateTo - _rotate) * progress;
					rotateTime -= elaspedTime;
				}
				else if (rotateTime > 0f)
				{
					result = true;
					_rotate = rotateTo;
					rotateTime = 0f;
				}
				break;
			case TransformType.Loop:
				result = true;
				rotateTime -= elaspedTime;
				while (rotateTime < (0f - rotateLoopTime) / 2f)
				{
					rotateTime += rotateLoopTime;
				}
				_rotate = rotateTo + Math.Abs(rotateTime) * rotateSpeed;
				break;
			case TransformType.Function:
				result = true;
				rotateTime += elaspedTime;
				_rotate = rotateFunction(rotateTime);
				break;
			}
			return result;
		}

		private bool UpdateAlpha(float elaspedTime)
		{
			bool done = false;
			switch (alphaType)
			{
			case TransformType.Global:
				if (_time > elaspedTime)
				{
					done = true;
					float progress = elaspedTime / _time;
					alphaTemp += ((float)alphaTo - alphaTemp) * progress;
				}
				else if (_time > 0f)
				{
					done = true;
					alphaTemp = alphaTo;
				}
				break;
			case TransformType.Local:
				if (alphaTime > elaspedTime)
				{
					done = true;
					float progress = elaspedTime / alphaTime;
					alphaTemp += ((float)alphaTo - alphaTemp) * progress;
					alphaTime -= elaspedTime;
				}
				else if (alphaTime > 0f)
				{
					done = true;
					alphaTemp = alphaTo;
					alphaTime = 0f;
				}
				break;
			case TransformType.Loop:
				done = true;
				alphaTime -= elaspedTime;
				while (alphaTime < (0f - alphaLoopTime) / 2f)
				{
					alphaTime += alphaLoopTime;
				}
				alphaTemp = (float)alphaTo + Math.Abs(alphaTime) * alphaSpeed;
				break;
			case TransformType.Function:
				done = true;
				alphaTime += elaspedTime;
				alphaTemp = alphaFunction(alphaTime);
				break;
			}
			if (done)
			{
				_alpha = (int)((double)alphaTemp + 0.5);
			}
			return done;
		}

		public void Update()
		{
		}

		public void Draw(Device device, VertexBuffer vb)
		{
			Draw(device, vb, true);
		}

		public virtual void Draw(Device device, VertexBuffer vb, bool target)
		{
			if (_isTransformSetup)
			{
				throw new Exception("Cannot draw inside transform setup.");
			}
			if (!_isCreated || _size.Width == 0 || _size.Height == 0 || _scale.Width == 0f || _scale.Height == 0f || _alpha == 0)
			{
				return;
			}
			if (device == (Device)null)
			{
				throw new ArgumentNullException("Device is null while drawing.");
			}
			if (_textured)
			{
				if (_texture == (Texture)null)
				{
					throw new NullReferenceException("Texture has been disposed.");
				}
				if ((Resource)(object)vb == (Resource)null)
				{
					throw new ArgumentNullException("Vertex buffer is null.");
				}
				TransformedColoredTextured[] vertices = (TransformedColoredTextured[])vb.Lock(0, (LockFlags)0);
				if (vertices.Length < 4)
				{
					throw new ArgumentException("Vertex buffer's number of vertices is less than 4.");
				}
				for (int i = 0; i < 4; i++)
				{
					if (target)
					{
						vertices[i].X = verts[i].X * _core.TargetBounds.Width + _core.TargetBounds.X;
						vertices[i].Y = verts[i].Y * _core.TargetBounds.Height + _core.TargetBounds.Y;
					}
					else
					{
						vertices[i].X = verts[i].X;
						vertices[i].Y = verts[i].Y;
					}
					vertices[i].Color = _argumentColor.ToArgb();
					vertices[i].Z = 0.5f;
					vertices[i].Rhw = 1f;
					vertices[i].Tu = (((i & 1) == 0) ? _bounds.Left : _bounds.Right);
					vertices[i].Tv = ((i >> 1 == 0) ? _bounds.Top : _bounds.Bottom);
				}
				vb.Unlock();
				vertices = null;
				device.SetTexture(0, (BaseTexture)(object)_texture);
				if (_argumentColor == Color.Transparent)
				{
					device.TextureState[0].ColorOperation = (TextureOperation)2;
					device.TextureState[0].ColorArgument1 = (TextureArgument)2;
				}
				else
				{
					device.TextureState[0].ColorOperation = (TextureOperation)16;
					device.TextureState[0].ColorArgument1 = (TextureArgument)1;
					device.TextureState[0].ColorArgument2 = (TextureArgument)2;
				}
				if (_alpha == 255)
				{
					device.TextureState[0].AlphaOperation = (TextureOperation)2;
					device.TextureState[0].AlphaArgument1 = (TextureArgument)2;
				}
				else
				{
					device.TextureState[0].AlphaOperation = (TextureOperation)4;
					device.TextureState[0].AlphaArgument1 = (TextureArgument)2;
					device.TextureState[0].AlphaArgument2 = (TextureArgument)3;
				}
				if (_alpha < 255 || _argumentColor != Color.Transparent)
				{
					device.RenderState.TextureFactor = Color.FromArgb(_alpha, Color.Black).ToArgb();
				}
				device.SamplerState[0].MagFilter = (TextureFilter)2;
				device.SamplerState[0].MinFilter = (TextureFilter)2;
				device.SamplerState[0].AddressU = (TextureAddress)3;
				device.SamplerState[0].AddressV = (TextureAddress)3;
				device.SetStreamSource(0, vb, 0);
				device.VertexFormat = (VertexFormats)324;
				device.DrawPrimitives((PrimitiveType)5, 0, 2);
			}
			else
			{
				if (_color == Color.Transparent)
				{
					return;
				}
				if ((Resource)(object)vb == (Resource)null)
				{
					throw new ArgumentNullException("Vertex buffer is null.");
				}
				TransformedColoredTextured[] vertices2 = (TransformedColoredTextured[])vb.Lock(0, (LockFlags)0);
				if (vertices2.Length < 4)
				{
					throw new ArgumentException("Vertex buffer's number of vertices is less than 4.");
				}
				for (int j = 0; j < 4; j++)
				{
					if (target)
					{
						vertices2[j].X = verts[j].X * _core.TargetBounds.Width + _core.TargetBounds.X;
						vertices2[j].Y = verts[j].Y * _core.TargetBounds.Height + _core.TargetBounds.Y;
					}
					else
					{
						vertices2[j].X = verts[j].X;
						vertices2[j].Y = verts[j].Y;
					}
					vertices2[j].Z = 0.5f;
					vertices2[j].Rhw = 1f;
					vertices2[j].Color = Color.FromArgb(_alpha * _color.A / 255, _color).ToArgb();
				}
				vb.Unlock();
				vertices2 = null;
				device.SetTexture(0, (BaseTexture)null);
				device.SetStreamSource(0, vb, 0);
				device.VertexFormat = (VertexFormats)324;
				device.DrawPrimitives((PrimitiveType)5, 0, 2);
			}
		}

		protected PointF[] CalculateVerts(Point position, SizeF scale, float rotate)
		{
			PointF[] corners = new PointF[4];
			corners[0].X = -_center.X;
			corners[0].Y = -_center.Y;
			corners[1].X = _size.Width - _center.X;
			corners[1].Y = -_center.Y;
			corners[2].X = -_center.X;
			corners[2].Y = _size.Height - _center.Y;
			corners[3].X = _size.Width - _center.X;
			corners[3].Y = _size.Height - _center.Y;
			for (int i = 0; i < 4; i++)
			{
				corners[i].X *= scale.Width;
				corners[i].Y *= scale.Height;
				float x = (float)((double)corners[i].X * Math.Cos(rotate) + (double)corners[i].Y * Math.Sin(rotate));
				float y = (float)((double)(0f - corners[i].X) * Math.Sin(rotate) + (double)corners[i].Y * Math.Cos(rotate));
				corners[i].X = x;
				corners[i].Y = y;
				corners[i].X += position.X;
				corners[i].Y += position.Y;
				corners[i].X -= 0.5f;
				corners[i].Y -= 0.5f;
			}
			return corners;
		}

		protected void RefreshVerts()
		{
			verts = CalculateVerts(_position, _scale, _rotate);
		}

		protected void CreateTextureThreadProc(object source)
		{
			//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
			//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
			//IL_0013: Unknown result type (might be due to invalid IL or missing references)
			//IL_0018: Unknown result type (might be due to invalid IL or missing references)
			if (source is string)
			{
				string text = source as string;
				try
				{
					ImageInformation fileInfo = TextureLoader.ImageInformationFromFile(text);
					_size.Width = fileInfo.Width;
					_size.Height = fileInfo.Height;
					_texture = TextureLoader.FromFile(_core.Device, text, _size.Width, _size.Height, 1, (Usage)0, (Format)21, (Pool)1, (Filter)1, (Filter)1, 0);
				}
				catch
				{
				}
				RefreshVerts();
				_isCreated = true;
			}
			else if (source is Stream)
			{
				try
				{
					Stream stream = source as Stream;
					stream.Seek(0L, SeekOrigin.Begin);
					ImageInformation streamInfo = TextureLoader.ImageInformationFromStream(stream);
					_size.Width = streamInfo.Width;
					_size.Height = streamInfo.Height;
					stream.Seek(0L, SeekOrigin.Begin);
					_texture = TextureLoader.FromStream(_core.Device, stream, _size.Width, _size.Height, 1, (Usage)0, (Format)21, (Pool)1, (Filter)1, (Filter)1, 0);
					stream.Close();
					stream.Dispose();
				}
				catch
				{
				}
				RefreshVerts();
				_isCreated = true;
			}
		}

		internal Picture(Game2DCore core, string fileName)
		{
			if (core == null)
			{
				throw new ArgumentNullException("Core must not be null.");
			}
			if (core.Device == (Device)null)
			{
				throw new ArgumentException("Core's device must not be null.");
			}
			_core = core;
			core.PictureList.Add(this);
			_textured = true;
			new Thread(CreateTextureThreadProc).Start(fileName);
		}

		internal Picture(Game2DCore core, Stream stream)
		{
			if (core == null)
			{
				throw new ArgumentNullException("Core must not be null.");
			}
			if (core.Device == (Device)null)
			{
				throw new ArgumentException("Core's device must not be null.");
			}
			_core = core;
			core.PictureList.Add(this);
			_textured = true;
			new Thread(CreateTextureThreadProc).Start(stream);
		}

		internal Picture(Game2DCore core, Color color)
		{
			if (core == null)
			{
				throw new ArgumentNullException("Core must not be null.");
			}
			_core = core;
			core.PictureList.Add(this);
			_textured = false;
			_color = color;
			RefreshVerts();
			_isCreated = true;
		}

		protected Picture()
		{
		}

		internal static Picture CreateInstant(Game2DCore core, string fileName)
		{
			if (core == null)
			{
				throw new ArgumentNullException("Core must not be null.");
			}
			if (core.Device == (Device)null)
			{
				throw new ArgumentException("Core's device must not be null.");
			}
			Picture picture = new Picture();
			picture._core = core;
			core.PictureList.Add(picture);
			picture._textured = true;
			picture.CreateTextureThreadProc(fileName);
			return picture;
		}

		internal static Picture CreateInstant(Game2DCore core, Stream stream)
		{
			if (core == null)
			{
				throw new ArgumentNullException("Core must not be null.");
			}
			if (core.Device == (Device)null)
			{
				throw new ArgumentException("Core's device must not be null.");
			}
			Picture picture = new Picture();
			picture._core = core;
			core.PictureList.Add(picture);
			picture._textured = true;
			picture.CreateTextureThreadProc(stream);
			return picture;
		}

		public virtual void Dispose()
		{
			if (_texture != (Texture)null)
			{
				_texture.Dispose();
				_texture = null;
			}
			_core.PictureList.Remove(this);
		}

		~Picture()
		{
			Dispose();
		}

		public virtual object Clone()
		{
			Picture picture = new Picture();
			picture._texture = _texture;
			picture._color = _color;
			picture._size = _size;
			picture._center = _center;
			picture._position = _position;
			picture._scale = _scale;
			picture._rotate = _rotate;
			picture._alpha = _alpha;
			picture._argumentColor = _argumentColor;
			picture._time = _time;
			picture.verts = (PointF[])verts.Clone();
			picture.fAlpha = fAlpha;
			picture._alphaTo = _alphaTo;
			picture.vertsTo = (PointF[])vertsTo.Clone();
			return picture;
		}
	}
}
