namespace SFMLUI;

public interface IMaskPainter : IPainter
{
	enum MaskPaintMode
	{
		Add,
		Subtract,
	}

	MaskPaintMode CurrentMode { get; set; }
}