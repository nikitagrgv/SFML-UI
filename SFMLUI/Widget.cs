using System.Text;
using OpenTK.Graphics.OpenGL.Compatibility;
using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public class Widget : Node
{
	private readonly RectangleShape _shape = new();

	private Color _color = Color.White;

	public Color FillColor
	{
		get => _color;
		set => _color = value;
	}

	private static Shader? _shader = null;

	protected override void Draw(RenderTarget target, DrawState drawState)
	{
		base.Draw(target, drawState);

		if (_shader == null)
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
				float sdRoundedBox(in vec2 p, in vec2 b, in vec4 r )
				{
				    r.xy = (p.x>0.0)?r.xy : r.zw;
				    r.x  = (p.y>0.0)?r.x  : r.y;
				    vec2 q = abs(p)-b+r.x;
				    return min(max(q.x,q.y),0.0) + length(max(q,0.0)) - r.x;
				}
				void main()
				{
					float x = gl_TexCoord[0].x - 0.5;
					float y = gl_TexCoord[0].y - 0.5;
					vec2 center = vec2(x, y);
					vec2 size = vec2(0.5, 0.5);
					vec4 radius = vec4(0.1, 0.1, 0.1, 0.1);
					float v = sdRoundedBox(center, size, radius);

				    if (v > 0)
						discard;
					gl_FragColor = vec4(0, 0, 0, 0);
				}
				""";
			var vertexStream = new MemoryStream(Encoding.UTF8.GetBytes(vertex));
			var fragmentStream = new MemoryStream(Encoding.UTF8.GetBytes(fragment));
			_shader = new Shader(vertexStream, null, fragmentStream);
		}

		GL.Enable(EnableCap.StencilTest);
		GL.StencilMask(0xFF);
		// GL.Clear(ClearBufferMask.StencilBufferBit);
		GL.StencilFunc(StencilFunction.Always, 0, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);

		_shape.TextureRect = new IntRect(0, 0, 1, 1);
		
		RenderStates state = RenderStates.Default;
		state.Shader = _shader;
		
		target.Draw(_shape, state);

		GL.StencilMask(0x00);
		GL.StencilFunc(StencilFunction.Less, 0, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

		_shape.FillColor = _color;
		_shape.Size = Size;
		target.Draw(_shape);

		GL.Disable(EnableCap.StencilTest);
	}
}