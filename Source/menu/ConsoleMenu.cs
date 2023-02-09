using System;

namespace menu;

public class ConsoleMenu
{
    public event EventHandler<EventArgs<int>>? Click;
    public Action<int>? OnClick { get; set; }
    public string? Prompt { get; set; }
    public IList<string> Items { get; set; } = Array.Empty<string>();

    public ConsoleMenu()
    {
        // intentionally left blank
    }
    public ConsoleMenu(string[] items, Action<int>? callback = null) : this()
    {
        Items = items;
        OnClick = callback;
    }
    public int Show(string? prompt = null)
    {
        Console.WriteLine();
        Console.WriteLine(prompt ?? Prompt ?? "Select an option:");
        DisplayMenu();
        SetMenuIndicatorToPosition(0);
        return WaitForUserMenuSelection();
    }
    public int WaitForUserMenuSelection()
    {
        try
        {
            var p = 0;
            ConsoleKeyInfo keyInfo;
            do
            {
                Console.CursorVisible = false;
                keyInfo = Console.ReadKey(true);

                switch (keyInfo.Key)
                {
                    case ConsoleKey.DownArrow:
                        p = Math.Min(++p, Items.Count - 1);
                        SetMenuIndicatorToPosition(p);
                        break;
                    case ConsoleKey.UpArrow:
                        p = Math.Max(0, --p);
                        SetMenuIndicatorToPosition(p);
                        break;
                    case ConsoleKey.Enter:
                        this.OnClick?.Invoke(p);
                        this.Click?.Invoke(this, new EventArgs<int>(p));
                        return p;
                    default:
                        // do nothing on other keys (for now)
                        break;
                }
            } while (keyInfo.Key != ConsoleKey.Escape);
            return -1;
        }
        finally
        {
            Console.CursorVisible = true;
        }
    }
    private void DisplayMenu()
    {
        Console.WriteLine();
        for (var i = 0; i < Items.Count; i++)
        {
            Console.WriteLine($"   {Items[i]}");
        }
    }
    private void SetMenuIndicatorToPosition(int p)
    {
        var cp = Console.GetCursorPosition();
        Console.SetCursorPosition(0, Math.Max(0, cp.Top - Items.Count));

        for (var i = 0; i < Items.Count; i++)
        {
            var indicator = i == p ? " > " : "   ";
            Console.ForegroundColor = i == p ? ConsoleColor.White : ConsoleColor.Gray;
            Console.WriteLine($"{indicator}{Items[i]}");
        }
    }

    public class EventArgs<T> : EventArgs
    {
        public T Value { get; set; }
        public EventArgs(T value)
        {
            Value = value;
        }
    }
}
