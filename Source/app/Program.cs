using Terminal.Gui;

using display;
using library;
using menu;

namespace app;

public static class Program
{
    public static void Main(string[] args)
    {
        try
        {
            switch(Menu(args))
            {
                case 0:
                    // Demo mode - no Bloomberg connection required
                    BbgConfig.DemoMode = true;
                    break;
                case 1:
                    // Authenticed access to Bloomberg SAPI on default server or user-provided server
                    if (!BbgConfig.Authenticate()) { Environment.Exit(1); }
                    break;
                case 2:
                    // Unauthenticed access to Bloomberg SAPI on default server or user-provided server
                    break;
                case 3:
                    // Authenticated access to Bloomberg Terminal on localhost
                    BbgConfig.Server.Hostname = "localhost";
                    break;
                default:
                    // Exit the application
                    Environment.Exit(0);
                    break;
            }
            Console.ResetColor();
            Application.Init();
            var bbgConnection = new BbgConnection().DemoMode().Load().Start();
            Application.Run(new BbgDisplay(bbgConnection));
            bbgConnection.Save().Stop();
        }
        finally
        {
            Application.Shutdown();
            Console.ResetColor();
        }
    }

    private static int Menu(string[] args)
    {
        string userProvidedHostname = args?.FirstOrDefault()??string.Empty;
        if (userProvidedHostname.Length > 0)
        {
            BbgConfig.Server.Hostname = userProvidedHostname;
            return 1;
        }
        var menuItems = new string[]
        {
            "Demo Mode (with fake data)",
            "Authenticate to Bloomberg SAPI (with username and uuid)",
            $"Connect to Bloomberg via SAPI on server/{BbgConfig.Server.Hostname} ({BbgConfig.Server.IPAddress})",
            $"Connect to Bloomberg Terminal on localhost/{BbgConfig.Client.Hostname} ({BbgConfig.Client.IPAddress})",
            "Exit"
        };
        return new ConsoleMenu(menuItems).Show("Connect to Bloomberg:");
    }
}
