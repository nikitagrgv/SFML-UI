using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public class Widget : Node
{
	private readonly RectangleShape _shape = new();

	private Color _color = Color.White;

	public Color FillColor
	{
		get => _color;
		set => _color = value;
	}

	protected override void Draw(RenderTarget target, DrawState drawState)
	{
		base.Draw(target, drawState);

		_shape.FillColor = _color;
		_shape.Size = Size;
		target.Draw(_shape);
	}
}