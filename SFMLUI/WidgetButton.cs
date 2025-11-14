using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SFMLUI;

public class WidgetButton : Widget
{
	// TODO: Reuse widget's shape
	private readonly RectangleShape _shape = new();

	enum PressState
	{
		NotPressed,
		PressedByMouse,
		PressedBySpace,
		PressedByEnter,
	}

	private PressState _pressState = PressState.NotPressed;

	private Color _hoverColor = new(235, 239, 247, 255);
	private Color _pressColor = new(201, 224, 247, 255);

	public event Action? Clicked;

	public bool IsPressed => _pressState != PressState.NotPressed;

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

	protected override void Draw(IPainter painter)
	{
		base.Draw(painter);

		_shape.Size = new Vector2f(Width, Height);

		switch (_pressState)
		{
			case PressState.NotPressed:
				if (IsHovered)
				{
					_shape.FillColor = HoverColor;
					painter.Draw(_shape);
				}

				break;
			case PressState.PressedByMouse:
				if (IsHovered)
				{
					_shape.FillColor = PressColor;
					painter.Draw(_shape);
				}

				break;
			case PressState.PressedBySpace:
			case PressState.PressedByEnter:
				_shape.FillColor = PressColor;
				painter.Draw(_shape);

				break;
			default:
				break;
		}
	}

	protected override bool HandleMousePressEvent(MousePressEvent e)
	{
		if (!IsPressed && e.Button == MouseButton.Left)
		{
			_pressState = PressState.PressedByMouse;
		}

		return base.HandleMousePressEvent(e);
	}

	protected override bool HandleMouseReleaseEvent(MouseReleaseEvent e)
	{
		if (_pressState == PressState.PressedByMouse && e.Button == MouseButton.Left)
		{
			_pressState = PressState.NotPressed;
			if (IsHovered)
			{
				Clicked?.Invoke();
			}
		}

		return base.HandleMouseReleaseEvent(e);
	}

	protected override bool HandleKeyPressEvent(KeyPressEvent e)
	{
		bool accepted = false;
		if (!IsPressed && !e.Repeat)
		{
			if (e.Key == Keyboard.Key.Space)
			{
				_pressState = PressState.PressedBySpace;
				accepted = true;
			}
			else if (e.Key == Keyboard.Key.Enter)
			{
				_pressState = PressState.PressedByEnter;
				accepted = true;
			}
		}

		if (IsPressed && !e.Repeat && e.Key == Keyboard.Key.Escape)
		{
			_pressState = PressState.NotPressed;
			accepted = true;
		}

		if (accepted)
		{
			return true;
		}

		return base.HandleKeyPressEvent(e);
	}

	protected override bool HandleKeyReleaseEvent(KeyReleaseEvent e)
	{
		bool accepted = false;
		if (_pressState == PressState.PressedByEnter && e.Key == Keyboard.Key.Enter)
		{
			_pressState = PressState.NotPressed;
			accepted = true;
			Clicked?.Invoke();
		}

		if (_pressState == PressState.PressedBySpace && e.Key == Keyboard.Key.Space)
		{
			_pressState = PressState.NotPressed;
			accepted = true;
			Clicked?.Invoke();
		}

		if (accepted)
		{
			return true;
		}

		return base.HandleKeyReleaseEvent(e);
	}

	protected override bool HandleUnfocusEvent(UnfocusEvent e)
	{
		_pressState = PressState.NotPressed;
		return base.HandleUnfocusEvent(e);
	}
}