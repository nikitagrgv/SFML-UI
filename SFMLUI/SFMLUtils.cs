using SFML.Window;

namespace SFMLUI;

public class KeyRegistry
{
	private readonly static int MinKey = GetMinKey();
	private readonly static int MaxKey = GetMaxKey();
	private readonly static int NumValues = MaxKey - MinKey + 1;

	private readonly List<bool> _keyStates = new(NumValues);

	public KeyRegistry()
	{
		_keyStates.AddRange(Enumerable.Repeat(false, NumValues));
	}

	public bool IsPressed(Keyboard.Key key)
	{
		int offset = GetOffset(key);
		return _keyStates[offset];
	}

	public void SetPressed(Keyboard.Key key, bool pressed)
	{
		int offset = GetOffset(key);
		_keyStates[offset] = pressed;
	}

	private static int GetOffset(Keyboard.Key key)
	{
		int value = (int)key;
		return value - MinKey;
	}

	private static int GetMinKey()
	{
		int min = Enum.GetValues<Keyboard.Key>().Cast<int>().Min();
		return min;
	}

	private static int GetMaxKey()
	{
		int max = Enum.GetValues<Keyboard.Key>().Cast<int>().Max();
		return max;
	}
}