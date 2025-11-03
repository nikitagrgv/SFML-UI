using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public class WidgetButton : Widget
{
	// TODO: Reuse widget's shape
	private readonly RectangleShape _shape = new();

	private bool _pressed = false;

	private Color _hoverColor = new(235, 239, 247, 255);
	private Color _pressColor = new(201, 224, 247, 255);

	public event Action? Clicked;

	public bool IsPressed => _pressed;

	public Color HoverColor
	{
		get => _hoverColor;
		set => _hoverColor = value;
	}

	public Color PressColor
	{
		get => _pressColor;
		set => _pressColor = value;
	}

	protected override void Draw(RenderTarget target)
	{
		base.Draw(target);

		if (IsHovered)
		{
			_shape.FillColor = IsPressed ? PressColor : HoverColor;
			_shape.Size = new Vector2f(Width, Height);
			target.Draw(_shape);
		}
	}

	protected override bool HandleMousePressEvent(MousePressEvent e)
	{
		if (e.Button == MouseButton.Left)
		{
			_pressed = true;
		}

		return base.HandleMousePressEvent(e);
	}

	protected override bool HandleMouseReleaseEvent(MouseReleaseEvent e)
	{
		if (e.Button == MouseButton.Left)
		{
			_pressed = false;
			if (IsHovered)
			{
				Clicked?.Invoke();
			}
		}

		return base.HandleMouseReleaseEvent(e);
	}
}