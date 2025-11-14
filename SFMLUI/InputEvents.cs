using SFML.System;

namespace SFMLUI;

public class InputEvent : Event
{
	public Modifier Modifiers;
}

public class MouseEvent : InputEvent
{
	public MouseEvent(
		float localX,
		float localY,
		float globalX,
		float globalY,
		MouseButton pressedButtons,
		Modifier modifiers)
	{
		GlobalX = globalX;
		GlobalY = globalY;
		LocalX = localX;
		LocalY = localY;
		PressedButtons = pressedButtons;
		Modifiers = modifiers;
	}

	public float GlobalX { get; set; }
	public float GlobalY { get; set; }
	public float LocalX { get; set; }
	public float LocalY { get; set; }
	public MouseButton PressedButtons { get; set; }

	public Vector2f GlobalPos => new(GlobalX, GlobalY);
	public Vector2f LocalPos => new(LocalX, LocalY);
}

public class MousePressEvent(
	float localX,
	float localY,
	float globalX,
	float globalY,
	MouseButton button,
	MouseButton pressedButtons,
	Modifier modifiers)
	: MouseEvent(localX, localY, globalX, globalY, pressedButtons, modifiers)
{
	public MouseButton Button { get; set; } = button;
}

public class MouseReleaseEvent(
	float localX,
	float localY,
	float globalX,
	float globalY,
	MouseButton button,
	MouseButton pressedButtons,
	Modifier modifiers)
	: MouseEvent(localX, localY, globalX, globalY, pressedButtons, modifiers)
{
	public MouseButton Button { get; set; } = button;
}

public class MouseMoveEvent(
	float localX,
	float localY,
	float globalX,
	float globalY,
	MouseButton pressedButtons,
	Modifier modifiers)
	: MouseEvent(localX, localY, globalX, globalY, pressedButtons, modifiers);

public class MouseScrollEvent(
	float scrollX,
	float scrollY,
	float localX,
	float localY,
	float globalX,
	float globalY,
	MouseButton pressedButtons,
	Modifier modifiers)
	: MouseEvent(localX, localY, globalX, globalY, pressedButtons, modifiers)
{
	public float ScrollX { get; set; } = scrollX;
	public float ScrollY { get; set; } = scrollY;
}