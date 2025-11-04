using System.Text;
using SFML.Graphics;

namespace SFMLUI;

public class RoundBorderMaskStyle : IMaskStyle
{
	private Shader _shader;

	public RoundBorderMaskStyle()
	{
		string vertex =
			"""
			void main()
			{
			    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;
			    gl_TexCoord[0] = gl_TextureMatrix[0] * gl_MultiTexCoord0;
			    gl_FrontColor = gl_Color;
			}
			""";
		string fragment =
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
	}
}