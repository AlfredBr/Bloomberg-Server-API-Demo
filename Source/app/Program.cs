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
                    // Bloomberg SAPI on default server or user-provided server
                    break;
                case 2:
                    BbgConfig.Server.Hostname = "localhost";
                    break;
                case 3:
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
            $"Bloomberg SAPI on server/{BbgConfig.Server.Hostname} ({BbgConfig.Server.IPAddress})",
            $"Bloomberg Terminal on localhost/{BbgConfig.Client.Hostname} ({BbgConfig.Client.IPAddress})",
            "Exit"
        };
        return new ConsoleMenu(menuItems).Show("Connect to Bloomberg:");
    }
}
