using System.Reflection;

namespace SFMLUI;

internal static class ResourceStream
{
	public static Stream? GetResourceStream(string filename)
	{
		Stream? stream = Assembly
			.GetExecutingAssembly()
			.GetManifestResourceStream($"SFMLUI.res.{filename}");
		return stream;
	}
}