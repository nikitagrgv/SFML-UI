using SFML.Graphics;

namespace SFMLUI;

internal class Painter : IPainter
{
	private readonly RenderTarget _target;

	public Painter(RenderTarget target)
	{
		_target = target;
	}

	public void Draw(Drawable drawable)
	{
		_target.Draw(drawable);
	}

	public void Draw(Drawable drawable, RenderStates states)
	{
		_target.Draw(drawable, states);
	}
}