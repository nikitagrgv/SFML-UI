using Facebook.Yoga;
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
	private readonly Root _root;

	private Node? _mouseCapturedNode;
	private Node? _hoveredNode;

	private MouseButton _mouseCapturedButton;
	private MouseButton _currentMouseState;

	public event Action? DrawBegin;
	public event Action? DrawEnd;

	public Node Root => _root;
	public Node? MouseCapturedNode => _mouseCapturedNode;
	public Node? HoveredNode => _hoveredNode;

	public static void InitializeGL()
	{
		if (!_glLoaded)
		{
			GLLoader.LoadBindings(new GLBindingsContext());
			_glLoaded = true;
		}
	}

	public UI(Vector2f size)
	{
		_view = new View();
		_root = new Root();

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

	public Node? MouseAcceptingNodeAt(float x, float y)
	{
		Node? node = NodeAt(x, y);
		if (node == null)
		{
			return null;
		}

		Vector2f local = node.MapToLocal(x, y);
		while (node != null)
		{
			if (node.AcceptsMouse(local.X, local.Y))
			{
				return node;
			}

			local = node.MapToParent(local.X, local.Y);
			node = node.Parent;
		}

		return null;
	}

	public Node? NodeAt(float x, float y)
	{
		return NodeAtHelper(_root, x, y);
	}

	public void OnKeyPressed(KeyEventArgs e)
	{
	}

	public void OnMousePressed(MouseButtonEventArgs e)
	{
		MouseButton button = Utils.ToMouseButton(e.Button);
		_currentMouseState |= button;

		if (_mouseCapturedNode != null)
		{
			Vector2f local = _mouseCapturedNode.MapToLocal(e.X, e.Y);
			MousePressEvent ev = new(local.X, local.Y, e.X, e.Y, button, _currentMouseState, Modifiers);
			SendMouseEvent(_mouseCapturedNode, ev);
		}
		else
		{
			Node? receiver = MouseAcceptingNodeAt(e.X, e.Y);
			if (receiver == null)
			{
				return;
			}

			_mouseCapturedButton = button;

			Vector2f local = receiver.MapToLocal(e.X, e.Y);
			MousePressEvent ev = new(local.X, local.Y, e.X, e.Y, button, _currentMouseState, Modifiers);
			Node? realReceiver = SendMouseEvent(receiver, ev);
			_mouseCapturedNode = realReceiver;
		}
	}

	public void OnMouseReleased(MouseButtonEventArgs e)
	{
		MouseButton button = Utils.ToMouseButton(e.Button);
		_currentMouseState &= ~button;

		if (_mouseCapturedNode == null)
		{
			return;
		}

		Vector2f local = _mouseCapturedNode.MapToLocal(e.X, e.Y);
		MouseReleaseEvent ev = new(local.X, local.Y, e.X, e.Y, button, _currentMouseState, Modifiers);
		SendMouseEvent(_mouseCapturedNode, ev);

		if (button == _mouseCapturedButton)
		{
			_mouseCapturedNode = null;
			_mouseCapturedButton = MouseButton.None;
		}
	}

	public void OnMouseMoved(MouseMoveEventArgs e)
	{
		Node? prevHovered = _hoveredNode;

		Node? node = _mouseCapturedNode;
		if (node == null)
		{
			node = MouseAcceptingNodeAt(e.X, e.Y);
			_hoveredNode = node;
		}
		else
		{
			_hoveredNode = node.ContainsGlobalPoint(e.X, e.Y) ? node : null;
		}

		HandleHoverUnhover(_hoveredNode, prevHovered);
		HandleEnterLeave(_hoveredNode, prevHovered);

		if (node == null)
		{
			return;
		}

		Vector2f local = node.MapToLocal(e.X, e.Y);
		MouseMoveEvent ev = new(local.X, local.Y, e.X, e.Y, _currentMouseState, Modifiers);
		SendMouseEvent(node, ev);
	}

	public void OnMouseScrolled(MouseWheelScrollEventArgs e)
	{
		Node? receiver = MouseAcceptingNodeAt(e.X, e.Y);
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

		Vector2f local = receiver.MapToLocal(e.X, e.Y);
		MouseScrollEvent ev = new(scrollX, scrollY, local.X, local.Y, e.X, e.Y, _currentMouseState, modifiers);
		SendMouseEvent(receiver, ev);
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
		_root.CalculateLayout();
		_root.UpdateLayout(0, 0);
		_root.DrawHierarchy(window, new Vector2f(), new FloatRect(0, 0, _root.Width, _root.Height));
	}

	private Node? SendMouseEvent(Node receiver, MouseEvent e)
	{
		Node? cur = receiver;
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

	private static Node NodeAtHelper(Node node, float x, float y)
	{
		while (true)
		{
			Node? child = node.ChildAt(x, y);
			if (child == null)
			{
				return node;
			}

			node = child;
			x -= child.PositionX;
			y -= child.PositionY;
		}
	}

	private static void HandleHoverUnhover(Node? hovered, Node? unhovered)
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

	private static void HandleEnterLeave(Node? hovered, Node? unhovered)
	{
		if (hovered == unhovered)
		{
			return;
		}

		EnterEvent enterEvent = EnterEvent.Instance;
		LeaveEvent leaveEvent = LeaveEvent.Instance;

		int hoverDepth = GetNodeDepth(hovered);
		int unhoverDepth = GetNodeDepth(unhovered);

		Node? topHovered = hovered;
		Node? topUnhovered = unhovered;

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

		Node? curUnhovered = unhovered;
		while (curUnhovered != topUnhovered)
		{
			curUnhovered !.HandleEvent(leaveEvent);
			curUnhovered = curUnhovered.Parent;
		}

		Node? curHovered = hovered;
		while (curHovered != topHovered)
		{
			curHovered!.HandleEvent(enterEvent);
			curHovered = curHovered.Parent;
		}
	}

	private static int GetNodeDepth(Node? node)
	{
		int depth = 0;
		Node? cur = node;
		while (cur != null)
		{
			depth++;
			cur = cur.Parent;
		}

		return depth;
	}
}