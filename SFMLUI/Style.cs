using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public interface IMask
{
	bool DrawMask(Node node, RenderTarget target);
	bool ContainsPoint(Node node, Vector2f point);
}

public class Style
{
	// For debug
	public bool EnableClipping { get; set; } = true;
	public bool EnableVisualizer { get; set; } = false;

	public IMask? DefaultMask { get; set; }
}