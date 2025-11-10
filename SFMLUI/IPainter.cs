using SFML.Graphics;

namespace SFMLUI;

public interface IPainter
{
	void Draw(Drawable drawable);
	void Draw(Drawable drawable, RenderStates states);
}