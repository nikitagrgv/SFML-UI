using OpenTK.Graphics.OpenGL;
using SFML.Graphics;

namespace SFMLUI;

internal class MaskPainter : IMaskPainter
{
	private IMaskPainter.MaskPaintMode _mode = IMaskPainter.MaskPaintMode.Add;
	private bool _dirty = true;

	public RenderTarget? Target { get; set; }

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
	}

	public void StartUseMask()
	{
		GL.StencilMask(0x00);
		GL.StencilFunc(StencilFunction.Equal, drawState.StencilDepth, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
		GL.ColorMask(true, true, true, true);
	}

	public void Draw(Drawable drawable)
	{
		Prepare();
		Target!.Draw(drawable);
	}

	public void Draw(Drawable drawable, RenderStates states)
	{
		Prepare();
		Target!.Draw(drawable, states);
	}

	private void Prepare()
	{
		if (Target == null)
		{
			throw new InvalidOperationException("No target set");
		}

		if (!_dirty)
		{
			return;
		}

		GL.StencilMask(0xFF);
		GL.StencilFunc(StencilFunction.Equal, drawState.StencilDepth, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);
		GL.ColorMask(false, false, false, false);

		_dirty = false;
	}
}