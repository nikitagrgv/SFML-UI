using SFML.Graphics;

namespace SFMLUI;

// TODO: Implement overriding of style parameters
public class Style
{
	// For debug
	public bool EnableClipping { get; set; } = true;
	public bool EnableVisualizer { get; set; } = false;

	public Font? Font { get; set; }
	public IMask? Mask { get; set; }
	public IBorder? Border { get; set; }
}