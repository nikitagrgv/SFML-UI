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

	protected override void Draw(RenderTarget target)
	{
		base.Draw(target);

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
				float sdBox(in vec2 p, in vec2 b)
				{
				    vec2 d = abs(p)-b;
				    return length(max(d,0.0)) + min(max(d.x,d.y),0.0);
				}

				void main()
				{
					float x = gl_TexCoord[0].x - 0.5;
					float y = gl_TexCoord[0].y - 0.5;
					vec2 center = vec2(x, y);
					vec2 size = vec2(0.2, 0.2);
					float v = sdBox(center, size);
					
					float r = sin(min(0, v) * 100) + min(0, v);
					float b = sin(max(0, v) * 100) + max(0, v);
				    if (v > 0.08f)
						discard;
					else
						gl_FragColor = vec4(r, 0, b, 1);
				}
				""";
			var vertexStream = new MemoryStream(Encoding.UTF8.GetBytes(vertex));
			var fragmentStream = new MemoryStream(Encoding.UTF8.GetBytes(fragment));
			_shader = new Shader(vertexStream, null, fragmentStream);
		}

		GL.Enable(EnableCap.StencilTest);
		GL.Clear(ClearBufferMask.StencilBufferBit);
		GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
		GL.StencilMask(0xFF);

		_shape.TextureRect = new IntRect(0, 0, 1, 1);
		
		RenderStates state = RenderStates.Default;
		state.Shader = _shader;
		target.Draw(_shape, state);

		GL.StencilMask(0x00);
		GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);

		_shape.FillColor = _color;
		_shape.Size = Size;
		target.Draw(_shape);

		GL.Disable(EnableCap.StencilTest);
	}
}