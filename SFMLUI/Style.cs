using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

// TODO: Implement overriding of style parameters
public class Style
{
	// For debug
	public bool EnableClipping { get; set; } = true;
	public bool EnableVisualizer { get; set; } = false;

	public IMask? Mask { get; set; }
}