using Terminal.Gui;
using AIoT.App.Themes;
using AppThemeManager = AIoT.App.Themes.ThemeManager;

namespace AIoT.App.Dialogs;

public sealed class MonitorPointsBatchImportDialog : Dialog
{
    private readonly TextView _textView;
    private bool _confirmed;

    public bool Confirmed => _confirmed;
    public List<MonitorPointDraft> Drafts { get; } = new();

    public MonitorPointsBatchImportDialog(string protocol)
    {
        var theme = AppThemeManager.Current;

        Title = " Batch Import Monitor Points ";
        Width = 80;
        Height = 22;
        ThemeStyler.ApplyToDialog(this, theme);

        var header = new Label
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1),
            Text = $"Protocol: {protocol}"
        };

        var hint = new Label
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(1),
            Text = "One per line: address[,type[,length]]   Example: DB1.0,bool | D100,int,1",
            ColorScheme = theme.MainColorScheme
        };

        _textView = new TextView
        {
            X = 1,
            Y = 4,
            Width = Dim.Fill(1),
            Height = Dim.Fill(3),
            Text = string.Empty,
            AllowsTab = false,
            WordWrap = false
        };

        var okButton = new Button
        {
            X = Pos.Center() - 10,
            Y = Pos.AnchorEnd(1),
            Text = $"{theme.ButtonPrefix}Import{theme.ButtonSuffix}",
            IsDefault = true,
            ColorScheme = theme.ButtonColorScheme
        };
        okButton.Accepting += (_, _) => Confirm(protocol);

        var cancelButton = new Button
        {
            X = Pos.Center() + 2,
            Y = Pos.AnchorEnd(1),
            Text = $"{theme.ButtonPrefix}Cancel{theme.ButtonSuffix}",
            ColorScheme = theme.ButtonColorScheme
        };
        cancelButton.Accepting += (_, _) =>
        {
            _confirmed = false;
            Application.RequestStop();
        };

        Add(header, hint, _textView, okButton, cancelButton);
        _textView.SetFocus();
    }

    private void Confirm(string protocol)
    {
        Drafts.Clear();

        var lines = (_textView.Text?.ToString() ?? string.Empty)
            .Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        foreach (var line in lines)
        {
            var parts = line.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                continue;

            var address = parts[0].Trim();
            if (string.IsNullOrWhiteSpace(address))
                continue;

            var defaultType = ProtocolDataTypeCatalog.GetDefaultDataType(protocol);
            var dataType = parts.Length > 1 && !string.IsNullOrWhiteSpace(parts[1])
                ? parts[1].Trim().ToLowerInvariant()
                : defaultType;

            int? length = null;
            if (parts.Length > 2 && int.TryParse(parts[2], out var parsedLength) && parsedLength > 0)
                length = parsedLength;

            Drafts.Add(new MonitorPointDraft(address, dataType, length));
        }

        if (Drafts.Count == 0)
        {
            MessageBox.ErrorQuery("Error", "No valid lines to import", "OK");
            return;
        }

        _confirmed = true;
        Application.RequestStop();
    }
}

public readonly record struct MonitorPointDraft(string Address, string DataType, int? DataLength);