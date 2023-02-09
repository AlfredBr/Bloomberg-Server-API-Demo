namespace menu;

public static class ConsoleBox
{
    public static ConsoleColor OutlineColor { get; set; } = ConsoleColor.DarkYellow;
    public static void WriteLine(params string[] contents)
    {
        var lengths = contents.Select(t => t.Length).ToArray();

        Console.WriteLine();
        Console.ForegroundColor = OutlineColor;
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
            Console.ForegroundColor = OutlineColor;
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