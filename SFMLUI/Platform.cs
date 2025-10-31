using System.Runtime.InteropServices;
using OpenTK;

namespace SFMLUI;

internal class GLBindingsContext : IBindingsContext
{
	[DllImport("opengl32.dll", EntryPoint = "wglGetProcAddress", CharSet = CharSet.Ansi)]
	private static extern IntPtr wglGetProcAddress(string name);

	[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
	private static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr GetModuleHandle(string lpModuleName);

	public IntPtr GetProcAddress(string procName)
	{
		IntPtr address = wglGetProcAddress(procName);
		if (address != IntPtr.Zero && address.ToInt64() != 1 && address.ToInt64() != 2 && address.ToInt64() != 3 &&
		    address.ToInt64() != -1)
		{
			return address;
		}

		IntPtr module = GetModuleHandle("opengl32.dll");
		return GetProcAddress(module, procName);
	}
}