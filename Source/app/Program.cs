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
                    BbgConfig.DemoMode = true;
                    break;
                case 1:
                    if (!BbgAuth.Authenticate()) { Environment.Exit(1); }
                    break;
                case 2:
                    // Bloomberg SAPI on default server or user-provided server
                    break;
                case 3:
                    BbgConfig.Server.Hostname = "localhost";
                    break;
                default:
                    Environment.Exit(0);
                    break;
            }
            Application.Init();
            var bbgConnection = new BbgConnection().DemoMode(BbgConfig.DemoMode).Load().Start();
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
            "Demo Mode",
            "Authenticate to Bloomberg SAPI",
            $"Connect to Bloomberg via SAPI on server/{BbgConfig.Server.Hostname} ({BbgConfig.Server.IPAddress})",
            $"Connect to Bloomberg Terminal on localhost/{BbgConfig.Client.Hostname} ({BbgConfig.Client.IPAddress})",
            "Exit"
        };
        return new ConsoleMenu(menuItems).Show("Connect to Bloomberg:");
    }
}
