using System.Text;
using Facebook.Yoga;
using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public class WidgetLabel : Widget
{
	private readonly Text _text = new(null, null, 10);
	private List<Text> _texts = new();
	private TextWrapMode _textWrap = TextWrapMode.NoWrap;
	private string _textString = "";

	public enum TextWrapMode
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

	public TextWrapMode TextWrap
	{
		get => _textWrap;
		set
		{
			if (value == _textWrap)
				return;
			_textWrap = value;
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

		if (_texts.Count == 0)
			return;

		Text first = _texts[0];
		float offset = -first.GetLocalBounds().Position.Y;

		float lineSpacing = _text.Font.GetLineSpacing(_text.CharacterSize);
		float curPos = offset;
		foreach (Text text in _texts)
		{
			text.Position = new Vector2f(0, curPos);
			painter.Draw(text);
			curPos += lineSpacing;
		}
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

		List<Text> texts = [];
		Font? font = self._text.Font;
		if (font != null)
		{
			uint fontSize = self._text.CharacterSize;
			float lineSpacing = font.GetLineSpacing(fontSize);

			string text = self._textString;
			float lineWidth = 0;

			StringBuilder curText = new();
			for (int i = 0; i < text.Length; i++)
			{
				float kerning = 0;
				char cur = text[i];
				if (i > 0)
				{
					char prev = text[i - 1];
					kerning = font.GetKerning(prev, cur, fontSize);
				}

				Glyph glyph = font.GetGlyph(cur, fontSize, bold: false, outlineThickness: 0);
				float advance = glyph.Advance;

				if (lineWidth + kerning + advance > width && curText.Length > 0)
				{
					string lineString = curText.ToString();
					texts.Add(new Text(lineString, font, fontSize));

					curText.Clear();
					curText.Append(cur);
					retHeight += lineSpacing;
					lineWidth = advance;
				}
				else
				{
					curText.Append(cur);
					lineWidth += kerning + advance;
				}
			}

			if (curText.Length > 0)
			{
				string lineString = curText.ToString();
				texts.Add(new Text(lineString, font, fontSize));
			}
		}

		self._texts = texts;

		switch (widthMode)
		{
			case YogaMeasureMode.Undefined:
				break;
			case YogaMeasureMode.Exactly:
				retWidth = width;
				break;
			case YogaMeasureMode.AtMost:
				retWidth = MathF.Min(retWidth, width);
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
				retHeight = MathF.Min(retHeight, height);
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