using SFML.Graphics;

namespace SFMLUI;

public interface IMaskStyle
{
	Shader GetMaskShader(
		float width,
		float height,
		float radiusBottomRight,
		float radiusTopRight,
		float radiusBottomLeft,
		float radiusTopLeft);
}

public class Style
{
	// For debug
	public bool EnableClipping { get; set; }
	public bool EnableVisualizer { get; set; }

	public IMaskStyle? Mask { get; set; }
}