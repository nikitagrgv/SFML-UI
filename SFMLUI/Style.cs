using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public interface IMask
{
	// TODO: Allow doing masks not only with shader but with rendering geometry
	Shader GetMaskShader(
		float width,
		float height,
		float radiusBottomRight,
		float radiusTopRight,
		float radiusBottomLeft,
		float radiusTopLeft);

	bool ContainsPoint(
		Vector2f point,
		Vector2f size,
		float radiusBottomRight,
		float radiusTopRight,
		float radiusBottomLeft,
		float radiusTopLeft);
}

public class Style
{
	// For debug
	public bool EnableClipping { get; set; } = true;
	public bool EnableVisualizer { get; set; } = false;

	public IMask? Mask { get; set; }
}