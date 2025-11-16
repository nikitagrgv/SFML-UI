using SFML.System;

namespace SFMLUI;

// TODO: Don't use Widget here but pass some widget info instead?
public interface IMask
{
	bool HasMask(Widget widget);
	void DrawMask(Widget widget, IMaskPainter painter);
	bool ContainsPoint(Widget widget, Vector2f point);
}