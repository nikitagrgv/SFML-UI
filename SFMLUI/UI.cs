using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace SFMLUI;

public class UI
{
	private static bool _glLoaded;

	private readonly View _view;
	private readonly Style _style;
	private readonly Root _root;

	private Widget? _mouseCapturedWidget;
	private Widget? _hoveredWidget;

	private Widget? _focusWidget;

	private readonly double _doubleClickTime = (double)GetDoubleClickTime() / 1000;
	private const int MouseDoubleClickCancelDistance = 3;

	private Vector2i _mousePosition;

	private long _mouseLeftClickTime = 0;
	private int _mouseLeftClickIndex = 0;
	private Vector2i _mouseLeftClickPosition = new(-1, -1);

	private MouseButton _mouseCapturedButton;
	private MouseButton _currentMouseState;

	private readonly KeyRegistry _keyRegistry = new();

	public event Action? DrawBegin;
	public event Action? DrawEnd;

	public Widget Root => _root;
	public Style Style => _style;
	public Widget? MouseCapturedWidget => _mouseCapturedWidget;
	public Widget? HoveredWidget => _hoveredWidget;
	public Widget? FocusWidget => _focusWidget;

	public IWindowProxy? WindowProxy { get; set; }

	public static void InitializeGL()
	{
		if (!_glLoaded)
		{
			GLLoader.LoadBindings(new GLBindingsContext());
			_glLoaded = true;
		}
	}

	public UI(Vector2f size, IWindowProxy? windowProxy)
	{
		WindowProxy = windowProxy;

		_style = new Style
		{
			Mask = new RoundBorderMask(),
			Border = new RoundBorder(),
		};

		_view = new View();
		_root = new Root(_style);

		Size = size;

		if (Mouse.IsButtonPressed(Mouse.Button.Left))
		{
			_currentMouseState |= MouseButton.Left;
		}

		if (Mouse.IsButtonPressed(Mouse.Button.Middle))
		{
			_currentMouseState |= MouseButton.Middle;
		}

		if (Mouse.IsButtonPressed(Mouse.Button.Right))
		{
			_currentMouseState |= MouseButton.Right;
		}
	}

	public Vector2f Size
	{
		get => _view.Size;
		set
		{
			_view.Size = value;
			_view.Center = value / 2;
			_root.FixedWidth = value.X;
			_root.FixedHeight = value.Y;
		}
	}

	public Modifier Modifiers
	{
		get
		{
			Modifier modifiers = Modifier.None;
			if (Keyboard.IsKeyPressed(Keyboard.Key.LAlt) || Keyboard.IsKeyPressed(Keyboard.Key.RAlt))
			{
				modifiers |= Modifier.Alt;
			}

			if (Keyboard.IsKeyPressed(Keyboard.Key.LControl) || Keyboard.IsKeyPressed(Keyboard.Key.RControl))
			{
				modifiers |= Modifier.Control;
			}

			if (Keyboard.IsKeyPressed(Keyboard.Key.LShift) || Keyboard.IsKeyPressed(Keyboard.Key.RShift))
			{
				modifiers |= Modifier.Shift;
			}

			return modifiers;
		}
	}

	public Widget? MouseAcceptingWidgetAt(Vector2f position)
	{
		Widget? widget = WidgetAt(position, checkMask: true);
		if (widget == null)
		{
			return null;
		}

		Vector2f local = widget.MapToLocal(position);
		while (widget != null)
		{
			if (widget.AcceptsMouse(local.X, local.Y))
			{
				return widget;
			}

			local = widget.MapToParent(local);
			widget = widget.Parent;
		}

		return null;
	}

	public Widget? WidgetAt(Vector2f position, bool checkMask)
	{
		return WidgetAtHelper(_root, position, checkMask);
	}

	public void OnTextEntered(TextEventArgs e)
	{
		if (_focusWidget != null)
		{
			TextEvent ev = new(e.Unicode);
			_focusWidget.HandleEvent(ev);
		}
	}

	public void OnKeyPressed(KeyEventArgs e)
	{
		bool repeat = _keyRegistry.IsPressed(e.Code);
		_keyRegistry.SetPressed(e.Code, pressed: true);

		Modifier modifiers = Modifier.None;
		if (e.Control)
		{
			modifiers |= Modifier.Control;
		}

		if (e.Alt)
		{
			modifiers |= Modifier.Alt;
		}

		if (e.Shift)
		{
			modifiers |= Modifier.Shift;
		}

		if (_focusWidget != null)
		{
			KeyPressEvent ev = new(e.Code, modifiers, repeat);
			_focusWidget.HandleEvent(ev);
		}
	}

	public void OnKeyReleased(KeyEventArgs e)
	{
		_keyRegistry.SetPressed(e.Code, pressed: false);

		Modifier modifiers = Modifier.None;
		if (e.Control)
		{
			modifiers |= Modifier.Control;
		}

		if (e.Alt)
		{
			modifiers |= Modifier.Alt;
		}

		if (e.Shift)
		{
			modifiers |= Modifier.Shift;
		}

		if (_focusWidget != null)
		{
			KeyReleaseEvent ev = new(e.Code, modifiers);
			_focusWidget.HandleEvent(ev);
		}
	}

	public void OnMousePressed(MouseButtonEventArgs e)
	{
		MouseButton button = Utils.ToMouseButton(e.Button);
		_currentMouseState |= button;

		if (button == MouseButton.Left)
		{
			long curTime = Stopwatch.GetTimestamp();
			Vector2i mouseLeftClickPosition = new(e.X, e.Y);
			Vector2i distance = mouseLeftClickPosition - _mouseLeftClickPosition;
			bool okDistance = Math.Abs(distance.X) < MouseDoubleClickCancelDistance &&
			                  Math.Abs(distance.Y) < MouseDoubleClickCancelDistance;

			if (_mouseLeftClickTime != 0 && okDistance)
			{
				double elapsed = Stopwatch.GetElapsedTime(_mouseLeftClickTime, curTime).TotalSeconds;
				if (elapsed <= _doubleClickTime)
				{
					_mouseLeftClickIndex++;
				}
				else
				{
					_mouseLeftClickIndex = 0;
				}
			}
			else
			{
				_mouseLeftClickIndex = 0;
			}

			_mouseLeftClickPosition = mouseLeftClickPosition;
			_mouseLeftClickTime = curTime;
		}
		else
		{
			_mouseLeftClickPosition = new Vector2i(-1, -1);
			_mouseLeftClickTime = 0;
			_mouseLeftClickIndex = 0;
		}

		Vector2f globalPos = new(e.X, e.Y);

		if (_mouseCapturedWidget != null)
		{
			Vector2f local = _mouseCapturedWidget.MapToLocal(globalPos);
			MousePressEvent ev = new(local.X,
				local.Y,
				e.X,
				e.Y,
				button,
				pressedButtons: _currentMouseState,
				Modifiers,
				_mouseLeftClickIndex);
			SendMouseEvent(_mouseCapturedWidget, ev);
		}
		else
		{
			Widget? receiver = MouseAcceptingWidgetAt(globalPos);
			if (receiver == null)
			{
				Widget? prevFocused = _focusWidget;
				_focusWidget = null;
				HandleFocusUnfocus(_focusWidget, prevFocused);
				return;
			}

			_mouseCapturedButton = button;

			Vector2f local = receiver.MapToLocal(globalPos);
			MousePressEvent ev = new(local.X,
				local.Y,
				e.X,
				e.Y,
				button,
				pressedButtons: _currentMouseState,
				Modifiers,
				_mouseLeftClickIndex);

			Widget? realReceiver = SendMouseEvent(receiver, ev);
			_mouseCapturedWidget = realReceiver;

			if (_focusWidget != realReceiver)
			{
				Widget? prevFocused = _focusWidget;
				_focusWidget = realReceiver;
				HandleFocusUnfocus(_focusWidget, prevFocused);
			}
		}
	}

	public void OnMouseReleased(MouseButtonEventArgs e)
	{
		MouseButton button = Utils.ToMouseButton(e.Button);
		_currentMouseState &= ~button;

		Vector2f globalPos = new(e.X, e.Y);

		if (_mouseCapturedWidget == null)
		{
			return;
		}

		Vector2f local = _mouseCapturedWidget.MapToLocal(globalPos);
		MouseReleaseEvent ev = new(local.X, local.Y, e.X, e.Y, button, _currentMouseState, Modifiers);
		SendMouseEvent(_mouseCapturedWidget, ev);

		if (button == _mouseCapturedButton)
		{
			_mouseCapturedWidget = null;
			_mouseCapturedButton = MouseButton.None;

			HandleMouseOrWidgetsMove(e.X, e.Y, sendMove: false);
		}
	}

	public void OnMouseMoved(MouseMoveEventArgs e)
	{
		HandleMouseOrWidgetsMove(e.X, e.Y, sendMove: true);
	}

	private void HandleMouseOrWidgetsMove(int curX, int curY, bool sendMove)
	{
		_mousePosition.X = curX;
		_mousePosition.Y = curY;

		Widget? prevHovered = _hoveredWidget;

		Vector2f globalPos = new(curX, curY);

		Widget? widget = _mouseCapturedWidget;
		if (widget == null)
		{
			widget = MouseAcceptingWidgetAt(globalPos);
			_hoveredWidget = widget;
		}
		else
		{
			_hoveredWidget = widget.ContainsGlobalPoint(globalPos, checkMask: true) ? widget : null;
		}

		HandleHoverUnhover(_hoveredWidget, prevHovered);
		HandleEnterLeave(_hoveredWidget, prevHovered);

		if (prevHovered != _hoveredWidget)
		{
			WindowProxy?.SetCursor(_hoveredWidget?.Cursor ?? CursorType.Arrow);
		}

		if (widget == null)
		{
			return;
		}

		if (sendMove)
		{
			Vector2f local = widget.MapToLocal(globalPos);
			MouseMoveEvent ev = new(local.X, local.Y, curX, curY, _currentMouseState, Modifiers);
			SendMouseEvent(widget, ev);
		}
	}

	public void OnMouseScrolled(MouseWheelScrollEventArgs e)
	{
		Vector2f globalPos = new(e.X, e.Y);
		Widget? receiver = MouseAcceptingWidgetAt(globalPos);
		if (receiver == null)
		{
			return;
		}

		float scrollX = 0;
		float scrollY = 0;
		if (e.Wheel == Mouse.Wheel.VerticalWheel)
		{
			scrollY = e.Delta;
		}
		else
		{
			scrollX = e.Delta;
		}

		Modifier modifiers = Modifiers;
		if (modifiers == Modifier.Shift)
		{
			(scrollX, scrollY) = (scrollY, scrollX);
		}

		Vector2f local = receiver.MapToLocal(globalPos);
		MouseScrollEvent ev = new(scrollX, scrollY, local.X, local.Y, e.X, e.Y, _currentMouseState, modifiers);
		SendMouseEvent(receiver, ev);
	}

	public void Update()
	{
		UITimer.Update();

		// NOTE: Some widgets may change their geometry after layout change events (like scroll areas). We update the UI
		// until it fully settles down. But limit attempts count, so we don't do this forever if something goes wrong.
		int attempts = 32;
		while (attempts > 0)
		{
			attempts--;
			_root.CalculateLayout();
			bool hasChanges = _root.UpdateLayout(0, 0);
			_root.NotifyLayoutChanges();
			if (!hasChanges)
			{
				break;
			}
		}

		HandleMouseOrWidgetsMove(_mousePosition.X, _mousePosition.Y, sendMove: false);
	}

	public void Draw(RenderWindow window)
	{
		View? prevView = window.GetView();

		window.SetView(_view);
		DrawBegin?.Invoke();
		window.SetView(_view);

		DoDraw(window);

		window.SetView(_view);
		DrawEnd?.Invoke();
		window.SetView(prevView);
	}

	private void DoDraw(RenderWindow window)
	{
		GL.StencilMask(0xFF);
		GL.Clear(ClearBufferMask.StencilBufferBit);

		Painter painter = new(window);
		MaskPainter maskPainter = new(window);
		_root.DrawHierarchy(
			window,
			new Vector2f(),
			new FloatRect(0, 0, _root.Width, _root.Height),
			painter,
			maskPainter);

		DrawDebug(window);
	}

	private void DrawDebug(RenderWindow window)
	{
		if (_style.EnableVisualizer)
		{
			Widget? widgetAt = WidgetAt((Vector2f)_mousePosition, true);
			if (widgetAt != null)
			{
				Vector2f globalPos = widgetAt.GlobalPosition;
				FloatRect geometry = widgetAt.InnerLayoutGeometry;
				geometry.Left = globalPos.X;
				geometry.Top = globalPos.Y;

				var shape = new RectangleShape()
				{
					Position = geometry.Position,
					Size = geometry.Size,
				};
				shape.FillColor = new Color(255, 255, 255, 150);

				window.SetView(_view);
				window.Draw(shape);
			}
		}
	}

	private Widget? SendMouseEvent(Widget receiver, MouseEvent e)
	{
		Widget? cur = receiver;
		while (cur != null && cur != _root)
		{
			bool accepted = cur.HandleEvent(e);
			if (accepted)
			{
				return cur;
			}

			e.LocalX += cur.PositionX;
			e.LocalY += cur.PositionY;
			cur = cur.Parent;
		}

		return null;
	}

	private static Widget WidgetAtHelper(Widget widget, Vector2f position, bool checkMask)
	{
		while (true)
		{
			Vector2f maxPos = widget.Size - widget.ScrollbarSize;
			if (position.X >= maxPos.X || position.Y >= maxPos.Y)
			{
				return widget;
			}

			Widget? child = widget.ChildAt(position, checkMask);
			if (child == null)
			{
				return widget;
			}

			widget = child;
			position -= child.Position;
		}
	}

	private static void HandleHoverUnhover(Widget? hovered, Widget? unhovered)
	{
		if (hovered == unhovered)
		{
			return;
		}

		if (hovered != null)
		{
			HoverEvent hoverEvent = HoverEvent.Instance;
			hovered.HandleEvent(hoverEvent);
		}

		if (unhovered != null)
		{
			UnhoverEvent unhoverEvent = UnhoverEvent.Instance;
			unhovered.HandleEvent(unhoverEvent);
		}
	}

	private static void HandleEnterLeave(Widget? hovered, Widget? unhovered)
	{
		if (hovered == unhovered)
		{
			return;
		}

		EnterEvent enterEvent = EnterEvent.Instance;
		LeaveEvent leaveEvent = LeaveEvent.Instance;

		int hoverDepth = GetWidgetDepth(hovered);
		int unhoverDepth = GetWidgetDepth(unhovered);

		Widget? topHovered = hovered;
		Widget? topUnhovered = unhovered;

		while (hoverDepth > unhoverDepth)
		{
			topHovered = topHovered!.Parent;
			hoverDepth--;
		}

		while (unhoverDepth > hoverDepth)
		{
			topUnhovered = topUnhovered!.Parent;
			unhoverDepth--;
		}

		while (topHovered != topUnhovered)
		{
			topHovered = topHovered!.Parent;
			topUnhovered = topUnhovered!.Parent;
		}

		Widget? curUnhovered = unhovered;
		while (curUnhovered != topUnhovered)
		{
			curUnhovered !.HandleEvent(leaveEvent);
			curUnhovered = curUnhovered.Parent;
		}

		Widget? curHovered = hovered;
		while (curHovered != topHovered)
		{
			curHovered!.HandleEvent(enterEvent);
			curHovered = curHovered.Parent;
		}
	}

	private static void HandleFocusUnfocus(Widget? focused, Widget? unfocused)
	{
		if (focused == unfocused)
		{
			return;
		}

		if (focused != null)
		{
			FocusEvent focusEvent = FocusEvent.Instance;
			focused.HandleEvent(focusEvent);
		}

		if (unfocused != null)
		{
			UnfocusEvent unfocusEvent = UnfocusEvent.Instance;
			unfocused.HandleEvent(unfocusEvent);
		}
	}

	private static int GetWidgetDepth(Widget? widget)
	{
		int depth = 0;
		Widget? cur = widget;
		while (cur != null)
		{
			depth++;
			cur = cur.Parent;
		}

		return depth;
	}

	// TODO: Implement for linux too
	[DllImport("user32.dll")]
	static extern uint GetDoubleClickTime();
}