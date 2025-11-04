using System.Text;
using SFML.Graphics;
using SFML.Graphics.Glsl;
using SFML.System;

namespace SFMLUI;

public class RoundBorderMask : IMask
{
	private readonly Shader _shader;

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
	}

	public Shader GetMaskShader(
		float width,
		float height,
		float radiusBottomRight,
		float radiusTopRight,
		float radiusBottomLeft,
		float radiusTopLeft)
	{
		_shader.SetUniform("u_size", new Vec2(width, height));
		_shader.SetUniform("u_radius", new Vec4(
			radiusBottomRight,
			radiusTopRight,
			radiusBottomLeft,
			radiusTopLeft));
		return _shader;
	}

	public bool ContainsPoint(
		Vector2f point,
		Vector2f size,
		float radiusBottomRight,
		float radiusTopRight,
		float radiusBottomLeft,
		float radiusTopLeft)
	{
		Vector2f halfsize = size / 2;
		Vector2f relpos = point - halfsize;

		float borderRadius = 0;
		if (relpos.X >= 0f)
		{
			if (relpos.Y >= 0f)
			{
				borderRadius = radiusBottomRight;
			}
			else
			{
				borderRadius = radiusTopRight;
			}
		}
		else
		{
			if (relpos.Y >= 0f)
			{
				borderRadius = radiusBottomLeft;
			}
			else
			{
				borderRadius = radiusTopLeft;
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