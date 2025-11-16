using System.Text;
using SFML.Graphics;
using SFML.Graphics.Glsl;
using SFML.System;

namespace SFMLUI;

public class RoundBorderMask : IMask
{
	private readonly Shader _shader;
	private readonly RenderStates _state;
	private readonly RectangleShape _shape;

	public RoundBorderMask()
	{
		using Stream? vertexStream = ResourceStream.GetResourceStream("base_vertex.glsl");
		using Stream? fragmentStream = ResourceStream.GetResourceStream("round_border_mask_fragment.glsl");
		_shader = new Shader(vertexStream, null, fragmentStream);
		_state = new RenderStates(BlendMode.None, Transform.Identity, null, _shader);
		_shape = new RectangleShape
		{
			TextureRect = new IntRect(0, 0, 1, 1),
		};
	}

	public bool HasMask(Widget widget)
	{
		return widget.BorderRadiusBottomRight > 0 ||
		       widget.BorderRadiusTopRight > 0 ||
		       widget.BorderRadiusBottomLeft > 0 ||
		       widget.BorderRadiusTopLeft > 0;
	}

	public void DrawMask(Widget widget, IMaskPainter painter)
	{
		_shader.SetUniform("u_size", widget.Size);
		_shader.SetUniform("u_radius", new Vec4(
			widget.BorderRadiusBottomRight,
			widget.BorderRadiusTopRight,
			widget.BorderRadiusBottomLeft,
			widget.BorderRadiusTopLeft));

		_shape.Size = widget.Size;
		painter.Draw(_shape, _state);
	}

	public bool ContainsPoint(Widget widget, Vector2f point)
	{
		Vector2f halfsize = widget.Size / 2;
		Vector2f relpos = point - halfsize;

		float borderRadius = 0;
		if (relpos.X >= 0f)
		{
			if (relpos.Y >= 0f)
			{
				borderRadius = widget.BorderRadiusBottomRight;
			}
			else
			{
				borderRadius = widget.BorderRadiusTopRight;
			}
		}
		else
		{
			if (relpos.Y >= 0f)
			{
				borderRadius = widget.BorderRadiusBottomLeft;
			}
			else
			{
				borderRadius = widget.BorderRadiusTopLeft;
			}
		}

		if (borderRadius == 0)
		{
			return true;
		}

		Vector2f q = relpos.Abs() - halfsize + new Vector2f(borderRadius, borderRadius);
		if (q.X < 0f || q.Y < 0f)
		{
			return true;
		}

		float length2 = q.Length2();
		return length2 < borderRadius * borderRadius;
	}
}