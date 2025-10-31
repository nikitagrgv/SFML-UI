using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public class ScrollAreaWidget : Widget
{
	private float _scrollX;
	private float _scrollY;

	private bool _hasScrollbarX;
	private bool _hasScrollbarY;

	private float _scrollbarThickness = 8f;

	private readonly RectangleShape _shape = new();

	public Color ScrollbarColor { get; set; } = new(44, 44, 44);
	public Color HandleColor { get; set; } = new(159, 159, 159);

	public float ScrollMultiplier { get; set; } = 20f;

	public float ScrollbarThickness
	{
		get => _scrollbarThickness;
		set
		{
			_scrollbarThickness = value;
			UpdateScrollbars();
		}
	}

	protected override bool HandleLayoutChangeEvent(LayoutChangeEvent e)
	{
		UpdateScrollbars();
		AdjustScroll();
		return base.HandleLayoutChangeEvent(e);
	}

	protected override bool HandleMousePressEvent(MousePressEvent e)
	{
		return base.HandleMousePressEvent(e);
	}

	protected override bool HasDrawAfterChildren()
	{
		return true;
	}

	protected override void DrawAfterChildren(RenderTarget target)
	{
		base.DrawAfterChildren(target);

		if (GetYScrollbarRect(out FloatRect yScrollbarRect))
		{
			_shape.FillColor = ScrollbarColor;
			_shape.Position = yScrollbarRect.Position;
			_shape.Size = yScrollbarRect.Size;
			target.Draw(_shape);
		}

		if (GetXScrollbarRect(out FloatRect xScrollbarRect))
		{
			_shape.FillColor = ScrollbarColor;
			_shape.Position = xScrollbarRect.Position;
			_shape.Size = xScrollbarRect.Size;
			target.Draw(_shape);
		}

		if (GetCrossRect(out FloatRect crossRect))
		{
			_shape.FillColor = ScrollbarColor;
			_shape.Position = crossRect.Position;
			_shape.Size = crossRect.Size;
			target.Draw(_shape);
		}

		if (_hasScrollbarY)
		{
			_shape.FillColor = HandleColor;
			float top = MapContentYPosToScrollbarPos(_scrollY);
			float bottom = MapContentYPosToScrollbarPos(_scrollY + Height);
			_shape.Position = new Vector2f(Width - ScrollbarThickness, top);
			_shape.Size = new Vector2f(ScrollbarThickness, bottom - top);
			target.Draw(_shape);
		}

		if (_hasScrollbarX)
		{
			_shape.FillColor = HandleColor;
			float left = MapContentXPosToScrollbarPos(_scrollX);
			float right = MapContentXPosToScrollbarPos(_scrollX + Width);
			_shape.Position = new Vector2f(left, Height - ScrollbarThickness);
			_shape.Size = new Vector2f(right - left, ScrollbarThickness);
			target.Draw(_shape);
		}
	}

	protected override bool HandleMouseScrollEvent(MouseScrollEvent e)
	{
		if ((e.Modifiers & Modifier.Control) != 0 || (e.Modifiers & Modifier.Alt) != 0)
		{
			return false;
		}

		_scrollX -= e.ScrollX * ScrollMultiplier;
		_scrollY -= e.ScrollY * ScrollMultiplier;
		AdjustScroll();

		// TODO: Don't notify parents? Or ok?
		Node? cur = this;
		while (cur != null)
		{
			cur.Yoga.MarkHasNewLayout();
			cur = cur.Parent;
		}

		foreach (Node node in Children)
		{
			node.Yoga.MarkHasNewLayout();
		}

		return true;
	}

	protected override void UpdateChildLayout(Node child)
	{
		child.UpdateLayout(-_scrollX, -_scrollY);
	}

	private void UpdateScrollbars()
	{
		bool prevHasScrollbarX = _hasScrollbarX;
		_hasScrollbarX = GetTotalChildrenWidth() > Width;
		if (prevHasScrollbarX != _hasScrollbarX)
		{
			Yoga.PaddingBottom = _hasScrollbarX ? ScrollbarThickness : 0;
		}

		bool prevHasScrollbarY = _hasScrollbarY;
		_hasScrollbarY = GetTotalChildrenHeight() > Height;
		if (prevHasScrollbarY != _hasScrollbarY)
		{
			Yoga.PaddingRight = _hasScrollbarY ? ScrollbarThickness : 0;
		}
	}

	private void AdjustScroll()
	{
		float maxScrollY = GetTotalChildrenHeight() - Yoga.LayoutHeight;
		float maxScrollX = GetTotalChildrenWidth() - Yoga.LayoutWidth;

		_scrollX = MathF.Min(maxScrollX, _scrollX);
		_scrollY = MathF.Min(maxScrollY, _scrollY);

		_scrollX = MathF.Max(0, _scrollX);
		_scrollY = MathF.Max(0, _scrollY);
	}

	private bool GetYScrollbarRect(out FloatRect rect)
	{
		if (!_hasScrollbarY)
		{
			rect = new FloatRect();
			return false;
		}

		Vector2f pos = new(Width - ScrollbarThickness, 0);
		float availableHeight = Height;
		if (_hasScrollbarX)
		{
			availableHeight -= ScrollbarThickness;
		}

		Vector2f size = new(ScrollbarThickness, availableHeight);
		rect = new FloatRect(pos, size);
		return true;
	}

	private bool GetXScrollbarRect(out FloatRect rect)
	{
		if (!_hasScrollbarX)
		{
			rect = new FloatRect();
			return false;
		}

		Vector2f pos = new(0, Height - ScrollbarThickness);
		float availableWidth = Width;
		if (_hasScrollbarY)
		{
			availableWidth -= ScrollbarThickness;
		}

		Vector2f size = new(availableWidth, ScrollbarThickness);
		rect = new FloatRect(pos, size);
		return true;
	}

	private bool GetCrossRect(out FloatRect rect)
	{
		if (!_hasScrollbarX || !_hasScrollbarY)
		{
			rect = new FloatRect();
			return false;
		}

		Vector2f pos = new(Width - ScrollbarThickness, Height - ScrollbarThickness);
		Vector2f size = new(ScrollbarThickness, ScrollbarThickness);
		rect = new FloatRect(pos, size);
		return true;
	}

	private float MapContentYPosToScrollbarPos(float y)
	{
		float total = GetTotalChildrenHeight();
		float normalized = y / total;
		float availableHeight = Height;
		if (_hasScrollbarX)
		{
			availableHeight -= ScrollbarThickness;
		}

		float mapped = normalized * availableHeight;
		return mapped;
	}

	private float MapContentXPosToScrollbarPos(float x)
	{
		float total = GetTotalChildrenWidth();
		float normalized = x / total;
		float availableWidth = Width;
		if (_hasScrollbarY)
		{
			availableWidth -= ScrollbarThickness;
		}

		float mapped = normalized * availableWidth;
		return mapped;
	}

	private float GetTotalChildrenHeight()
	{
		float maxHeight = 0;
		foreach (Node node in Children)
		{
			float height = node.Yoga.LayoutHeight + node.Yoga.LayoutY;
			if (height > maxHeight)
			{
				maxHeight = height;
			}
		}

		return maxHeight;
	}

	private float GetTotalChildrenWidth()
	{
		float maxWidth = 0;
		foreach (Node node in Children)
		{
			float width = node.Yoga.LayoutWidth + node.Yoga.LayoutX;
			if (width > maxWidth)
			{
				maxWidth = width;
			}
		}

		return maxWidth;
	}
}