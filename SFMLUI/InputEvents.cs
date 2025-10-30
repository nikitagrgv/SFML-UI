namespace SFMLUI;

public class InputEvent : Event
{
	public Modifier Modifiers;
}

public class MouseEvent(float localX, float localY, float globalX, float globalY, MouseButton pressedButtons)
	: InputEvent
{
	public float GlobalX { get; set; } = globalX;
	public float GlobalY { get; set; } = globalY;
	public float LocalX { get; set; } = localX;
	public float LocalY { get; set; } = localY;
	public MouseButton PressedButtons { get; set; } = pressedButtons;
}

public class MousePressEvent(
	float localX,
	float localY,
	float globalX,
	float globalY,
	MouseButton button,
	MouseButton pressedButtons)
	: MouseEvent(localX, localY, globalX, globalY, pressedButtons)
{
	public MouseButton Button { get; set; } = button;
}

public class MouseReleaseEvent(
	float localX,
	float localY,
	float globalX,
	float globalY,
	MouseButton button,
	MouseButton pressedButtons)
	: MouseEvent(localX, localY, globalX, globalY, pressedButtons)
{
	public MouseButton Button { get; set; } = button;
}

public class MouseMoveEvent(
	float localX,
	float localY,
	float globalX,
	float globalY,
	MouseButton pressedButtons)
	: MouseEvent(localX, localY, globalX, globalY, pressedButtons);

public class MouseScrollEvent(
	float scrollX,
	float scrollY,
	float localX,
	float localY,
	float globalX,
	float globalY,
	MouseButton pressedButtons)
	: MouseEvent(localX, localY, globalX, globalY, pressedButtons)
{
	public float ScrollX { get; set; } = scrollX;
	public float ScrollY { get; set; } = scrollY;
}