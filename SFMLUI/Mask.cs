using SFML.System;

namespace SFMLUI;

// TODO: Don't use Node here but pass some node info instead?
public interface IMask
{
	bool HasMask(Node node);

	// TODO: Add some functionality for combining masks with add/subtract
	void DrawMask(Node node, IMaskPainter painter);

	bool ContainsPoint(Node node, Vector2f point);
}