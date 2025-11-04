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
		const string vertex =
			"""
			void main()
			{
			    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
			    gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;
			    gl_FrontColor = gl_Color;
			}
			""";
		const string fragment =
			"""
			uniform vec2 u_size;
			uniform vec4 u_radius;

			float sdRoundedBox(in vec2 p, in vec2 b, in vec4 r )
			{
			    r.xy = (p.x>0.0)?r.xy : r.zw;
			    r.x  = (p.y>0.0)?r.x  : r.y;
			    vec2 q = abs(p)-b+r.x;
			    return min(max(q.x,q.y),0.0) + length(max(q,0.0)) - r.x;
			}
			void main()
			{
				float x = (gl_TexCoord[0].x - 0.5) * u_size.x;
				float y = (gl_TexCoord[0].y - 0.5) * u_size.y;
				vec2 center = vec2(x, y);
				vec2 size = vec2(0.5 * u_size.x, 0.5 * u_size.y);
				float v = sdRoundedBox(center, size, u_radius);

			    if (v > 0)
					discard;
			}
			""";
		MemoryStream vertexStream = new(Encoding.UTF8.GetBytes(vertex));
		MemoryStream fragmentStream = new(Encoding.UTF8.GetBytes(fragment));
		_shader = new Shader(vertexStream, null, fragmentStream);
		// TODO# NONE?
		_state = new RenderStates(BlendMode.Alpha, Transform.Identity, null, _shader);
		_shape = new RectangleShape
		{
			TextureRect = new IntRect(0, 0, 1, 1)
		};
	}

	public void DrawMask(Node node, RenderTarget target)
	{
		_shader.SetUniform("u_size", node.Size);
		_shader.SetUniform("u_radius", new Vec4(
			node.BorderRadiusBottomRight,
			node.BorderRadiusTopRight,
			node.BorderRadiusBottomLeft,
			node.BorderRadiusTopLeft));

		_shape.Size = node.Size;
		target.Draw(_shape, _state);
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