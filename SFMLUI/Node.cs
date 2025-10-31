using Facebook.Yoga;
using OpenTK.Graphics.OpenGLES2;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SFMLUI;

public class Node
{
	private YogaNode _yoga = new();
	private Node? _parent;
	private List<Node> _children = new();
	private bool _hovered = false;

	private float _x;
	private float _y;
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
	public float PositionX => _x;
	public float PositionY => _y;

	public Vector2f RelToParentPosition => new(PositionX, PositionY);
	public Vector2f GlobalPosition => MapToGlobal(0, 0);
	public Vector2f Size => new(Width, Height);
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
		_yoga.AddChild(child._yoga);
	}

	public Node? ChildAt(float x, float y)
	{
		foreach (Node node in Children)
		{
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

	public Vector2f MapToGlobal(float localX, float localY)
	{
		Node? cur = this;
		while (cur != null)
		{
			localX += cur.PositionX;
			localY += cur.PositionY;
			cur = cur._parent;
		}

		return new Vector2f(localX, localY);
	}

	public Vector2f MapToLocal(float globalX, float globalY)
	{
		Node? cur = this;
		while (cur != null)
		{
			globalX -= cur.PositionX;
			globalY -= cur.PositionY;
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
		if (!Yoga.HasNewLayout)
		{
			return;
		}

		Yoga.MarkLayoutSeen();

		_x = Yoga.LayoutX + arrangeOffsetX;
		_y = Yoga.LayoutY + arrangeOffsetY;
		_width = Yoga.LayoutWidth;
		_height = Yoga.LayoutHeight;

		HandleEvent(LayoutChangeEvent.Instance);

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

		if (!paintRect.Intersects(rect, out FloatRect overlap))
		{
			return;
		}

		Vector2i targetSizeI = (Vector2i)target.Size;
		Vector2f targetSizeF = (Vector2f)targetSizeI;
		Vector2i sizeI = (Vector2i)size;
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