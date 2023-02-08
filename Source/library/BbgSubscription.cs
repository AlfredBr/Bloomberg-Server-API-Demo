using Bloomberglp.Blpapi;

using System.Diagnostics;

namespace library;

public class BbgSubscription
{
    private readonly List<string> topics = new();
    private readonly List<string> fields = new();
    private readonly List<string> defaultFields = new()
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
    public BbgSubscription()
    {
        Clear();
    }
    internal void AddField(string field) => fields.Add(field.ToUpperInvariant());
    internal void AddFields(IEnumerable<string> fields) => this.fields.AddRange(fields.Select(f => f.ToUpperInvariant()));
    internal void AddTopic(string topic)
    {
        if (topic.Length > 0 && topic.IndexOf(' ') < 0)
        {
            topic = $"{topic.ToUpperInvariant()} US Equity";
        }
        this.topics.Add(topic.Trim());
    }
    internal void AddTopics(IEnumerable<string> topics) => topics.ToList().ForEach(t => AddTopic(t));
    internal void RemoveTopic(string topic) => topics.Remove(topic.Trim());
    internal bool IsFieldOfInterest(string field) => fields.Contains(field);
    internal void Clear()
    {
        topics.Clear();
        fields.Clear();
        fields.AddRange(defaultFields);
    }
    internal IEnumerable<string> Topics => topics.Distinct().OrderBy(t => t);
    internal IEnumerable<string> Fields => fields.Distinct().OrderBy(t => t).Select(t => t.ToUpperInvariant());
    internal IList<Subscription> ToList()
    {
        var bbgFields = string.Join(",", Fields);
        return Topics.Select(topic => new Subscription(topic, bbgFields, new CorrelationID(topic))).ToList();
    }
}