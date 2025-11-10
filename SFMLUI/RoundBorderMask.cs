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
		using Stream? vertexStream = ResourceStream.GetResourceStream("round_border_mask_vertex.glsl");
		using Stream? fragmentStream = ResourceStream.GetResourceStream("round_border_mask_fragment.glsl");
		_shader = new Shader(vertexStream, null, fragmentStream);
		_state = new RenderStates(BlendMode.None, Transform.Identity, null, _shader);
		_shape = new RectangleShape
		{
			TextureRect = new IntRect(0, 0, 1, 1)
		};
	}

	public bool HasMask(Node node)
	{
		return node.BorderRadiusBottomRight > 0 ||
		       node.BorderRadiusTopRight > 0 ||
		       node.BorderRadiusBottomLeft > 0 ||
		       node.BorderRadiusTopLeft > 0;
	}

	public void DrawMask(Node node, IMaskPainter painter)
	{
		_shader.SetUniform("u_size", node.Size);
		_shader.SetUniform("u_radius", new Vec4(
			node.BorderRadiusBottomRight,
			node.BorderRadiusTopRight,
			node.BorderRadiusBottomLeft,
			node.BorderRadiusTopLeft));

		_shape.Size = node.Size;
		painter.Draw(_shape, _state);
	}

	public bool ContainsPoint(Node node, Vector2f point)
	{
		Vector2f halfsize = node.Size / 2;
		Vector2f relpos = point - halfsize;

		float borderRadius = 0;
		if (relpos.X >= 0f)
		{
			if (relpos.Y >= 0f)
			{
				borderRadius = node.BorderRadiusBottomRight;
			}
			else
			{
				borderRadius = node.BorderRadiusTopRight;
			}
		}
		else
		{
			if (relpos.Y >= 0f)
			{
				borderRadius = node.BorderRadiusBottomLeft;
			}
			else
			{
				borderRadius = node.BorderRadiusTopLeft;
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