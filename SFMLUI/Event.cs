namespace SFMLUI;

public class Event
{
}

public class RootChangedEvent : Event
{
	public static RootChangedEvent Instance { get; } = new();
}

public class LayoutChangeEvent : Event
{
	public static LayoutChangeEvent Instance { get; } = new();
}

public class EnterEvent : Event
{
	public static EnterEvent Instance { get; } = new();
}

public class LeaveEvent : Event
{
	public static LeaveEvent Instance { get; } = new();
}

public class HoverEvent : Event
{
	public static HoverEvent Instance { get; } = new();
}

public class UnhoverEvent : Event
{
	public static UnhoverEvent Instance { get; } = new();
}