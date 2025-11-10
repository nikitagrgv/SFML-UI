using OpenTK.Graphics.OpenGL;
using SFML.Graphics;

namespace SFMLUI;

internal class MaskPainter : IMaskPainter
{
	private readonly RenderTarget _target;
	private IMaskPainter.MaskPaintMode _mode = IMaskPainter.MaskPaintMode.Add;

	private readonly RectangleShape _clearShape = new();

	private int _stencilDepth;
	private FloatRect _paintRect;

	private bool _needApplyDrawMask = true;
	private bool _needApplyMode = true;
	private bool _maskDrawn;


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
			_needApplyMode = true;
		}
	}

	public void StartDrawMask()
	{
		_mode = IMaskPainter.MaskPaintMode.Add;
		_needApplyDrawMask = true;
		_needApplyMode = true;
		_maskDrawn = false;
	}

	public bool FinishDrawMask()
	{
		bool drawn = _maskDrawn;
		if (drawn)
		{
			_stencilDepth++;
		}

		_maskDrawn = false;
		return drawn;
	}

	public void StartUseMask()
	{
		GL.StencilMask(0x00);
		GL.StencilFunc(StencilFunction.Equal, _stencilDepth, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
		GL.ColorMask(true, true, true, true);
	}

	public void FinishUseMask(bool maskDrawn)
	{
		if (maskDrawn)
		{
			_stencilDepth--;

			// Clear mask
			GL.StencilMask(0xFF);
			GL.StencilFunc(StencilFunction.Less, _stencilDepth, 0xFF);
			GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
			GL.ColorMask(false, false, false, false);

			_clearShape.Size = _paintRect.Size;
			_target.Draw(_clearShape);
		}

		GL.StencilMask(0x00);
		GL.StencilFunc(StencilFunction.Equal, _stencilDepth, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
		GL.ColorMask(true, true, true, true);
	}

	public void Draw(Drawable drawable)
	{
		PrepareToDraw();
		_target.Draw(drawable);
		_maskDrawn = true;
	}

	public void Draw(Drawable drawable, RenderStates states)
	{
		PrepareToDraw();
		_target.Draw(drawable, states);
		_maskDrawn = true;
	}

	private void PrepareToDraw()
	{
		if (_needApplyDrawMask)
		{
			_needApplyDrawMask = false;

			GL.StencilMask(0xFF);
			GL.ColorMask(false, false, false, false);
		}

		if (_needApplyMode)
		{
			_needApplyMode = false;

			if (_mode == IMaskPainter.MaskPaintMode.Add)
			{
				GL.StencilFunc(StencilFunction.Equal, _stencilDepth, 0xFF);
				GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);
			}
			else
			{
				GL.StencilFunc(StencilFunction.Less, _stencilDepth, 0xFF);
				GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
			}
		}
	}

	public void SetPaintRect(FloatRect rect)
	{
		_paintRect = rect;
	}
}