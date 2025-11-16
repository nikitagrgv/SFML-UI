using SFML.Graphics;
using SFML.Window;
using SFMLUI;

namespace SFML_UI;

public class WindowProxy : IWindowProxy
{
	private readonly static int MinKey = GetMinKey();
	private readonly static int MaxKey = GetMaxKey();
	private readonly static int NumValues = MaxKey - MinKey + 1;

	private readonly List<Cursor?> _cursors = new(NumValues);

	private readonly RenderWindow _window;

	public WindowProxy(RenderWindow window)
	{
		_window = window;
		_cursors.AddRange(Enumerable.Repeat<Cursor?>(null, NumValues));
	}

	public void SetCursor(CursorType cursorType)
	{
		int offset = GetOffset(cursorType);
		Cursor? cursor = _cursors[offset];
		if (cursor == null)
		{
			cursor = new Cursor(ToSfmlCursor(cursorType));
			_cursors[offset] = cursor;
		}

		_window.SetMouseCursor(cursor);
	}

	private static Cursor.CursorType ToSfmlCursor(CursorType cursorType)
	{
		return cursorType switch
		{
			CursorType.Arrow => Cursor.CursorType.Arrow,
			CursorType.ArrowWait => Cursor.CursorType.ArrowWait,
			CursorType.Wait => Cursor.CursorType.Wait,
			CursorType.Text => Cursor.CursorType.Text,
			CursorType.Hand => Cursor.CursorType.Hand,
			CursorType.SizeHorizontal => Cursor.CursorType.SizeHorizontal,
			CursorType.SizeVertical => Cursor.CursorType.SizeVertical,
			CursorType.SizeTopLeftBottomRight => Cursor.CursorType.SizeTopLeftBottomRight,
			CursorType.SizeBottomLeftTopRight => Cursor.CursorType.SizeBottomLeftTopRight,
			CursorType.SizeLeft => Cursor.CursorType.SizeLeft,
			CursorType.SizeRight => Cursor.CursorType.SizeRight,
			CursorType.SizeTop => Cursor.CursorType.SizeTop,
			CursorType.SizeBottom => Cursor.CursorType.SizeBottom,
			CursorType.SizeTopLeft => Cursor.CursorType.SizeTopLeft,
			CursorType.SizeBottomRight => Cursor.CursorType.SizeBottomRight,
			CursorType.SizeBottomLeft => Cursor.CursorType.SizeBottomLeft,
			CursorType.SizeTopRight => Cursor.CursorType.SizeTopRight,
			CursorType.SizeAll => Cursor.CursorType.SizeAll,
			CursorType.Cross => Cursor.CursorType.Cross,
			CursorType.Help => Cursor.CursorType.Help,
			CursorType.NotAllowed => Cursor.CursorType.NotAllowed,
			_ => throw new NotImplementedException(),
		};
	}

	private static int GetOffset(CursorType key)
	{
		int value = (int)key;
		return value - MinKey;
	}

	private static int GetMinKey()
	{
		int min = Enum.GetValues<CursorType>().Cast<int>().Min();
		return min;
	}

	private static int GetMaxKey()
	{
		int max = Enum.GetValues<CursorType>().Cast<int>().Max();
		return max;
	}
}