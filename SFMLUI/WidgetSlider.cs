using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public class WidgetSlider : Widget
{
	private float _minValue = 0;
	private float _maxValue = 0;
	private float _normalizedValue;
	private float _lineThickness = 5f;

	private bool _handleHovered = false;
	private bool _pressed = false;
	public bool IsPressed => _pressed;

	public float ScrollMultiplier { get; set; } = 0.05f;

	public float PressedHandleSizeMultiplier { get; set; } = 0.85f;

	// TODO: Use rectangle with SDF?
	private readonly CircleShape _handleShape = new();

	private readonly RectangleShape _lineBefore = new()
	{
		FillColor = new Color(255, 127, 40),
	};

	private readonly RectangleShape _lineAfter = new()
	{
		FillColor = new Color(170, 170, 170),
	};

	public delegate void ValueChangedDelegate(float newValue, float oldValue);

	public event ValueChangedDelegate? ValueChanged;

	public Color LineBeforeColor
	{
		get => _lineBefore.FillColor;
		set => _lineBefore.FillColor = value;
	}

	public Color LineAfterColor
	{
		get => _lineAfter.FillColor;
		set => _lineAfter.FillColor = value;
	}

	public Color HandleColor { get; set; } = new(190, 190, 190);
	public Color ActiveHandleColor { get; set; } = Color.White;

	public float NormalizedValue
	{
		get => _normalizedValue;
		set
		{
			value = MathF.Max(value, 0);
			value = MathF.Min(value, 1);

			if (value == _normalizedValue)
			{
				return;
			}

			float oldValue = Value;
			_normalizedValue = value;
			float newValue = Value;
			ValueChanged?.Invoke(newValue, oldValue);
		}
	}

	public float MinValue
	{
		get => _minValue;
		set
		{
			if (value == _minValue)
			{
				return;
			}

			float oldValue = ToScaledValue(_normalizedValue);
			_minValue = value;
			Value = oldValue;
		}
	}

	public float MaxValue
	{
		get => _maxValue;
		set
		{
			if (value == _maxValue)
			{
				return;
			}

			float oldValue = ToScaledValue(_normalizedValue);
			_maxValue = value;
			Value = oldValue;
		}
	}

	public float Value
	{
		get => ToScaledValue(NormalizedValue);
		set => NormalizedValue = ToNormalizedValue(value);
	}

	public float ToScaledValue(float normalized)
	{
		return _minValue + (_maxValue - _minValue) * normalized;
	}

	public float ToNormalizedValue(float scaled)
	{
		if (_maxValue == _minValue)
		{
			return 0;
		}

		return (scaled - _minValue) / (_maxValue - _minValue);
	}

	protected override void Draw(IPainter painter)
	{
		base.Draw(painter);

		float beginPos = MapToPosition(0f);
		float endPos = MapToPosition(1f);
		float valuePos = MapToPosition(_normalizedValue);

		_lineBefore.Size = new Vector2f(valuePos - beginPos, _lineThickness);
		_lineBefore.Origin = new Vector2f(0, _lineThickness / 2);
		_lineBefore.Position = new Vector2f(beginPos, Height / 2);
		painter.Draw(_lineBefore);

		_lineAfter.Size = new Vector2f(endPos - valuePos, _lineThickness);
		_lineAfter.Origin = new Vector2f(0, _lineThickness / 2);
		_lineAfter.Position = new Vector2f(valuePos, Height / 2);
		painter.Draw(_lineAfter);

		float handleRadius = GetHandleSize(_pressed) / 2f;
		_handleShape.Radius = handleRadius;
		_handleShape.Origin = new Vector2f(handleRadius, handleRadius);
		_handleShape.Position = new Vector2f(valuePos, Height / 2);

		bool handleActive = _pressed || _handleHovered;
		_handleShape.FillColor = handleActive ? ActiveHandleColor : HandleColor;
		painter.Draw(_handleShape);
	}

	protected override bool HandleMousePressEvent(MousePressEvent e)
	{
		if (e.Button == MouseButton.Left)
		{
			_pressed = true;

			float pos = e.LocalX;
			float value = MapToValue(pos);
			NormalizedValue = value;
		}

		return base.HandleMousePressEvent(e);
	}

	protected override bool HandleMouseReleaseEvent(MouseReleaseEvent e)
	{
		if (e.Button == MouseButton.Left)
		{
			_pressed = false;
		}

		return base.HandleMouseReleaseEvent(e);
	}

	protected override bool HandleMouseScrollEvent(MouseScrollEvent e)
	{
		if (IsFocused)
		{
			NormalizedValue += e.ScrollY * ScrollMultiplier;
			return true;
		}

		return base.HandleMouseScrollEvent(e);
	}

	protected override bool HandleMouseMoveEvent(MouseMoveEvent e)
	{
		if (_pressed)
		{
			float pos = e.LocalX;
			float value = MapToValue(pos);
			NormalizedValue = value;
		}

		float handleX = MapToPosition(_normalizedValue);
		float handleY = Height / 2;
		Vector2f handlePos = new(handleX, handleY);
		Vector2f diff = e.LocalPos - handlePos;

		float handleRadius = GetHandleSize(pressed: false) / 2f;
		_handleHovered = diff.Length2() <= handleRadius * handleRadius;

		return base.HandleMouseMoveEvent(e);
	}

	protected override bool HandleUnhoverEvent(UnhoverEvent e)
	{
		_handleHovered = false;
		return base.HandleUnhoverEvent(e);
	}

	protected virtual float GetHandleSize(bool pressed)
	{
		if (pressed)
		{
			return Height * PressedHandleSizeMultiplier;
		}

		return Height;
	}

	protected float MapToValue(float x)
	{
		float handleSize = GetHandleSize(pressed: false);
		float handleRadius = handleSize / 2;
		float lineSize = Width - handleSize;
		float value = (x - handleRadius) / lineSize;
		return value;
	}

	protected float MapToPosition(float value)
	{
		float handleSize = GetHandleSize(pressed: false);
		float handleRadius = handleSize / 2;
		float lineSize = Width - handleSize;
		float pos = value * lineSize + handleRadius;
		return pos;
	}
}