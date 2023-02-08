using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

using Bloomberglp.Blpapi;

namespace library;

public partial class BbgConnection
{
    public event EventHandler<BbgEventArgs>? CollectionChanged;
    public Session BbgMarketDataSession { get; init; }
    public Session BbgReferenceDataSession { get; init; }
    public Service? BbgReferenceDataService { get; set; }
    public DemoSession? DemoSession { get; set; }
    public BbgSubscription BbgSubscription { get; init; }
    public ConcurrentDictionary<Tuple<string, string>, BbgResponse> BbgMarketDataResponses { get; init; } = new();
    public BbgConnection()
    {
        var bbgHandler = new Bloomberglp.Blpapi.EventHandler(BbgHandler);
        BbgMarketDataSession = new Session(BbgConfig.Options.MarketData, bbgHandler);
        Debug.Assert(BbgMarketDataSession is not null);
        BbgReferenceDataSession = new Session(BbgConfig.Options.ReferenceData, bbgHandler);
        Debug.Assert(BbgReferenceDataSession is not null);
        BbgSubscription = new BbgSubscription();
    }
    public BbgConnection Load()
    {
        var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var path = Path.Combine(myDocuments, BbgConfig.Filename);
        if (File.Exists(path))
        {
            var contents = File.ReadAllText(path);
            var savedSettings = JsonSerializer.Deserialize<BbgConfig.SavedSettings>(contents) ?? new BbgConfig.SavedSettings();
            BbgSubscription.AddTopics(savedSettings.Topics);
            BbgSubscription.AddFields(savedSettings.Fields);
        }
        return this;
    }
    public BbgConnection Start()
    {
        BbgMarketDataSession.Start();
        Debug.Assert(BbgMarketDataSession is not null);
        BbgReferenceDataSession.Start();
        Debug.Assert(BbgReferenceDataSession is not null);
        var referenceServiceIsOpen = BbgReferenceDataSession.OpenService(BbgConfig.Options.ReferenceData.DefaultSubscriptionService);
        Debug.Assert(referenceServiceIsOpen);
        BbgReferenceDataService = BbgReferenceDataSession.GetService(BbgConfig.Options.ReferenceData.DefaultSubscriptionService);
        Debug.Assert(BbgReferenceDataService is not null);

        var list = BbgSubscription.ToList();
        if (list.Any())
        {
            BbgMarketDataSession.Subscribe(list);
            UpdateReferenceData();
        }
        return this;
    }
    public BbgConnection Restart() => Clear().Load().Start();
    public BbgConnection DemoMode(bool startDemoSession)
    {
        if (startDemoSession)
        {
            BbgSubscription.AddTopics(DemoSession.Topics);
            DemoSession = new DemoSession(this);
            DemoSession.CollectionChanged += (object? sender, BbgEventArgs e) => CollectionChanged?.Invoke(sender, e);
        }
        return this;
    }
    public BbgConnection Save()
    {
        var existingSubscriptions = BbgMarketDataSession.GetSubscriptions();
        if (existingSubscriptions.Any())
        {
            var topics = existingSubscriptions.Select(t => t.CorrelationID.Object.ToString()).OrderBy(t => t).Cast<string>();
            var fields = existingSubscriptions.First().SubscriptionString.Contains("fields=")
                ? Regex.Split(existingSubscriptions.First().SubscriptionString, "fields=")[1].Split(',').OrderBy(t => t).ToArray()
                : new string[] { };
            var savedSettings = new BbgConfig.SavedSettings { Topics = topics, Fields = fields };
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var jsonString = JsonSerializer.Serialize(savedSettings, jsonOptions);
            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var path = Path.Combine(myDocuments, BbgConfig.Filename);
            File.WriteAllText(path, jsonString);
        }
        return this;
    }
    public BbgConnection Clear()
    {
        BbgSubscription.Clear();
        BbgMarketDataResponses.Clear();
        return this;
    }
    public BbgConnection ModifySubscriptions()
    {
        try
        {
            var existingSubscriptions = BbgMarketDataSession.GetSubscriptions();
            var existingTopics = existingSubscriptions.Select(t => t.CorrelationID.Object.ToString()).Cast<string>();
            var proposedSubscriptions = BbgSubscription.ToList();
            var proposedTopics = proposedSubscriptions.Select(t => t.CorrelationID.Object.ToString()).Cast<string>();
            var topicsToAdd = proposedTopics.Except(existingTopics);
            var topicsToRemove = existingTopics.Except(proposedTopics);
            var topicsToResubscribe = existingTopics.Intersect(proposedTopics);
            var subscriptionsToAdd = proposedSubscriptions.Where(t => topicsToAdd.Contains(t.CorrelationID.Object.ToString())).ToList();
            var subscriptionsToRemove = existingSubscriptions.Where(t => topicsToRemove.Contains(t.CorrelationID.Object.ToString())).ToList();
            var subscriptionsToResubscribe = proposedSubscriptions.Where(t => topicsToResubscribe.Contains(t.CorrelationID.Object.ToString())).ToList();
            if (subscriptionsToAdd.Any())
            {
                BbgMarketDataSession.Subscribe(subscriptionsToAdd);
            }
            if (subscriptionsToRemove.Any())
            {
                BbgMarketDataSession.Unsubscribe(subscriptionsToRemove);
            }
            if (subscriptionsToResubscribe.Any())
            {
                BbgMarketDataSession.Resubscribe(subscriptionsToResubscribe);
            }
            UpdateReferenceData();
        }
        catch (Exception ex)
        {
            var message = $"BbgConnection:ModifySubscriptions(): {ex.Message}";
            Debug.WriteLine(message);
            Console.WriteLine(message);
            return Restart();
        }
        return this;
    }
    public BbgConnection AddTopic(string ticker) { BbgSubscription.AddTopic(ticker); return this; }
    public BbgConnection RemoveTopic(string ticker) { BbgSubscription.RemoveTopic(ticker); return this; }
    public BbgConnection AddField(string field) { BbgSubscription.AddField(field); return this; }
    public void Stop()
    {
        BbgMarketDataSession.Stop();
        BbgReferenceDataSession.Stop();
    }
    private void UpdateReferenceData()
    {
        Debug.Assert(BbgReferenceDataService is not null);
        Request request = BbgReferenceDataService.CreateRequest("ReferenceDataRequest");
        foreach(var topic in BbgSubscription.Topics) { request.Append(new Name("securities"), topic); }
        foreach(var field in BbgSubscription.Fields) { request.Append(new Name("fields"), field); }
        Debug.Assert(BbgReferenceDataSession is not null);
        BbgReferenceDataSession.SendRequest(request, null);
    }
    private void BbgHandler(Event eventObj, Session session)
    {
        foreach (Message message in eventObj)
        {
            var eventType = eventObj.Type;

            switch (eventType)
            {
                case Event.EventType.SUBSCRIPTION_DATA:

                    var elements = message.Elements.Where(t => t is not null);
                    foreach (var element in elements)
                    {
                        var topic = message.CorrelationID.Object.ToString();
                        var field = element.Name.ToString();
                        if (topic is not null && BbgSubscription.IsFieldOfInterest(field))
                        {
                            if (element.IsNull) { continue; }
                            var key = Tuple.Create(topic, field);
                            var value = element.GetValueAsString();
                            var response = new BbgResponse(topic, field, value);
                            BbgMarketDataResponses.AddOrUpdate(key, response, (k, v) => response);
                            CollectionChanged?.Invoke(this, new BbgEventArgs(response));
                            Debug.WriteLine($"{eventType} {response}");
                            Console.WriteLine($"{eventType} {response}");
                        }
                    }
                    break;

                case Event.EventType.RESPONSE:
                case Event.EventType.PARTIAL_RESPONSE:

                    if (!message.HasElement("securityData")) { break; }
                    var securities = message.GetElement("securityData");
                    Debug.Assert(securities is not null);
                    for (int i = 0; i < securities.NumValues; i++)
                    {
                        var security = securities.GetValueAsElement(i);
                        if (security.IsNull) { continue; }
                        var topic = security.GetElementAsString("security");
                        Debug.Assert(!string.IsNullOrEmpty(topic));
                        var fields = security.GetElement("fieldData");
                        Debug.Assert(fields is not null);
                        for (int j = 0; j < fields.NumElements; j++)
                        {
                            var element = fields.GetElement(j);
                            if (element.IsNull) { continue; }
                            var field = element.Name.ToString();
                            if (topic is not null)
                            {
                                var key = Tuple.Create(topic, field);
                                var value = element.GetValueAsString();
                                var response = new BbgResponse(topic, field, value);
                                BbgMarketDataResponses.AddOrUpdate(key, response, (k, v) => response);
                                CollectionChanged?.Invoke(this, new BbgEventArgs(response));
                                Debug.WriteLine($"{eventType} {response}");
                                Console.WriteLine($"{eventType} {response}");
                            }
                        }
                    }
                    break;

                default:

                    var messageText = Regex.Replace(message.AsElement.ToString(), @"\s+", " ")?.Trim();
                    Debug.WriteLine($"{eventType} {messageText}");
                    Console.WriteLine($"{eventType} {messageText}");
                    break;
            }
        }
    }
}