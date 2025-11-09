using SFML.Graphics;

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

	protected override void Draw(IPainter painter)
	{
		base.Draw(painter);

		_shape.FillColor = _color;
		_shape.Size = Size;
		painter.Draw(_shape);
	}
}