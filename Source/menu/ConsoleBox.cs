namespace menu;

public class ConsoleBox
{
	static ConsoleBox()
	{
		consoleBox = new ConsoleBox();
	}
	public static ConsoleBox SetOutlineColor(ConsoleColor color)
	{
		consoleBox.outlineColor = color;
		return consoleBox;
	}
	public static void WriteLine(params string[] contents)
	{
		consoleBox.WriteLineInternal(contents);
	}
	private static ConsoleBox consoleBox;
    internal ConsoleColor outlineColor = ConsoleColor.DarkYellow;
	internal void WriteLineInternal(params string[] contents)
	{
		int[] lengths = contents.Select(t => t.Length).ToArray();

		Console.WriteLine();
		Console.ForegroundColor = consoleBox.outlineColor;
		Console.Write("┌");
		for (int l = 0; l < lengths.Length - 1; l++)
		{
			var length = lengths[l];
			Console.Write("─".PadRight(length + 2, '─'));
			Console.Write("┬");
		}
		Console.Write("─".PadRight(lengths.Last() + 2, '─'));
		Console.WriteLine("┐");

		foreach (var content in contents)
		{
			Console.Write("│ ");
			Console.ForegroundColor = ConsoleColor.White;
			Console.Write(content);
			Console.ForegroundColor = consoleBox.outlineColor;
			Console.Write(" ");
		}
		Console.WriteLine("│");

		Console.Write("└");
		for (int l = 0; l < lengths.Length - 1; l++)
		{
			var length = lengths[l];
			Console.Write("─".PadRight(length + 2, '─'));
			Console.Write("┴");
		}
		Console.Write("─".PadRight(lengths.Last() + 2, '─'));
		Console.WriteLine("┘");
		Console.ForegroundColor = ConsoleColor.White;
	}
}
public static class ConsoleBoxExtensions
{
    public static void WriteLine(this ConsoleBox consoleBox, params string[] contents)
    {
		consoleBox.WriteLineInternal(contents);
	}
}