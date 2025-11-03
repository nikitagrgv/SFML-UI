using OpenTK.Graphics;
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

	private Vector2i _mousePosition;

	private MouseButton _mouseCapturedButton;
	private MouseButton _currentMouseState;

	public event Action? DrawBegin;
	public event Action? DrawEnd;

	public Node Root => _root;
	public Node? MouseCapturedNode => _mouseCapturedNode;
	public Node? HoveredNode => _hoveredNode;

	// For debug
	public bool EnableClipping { get; set; } = true;
	public bool EnableVisualizer { get; set; } = false;

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
		_root = new Root(this);

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

	public Node? MouseAcceptingNodeAt(Vector2f position)
	{
		Node? node = NodeAt(position);
		if (node == null)
		{
			return null;
		}

		Vector2f local = node.MapToLocal(position);
		while (node != null)
		{
			if (node.AcceptsMouse(local.X, local.Y))
			{
				return node;
			}

			local = node.MapToParent(local);
			node = node.Parent;
		}

		return null;
	}

	public Node? NodeAt(Vector2f position)
	{
		return NodeAtHelper(_root, position);
	}

	public void OnKeyPressed(KeyEventArgs e)
	{
	}

	public void OnMousePressed(MouseButtonEventArgs e)
	{
		MouseButton button = Utils.ToMouseButton(e.Button);
		_currentMouseState |= button;

		Vector2f globalPos = new(e.X, e.Y);

		if (_mouseCapturedNode != null)
		{
			Vector2f local = _mouseCapturedNode.MapToLocal(globalPos);
			MousePressEvent ev = new(local.X, local.Y, e.X, e.Y, button, _currentMouseState, Modifiers);
			SendMouseEvent(_mouseCapturedNode, ev);
		}
		else
		{
			Node? receiver = MouseAcceptingNodeAt(globalPos);
			if (receiver == null)
			{
				return;
			}

			_mouseCapturedButton = button;

			Vector2f local = receiver.MapToLocal(globalPos);
			MousePressEvent ev = new(local.X, local.Y, e.X, e.Y, button, _currentMouseState, Modifiers);
			Node? realReceiver = SendMouseEvent(receiver, ev);
			_mouseCapturedNode = realReceiver;
		}
	}

	public void OnMouseReleased(MouseButtonEventArgs e)
	{
		MouseButton button = Utils.ToMouseButton(e.Button);
		_currentMouseState &= ~button;

		Vector2f globalPos = new(e.X, e.Y);

		if (_mouseCapturedNode == null)
		{
			return;
		}

		Vector2f local = _mouseCapturedNode.MapToLocal(globalPos);
		MouseReleaseEvent ev = new(local.X, local.Y, e.X, e.Y, button, _currentMouseState, Modifiers);
		SendMouseEvent(_mouseCapturedNode, ev);

		if (button == _mouseCapturedButton)
		{
			_mouseCapturedNode = null;
			_mouseCapturedButton = MouseButton.None;

			// TODO: Shitty?
			SFML.Window.MouseMoveEvent moveEvent = new()
			{
				X = e.X,
				Y = e.Y,
			};
			OnMouseMoved(new MouseMoveEventArgs(moveEvent));
		}
	}

	public void OnMouseMoved(MouseMoveEventArgs e)
	{
		_mousePosition.X = e.X;
		_mousePosition.Y = e.Y;

		Node? prevHovered = _hoveredNode;

		Vector2f globalPos = new(e.X, e.Y);

		Node? node = _mouseCapturedNode;
		if (node == null)
		{
			node = MouseAcceptingNodeAt(globalPos);
			_hoveredNode = node;
		}
		else
		{
			_hoveredNode = node.ContainsGlobalPoint(globalPos) ? node : null;
		}

		HandleHoverUnhover(_hoveredNode, prevHovered);
		HandleEnterLeave(_hoveredNode, prevHovered);

		if (node == null)
		{
			return;
		}

		Vector2f local = node.MapToLocal(globalPos);
		MouseMoveEvent ev = new(local.X, local.Y, e.X, e.Y, _currentMouseState, Modifiers);
		SendMouseEvent(node, ev);
	}

	public void OnMouseScrolled(MouseWheelScrollEventArgs e)
	{
		Vector2f globalPos = new(e.X, e.Y);
		Node? receiver = MouseAcceptingNodeAt(globalPos);
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
		_root.CalculateLayout();
		_root.UpdateLayout(0, 0);

		CheckMousePosition();
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

	private void CheckMousePosition()
	{
		// TODO: Shitty?

		Vector2f mousePos = (Vector2f)_mousePosition;
		Node? newHoveredNode = MouseAcceptingNodeAt(mousePos);
		if (newHoveredNode == _hoveredNode)
		{
			return;
		}

		SFML.Window.MouseMoveEvent moveEvent = new()
		{
			X = _mousePosition.X,
			Y = _mousePosition.Y,
		};
		OnMouseMoved(new MouseMoveEventArgs(moveEvent));
	}

	private void DoDraw(RenderWindow window)
	{
		_root.DrawHierarchy(window, new Vector2f(), new FloatRect(0, 0, _root.Width, _root.Height));
		DrawDebug(window);
	}

	private void DrawDebug(RenderWindow window)
	{
		if (_hoveredNode != null)
		{
			Vector2f globalPos = _hoveredNode.GlobalPosition;
			FloatRect geometry = _hoveredNode.InnerLayoutGeometry;
			geometry.Left += globalPos.X;
			geometry.Top += globalPos.Y;

			var shape = new RectangleShape()
			{
				Position = geometry.Position,
				Size = geometry.Size,
			};
			shape.FillColor = new Color(150, 40, 150, 100);

			window.SetView(_view);
			window.Draw(shape);
		}
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

	private static Node NodeAtHelper(Node node, Vector2f position)
	{
		while (true)
		{
			Vector2f maxPos = node.Size - node.ScrollbarSize;
			if (position.X >= maxPos.X || position.Y >= maxPos.Y)
			{
				return node;
			}

			Node? child = node.ChildAt(position);
			if (child == null)
			{
				return node;
			}

			node = child;
			position -= child.Position;
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