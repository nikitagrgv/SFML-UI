using Facebook.Yoga;
using OpenTK.Graphics.OpenGL;
using SFML.Graphics;
using SFML.System;

namespace SFMLUI;

public class Widget
{
	private readonly YogaNode _yoga = new();
	private readonly List<Widget> _children = new();
	private Widget? _parent;
	private Style? _style;
	private bool _hovered;
	private bool _focused;

	private float _arrangeOffsetX;
	private float _arrangeOffsetY;

	private bool _layoutDirtySelf = false;
	private bool _layoutDirtyChildren = false;
	CalculatedGeometry _calculatedGeometry = new();

	private Color _borderColor = new(40, 40, 40);
	private Color _borderHoverColor = new(40, 40, 40);
	private Color _borderFocusColor = new(40, 40, 40);

	private readonly RectangleShape _shape = new();
	private Color _color = Color.White;

	public string? Name { get; set; } = null;

	protected YogaNode OuterYoga => _yoga;
	protected virtual YogaNode InnerYoga => _yoga;

	private protected Style? Style
	{
		get => _style;
		set => _style = value;
	}

	public Widget? Parent => _parent;

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
		get
		{
			float value = OuterYoga.BorderWidth;
			return float.IsNaN(value) ? 0 : value;
		}
		set => OuterYoga.BorderWidth = value;
	}

	public CursorType Cursor { get; set; } = CursorType.Arrow;

	// TODO: Implement border rendering with different widths

	// public float BorderTopWidth
	// {
	// 	get => OuterYoga.BorderTopWidth;
	// 	set => OuterYoga.BorderTopWidth = value;
	// }
	//
	// public float BorderLeftWidth
	// {
	// 	get => OuterYoga.BorderLeftWidth;
	// 	set => OuterYoga.BorderLeftWidth = value;
	// }
	//
	// public float BorderBottomWidth
	// {
	// 	get => OuterYoga.BorderBottomWidth;
	// 	set => OuterYoga.BorderBottomWidth = value;
	// }
	//
	// public float BorderRightWidth
	// {
	// 	get => OuterYoga.BorderRightWidth;
	// 	set => OuterYoga.BorderRightWidth = value;
	// }

	// public void GetBorders(out float left, out float top, out float right, out float bottom)
	// {
	// 	float fallback = OuterYoga.BorderWidth;
	// 	fallback = float.IsNaN(fallback) ? 0 : fallback;
	//
	// 	left = OuterYoga.BorderLeftWidth;
	// 	if (float.IsNaN(left))
	// 		left = fallback;
	//
	// 	top = OuterYoga.BorderTopWidth;
	// 	if (float.IsNaN(top))
	// 		top = fallback;
	//
	// 	right = OuterYoga.BorderRightWidth;
	// 	if (float.IsNaN(right))
	// 		right = fallback;
	//
	// 	bottom = OuterYoga.BorderBottomWidth;
	// 	if (float.IsNaN(bottom))
	// 		bottom = fallback;
	// }

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

	public Color BorderColor
	{
		get => _borderColor;
		set => _borderColor = value;
	}

	public Color BorderHoverColor
	{
		get => _borderHoverColor;
		set => _borderHoverColor = value;
	}

	public Color BorderFocusColor
	{
		get => _borderFocusColor;
		set => _borderFocusColor = value;
	}

	public Color FillColor
	{
		get => _color;
		set => _color = value;
	}

	public bool IsHovered => _hovered;
	public bool IsFocused => _focused;

	public bool IsVisible
	{
		get
		{
			Widget? cur = this;
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
			Widget? cur = this;
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

	public float Width => _calculatedGeometry.Width;
	public float Height => _calculatedGeometry.Height;
	public float PositionX => _calculatedGeometry.X + _arrangeOffsetX;
	public float PositionY => _calculatedGeometry.Y + _arrangeOffsetY;

	public Vector2f OriginalPosition => new(_calculatedGeometry.X, _calculatedGeometry.Y);
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
		_calculatedGeometry.X - _calculatedGeometry.LayoutMarginLeft,
		_calculatedGeometry.Y - _calculatedGeometry.LayoutMarginTop,
		Width + _calculatedGeometry.LayoutMarginLeft + _calculatedGeometry.LayoutMarginRight,
		Height + _calculatedGeometry.LayoutMarginTop + _calculatedGeometry.LayoutMarginBottom
	);

	public IReadOnlyList<Widget> Children => _children;

	public Widget()
	{
		_yoga.Data = this;
	}

	public void AddChild(Widget child)
	{
		if (child._parent != null)
		{
			throw new NotImplementedException();
		}

		Style? oldStyle = child.Style;
		Widget? oldParent = child.Parent;

		child._parent = this;
		child._style = _style;
		_children.Add(child);
		InnerYoga.AddChild(child.OuterYoga);

		if (oldStyle != child.Style)
		{
			child.HandleEvent(new StyleChangeEvent
			{
				OldStyle = oldStyle,
				NewStyle = child.Style,
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

	public Widget? ChildAt(Vector2f position, bool checkMask)
	{
		// Pick from last, so the visual order of rendered widget correspond to the pick order
		for (int index = Children.Count - 1; index >= 0; index--)
		{
			Widget widget = Children[index];
			FloatRect rect = new(widget.Position, widget.Size);
			if (!rect.Contains(position))
			{
				continue;
			}

			if (!checkMask)
			{
				return widget;
			}

			Vector2f local = widget.MapFromParent(position);
			if (widget.MaskContainsPoint(local))
			{
				return widget;
			}
		}

		return null;
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
		Widget? cur = this;
		while (cur != null)
		{
			local = cur.MapToParent(local);
			cur = cur._parent;
		}

		return local;
	}

	public Vector2f MapToLocal(Vector2f global)
	{
		Widget? cur = this;
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

	public bool HasInParents(Widget widget)
	{
		Widget? cur = _parent;
		while (cur != null)
		{
			if (cur == widget)
			{
				return true;
			}

			cur = cur._parent;
		}

		return false;
	}

	internal bool UpdateLayout(float arrangeOffsetX, float arrangeOffsetY)
	{
		bool hasNewLayout = OuterYoga.HasNewLayout;
		OuterYoga.MarkLayoutSeen();
		if (!hasNewLayout && arrangeOffsetX == _arrangeOffsetX && arrangeOffsetY == _arrangeOffsetY)
		{
			return false;
		}

		CalculatedGeometry newGeometry = CalculatedGeometry.FromYoga(OuterYoga);
		bool sameGeometry = _calculatedGeometry.Equals(newGeometry) &&
		                    arrangeOffsetX == _arrangeOffsetX &&
		                    arrangeOffsetY == _arrangeOffsetY;
		if (!sameGeometry)
		{
			_calculatedGeometry = newGeometry;
			_arrangeOffsetX = arrangeOffsetX;
			_arrangeOffsetY = arrangeOffsetY;
		}

		bool childrenHasChanges = UpdateChildrenLayout();

		_layoutDirtySelf = !sameGeometry;
		_layoutDirtyChildren = childrenHasChanges;

		return _layoutDirtySelf || _layoutDirtyChildren;
	}

	internal void NotifyLayoutChanges()
	{
		if (_layoutDirtyChildren)
		{
			_layoutDirtyChildren = false;
			foreach (Widget child in Children)
			{
				child.NotifyLayoutChanges();
			}

			HandleEvent(ChildrenLayoutChangeEvent.Instance);
		}

		if (_layoutDirtySelf)
		{
			_layoutDirtySelf = false;
			HandleEvent(LayoutChangeEvent.Instance);
		}
	}

	protected bool UpdateChildrenLayout()
	{
		bool anyHasChanges = false;
		foreach (Widget child in Children)
		{
			bool hasChanges = UpdateChildLayout(child);
			anyHasChanges |= hasChanges;
		}

		return anyHasChanges;
	}

	protected virtual bool UpdateChildLayout(Widget child)
	{
		return child.UpdateLayout(0, 0);
	}

	protected virtual void Draw(IPainter painter)
	{
		_shape.FillColor = _color;
		_shape.Size = Size;
		painter.Draw(_shape);
	}

	protected virtual void DrawBorder(IPainter painter)
	{
		if (Style is { Border: { } border })
		{
			border.DrawBorder(this, painter);
		}
	}

	protected bool HasMask()
	{
		if (Style is { Mask: { } mask })
		{
			return mask.HasMask(this);
		}

		return false;
	}

	protected void DrawMask(IMaskPainter painter)
	{
		if (Style is { Mask: { } mask })
		{
			mask.DrawMask(this, painter);
		}
	}

	protected bool MaskContainsPoint(Vector2f local)
	{
		if (Style is not { Mask: { } mask })
		{
			return true;
		}

		return mask.ContainsPoint(this, local);
	}

	public virtual bool AcceptsMouse(float x, float y)
	{
		return true;
	}

	public virtual bool HandleEvent(Event e)
	{
		return e switch
		{
			MousePressEvent ev => HandleMousePressEvent(ev),
			MouseReleaseEvent ev => HandleMouseReleaseEvent(ev),
			MouseMoveEvent ev => HandleMouseMoveEvent(ev),
			MouseScrollEvent ev => HandleMouseScrollEvent(ev),
			StyleChangeEvent ev => HandleRootChangeEvent(ev),
			ParentChangeEvent ev => HandleParentChangeEvent(ev),
			ChildAddEvent ev => HandleChildAddEvent(ev),
			LayoutChangeEvent ev => HandleLayoutChangeEvent(ev),
			ChildrenLayoutChangeEvent ev => HandleChildrenLayoutChangeEvent(ev),
			EnterEvent ev => HandleEnterEvent(ev),
			LeaveEvent ev => HandleLeaveEvent(ev),
			HoverEvent ev => HandleHoverEvent(ev),
			UnhoverEvent ev => HandleUnhoverEvent(ev),
			FocusEvent ev => HandleFocusEvent(ev),
			UnfocusEvent ev => HandleUnfocusEvent(ev),
			KeyPressEvent ev => HandleKeyPressEvent(ev),
			KeyReleaseEvent ev => HandleKeyReleaseEvent(ev),
			TextEvent ev => HandleTextEvent(ev),
			_ => false,
		};
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

	protected virtual bool HandleChildrenLayoutChangeEvent(ChildrenLayoutChangeEvent e)
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

	protected virtual bool HandleFocusEvent(FocusEvent e)
	{
		_focused = true;
		return true;
	}

	protected virtual bool HandleUnfocusEvent(UnfocusEvent e)
	{
		_focused = false;
		return true;
	}

	protected virtual bool HandleKeyPressEvent(KeyPressEvent e)
	{
		return false;
	}

	protected virtual bool HandleKeyReleaseEvent(KeyReleaseEvent e)
	{
		return false;
	}

	protected virtual bool HandleTextEvent(TextEvent e)
	{
		return false;
	}

	// TODO: Shitty. Make any widget scrollable and move all code from scroll widget here?
	internal virtual Vector2f ScrollbarSize => new(0, 0);

	internal void DrawHierarchy(
		RenderTarget target,
		Vector2f origin,
		FloatRect paintRect,
		Painter painter,
		MaskPainter maskPainter)
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

		maskPainter.SetPaintRect(overlap);

		int scissorW = (int)overlap.Width;
		int scissorH = (int)overlap.Height;
		int scissorX = (int)overlap.Left;
		int scissorY = targetSizeI.Y - ((int)overlap.Top + scissorH);
		if (enableClipping)
		{
			GL.Scissor(scissorX, scissorY, scissorW, scissorH);
		}

		bool maskDrawn = false;
		if (enableClipping)
		{
			GL.Enable(EnableCap.ScissorTest);
			GL.Enable(EnableCap.StencilTest);

			if (HasMask())
			{
				maskPainter.StartDrawMask();
				DrawMask(maskPainter);
				maskDrawn = maskPainter.FinishDrawMask();
			}
		}

		if (enableClipping)
		{
			maskPainter.StartUseMask();
		}

		Draw(painter);
		DrawBorder(painter);

		FloatRect childrenRect = new(topLeft, size - ScrollbarSize);
		if (paintRect.Intersects(childrenRect, out FloatRect childrenOverlap) || !enableClipping)
		{
			foreach (Widget child in _children)
			{
				child.DrawHierarchy(target, topLeft, childrenOverlap, painter, maskPainter);
			}
		}

		if (enableClipping)
		{
			GL.Scissor(scissorX, scissorY, scissorW, scissorH);
		}

		maskPainter.FinishUseMask(maskDrawn);

		if (enableClipping)
		{
			GL.Disable(EnableCap.StencilTest);
			GL.Disable(EnableCap.ScissorTest);
		}
	}

	private static bool ContainsLocalPoint(Widget widget, Vector2f local, bool checkMask)
	{
		Widget? cur = widget;
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