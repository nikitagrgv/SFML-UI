using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public static class Extensions
{
	public static float GetBottom(this FloatRect rect)
	{
		return rect.Top + rect.Height;
	}

	public static float GetRight(this FloatRect rect)
	{
		return rect.Left + rect.Width;
	}

	public static FloatRect Extend(this FloatRect rect, FloatRect another)
	{
		float left = MathF.Min(rect.Left, another.Left);
		float top = MathF.Min(rect.Top, another.Top);
		float right = MathF.Max(rect.GetRight(), another.GetRight());
		float bottom = MathF.Max(rect.GetBottom(), another.GetBottom());
		float width = right - left;
		float height = bottom - top;
		return new FloatRect(left, top, width, height);
	}

	public static FloatRect Extend(this FloatRect rect, Vector2f point)
	{
		float left = MathF.Min(rect.Left, point.X);
		float top = MathF.Min(rect.Top, point.Y);
		float right = MathF.Max(rect.GetRight(), point.X);
		float bottom = MathF.Max(rect.GetBottom(), point.Y);
		float width = right - left;
		float height = bottom - top;
		return new FloatRect(left, top, width, height);
	}

	public static void SetSides(this ref FloatRect rect, float left, float top, float right, float bottom)
	{
		rect.Left = left;
		rect.Top = top;
		rect.Width = right - left;
		rect.Height = bottom - top;
	}
}