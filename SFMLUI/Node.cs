using Facebook.Yoga;
using OpenTK.Graphics.OpenGLES2;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SFMLUI;

public class Node
{
	private readonly YogaNode _yoga = new();
	private readonly List<Node> _children = new();
	private Node? _parent;
	private bool _hovered;

	private float _originalX;
	private float _originalY;
	private float _arrangeOffsetX;
	private float _arrangeOffsetY;
	private float _width;
	private float _height;

	// For debug
	public static bool EnableClipping { get; set; } = true;

	public string? Name { get; set; } = null;

	public YogaNode Yoga => _yoga;
	public Node? Parent => _parent;

	public bool IsHovered => _hovered;

	public bool IsVisible
	{
		get
		{
			Node? cur = this;
			while (cur != null)
			{
				if (!cur.IsVisibleSelf)
				{
					return false;
				}

				cur = cur.Parent;
			}

			return true;
		}
	}

	public bool IsVisibleSelf
	{
		get => _yoga.Display != YogaDisplay.None;
		set => _yoga.Display = value ? YogaDisplay.Flex : YogaDisplay.None;
	}

	public bool IsEnabled
	{
		get
		{
			Node? cur = this;
			while (cur != null)
			{
				if (!cur.IsEnabledSelf)
				{
					return false;
				}

				cur = cur.Parent;
			}

			return true;
		}
	}

	public bool IsEnabledSelf { get; set; } = true;

	public float Width => _width;
	public float Height => _height;
	public float PositionX => _originalX + _arrangeOffsetX;
	public float PositionY => _originalY + _arrangeOffsetY;

	public Vector2f RelToParentPosition => new(PositionX, PositionY);
	public Vector2f GlobalPosition => MapToGlobal(0, 0);
	public Vector2f Size => new(Width, Height);
	public FloatRect RelToParentOriginalGeometry => new(_originalX, _originalY, Width, Height);

	public FloatRect RelToParentOriginalMarginRect => new(
		_originalX - _yoga.LayoutMarginLeft,
		_originalY - _yoga.LayoutMarginTop,
		Width + _yoga.LayoutMarginLeft + _yoga.LayoutMarginRight,
		Height + _yoga.LayoutMarginTop + _yoga.LayoutMarginBottom
	);

	public FloatRect RelToParentGeometry => new(PositionX, PositionY, Width, Height);
	public FloatRect Geometry => new(0, 0, Width, Height);

	public FloatRect GlobalGeometry => new(GlobalPosition, Size);

	public IReadOnlyList<Node> Children => _children;

	public Node()
	{
		_yoga.Data = this;
	}

	public void AddChild(Node child)
	{
		if (child._parent != null)
		{
			throw new NotImplementedException();
		}

		child._parent = this;
		_children.Add(child);
		AddChildToLayout(child);
	}

	protected virtual void AddChildToLayout(Node child)
	{
		_yoga.AddChild(child._yoga);
	}

	public Node? ChildAt(float x, float y)
	{
		// From last, so the visual order of rendered widget correspond to pick order 
		for (int index = Children.Count - 1; index >= 0; index--)
		{
			Node node = Children[index];
			if (x >= node.PositionX
			    && y >= node.PositionY
			    && x <= node.PositionX + node.Width
			    && y <= node.PositionY + node.Height)
			{
				return node;
			}
		}

		return null;
	}

	public Vector2f MapToParent(float localX, float localY)
	{
		return new Vector2f(localX + PositionX, localY + PositionY);
	}

	public Vector2f MapFromParent(float localX, float localY)
	{
		return new Vector2f(localX - PositionX, localY - PositionY);
	}

	public Vector2f MapToGlobal(float localX, float localY)
	{
		Node? cur = this;
		while (cur != null)
		{
			Vector2f toParent = cur.MapToParent(localX, localY);
			localX = toParent.X;
			localY = toParent.Y;
			cur = cur._parent;
		}

		return new Vector2f(localX, localY);
	}

	public Vector2f MapToLocal(float globalX, float globalY)
	{
		Node? cur = this;
		while (cur != null)
		{
			Vector2f fromParent = cur.MapFromParent(globalX, globalY);
			globalX = fromParent.X;
			globalY = fromParent.Y;
			cur = cur._parent;
		}

		return new Vector2f(globalX, globalY);
	}

	public bool ContainsGlobalPoint(float globalX, float globalY)
	{
		Node? cur = this;
		while (cur != null)
		{
			bool containsSelf = cur.GlobalGeometry.Contains(globalX, globalY);
			if (!containsSelf)
			{
				return false;
			}

			cur = cur.Parent;
		}

		return true;
	}

	public bool HasInParents(Node node)
	{
		Node? cur = _parent;
		while (cur != null)
		{
			if (cur == node)
			{
				return true;
			}

			cur = cur._parent;
		}

		return false;
	}

	internal void UpdateLayout(float arrangeOffsetX, float arrangeOffsetY)
	{
		bool hasNewLayout = Yoga.HasNewLayout;
		if (!hasNewLayout && arrangeOffsetX == _arrangeOffsetX && arrangeOffsetY == _arrangeOffsetY)
		{
			return;
		}

		_originalX = Yoga.LayoutX;
		_originalY = Yoga.LayoutY;
		_arrangeOffsetX = arrangeOffsetX;
		_arrangeOffsetY = arrangeOffsetY;
		_width = Yoga.LayoutWidth;
		_height = Yoga.LayoutHeight;

		UpdateChildrenLayout();

		// TODO# Do this after ALL hierarchy is updated?
		if (hasNewLayout)
		{
			Yoga.MarkLayoutSeen();
			HandleEvent(LayoutChangeEvent.Instance);
		}
	}

	protected void UpdateChildrenLayout()
	{
		foreach (Node child in Children)
		{
			UpdateChildLayout(child);
		}
	}


	protected virtual void UpdateChildLayout(Node child)
	{
		child.UpdateLayout(0, 0);
	}

	protected virtual void Draw(RenderTarget target)
	{
	}

	protected virtual void DrawAfterChildren(RenderTarget target)
	{
	}

	protected virtual bool HasDrawAfterChildren()
	{
		return false;
	}

	public virtual bool AcceptsMouse(float x, float y)
	{
		return true;
	}

	public virtual bool HandleEvent(Event e)
	{
		switch (e)
		{
			case MousePressEvent ev:
			{
				return HandleMousePressEvent(ev);
			}
			case MouseReleaseEvent ev:
			{
				return HandleMouseReleaseEvent(ev);
			}
			case MouseMoveEvent ev:
			{
				return HandleMouseMoveEvent(ev);
			}
			case MouseScrollEvent ev:
			{
				return HandleMouseScrollEvent(ev);
			}
			case LayoutChangeEvent ev:
			{
				return HandleLayoutChangeEvent(ev);
			}
			case EnterEvent ev:
			{
				return HandleEnterEvent(ev);
			}
			case LeaveEvent ev:
			{
				return HandleLeaveEvent(ev);
			}
			case HoverEvent ev:
			{
				return HandleHoverEvent(ev);
			}
			case UnhoverEvent ev:
			{
				return HandleUnhoverEvent(ev);
			}
		}

		return false;
	}

	protected virtual bool HandleMousePressEvent(MousePressEvent e)
	{
		return true;
	}

	protected virtual bool HandleMouseReleaseEvent(MouseReleaseEvent e)
	{
		return true;
	}

	protected virtual bool HandleMouseMoveEvent(MouseMoveEvent e)
	{
		return true;
	}

	protected virtual bool HandleMouseScrollEvent(MouseScrollEvent e)
	{
		return false;
	}

	protected virtual bool HandleLayoutChangeEvent(LayoutChangeEvent e)
	{
		return true;
	}

	protected virtual bool HandleEnterEvent(EnterEvent e)
	{
		return true;
	}

	protected virtual bool HandleLeaveEvent(LeaveEvent e)
	{
		return true;
	}

	protected virtual bool HandleHoverEvent(HoverEvent e)
	{
		_hovered = true;
		return true;
	}

	protected virtual bool HandleUnhoverEvent(UnhoverEvent e)
	{
		_hovered = false;
		return true;
	}

	internal void DrawHierarchy(RenderTarget target, Vector2f origin, FloatRect paintRect)
	{
		if (!IsVisibleSelf)
		{
			return;
		}

		Vector2f topLeft = origin + RelToParentPosition;
		Vector2f size = Size;
		FloatRect rect = new(topLeft, size);

		if (!paintRect.Intersects(rect, out FloatRect overlap) && EnableClipping)
		{
			return;
		}

		Vector2i targetSizeI = (Vector2i)target.Size;
		Vector2f targetSizeF = (Vector2f)targetSizeI;
		View view = new(new FloatRect(0, 0, size.X, size.Y));
		view.Viewport = new FloatRect(
			topLeft.X / targetSizeF.X,
			topLeft.Y / targetSizeF.Y,
			size.X / targetSizeF.X,
			size.Y / targetSizeF.Y
		);

		target.SetView(view);

		int scissorW = (int)paintRect.Width;
		int scissorH = (int)paintRect.Height;
		int scissorX = (int)paintRect.Left;
		int scissorY = targetSizeI.Y - ((int)paintRect.Top + scissorH);
		GL.Scissor(scissorX, scissorY, scissorW, scissorH);

		if (EnableClipping)
		{
			GL.Enable(EnableCap.ScissorTest);
		}

		Draw(target);
		GL.Disable(EnableCap.ScissorTest);

		foreach (Node child in _children)
		{
			child.DrawHierarchy(target, topLeft, overlap);
		}

		if (!HasDrawAfterChildren())
		{
			return;
		}

		target.SetView(view);

		GL.Scissor(scissorX, scissorY, scissorW, scissorH);

		if (EnableClipping)
		{
			GL.Enable(EnableCap.ScissorTest);
		}

		DrawAfterChildren(target);
		GL.Disable(EnableCap.ScissorTest);
	}
}