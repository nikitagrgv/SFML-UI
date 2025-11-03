namespace SFMLUI;

public class Event
{
}

public class RootChangeEvent : Event
{
	public static RootChangeEvent Instance { get; } = new();
}

public class ParentChangeEvent : Event
{
	public Node? OldParent { get; set; }
	public Node? NewParent { get; set; }
}

public class ChildAddEvent : Event
{
	public Node? Child { get; set; }
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