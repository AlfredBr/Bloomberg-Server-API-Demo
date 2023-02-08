using Terminal.Gui;
using display;
using library;

namespace app;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            bool demoMode = args?.FirstOrDefault()?.Equals("demo", StringComparison.OrdinalIgnoreCase)??false;
            Application.Init();
            var bbgConnection = new BbgConnection().DemoMode(demoMode).Load().Start();
            Application.Run(new BbgDisplay(bbgConnection));
            bbgConnection.Save().Stop();
        }
        finally
        {
            Application.Shutdown();
        }
    }
}
