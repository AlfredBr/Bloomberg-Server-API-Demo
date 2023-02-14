using Bloomberglp.Blpapi;
using System.Net;
using System.Net.Sockets;

namespace library;

public static class BbgConfig
{
    public static string Filename => "BbgConsole.SavedSettings.json";
    public static bool DemoMode { get; set; } = false;
    public static class Default
    {
        public static IEnumerable<string> Fields => new[]
        {
            "LAST_DIR",
            "LAST_PRICE",
            "PRICE_CHANGE_ON_DAY_RT",
            "PRICE_LAST_TIME_RT",
            "OPEN",
            "BID",
            "ASK",
            "HIGH",
            "LOW",
            "PX_OFFICIAL_CLOSE_RT",
            "VOLUME",
        };
    }
    public static class Server
    {
        public static string Hostname { get; set; } = "BBRG-SAPI";
        public static string IPAddress
        {
            get
            {
                try
                {
                    return Array.Find(Dns.GetHostEntry(Hostname).AddressList, ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString()??string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
        public static int Port { get; set; } = 8194;
    }
    public static class Client
    {
        public static string Hostname => Dns.GetHostName();
        public static string IPAddress
        {
            get
            {
                try
                {
                    return Array.Find(Dns.GetHostEntry(Hostname).AddressList, ip => ip.AddressFamily == AddressFamily.InterNetwork)?.ToString()??string.Empty;
                }
                catch
                {
                    return string.Empty;
                }
            }
        }
    }
    public static class Service
    {
        public static string Authentication => "//blp/apiauth";
        public static string Emsx => "//blp/emapisvc";
        public static string Flds => "//blp/apiflds";
        public static string MarketData => "//blp/mktdata";
        public static string ReferenceData => "//blp/refdata";
    }
    public static class Options
    {
        public static SessionOptions Authentication => new()
        {
            ServerHost = Server.Hostname,
            ServerPort = Server.Port,
            DefaultSubscriptionService = Service.Authentication
        };
        public static SessionOptions MarketData => new()
        {
            ServerHost = Server.Hostname,
            ServerPort = Server.Port,
            DefaultSubscriptionService = Service.MarketData
        };
        public static SessionOptions ReferenceData => new()
        {
            ServerHost = Server.Hostname,
            ServerPort = Server.Port,
            DefaultSubscriptionService = Service.ReferenceData
        };
        public static SessionOptions Emsx => new()
        {
            ServerHost = Server.Hostname,
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