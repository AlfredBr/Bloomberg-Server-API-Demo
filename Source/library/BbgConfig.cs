using Bloomberglp.Blpapi;

using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.RegularExpressions;

using menu;

namespace library;

public static class BbgConfig
{
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
    public static bool Authenticate()
    {
        Console.ResetColor();
        var savedSettings = BbgConfig.SavedSettings.Load();
        var clientIPAddress = BbgConfig.Client.IPAddress;
        Console.Write($"Client IP Address [{clientIPAddress}]: ");
        var ipAddress = Console.ReadLine();
        if (string.IsNullOrEmpty(ipAddress)) { ipAddress = clientIPAddress; }
        var pattern = @"^([0-9]{1,3}\.){3}[0-9]{1,3}$";
        var regex = new Regex(pattern);
        if (!regex.IsMatch(ipAddress))
        {
            ConsoleBox.WriteLine("FAILURE", "IP Address is Required");
            return false;
        }
        Console.Write($"Bloomberg Username [{savedSettings.BbgUsername}]: ");
        var bbgUsername = Console.ReadLine();
        if (string.IsNullOrEmpty(bbgUsername))
        {
            bbgUsername = savedSettings.BbgUsername;
        }
        if (string.IsNullOrEmpty(bbgUsername))
        {
            ConsoleBox.WriteLine("FAILURE", "Bloomberg Username is Required");
            return false;
        }
        Console.Write($"Bloomberg UUID [{savedSettings.BbgUuid}]: ");
        var uuid = Console.ReadLine();
        if (string.IsNullOrEmpty(uuid))
        {
            uuid = savedSettings.BbgUuid.ToString();
        }
        var bbgUuid = Int32.TryParse(uuid, out int val) ? Math.Abs(val) : 0;
        if (bbgUuid == 0)
        {
            ConsoleBox.WriteLine("FAILURE", "Bloomberg UUID is Required");
            return false;
        }
        var session = new Session(BbgConfig.Options.Authentication);
        Debug.Assert(session is not null);
        if (!session.Start())
        {
            ConsoleBox.WriteLine("FAILURE", "Bloomberg Session Start Failed");
            return false;
        }
        var isOpenService = session.OpenService(BbgConfig.Service.Authentication);
        Debug.Assert(isOpenService);
        var service = session.GetService(BbgConfig.Service.Authentication);
        Debug.Assert(service is not null);
        var request = service.CreateAuthorizationRequest();
        Debug.Assert(request is not null);
        request.Set(Name.GetName("ipAddress"), ipAddress);
        request.Set(Name.GetName("uuid"), bbgUuid);
        var identity = session.CreateIdentity();
        var correlationId = new CorrelationID(bbgUsername);
        session.SendAuthorizationRequest(request, identity, correlationId);
        ConsoleBox.WriteLine("WAITING", "Please wait for Authorization Response");

        while (true)
        {
            var eventObj = session.NextEvent();
            foreach (Message message in eventObj)
            {
                var eventType = eventObj.Type;
                string? messageText = Regex.Replace(message.AsElement.ToString(), @"\s+", " ")?.Trim();
                Debug.WriteLine($"{eventType} {messageText}");
                Console.WriteLine($"{eventType} {messageText}");

                switch (eventType)
                {
                    case Event.EventType.RESPONSE:
                        var messageType = message.MessageType.ToString();
                        if (messageType.Contains("success", StringComparison.OrdinalIgnoreCase))
                        {
                            SavedSettings.Save(bbgUsername, bbgUuid);
                            return true;
                        }
                        if (messageType.Contains("failure", StringComparison.OrdinalIgnoreCase))
                        {
                            var reason = message.AsElement.GetElement(Name.GetName("reason"));
                            var failureMessage = reason.GetElement(Name.GetName("message")).GetValue()?.ToString()?.Trim() ?? string.Empty;
                            ConsoleBox.SetOutlineColor(ConsoleColor.Red).WriteLine(messageType.ToUpper(), failureMessage);
                            return false;
                        }
                        break;

                    case Event.EventType.TIMEOUT:
                        ConsoleBox.WriteLine("FAILURE", "Bloomberg Authorization Request Timed Out");
                        return false;

                    default:
                        break;
                }
            }
        }
    }
    public class SavedSettings
    {
        public static string Filename => "BbgConsole.SavedSettings.json";
        public IEnumerable<string> Topics { get; set; } = Array.Empty<string>();
        public IEnumerable<string> Fields { get; set; } = Array.Empty<string>();
        public string BbgUuid { get; set; } = string.Empty;
        public string BbgUsername { get; set; } = string.Empty;
        public static SavedSettings Load()
        {
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path = Path.Combine(myDocuments, BbgConfig.SavedSettings.Filename);
            if (!File.Exists(path)) { return new BbgConfig.SavedSettings(); }
            var contents = File.ReadAllText(path);
            var savedSettings = JsonSerializer.Deserialize<BbgConfig.SavedSettings>(contents) ?? new BbgConfig.SavedSettings();
            return savedSettings;
        }
        public static void Save(SavedSettings savedSettings)
        {
            if (DemoMode) { return; }
            if (savedSettings is null) { throw new ArgumentNullException(nameof(savedSettings)); }
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(savedSettings, jsonOptions);
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path = Path.Combine(myDocuments, BbgConfig.SavedSettings.Filename);
            File.WriteAllText(path, jsonString);
        }
        public static void Save(string bbgUsername, int bbgUuid)
        {
            if (DemoMode) { return; }
            if (string.IsNullOrEmpty(bbgUsername)) { throw new ArgumentNullException(nameof(bbgUsername)); }
            if (bbgUuid == 0) { throw new ArgumentNullException(nameof(bbgUuid)); }
            var savedSettings = Load();
            savedSettings.BbgUsername = bbgUsername;
            savedSettings.BbgUuid = bbgUuid.ToString();
            Save(savedSettings);
        }
    }
}