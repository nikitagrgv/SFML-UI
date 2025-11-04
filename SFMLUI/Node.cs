using System.Text;
using Facebook.Yoga;
using OpenTK.Graphics.OpenGL;
using SFML.Graphics;
using SFML.Graphics.Glsl;
using SFML.System;
using SFML.Window;

namespace SFMLUI;

public class Node
{
	private readonly YogaNode _yoga = new();
	private readonly List<Node> _children = new();
	private Node? _parent;
	private Style? _style;
	private bool _hovered;

	private float _originalX;
	private float _originalY;
	private float _arrangeOffsetX;
	private float _arrangeOffsetY;
	private float _width;
	private float _height;

	public string? Name { get; set; } = null;

	protected YogaNode OuterYoga => _yoga;
	protected virtual YogaNode InnerYoga => _yoga;

	private protected Style? Style
	{
		get => _style;
		set => _style = value;
	}

	public Node? Parent => _parent;

	public YogaValue FixedWidth
	{
		get => OuterYoga.Width;
		set => OuterYoga.Width = value;
	}

	public YogaValue FixedHeight
	{
		get => OuterYoga.Height;
		set => OuterYoga.Height = value;
	}

	public YogaValue Top
	{
		get => OuterYoga.Top;
		set => OuterYoga.Top = value;
	}

	public YogaValue Left
	{
		get => OuterYoga.Left;
		set => OuterYoga.Left = value;
	}

	public YogaValue Bottom
	{
		get => OuterYoga.Bottom;
		set => OuterYoga.Bottom = value;
	}

	public YogaValue Right
	{
		get => OuterYoga.Right;
		set => OuterYoga.Right = value;
	}

	public YogaValue Start
	{
		get => OuterYoga.Start;
		set => OuterYoga.Start = value;
	}

	public YogaValue End
	{
		get => OuterYoga.End;
		set => OuterYoga.End = value;
	}

	public YogaValue MinWidth
	{
		get => OuterYoga.MinWidth;
		set => OuterYoga.MinWidth = value;
	}

	public YogaValue MinHeight
	{
		get => OuterYoga.MinHeight;
		set => OuterYoga.MinHeight = value;
	}

	public YogaValue Margin
	{
		get => OuterYoga.Margin;
		set => OuterYoga.Margin = value;
	}

	public YogaValue MarginTop
	{
		get => OuterYoga.MarginTop;
		set => OuterYoga.MarginTop = value;
	}

	public YogaValue MarginLeft
	{
		get => OuterYoga.MarginLeft;
		set => OuterYoga.MarginLeft = value;
	}

	public YogaValue MarginBottom
	{
		get => OuterYoga.MarginBottom;
		set => OuterYoga.MarginBottom = value;
	}

	public YogaValue MarginRight
	{
		get => OuterYoga.MarginRight;
		set => OuterYoga.MarginRight = value;
	}

	public YogaValue Padding
	{
		get => InnerYoga.Padding;
		set => InnerYoga.Padding = value;
	}

	public YogaValue PaddingTop
	{
		get => InnerYoga.PaddingTop;
		set => InnerYoga.PaddingTop = value;
	}

	public YogaValue PaddingLeft
	{
		get => InnerYoga.PaddingLeft;
		set => InnerYoga.PaddingLeft = value;
	}

	public YogaValue PaddingBottom
	{
		get => InnerYoga.PaddingBottom;
		set => InnerYoga.PaddingBottom = value;
	}

	public YogaValue PaddingRight
	{
		get => InnerYoga.PaddingRight;
		set => InnerYoga.PaddingRight = value;
	}

	public float BorderWidth
	{
		get => OuterYoga.BorderWidth;
		set => OuterYoga.BorderWidth = value;
	}

	public float BorderTopWidth
	{
		get => OuterYoga.BorderTopWidth;
		set => OuterYoga.BorderTopWidth = value;
	}

	public float BorderLeftWidth
	{
		get => OuterYoga.BorderLeftWidth;
		set => OuterYoga.BorderLeftWidth = value;
	}

	public float BorderBottomWidth
	{
		get => OuterYoga.BorderBottomWidth;
		set => OuterYoga.BorderBottomWidth = value;
	}

	public float BorderRightWidth
	{
		get => OuterYoga.BorderRightWidth;
		set => OuterYoga.BorderRightWidth = value;
	}

	public float BorderRadiusBottomRight { get; set; }
	public float BorderRadiusTopRight { get; set; }
	public float BorderRadiusBottomLeft { get; set; }
	public float BorderRadiusTopLeft { get; set; }

	public float BorderRadius
	{
		set
		{
			BorderRadiusBottomRight = value;
			BorderRadiusTopRight = value;
			BorderRadiusBottomLeft = value;
			BorderRadiusTopLeft = value;
		}
	}

	public YogaWrap Wrap
	{
		get => OuterYoga.Wrap;
		set => OuterYoga.Wrap = value;
	}

	public YogaValue FlexBasis
	{
		get => OuterYoga.FlexBasis;
		set => OuterYoga.FlexBasis = value;
	}

	public float FlexGrow
	{
		get => OuterYoga.FlexGrow;
		set => OuterYoga.FlexGrow = value;
	}

	public float FlexShrink
	{
		get => OuterYoga.FlexShrink;
		set => OuterYoga.FlexShrink = value;
	}

	public YogaFlexDirection FlexDirection
	{
		get => InnerYoga.FlexDirection;
		set => InnerYoga.FlexDirection = value;
	}

	public YogaPositionType PositionType
	{
		get => OuterYoga.PositionType;
		set => OuterYoga.PositionType = value;
	}

	public YogaAlign AlignContent
	{
		get => InnerYoga.AlignContent;
		set => InnerYoga.AlignContent = value;
	}

	public YogaAlign AlignSelf
	{
		get => OuterYoga.AlignSelf;
		set => OuterYoga.AlignSelf = value;
	}

	public YogaAlign AlignItems
	{
		get => InnerYoga.AlignItems;
		set => InnerYoga.AlignItems = value;
	}

	public YogaJustify JustifyContent
	{
		get => InnerYoga.JustifyContent;
		set => InnerYoga.JustifyContent = value;
	}

	public float AspectRatio
	{
		get => OuterYoga.AspectRatio;
		set => OuterYoga.AspectRatio = value;
	}

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

	public Vector2f OriginalPosition => new(_originalX, _originalY);
	public Vector2f Position => new(PositionX, PositionY);
	public Vector2f Size => new(Width, Height);

	public Vector2f GlobalPosition => MapToGlobal(new Vector2f());
	public FloatRect GlobalGeometry => new(GlobalPosition, Size);

	// TODO: Remove or hide
	public FloatRect InnerLayoutGeometry => new(
		InnerYoga.LayoutX,
		InnerYoga.LayoutY,
		InnerYoga.LayoutWidth,
		InnerYoga.LayoutHeight
	);

	public FloatRect RelToParentOriginalMarginRect => new(
		_originalX - _yoga.LayoutMarginLeft,
		_originalY - _yoga.LayoutMarginTop,
		Width + _yoga.LayoutMarginLeft + _yoga.LayoutMarginRight,
		Height + _yoga.LayoutMarginTop + _yoga.LayoutMarginBottom
	);

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

		Style? oldStyle = child.Style;
		Node? oldParent = child.Parent;

		child._parent = this;
		child._style = _style;
		_children.Add(child);
		InnerYoga.AddChild(child.OuterYoga);

		if (oldStyle != child.Style)
		{
			child.HandleEvent(new StyleChangeEvent
			{
				OldStyle = oldStyle,
				NewStyle = child.Style
			});
		}

		child.HandleEvent(new ParentChangeEvent
		{
			OldParent = oldParent,
			NewParent = child.Parent,
		});

		HandleEvent(new ChildAddEvent
		{
			Child = child,
		});
	}

	public Node? ChildAt(Vector2f position, bool checkMask)
	{
		// Pick from last, so the visual order of rendered widget correspond to the pick order 
		for (int index = Children.Count - 1; index >= 0; index--)
		{
			Node node = Children[index];
			FloatRect rect = new(node.Position, node.Size);
			if (!rect.Contains(position))
			{
				continue;
			}

			if (!checkMask)
			{
				return node;
			}

			Vector2f local = node.MapFromParent(position);
			if (node.MaskContainsPoint(local))
			{
				return node;
			}
		}

		return null;
	}

	private bool MaskContainsPoint(Vector2f local)
	{
		if (Style is not { Mask: { } mask })
		{
			return true;
		}

		bool contains = mask.ContainsPoint(
			local,
			Size,
			BorderRadiusBottomRight,
			BorderRadiusTopRight,
			BorderRadiusBottomLeft,
			BorderRadiusTopLeft
		);
		return contains;
	}

	public Vector2f MapToParent(Vector2f local)
	{
		return local + Position;
	}

	public Vector2f MapFromParent(Vector2f parentsLocal)
	{
		return parentsLocal - Position;
	}

	public FloatRect MapToParent(FloatRect local)
	{
		FloatRect mapped = new(MapToParent(local.Position), local.Size);
		return mapped;
	}

	public FloatRect MapFromParent(FloatRect parentsLocal)
	{
		FloatRect mapped = new(MapFromParent(parentsLocal.Position), parentsLocal.Size);
		return mapped;
	}

	public Vector2f MapToGlobal(Vector2f local)
	{
		Node? cur = this;
		while (cur != null)
		{
			local = cur.MapToParent(local);
			cur = cur._parent;
		}

		return local;
	}

	public Vector2f MapToLocal(Vector2f global)
	{
		Node? cur = this;
		while (cur != null)
		{
			global = cur.MapFromParent(global);
			cur = cur._parent;
		}

		return global;
	}

	public bool ContainsLocalPoint(Vector2f local, bool checkMask)
	{
		return ContainsLocalPoint(this, local, checkMask);
	}

	public bool ContainsGlobalPoint(Vector2f global, bool checkMask)
	{
		Vector2f local = MapToLocal(global);
		return ContainsLocalPoint(local, checkMask);
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
		bool hasNewLayout = OuterYoga.HasNewLayout;
		if (!hasNewLayout && arrangeOffsetX == _arrangeOffsetX && arrangeOffsetY == _arrangeOffsetY)
		{
			return;
		}

		_originalX = OuterYoga.LayoutX;
		_originalY = OuterYoga.LayoutY;
		_arrangeOffsetX = arrangeOffsetX;
		_arrangeOffsetY = arrangeOffsetY;
		_width = OuterYoga.LayoutWidth;
		_height = OuterYoga.LayoutHeight;

		UpdateChildrenLayout();

		// TODO# Do this after ALL hierarchy is updated?
		if (hasNewLayout)
		{
			OuterYoga.MarkLayoutSeen();
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
			case StyleChangeEvent ev:
			{
				return HandleRootChangeEvent(ev);
			}
			case ParentChangeEvent ev:
			{
				return HandleParentChangeEvent(ev);
			}
			case ChildAddEvent ev:
			{
				return HandleChildAddEvent(ev);
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

	protected virtual bool HandleRootChangeEvent(StyleChangeEvent e)
	{
		return true;
	}

	protected virtual bool HandleParentChangeEvent(ParentChangeEvent e)
	{
		return true;
	}

	protected virtual bool HandleChildAddEvent(ChildAddEvent e)
	{
		return true;
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

	// TODO: Shitty. Make any node scrollable and move all code from scroll widget here?
	internal virtual Vector2f ScrollbarSize => new(0, 0);

	internal class DrawState
	{
		public int StencilDepth { get; set; }
	}

	internal void DrawHierarchy(RenderTarget target, Vector2f origin, FloatRect paintRect, DrawState drawState)
	{
		if (!IsVisibleSelf)
		{
			return;
		}

		bool enableClipping = Style?.EnableClipping ?? true;

		Vector2f topLeft = origin + Position;
		Vector2f size = Size;
		FloatRect rect = new(topLeft, size);

		if (!paintRect.Intersects(rect, out FloatRect overlap) && enableClipping)
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

		int scissorW = (int)overlap.Width;
		int scissorH = (int)overlap.Height;
		int scissorX = (int)overlap.Left;
		int scissorY = targetSizeI.Y - ((int)overlap.Top + scissorH);
		GL.Scissor(scissorX, scissorY, scissorW, scissorH);

		if (enableClipping)
		{
			GL.Enable(EnableCap.ScissorTest);
			GL.Enable(EnableCap.StencilTest);
		}

		////////////////////
		GL.StencilMask(0xFF);
		GL.StencilFunc(StencilFunction.Always, 1, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Incr);

		var shape = new RectangleShape
		{
			Size = Size,
			TextureRect = new IntRect(0, 0, 1, 1),
		};

		RenderStates state = RenderStates.Default;
		state.Shader = Style?.Mask?.GetMaskShader(
			Width,
			Height,
			BorderRadiusBottomRight,
			BorderRadiusTopRight,
			BorderRadiusBottomLeft,
			BorderRadiusTopLeft);

		// GL.ColorMask(false, false, false, false);
		target.Draw(shape, state);
		shape.Size = new Vector2f(shape.Size.X * 0.7f, shape.Size.Y * 0.4f);
		shape.Position += new Vector2f(5, 15);
		target.Draw(shape, state);
		
		
		drawState.StencilDepth++;

		GL.StencilMask(0x00);
		GL.StencilFunc(StencilFunction.Equal, drawState.StencilDepth, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Keep);
		GL.ColorMask(true, true, true, true);
		///////////////////

		Draw(target);

		FloatRect childrenRect = new(topLeft, size - ScrollbarSize);
		if (paintRect.Intersects(childrenRect, out FloatRect childrenOverlap) || !enableClipping)
		{
			foreach (Node child in _children)
			{
				child.DrawHierarchy(target, topLeft, childrenOverlap, drawState);
			}
		}

		drawState.StencilDepth--;

		GL.StencilMask(0xFF);
		GL.StencilFunc(StencilFunction.Less, drawState.StencilDepth, 0xFF);
		GL.StencilOp(StencilOp.Keep, StencilOp.Keep, StencilOp.Replace);
		GL.ColorMask(false, false, false, false);

		shape.Size = Size;
		shape.Position = new Vector2f();
		target.Draw(shape);

		GL.ColorMask(true, true, true, true);
		GL.Disable(EnableCap.StencilTest);
		GL.Disable(EnableCap.ScissorTest);
	}

	private static bool ContainsLocalPoint(Node node, Vector2f local, bool checkMask)
	{
		Node? cur = node;
		FloatRect rect = new(new Vector2f(0, 0), cur.Size);
		while (true)
		{
			if (!rect.Contains(local))
			{
				return false;
			}

			if (checkMask && !cur.MaskContainsPoint(local))
			{
				return false;
			}

			local = cur.MapToParent(local);
			cur = cur.Parent;
			if (cur == null)
			{
				break;
			}

			rect = new FloatRect(new Vector2f(0, 0), cur.Size);
			if (checkMask)
			{
				Vector2f scrollbarSize = cur.ScrollbarSize;
				rect.Width -= scrollbarSize.X;
				rect.Height -= scrollbarSize.Y;
			}
		}

		return true;
	}
}