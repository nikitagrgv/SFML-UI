using OpenTK.Graphics.OpenGL;
using SFML.Graphics;

namespace SFMLUI;

internal class MaskPainter : IMaskPainter
{
	private RenderTarget _target;
	private IMaskPainter.MaskPaintMode _mode = IMaskPainter.MaskPaintMode.Add;
	private bool _dirty = true;
	private int _stencilDepth;
	private bool _maskDrawn;
	private FloatRect _paintRect;

	public MaskPainter(RenderTarget target)
	{
		_target = target;
	}

	public IMaskPainter.MaskPaintMode CurrentMode
	{
		get => _mode;
		set
		{
			if (value == _mode)
			{
				return;
			}

			_mode = value;
			_dirty = true;
		}
	}

	public void StartDrawMask()
	{
		_mode = IMaskPainter.MaskPaintMode.Add;
		_dirty = true;
		_maskDrawn = false;
	}

	public bool FinishDrawMask()
	{
		if (!_maskDrawn)
		{
			return false;
		}

		_stencilDepth++;
		return true;
	}

	public void StartUseMask()
	{
		GL.StencilMask(0x00);
		GL.StencilFunc(StencilFunction.Equal, _stencilDepth, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
		GL.ColorMask(true, true, true, true);
	}

	public void FinishUseMask()
	{
		if (_maskDrawn)
		{
			_stencilDepth--;

			GL.StencilMask(0xFF);
			GL.StencilFunc(StencilFunction.Less, _stencilDepth, 0xFF);
			GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
			GL.ColorMask(false, false, false, false);

			// TODO# reuse!
			var shape = new RectangleShape();
			shape.Size = _paintRect.Size;
			shape.Position = _paintRect.Position;
			_target.Draw(shape);
		}
		
		GL.StencilMask(0x00);
		GL.StencilFunc(StencilFunction.Equal, _stencilDepth, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
		GL.ColorMask(true, true, true, true);

	}

	public void Draw(Drawable drawable)
	{
		Prepare();
		_target.Draw(drawable);
	}

	public void Draw(Drawable drawable, RenderStates states)
	{
		Prepare();
		_target.Draw(drawable, states);
		_maskDrawn = true;
	}

	private void Prepare()
	{
		if (!_dirty)
		{
			return;
		}

		GL.StencilMask(0xFF);
		GL.StencilFunc(StencilFunction.Equal, _stencilDepth, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);
		GL.ColorMask(false, false, false, false);

		_dirty = false;
	}

	public void SetPaintRect(FloatRect rect)
	{
		_paintRect = rect;
	}
}