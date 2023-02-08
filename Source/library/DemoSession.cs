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
        foreach (var topic in topics)
        {
            var randomValue = new Random().NextDouble() * 200 + 100;
            var bbgResponse = new BbgResponse(topic!, "LAST_PRICE", randomValue.ToString("0.00"));
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
