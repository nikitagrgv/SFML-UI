using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public class ScrollAreaWidget : Widget
{
	[Flags]
	private enum ScrollDirection
	{
		None = 0,
		Vertical = 1 << 0,
		Horizontal = 1 << 1,
		Both = Vertical | Horizontal
	}

	private float _scrollX;
	private float _scrollY;

	private bool _hasScrollbarX;
	private bool _hasScrollbarY;

	private FloatRect _totalChildrenRect;

	private float _scrollbarThickness = 8f;

	private readonly RectangleShape _shape = new();

	private ScrollDirection _hoveredHandle = ScrollDirection.None;
	private ScrollDirection _pressedScroll = ScrollDirection.None;
	private float _pressedScrollOffset = 0f;

	public Color ScrollbarColor { get; set; } = new(44, 44, 44);
	public Color HandleColor { get; set; } = new(159, 159, 159);
	public Color HoveredHandleColor { get; set; } = new(200, 200, 200);

	public float ScrollMultiplier { get; set; } = 20f;

	public float ScrollbarThickness
	{
		get => _scrollbarThickness;
		set
		{
			_scrollbarThickness = value;
			UpdateScrollbarsVisibility();
		}
	}

	protected override bool HandleLayoutChangeEvent(LayoutChangeEvent e)
	{
		_totalChildrenRect = GetTotalChildrenRect();
		float left = MathF.Min(0, _totalChildrenRect.Left);
		float top = MathF.Min(0, _totalChildrenRect.Top);
		_totalChildrenRect.SetSides(left, top, _totalChildrenRect.GetRight(), _totalChildrenRect.GetBottom());

		UpdateScrollbarsVisibility();
		AdjustScroll();
		return base.HandleLayoutChangeEvent(e);
	}

	protected override bool HandleMousePressEvent(MousePressEvent e)
	{
		if (e.Button != MouseButton.Left)
		{
			return base.HandleMousePressEvent(e);
		}

		if (GetHandleYRect(out FloatRect handleYRect))
		{
			if (handleYRect.Contains(e.LocalX, e.LocalY))
			{
				_pressedScroll = ScrollDirection.Vertical;
				_pressedScrollOffset = e.LocalY - handleYRect.Top;
				return base.HandleMousePressEvent(e);
			}
		}

		if (GetHandleXRect(out FloatRect handleXRect))
		{
			if (handleXRect.Contains(e.LocalX, e.LocalY))
			{
				_pressedScroll = ScrollDirection.Horizontal;
				_pressedScrollOffset = e.LocalX - handleXRect.Left;
				return base.HandleMousePressEvent(e);
			}
		}

		if (GetScrollbarYRect(out FloatRect scrollbarYRect))
		{
			if (scrollbarYRect.Contains(e.LocalX, e.LocalY))
			{
				_pressedScroll = ScrollDirection.Vertical;
				_pressedScrollOffset = handleYRect.Height / 2f;
				float scrollPos = e.LocalY - _pressedScrollOffset;
				float contentPos = MapScrollbarYPosToContentYPos(scrollPos);
				UpdateScrolls(_scrollX, contentPos);
				return base.HandleMousePressEvent(e);
			}
		}

		if (GetScrollbarXRect(out FloatRect scrollbarXRect))
		{
			if (scrollbarXRect.Contains(e.LocalX, e.LocalY))
			{
				_pressedScroll = ScrollDirection.Horizontal;
				_pressedScrollOffset = handleXRect.Width / 2f;
				float scrollPos = e.LocalX - _pressedScrollOffset;
				float contentPos = MapScrollbarXPosToContentYPos(scrollPos);
				UpdateScrolls(contentPos, _scrollY);
				return base.HandleMousePressEvent(e);
			}
		}

		return base.HandleMousePressEvent(e);
	}

	protected override bool HandleMouseReleaseEvent(MouseReleaseEvent e)
	{
		if (e.Button == MouseButton.Left)
		{
			_pressedScroll = ScrollDirection.None;
			_pressedScrollOffset = 0f;
		}

		return base.HandleMouseReleaseEvent(e);
	}

	protected override bool HandleMouseMoveEvent(MouseMoveEvent e)
	{
		_hoveredHandle = ScrollDirection.None;
		if (GetHandleYRect(out FloatRect yRect))
		{
			if (yRect.Contains(e.LocalX, e.LocalY))
			{
				_hoveredHandle |= ScrollDirection.Vertical;
			}
		}

		if (GetHandleXRect(out FloatRect xRect))
		{
			if (xRect.Contains(e.LocalX, e.LocalY))
			{
				_hoveredHandle |= ScrollDirection.Horizontal;
			}
		}

		if (_pressedScroll == ScrollDirection.Vertical)
		{
			float scrollPos = e.LocalY - _pressedScrollOffset;
			float contentPos = MapScrollbarYPosToContentYPos(scrollPos);
			UpdateScrolls(_scrollX, contentPos);
		}
		else if (_pressedScroll == ScrollDirection.Horizontal)
		{
			float scrollPos = e.LocalX - _pressedScrollOffset;
			float contentPos = MapScrollbarXPosToContentYPos(scrollPos);
			UpdateScrolls(contentPos, _scrollY);
		}

		return base.HandleMouseMoveEvent(e);
	}

	protected override bool HandleUnhoverEvent(UnhoverEvent e)
	{
		_hoveredHandle = ScrollDirection.None;
		return base.HandleUnhoverEvent(e);
	}

	protected override bool HasDrawAfterChildren()
	{
		return true;
	}

	protected override void DrawAfterChildren(RenderTarget target)
	{
		base.DrawAfterChildren(target);

		if (GetScrollbarYRect(out FloatRect scrollbarYRect))
		{
			_shape.FillColor = ScrollbarColor;
			_shape.Position = scrollbarYRect.Position;
			_shape.Size = scrollbarYRect.Size;
			target.Draw(_shape);
		}

		if (GetScrollbarXRect(out FloatRect scrollbarXRect))
		{
			_shape.FillColor = ScrollbarColor;
			_shape.Position = scrollbarXRect.Position;
			_shape.Size = scrollbarXRect.Size;
			target.Draw(_shape);
		}

		if (GetCrossRect(out FloatRect crossRect))
		{
			_shape.FillColor = ScrollbarColor;
			_shape.Position = crossRect.Position;
			_shape.Size = crossRect.Size;
			target.Draw(_shape);
		}

		ScrollDirection hoveredOrPressed = _hoveredHandle | _pressedScroll;
		if (GetHandleYRect(out FloatRect handleYRect))
		{
			_shape.FillColor = (hoveredOrPressed & ScrollDirection.Vertical) != 0 ? HoveredHandleColor : HandleColor;
			_shape.Position = handleYRect.Position;
			_shape.Size = handleYRect.Size;
			target.Draw(_shape);
		}

		if (GetHandleXRect(out FloatRect handleXRect))
		{
			_shape.FillColor = (hoveredOrPressed & ScrollDirection.Horizontal) != 0 ? HoveredHandleColor : HandleColor;
			_shape.Position = handleXRect.Position;
			_shape.Size = handleXRect.Size;
			target.Draw(_shape);
		}
	}

	protected override bool HandleMouseScrollEvent(MouseScrollEvent e)
	{
		if ((e.Modifiers & Modifier.Control) != 0 || (e.Modifiers & Modifier.Alt) != 0)
		{
			return false;
		}

		float scrollX = _scrollX - e.ScrollX * ScrollMultiplier;
		float scrollY = _scrollY - e.ScrollY * ScrollMultiplier;
		return UpdateScrolls(scrollX, scrollY);
	}

	private bool UpdateScrolls(float scrollX, float scrollY)
	{
		float oldScrollX = _scrollX;
		float oldScrollY = _scrollY;

		_scrollX = scrollX;
		_scrollY = scrollY;
		AdjustScroll();

		if (oldScrollX == _scrollX && oldScrollY == _scrollY)
		{
			return false;
		}

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

	private void UpdateScrollbarsVisibility()
	{
		bool prevHasScrollbarX = _hasScrollbarX;
		_hasScrollbarX = _totalChildrenRect.Width > Width;
		if (prevHasScrollbarX != _hasScrollbarX)
		{
			Yoga.PaddingBottom = _hasScrollbarX ? ScrollbarThickness : 0;
		}

		bool prevHasScrollbarY = _hasScrollbarY;
		_hasScrollbarY = _totalChildrenRect.Height > Height;
		if (prevHasScrollbarY != _hasScrollbarY)
		{
			Yoga.PaddingRight = _hasScrollbarY ? ScrollbarThickness : 0;
		}
	}

	private void AdjustScroll()
	{
		float minScrollX = MathF.Min(0, _totalChildrenRect.Left);
		float minScrollY = MathF.Min(0, _totalChildrenRect.Top);
		float maxScrollX = _totalChildrenRect.GetRight() - Width;
		float maxScrollY = _totalChildrenRect.GetBottom() - Height;

		_scrollX = MathF.Min(maxScrollX, _scrollX);
		_scrollY = MathF.Min(maxScrollY, _scrollY);

		_scrollX = MathF.Max(minScrollX, _scrollX);
		_scrollY = MathF.Max(minScrollY, _scrollY);
	}

	private bool GetScrollbarYRect(out FloatRect rect)
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

	private bool GetScrollbarXRect(out FloatRect rect)
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

	private bool GetHandleYRect(out FloatRect rect)
	{
		if (!_hasScrollbarY)
		{
			rect = new FloatRect();
			return false;
		}

		float top = MapContentYPosToScrollbarPos(_scrollY);
		float bottom = MapContentYPosToScrollbarPos(_scrollY + Height);
		Vector2f pos = new(Width - ScrollbarThickness, top);
		Vector2f size = new(ScrollbarThickness, bottom - top);

		rect = new FloatRect(pos, size);
		return true;
	}

	private bool GetHandleXRect(out FloatRect rect)
	{
		if (!_hasScrollbarX)
		{
			rect = new FloatRect();
			return false;
		}

		float left = MapContentXPosToScrollbarPos(_scrollX);
		float right = MapContentXPosToScrollbarPos(_scrollX + Width);
		Vector2f pos = new(left, Height - ScrollbarThickness);
		Vector2f size = new(right - left, ScrollbarThickness);
		rect = new FloatRect(pos, size);

		return true;
	}

	private float MapContentYPosToScrollbarPos(float y)
	{
		float total = _totalChildrenRect.Height;
		float availableHeight = Height;
		if (_hasScrollbarX)
		{
			availableHeight -= ScrollbarThickness;
		}

		float normalized = y / total;
		float mapped = normalized * availableHeight;
		return mapped;
	}

	private float MapContentXPosToScrollbarPos(float x)
	{
		float total = _totalChildrenRect.Width;
		float availableWidth = Width;
		if (_hasScrollbarY)
		{
			availableWidth -= ScrollbarThickness;
		}

		float normalized = x / total;

		float mapped = normalized * availableWidth;
		return mapped;
	}

	private float MapScrollbarYPosToContentYPos(float y)
	{
		float total = _totalChildrenRect.Height;
		float availableHeight = Height;
		if (_hasScrollbarX)
		{
			availableHeight -= ScrollbarThickness;
		}

		float normalized = y / availableHeight;
		float mapped = normalized * total;
		return mapped;
	}

	private float MapScrollbarXPosToContentYPos(float x)
	{
		float total = _totalChildrenRect.Width;
		float availableWidth = Width;
		if (_hasScrollbarY)
		{
			availableWidth -= ScrollbarThickness;
		}

		float normalized = x / availableWidth;
		float mapped = normalized * total;
		return mapped;
	}
}