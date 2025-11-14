using SFML.System;
using SFML.Window;

namespace SFMLUI;

public class InputEvent(
	Modifier modifier) : Event
{
	public Modifier Modifiers = modifier;
}

public class MouseEvent(
	float localX,
	float localY,
	float globalX,
	float globalY,
	MouseButton pressedButtons,
	Modifier modifiers) : InputEvent(modifiers)
{
	public float GlobalX { get; set; } = globalX;
	public float GlobalY { get; set; } = globalY;
	public float LocalX { get; set; } = localX;
	public float LocalY { get; set; } = localY;

	public MouseButton PressedButtons { get; set; } = pressedButtons;

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

public class TextEvent(
	string text) : Event
{
	public string Text { get; set; } = text;
}

public class KeyEvent(
	Keyboard.Key key,
	Modifier modifier) : InputEvent(modifier)
{
	public Keyboard.Key Key { get; set; } = key;
}

public class KeyPressEvent(
	Keyboard.Key key,
	Modifier modifiers,
	bool repeat) : KeyEvent(key, modifiers)
{
	public bool Repeat { get; set; } = repeat;
}

public class KeyReleaseEvent(
	Keyboard.Key key,
	Modifier modifiers) : KeyEvent(key, modifiers)
{
}