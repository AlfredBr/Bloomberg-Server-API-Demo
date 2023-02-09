using library;
using System.Data;

using Terminal.Gui;

namespace display;

public partial class BbgDisplay : Window
{
    private readonly BbgConnection? bbgConnection;
    private TableView? tableView;
    private readonly DataTable dataTable = new();
    private readonly Dictionary<string, Dictionary<string, string>> BbgMarketDataResponses = new();
    private DateTime updateTime = DateTime.Now;
    private readonly Timer refreshTimer;
    private BbgDisplay()
    {
        Colors.ColorSchemes["TopLevel"].Normal = Application.Driver.MakeAttribute(Color.Brown, Color.Black);
        Colors.ColorSchemes["TopLevel"].Focus = Application.Driver.MakeAttribute(Color.Black, Color.BrightYellow);
        refreshTimer = new Timer((_) => OnCollectionChanged(null, BbgEventArgs.Empty), null, 10000, 10000);
        Background();
        InputControls();
        OutputControls();
    }
    public BbgDisplay(BbgConnection? bbgConnection) : this()
    {
        if (bbgConnection is not null)
        {
            this.bbgConnection = bbgConnection;
            this.bbgConnection.CollectionChanged += OnCollectionChanged;
        }
    }
    private void Background()
    {
        this.Width = Dim.Fill(0);
        this.Height = Dim.Fill(0);
        this.X = 0;
        this.Y = 0;
        this.Modal = false;
        this.Text = "";
        this.Border.BorderStyle = Terminal.Gui.BorderStyle.Single;
        this.Border.Effect3D = false;
        this.Border.DrawMarginFrame = true;
        this.TextAlignment = Terminal.Gui.TextAlignment.Left;
        this.Title = "BBG SAPI Console (Ctrl-Q to quit)";
        this.ColorScheme = Colors.ColorSchemes["TopLevel"];

        var modeIndicator = BbgConfig.DemoMode ? "(Demo Mode)" : $"{BbgConfig.Server.Hostname} ({BbgConfig.Server.IPAddress})";
        var label = new Label("modeIndicator")
        {
            Text = modeIndicator,
            X = Pos.AnchorEnd() - modeIndicator.Length,
            Y = 0,
            Width = Dim.Fill(),
            Height = 1,
            TextAlignment = TextAlignment.Centered,
            ColorScheme = Colors.ColorSchemes["TopLevel"]
        };
        this.Add(label);
    }
    private void InputControls()
    {
        var tickerLabel = new Label("Ticker:")
        {
            X = 0,
            Y = 0,
            Width = 7,
            TextAlignment = TextAlignment.Right,
        };
        var tickerInput = new TextField("")
        {
            X = Pos.Right(tickerLabel),
            Y = 0,
            Width = Dim.Percent(30),
        };
        tickerInput.KeyPress += (e) => OnTickerChanged(e, tickerInput);
        this.Add(tickerLabel, tickerInput);

        var fieldLabel = new Label("Field:")
        {
            X = Pos.Right(tickerInput) + 1,
            Y = 0,
            Width = 7,
            TextAlignment = TextAlignment.Right,
        };
        var fieldInput = new TextField("")
        {
            X = Pos.Right(fieldLabel),
            Y = 0,
            Width = Dim.Percent(30),
        };
        fieldInput.KeyPress += (e) => OnFieldChanged(e, fieldInput);
        this.Add(fieldLabel, fieldInput);
    }
    private void OutputControls()
    {
        tableView = new TableView()
        {
            X = -1,
            Y = 1,
            Width = Dim.Fill() + 1,
            Height = Dim.Fill()
        };
        tableView.Style.AlwaysShowHeaders = true;
        tableView.Style.ExpandLastColumn = true;
        tableView.FullRowSelect = true;
        tableView.KeyPress += (e) => OnTableViewKeyPress(e);

        var columns = new[]
        {
            new DataColumn(" Ticker ",typeof(string)),  // ticker
            new DataColumn(" D ", typeof(string)),      // direction
            new DataColumn(" Last ", typeof(string)),   // last price
            new DataColumn(" Change ", typeof(string)), // price change
            new DataColumn(" Time ", typeof(string)),   // price time
            new DataColumn(" Open ", typeof(string)),   // open price
            new DataColumn(" Bid ", typeof(string)),    // bid price
            new DataColumn(" Ask ", typeof(string)),    // ask price
            new DataColumn(" High ", typeof(string)),   // ask price
            new DataColumn(" Low ", typeof(string)),    // ask price
            new DataColumn(" Close ", typeof(string)),  // close price
            new DataColumn(" Volume ", typeof(string))  // volume
        };

        TableView.ColumnStyle? alignRight(DataColumn dataColumn)
        {
            ColorScheme? GetColorScheme(TableView.CellColorGetterArgs a, DataColumn dataColumn)
            {
                var redColorScheme = new ColorScheme()
                {
                    Disabled = this.ColorScheme.Disabled,
                    HotFocus = this.ColorScheme.HotFocus,
                    Focus = this.ColorScheme.Focus,
                    HotNormal = this.ColorScheme.HotNormal,
                    Normal = Application.Driver.MakeAttribute(Color.BrightRed, this.ColorScheme.Normal.Background)
                };
                var greenColorScheme = new ColorScheme()
                {
                    Disabled = this.ColorScheme.Disabled,
                    HotFocus = this.ColorScheme.HotFocus,
                    Focus = this.ColorScheme.Focus,
                    HotNormal = this.ColorScheme.HotNormal,
                    Normal = Application.Driver.MakeAttribute(Color.BrightGreen, this.ColorScheme.Normal.Background)
                };
                if (dataColumn.ColumnName == " Change ")
                {
                    if (a.CellValue is null || a.CellValue is DBNull) { return null; }
                    return (Convert.ToDouble(a.CellValue) < 0d) ? redColorScheme : greenColorScheme;
                }
                if (dataColumn.ColumnName == " D ")
                {
                    var cellValue = a.CellValue?.ToString()??string.Empty;
                    if (cellValue.Contains('↓'))
                    {
                        return redColorScheme;
                    }
                    else if (cellValue.Contains('↑'))
                    {
                        return greenColorScheme;
                    }
                    else
                    {
                        return null;
                    }
                }
                return null;
            }

            return new TableView.ColumnStyle()
            {
                MinWidth = dataColumn.ColumnName == " D " ? 1 : 10,
                Alignment = dataColumn.ColumnName == " D " ? TextAlignment.Centered : TextAlignment.Right,
                ColorGetter = (a) => GetColorScheme(a, dataColumn)
            };
        }

        foreach (var column in columns)
        {
            dataTable.Columns.Add(column);
            tableView.Style.ColumnStyles.Add(column, alignRight(column));
        }

        tableView.Table = dataTable;
        this.Add(tableView);
    }
    private void OnTableViewKeyPress(KeyEventEventArgs args)
    {
        var keyEvent = args.KeyEvent;
        if (keyEvent.Key == Key.Enter || keyEvent.Key == Key.Space)
        {
            args.Handled = true;
            if (tableView is null) { return; }
            if (tableView.SelectedRow < 0) { return; }
            var row = tableView.Table.Rows[tableView.SelectedRow];
            var ticker = row[" Ticker "].ToString();
            if (ticker is null) { return; }
            Application.Run(FieldDialog(ticker));
        }
        if (keyEvent.Key == Key.DeleteChar)
        {
            args.Handled = true;
            if (tableView is null) { return; }
            if (tableView.SelectedRow < 0) { return; }
            var row = tableView.Table.Rows[tableView.SelectedRow];
            var ticker = row[" Ticker "].ToString()?.Trim();
            if (ticker is null) { return; }
            bbgConnection?.RemoveTopic(ticker)?.ModifySubscriptions();
            BbgMarketDataResponses.Remove(ticker);
            OnCollectionChanged(null, BbgEventArgs.Empty);
        }
    }
    private void OnTickerChanged(KeyEventEventArgs args, TextField textField)
    {
        var keyEvent = args.KeyEvent;
        if (keyEvent.Key == Key.Enter)
        {
            args.Handled = true;
            if (textField is null) { return; }
            var text = textField.Text.ToString();
            if (string.IsNullOrEmpty(text)) { OnCollectionChanged(null, BbgEventArgs.Empty); return; }
            bbgConnection?.AddTopic(text)?.ModifySubscriptions();
            textField.Text = string.Empty;
        }
    }
    private void OnFieldChanged(KeyEventEventArgs args, TextField textField)
    {
        var keyEvent = args.KeyEvent;
        if (keyEvent.Key == Key.Enter)
        {
            args.Handled = true;
            if (textField is null) { return; }
            var text = textField.Text.ToString();
            if (string.IsNullOrEmpty(text)) { return; }
            bbgConnection?.AddField(text)?.ModifySubscriptions();
            textField.Text = string.Empty;
        }
    }
    private void OnCollectionChanged(object? sender, BbgEventArgs e)
    {
        if (e?.BbgResponse is not null)
        {
            var topic = e.BbgResponse.Topic;
            var field = e.BbgResponse.Field;
            var value = e.BbgResponse.Value;

            if (!BbgMarketDataResponses.ContainsKey(topic))
            {
                BbgMarketDataResponses.Add(topic, new Dictionary<string, string>());
                BbgMarketDataResponses[topic]["TOPIC"] = $" {topic} ";
            }

            switch (field)
            {
                case "LAST_PRICE":
                case "PRICE_CHANGE_ON_DAY_RT":
                case "OPEN":
                case "BID":
                case "ASK":
                case "HIGH":
                case "LOW":
                case "PX_OFFICIAL_CLOSE_RT":
                    value = Convert.ToDouble(value).ToString("N2");
                    break;
                case "VOLUME":
                    value = Convert.ToDouble(value).ToString("N0");
                    break;
                case "PRICE_LAST_TIME_RT":
                    value = Convert.ToDateTime(value).ToString("HH:mm:ss");
                    break;
                case "LAST_DIR":
                    value = Convert.ToInt32(value) == 1 ? "↑" : "↓";
                    break;
                default:
                    break;
            }

            BbgMarketDataResponses[topic][field] = $" {value} ";

            if (DateTime.Now < updateTime.AddMilliseconds(500)) { return; }
            updateTime = DateTime.Now;
        }

        Application.MainLoop.Invoke(() =>
        {
            dataTable.Rows.Clear();
            foreach(var key in BbgMarketDataResponses.Keys.OrderBy(t => t))
            {
                var values = BbgMarketDataResponses[key];
                dataTable.Rows.Add(
                    values["TOPIC"],
                    values.TryGetValue("LAST_DIR", out _) ? values["LAST_DIR"] : null,
                    values.TryGetValue("LAST_PRICE", out _) ? values["LAST_PRICE"] : null,
                    values.TryGetValue("PRICE_CHANGE_ON_DAY_RT", out _) ? values["PRICE_CHANGE_ON_DAY_RT"] : null,
                    values.TryGetValue("PRICE_LAST_TIME_RT", out _) ? values["PRICE_LAST_TIME_RT"] : null,
                    values.TryGetValue("OPEN", out _) ? values["OPEN"] : null,
                    values.TryGetValue("BID", out _) ? values["BID"] : null,
                    values.TryGetValue("ASK", out _) ? values["ASK"] : null,
                    values.TryGetValue("HIGH", out _) ? values["HIGH"] : null,
                    values.TryGetValue("LOW", out _) ? values["LOW"] : null,
                    values.TryGetValue("PX_OFFICIAL_CLOSE_RT", out _) ? values["PX_OFFICIAL_CLOSE_RT"] : null,
                    values.TryGetValue("VOLUME", out _) ? values["VOLUME"] : null);
            }
            if (tableView is null) { return; }
            tableView.Update();
            tableView.SetNeedsDisplay();
        });
    }
    private static Dialog ErrorDialog(string title)
    {
        var dialog = new Dialog("Error", 50, 7)
        {
            Width = Math.Max(50, title.Length + 6),
        };
        dialog.Border.Effect3D = false;
        var label = new Label(title)
        {
            X = Pos.Center(),
            Y = Pos.Center() - 1
        };
        var button = new Button("_Close", true)
        {
            X = Pos.Center(),
            Y = Pos.AnchorEnd()
        };
        button.Clicked += () => Application.RequestStop();
        dialog.Add(label, button);
        return dialog;
    }
    private Dialog FieldDialog(string? ticker)
    {
        if (string.IsNullOrEmpty(ticker)) { return ErrorDialog("Sorry, No Data."); }
        var topic = $"{ticker.Trim()}";
        var title = $"{topic}";
        var fields = BbgMarketDataResponses[topic];
        var fieldDataTable = new DataTable();
        fieldDataTable.Columns.Add(new DataColumn(" Field ", typeof(string)));
        fieldDataTable.Columns.Add(new DataColumn(" Value ", typeof(string)));
        foreach (var key in fields.Keys.OrderBy(t => t))
        {
            fieldDataTable.Rows.Add($" {key} ", $"{fields[key]}");
        }
        var fieldTableView = new TableView(fieldDataTable)
        {
            X = Pos.Center(),
            Y = 1,
            Width = Dim.Percent(90),
            Height = Dim.Fill(1),
            Table = fieldDataTable
        };
        fieldTableView.Style.AlwaysShowHeaders = true;
        fieldTableView.FullRowSelect = true;
        var dialog = new Dialog(title)
        {
            Width = Dim.Percent(60),
            Height = Dim.Percent(80),
        };
        void cancelDialog(KeyEventEventArgs args)
        {
            var keyEvent = args.KeyEvent;
            if (keyEvent.Key == Key.Esc || keyEvent.Key == Key.Space)
            {
                args.Handled = true;
                Application.RequestStop();
            }
        }
        dialog.Border.Effect3D = false;
        dialog.KeyPress += (e) => cancelDialog(e);
        dialog.Add(fieldTableView);
        return dialog;
    }
}
