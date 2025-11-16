using Facebook.Yoga;

namespace SFMLUI;

public struct CalculatedGeometry : IEquatable<CalculatedGeometry>
{
	public static CalculatedGeometry FromYoga(YogaNode node)
	{
		CalculatedGeometry geometry = new()
		{
			Width = node.LayoutWidth,
			Height = node.LayoutHeight,
			X = node.LayoutX,
			Y = node.LayoutY,
			LayoutMarginLeft = node.LayoutMarginLeft,
			LayoutMarginRight = node.LayoutMarginRight,
			LayoutMarginTop = node.LayoutMarginTop,
			LayoutMarginBottom = node.LayoutMarginBottom,
		};
		return geometry;
	}

	public float Width { get; set; }
	public float Height { get; set; }
	public float X { get; set; }
	public float Y { get; set; }
	public float LayoutMarginLeft { get; set; }
	public float LayoutMarginRight { get; set; }
	public float LayoutMarginTop { get; set; }
	public float LayoutMarginBottom { get; set; }

	public bool Equals(CalculatedGeometry other)
	{
		return Width.Equals(other.Width) &&
		       Height.Equals(other.Height) &&
		       X.Equals(other.X) &&
		       Y.Equals(other.Y) &&
		       LayoutMarginLeft.Equals(other.LayoutMarginLeft) &&
		       LayoutMarginRight.Equals(other.LayoutMarginRight) &&
		       LayoutMarginTop.Equals(other.LayoutMarginTop) &&
		       LayoutMarginBottom.Equals(other.LayoutMarginBottom);
	}

	public override bool Equals(object? obj)
	{
		return obj is CalculatedGeometry other && Equals(other);
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Width, Height, X, Y, LayoutMarginLeft, LayoutMarginRight, LayoutMarginTop,
			LayoutMarginBottom);
	}
}