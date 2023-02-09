using Bloomberglp.Blpapi;

namespace library;

public class BbgSubscription
{
    private readonly List<string> topics = new();
    private readonly List<string> fields = new();
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
        fields.AddRange(BbgConfig.Default.Fields);
    }
    internal IEnumerable<string> Topics => topics.Distinct().OrderBy(t => t);
    internal IEnumerable<string> Fields => fields.Distinct().OrderBy(t => t).Select(t => t.ToUpperInvariant());
    internal IList<Subscription> ToList()
    {
        var bbgFields = string.Join(",", Fields);
        return Topics.Select(topic => new Subscription(topic, bbgFields, new CorrelationID(topic))).ToList();
    }
}
