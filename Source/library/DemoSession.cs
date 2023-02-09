namespace library;

public class DemoSession
{
    private readonly Timer demoTimer;
    private readonly BbgConnection bbgConnection;
    public event EventHandler<BbgEventArgs>? CollectionChanged;
    public DemoSession(BbgConnection bbgConnection)
    {
        this.bbgConnection = bbgConnection;
        this.demoTimer = new Timer(CallBack, null, 1000, 100);
    }
    private void CallBack(object? state)
    {
        var bbgSubscriptions = bbgConnection.BbgSubscription.ToList();
        var topics = bbgSubscriptions.Select(t => t.CorrelationID.Object.ToString());
        var fields = BbgConfig.Default.Fields.ToList();
        string randomValue(string field)
        {
            switch(field)
            {
                case "LAST_PRICE":
                case "OPEN":
                case "BID":
                case "ASK":
                case "HIGH":
                case "LOW":
                case "PX_OFFICIAL_CLOSE_RT":
                    return (new Random().NextDouble() * 200 + 1).ToString("0.00");
                case "PRICE_CHANGE_ON_DAY_RT":
                    return (new Random().NextDouble() * 200 - 100).ToString("0.00");
                case "LAST_DIR":
                    return new Random().Next(0, 2) == 0 ? "+1" : "-1";
                case "VOLUME":
                    return new Random().Next(1_000, 10_000_000).ToString("#,#");
                case "PRICE_LAST_TIME_RT":
                    return DateTime.Now.AddSeconds(new Random().Next(0, 9) * -1).ToString("HH:mm:ss");
                default:
                    return string.Empty;
            }
        }
        foreach (var topic in topics)
        {
            var randomField = fields[new Random().Next(0, fields.Count())];
            var bbgResponse = new BbgResponse(topic!, randomField, randomValue(randomField));
            var bbgEventArgs = new BbgEventArgs(bbgResponse);
            CollectionChanged?.Invoke(this, bbgEventArgs);
        }
    }
    internal static readonly IEnumerable<string> Topics = new List<string>
    {
        "MSFT US Equity",
        "AAPL US Equity",
        "TSLA US Equity",
        "CHTR US Equity",
        "NVDA US Equity",
        "AMZN US Equity",
        "GOOG US Equity",
        "NFLX US Equity",
        "META US Equity"
    };
}
