namespace library;

public class BbgEventArgs : EventArgs
{
    new public static readonly BbgEventArgs Empty = new();
    public BbgResponse? BbgResponse { get; init; }
    public BbgEventArgs() { /* intentionally left blank */ }
    public BbgEventArgs(BbgResponse bbgResponse) : this() { BbgResponse = bbgResponse; }
}
