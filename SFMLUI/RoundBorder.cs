using SFML.Graphics;
using SFML.Graphics.Glsl;

namespace SFMLUI;

public class RoundBorder : IBorder
{
	private readonly Shader _shader;
	private readonly RenderStates _state;
	private readonly RectangleShape _shape;

	public RoundBorder()
	{
		using Stream? vertexStream = ResourceStream.GetResourceStream("base_vertex.glsl");
		using Stream? fragmentStream = ResourceStream.GetResourceStream("round_border_fragment.glsl");
		_shader = new Shader(vertexStream, null, fragmentStream);
		_state = new RenderStates(BlendMode.Alpha, Transform.Identity, null, _shader);
		_shape = new RectangleShape
		{
			TextureRect = new IntRect(0, 0, 1, 1),
		};
	}

	public void DrawBorder(Widget widget, IPainter painter)
	{
		// TODO: Implement border rendering with different widths  
		// widget.GetBorders(out float left, out float top, out float right, out float bottom);
		// if (left <= 0 && top <= 0 && right <= 0 && bottom <= 0)
		// {
		// 	return;
		// }

		if (widget.BorderWidth <= 0)
		{
			return;
		}

		Color color;
		if (widget.IsFocused)
		{
			color = widget.BorderFocusColor;
		}
		else if (widget.IsHovered)
		{
			color = widget.BorderHoverColor;
		}
		else
		{
			color = widget.BorderColor;
		}

		_shader.SetUniform("u_size", widget.Size);
		_shader.SetUniform("u_radius", new Vec4(
			widget.BorderRadiusBottomRight,
			widget.BorderRadiusTopRight,
			widget.BorderRadiusBottomLeft,
			widget.BorderRadiusTopLeft));
		_shader.SetUniform("u_border_width", widget.BorderWidth);
		_shader.SetUniform("u_color", new Vec4(color));

		_shape.Size = widget.Size;
		painter.Draw(_shape, _state);
	}
}