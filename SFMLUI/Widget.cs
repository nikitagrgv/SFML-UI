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

	protected override void Draw(RenderTarget target)
	{
		base.Draw(target);

		_shape.FillColor = _color;
		_shape.Size = new Vector2f(Width, Height);
		target.Draw(_shape);
	}
}