using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public class ScrollAreaWidget : Widget
{
	private float _scrollX;
	private float _scrollY;

	private bool _hasHandleX;
	private bool _hasHandleY;

	private float _handleThickness = 5f;

	private readonly RectangleShape _shape = new();

	public float ScrollMultiplier { get; set; } = 15f;

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

	protected override void Draw(RenderTarget target)
	{
		base.Draw(target);

		if (_hasHandleY)
		{
			_shape.FillColor = Color.White;
			_shape.Position = new Vector2f(Width - HandleThickness, 0);
			_shape.Size = new Vector2f(HandleThickness, Height);
			target.Draw(_shape);
		}

		if (_hasHandleX)
		{
			_shape.FillColor = Color.White;
			_shape.Position = new Vector2f(0, Height - HandleThickness);
			_shape.Size = new Vector2f(Width, HandleThickness);
			target.Draw(_shape);
		}
	}

	protected override bool HandleMouseScrollEvent(MouseScrollEvent e)
	{
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