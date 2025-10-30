namespace SFMLUI;

public class Event
{
}

public class LayoutChangeEvent : Event
{
	public static LayoutChangeEvent Instance { get; } = new();
}

public class HoverEvent : Event
{
	public static HoverEvent Instance { get; } = new();
}

public class UnhoverEvent : Event
{
	public static UnhoverEvent Instance { get; } = new();
}