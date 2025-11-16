using Facebook.Yoga;
using SFML.Graphics;

namespace SFMLUI;

public class WidgetLabel : Widget
{
	private readonly Text _text = new(null, null, 10);
	private Font? _customFont;

	public Font? CustomFont
	{
		get => _customFont;
		set
		{
			_customFont = value;
			OuterYoga.MarkDirty();
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
			OuterYoga.MarkDirty();
		}
	}

	public uint FontSize
	{
		get => _text.CharacterSize;
		set
		{
			_text.CharacterSize = value;
			OuterYoga.MarkDirty();
		}
	}

	public WidgetLabel()
	{
		OuterYoga.SetMeasureFunction(MeasureFunction);
	}

	protected override void Draw(IPainter painter)
	{
		base.Draw(painter);

		EnsureFont();

		FloatRect bounds = _text.GetLocalBounds();
		_text.Position = -bounds.Position;
		painter.Draw(_text);
	}

	private void EnsureFont()
	{
		if (CustomFont != null)
		{
			_text.Font = CustomFont;
		}
		else if (Style is { Font: { } font })
		{
			_text.Font = font;
		}
		else
		{
			_text.Font = null;
		}
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

		self.EnsureFont();

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

		return new YogaSize
		{
			height = retHeight,
			width = retWidth
		};
	}
}