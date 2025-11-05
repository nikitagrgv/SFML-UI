using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public interface IMask
{
	// TODO: Don't use Node here but pass some node info instead?
	bool HasMask(Node node);
	void DrawMask(Node node, RenderTarget target);
	bool ContainsPoint(Node node, Vector2f point);
}

// TODO: Implement overriding of style parameters
public class Style
{
	// For debug
	public bool EnableClipping { get; set; } = true;
	public bool EnableVisualizer { get; set; } = false;

	public IMask? DefaultMask { get; set; }
}