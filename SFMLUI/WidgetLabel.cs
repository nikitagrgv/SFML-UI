using Facebook.Yoga;
using SFML.Graphics;

namespace SFMLUI;

public class WidgetLabel : Widget
{
	private readonly Text _text = new(null, null, 10);
	private WrapMode _wrap = WrapMode.NoWrap;
	private string _textString = "";

	public enum WrapMode
	{
		NoWrap,
		CharWrap,
		WordWrap,
	}

	public Color TextColor
	{
		get => _text.FillColor;
		set => _text.FillColor = value;
	}

	public string Text
	{
		get => _textString;
		set
		{
			_textString = value;
			UpdateString();
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

	public WrapMode Wrap
	{
		get => _wrap;
		set
		{
			if (value == _wrap)
				return;
			_wrap = value;
			OuterYoga.MarkDirty();
		}
	}

	public WidgetLabel()
	{
		OuterYoga.SetMeasureFunction(MeasureFunction);
	}

	protected override bool HandleStyleChangeEvent(StyleChangeEvent e)
	{
		UpdateFont();
		return base.HandleStyleChangeEvent(e);
	}

	protected override void Draw(IPainter painter)
	{
		base.Draw(painter);

		FloatRect bounds = _text.GetLocalBounds();
		_text.Position = -bounds.Position;
		painter.Draw(_text);
	}

	private void UpdateFont()
	{
		_text.Font = Style?.Font;
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

	private void UpdateString()
	{
		_text.DisplayedString = _textString;
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

		return new YogaSize
		{
			height = retHeight,
			width = retWidth
		};
	}
}