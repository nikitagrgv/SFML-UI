using Facebook.Yoga;
using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public class WidgetLabel : Widget
{
	private readonly Text _text = new(null, null, 10);

	public Font Font
	{
		get => _text.Font;
		set
		{
			_text.Font = value;
			Yoga.MarkDirty();
		}
	}

	public Color TextColor
	{
		get => _text.FillColor;
		set => _text.FillColor = value;
	}

	public string Text
	{
		get => _text.DisplayedString;
		set
		{
			_text.DisplayedString = value;
			Yoga.MarkDirty();
		}
	}

	public uint FontSize
	{
		get => _text.CharacterSize;
		set
		{
			_text.CharacterSize = value;
			Yoga.MarkDirty();
		}
	}

	public WidgetLabel()
	{
		Yoga.SetMeasureFunction(MeasureFunction);
	}

	protected override void Draw(RenderTarget target)
	{
		base.Draw(target);

		FloatRect bounds = _text.GetLocalBounds();
		_text.Position = -bounds.Position;
		target.Draw(_text);
	}

	public override bool AcceptsMouse(float x, float y)
	{
		return false;
	}

	protected override bool HandleMousePressEvent(MousePressEvent e)
	{
		base.HandleMousePressEvent(e);
		return false;
	}

	protected override bool HandleMouseReleaseEvent(MouseReleaseEvent e)
	{
		base.HandleMouseReleaseEvent(e);
		return false;
	}

	private static YogaSize MeasureFunction(
		YogaNode node,
		float width,
		YogaMeasureMode widthMode,
		float height,
		YogaMeasureMode heightMode)
	{
		WidgetLabel self = (WidgetLabel)node.Data;
		FloatRect bounds = self._text.GetLocalBounds();

		float naturalWidth = bounds.Width;
		float naturalHeight = bounds.Height;

		float retWidth = naturalWidth;
		float retHeight = naturalHeight;

		switch (widthMode)
		{
			case YogaMeasureMode.Undefined:
				break;
			case YogaMeasureMode.Exactly:
				retWidth = width;
				break;
			case YogaMeasureMode.AtMost:
				retWidth = MathF.Min(naturalWidth, width);
				break;
			default:
				break;
		}

		switch (heightMode)
		{
			case YogaMeasureMode.Undefined:
				break;
			case YogaMeasureMode.Exactly:
				retHeight = height;
				break;
			case YogaMeasureMode.AtMost:
				retHeight = MathF.Min(naturalHeight, height);
				break;
			default:
				break;
		}

		return new YogaSize { height = retHeight, width = retWidth };
	}
}