using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public class ScrollAreaWidget : Widget
{
	private float _scrollX;
	private float _scrollY;

	private bool _hasHandleX;
	private bool _hasHandleY;

	private float _handleThickness = 8f;

	private readonly RectangleShape _shape = new();

	public Color HandleBackgroundColor { get; set; } = new(44, 44, 44);
	public Color HandleColor { get; set; } = new(159, 159, 159);

	public float ScrollMultiplier { get; set; } = 20f;

	public float HandleThickness
	{
		get => _handleThickness;
		set
		{
			_handleThickness = value;
			UpdateHandles();
		}
	}

	protected override bool HandleLayoutChangeEvent(LayoutChangeEvent e)
	{
		UpdateHandles();
		AdjustScroll();
		return base.HandleLayoutChangeEvent(e);
	}

	protected override bool HasDrawAfterChildren()
	{
		return true;
	}

	protected override void DrawAfterChildren(RenderTarget target)
	{
		base.DrawAfterChildren(target);

		if (_hasHandleY)
		{
			_shape.FillColor = HandleBackgroundColor;
			_shape.Position = new Vector2f(Width - HandleThickness, 0);
			_shape.Size = new Vector2f(HandleThickness, Height);
			target.Draw(_shape);
		}

		if (_hasHandleX)
		{
			_shape.FillColor = HandleBackgroundColor;
			_shape.Position = new Vector2f(0, Height - HandleThickness);
			_shape.Size = new Vector2f(Width, HandleThickness);
			target.Draw(_shape);
		}

		if (_hasHandleY)
		{
			_shape.FillColor = HandleColor;
			float top = MapContentYPosToHandlePos(_scrollY);
			float bottom = MapContentYPosToHandlePos(_scrollY + Height);
			_shape.Position = new Vector2f(Width - HandleThickness, top);
			_shape.Size = new Vector2f(HandleThickness, bottom - top);
			target.Draw(_shape);
		}

		if (_hasHandleX)
		{
			_shape.FillColor = HandleColor;
			float left = MapContentXPosToHandlePos(_scrollX);
			float right = MapContentXPosToHandlePos(_scrollX + Width);
			_shape.Position = new Vector2f(left, Height - HandleThickness);
			_shape.Size = new Vector2f(right - left, HandleThickness);
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

	private void UpdateHandles()
	{
		bool prevHasHandleX = _hasHandleX;
		_hasHandleX = GetTotalChildrenWidth() > Width;
		if (prevHasHandleX != _hasHandleX)
		{
			Yoga.PaddingBottom = _hasHandleX ? HandleThickness : 0;
		}

		bool prevHasHandleY = _hasHandleY;
		_hasHandleY = GetTotalChildrenHeight() > Height;
		if (prevHasHandleY != _hasHandleY)
		{
			Yoga.PaddingRight = _hasHandleY ? HandleThickness : 0;
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

	private FloatRect GetYHandleRect()
	{
		Vector2f pos = new(Width - HandleThickness, 0);
		float availableHeight = Height;
		if (_hasHandleX)
		{
			availableHeight -= HandleThickness;
		}

		Vector2f size = new(HandleThickness, availableHeight);
		return new FloatRect(pos, size);
	}

	private FloatRect GetXHandleRect()
	{
		Vector2f pos = new(0, Height - HandleThickness);
		float availableWidth = Width;
		if (_hasHandleY)
		{
			availableWidth -= HandleThickness;
		}

		Vector2f size = new(HandleThickness, availableWidth);
		return new FloatRect(pos, size);
	}

	private float MapContentYPosToHandlePos(float y)
	{
		float total = GetTotalChildrenHeight();
		float normalized = y / total;
		float availableHeight = Height;
		if (_hasHandleX)
		{
			availableHeight -= HandleThickness;
		}

		float handle = normalized * availableHeight;
		return handle;
	}

	private float MapContentXPosToHandlePos(float x)
	{
		float total = GetTotalChildrenWidth();
		float normalized = x / total;
		float availableWidth = Width;
		if (_hasHandleY)
		{
			availableWidth -= HandleThickness;
		}

		float handle = normalized * availableWidth;
		return handle;
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