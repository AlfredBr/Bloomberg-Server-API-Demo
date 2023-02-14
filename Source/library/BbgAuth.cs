using Bloomberglp.Blpapi;

using menu;

using System.Diagnostics;
using System.Text.RegularExpressions;

namespace library;

public static class BbgAuth
{
    public static bool Authenticate()
    {
        Console.ResetColor();
        var clientIPAddress = BbgConfig.Client.IPAddress;
        Console.Write($"Client IP Address: [{clientIPAddress}]: ");
        var ipAddress = Console.ReadLine() ?? clientIPAddress;
        if (ipAddress.Length == 0) { ipAddress = clientIPAddress; }
        // validate IP address
        var pattern = @"^([0-9]{1,3}\.){3}[0-9]{1,3}$";
        var regex = new Regex(pattern);
        if (!regex.IsMatch(ipAddress))
        {
            ConsoleBox.WriteLine("FAILURE", "IP Address is Required");
            return false;
        }
        Console.Write($"User Bloomberg Username: ");
        var bbgUsername = Console.ReadLine();
        if (string.IsNullOrEmpty(bbgUsername))
        {
            ConsoleBox.WriteLine("FAILURE", "Bloomberg Username is Required");
            return false;
        }
        Console.Write($"User Bloomberg UUID: ");
        var uuid = Console.ReadLine();
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
        request.Set("ipAddress", ipAddress);
        request.Set("uuid", bbgUuid);
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
                            return true;
                        }
                        if (messageType.Contains("failure", StringComparison.OrdinalIgnoreCase))
                        {
                            var reason = message.AsElement.GetElement("reason");
                            var failureMessage = reason.GetElement("message").GetValue()?.ToString()?.Trim()??string.Empty;
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
}