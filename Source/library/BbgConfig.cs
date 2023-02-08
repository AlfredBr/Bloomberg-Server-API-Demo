using Bloomberglp.Blpapi;

namespace library;

public static class BbgConfig
{
    public static string Filename => "BbgConsole.SavedSettings.json";
    public static class Server
    {
        public static string Host { get; set; } = "<your bloomberg hostname or ip>";
        public static int Port { get; set; } = 8194;
    }
    public static class Service
    {
        public static string MarketData => "//blp/mktdata";
        public static string ReferenceData => "//blp/refdata";
        public static string Emsx => "//blp/emapisvc";
    }
    public static class Options
    {
        public static SessionOptions MarketData => new()
        {
            ServerHost = Server.Host,
            ServerPort = Server.Port,
            DefaultSubscriptionService = Service.MarketData
        };
        public static SessionOptions ReferenceData => new()
        {
            ServerHost = Server.Host,
            ServerPort = Server.Port,
            DefaultSubscriptionService = Service.ReferenceData
        };
        public static SessionOptions Emsx => new()
        {
            ServerHost = Server.Host,
            ServerPort = Server.Port,
            DefaultSubscriptionService = Service.Emsx
        };
    }
    public class SavedSettings
    {
        public IEnumerable<string> Topics { get; set; } = Array.Empty<string>();
        public IEnumerable<string> Fields { get; set; } = Array.Empty<string>();
    }
}