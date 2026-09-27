using System.Collections.ObjectModel;
using Terminal.Gui;
using AIoT.App.Themes;
using AppThemeManager = AIoT.App.Themes.ThemeManager;

namespace AIoT.App.Dialogs;

internal sealed class ProtocolPickerDialog : Dialog
{
    private readonly List<string> _protocolKeys;
    private readonly ListView _listView;
    private bool _confirmed;

    public bool Confirmed => _confirmed;
    public string? SelectedProtocol { get; private set; }

    public ProtocolPickerDialog(IReadOnlyList<string> protocols, string? selectedProtocol = null)
    {
        var theme = AppThemeManager.Current;

        Title = " Select Protocol ";
        Width = 46;
        Height = 18;
        ThemeStyler.ApplyToDialog(this, theme);

        _protocolKeys = protocols
            .Select(p => ProtocolDataTypeCatalog.NormalizeProtocolKey(p))
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (_protocolKeys.Count == 0)
            _protocolKeys.Add("opcua");

        var header = new Label
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1),
            Text = "Use ↑/↓ to choose protocol, Enter to confirm"
        };

        _listView = new ListView
        {
            X = 1,
            Y = 3,
            Width = Dim.Fill(1),
            Height = Dim.Fill(3),
            ColorScheme = theme.MainColorScheme
        };

        var displayRows = _protocolKeys
            .Select(GetProtocolLabel)
            .ToList();
        _listView.SetSource(new ObservableCollection<string>(displayRows));

        var initial = _protocolKeys.FindIndex(p => p.Equals(ProtocolDataTypeCatalog.NormalizeProtocolKey(selectedProtocol), StringComparison.OrdinalIgnoreCase));
        if (initial >= 0)
            _listView.SelectedItem = initial;

        _listView.OpenSelectedItem += (_, _) => Confirm();

        var okButton = new Button
        {
            X = Pos.Center() - 10,
            Y = Pos.AnchorEnd(1),
            Text = $"{theme.ButtonPrefix}OK{theme.ButtonSuffix}",
            IsDefault = true,
            ColorScheme = theme.ButtonColorScheme
        };
        okButton.Accepting += (_, _) => Confirm();

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

        Add(header, _listView, okButton, cancelButton);
        _listView.SetFocus();
    }

    private void Confirm()
    {
        if (_listView.SelectedItem < 0 || _listView.SelectedItem >= _protocolKeys.Count)
            return;

        SelectedProtocol = _protocolKeys[_listView.SelectedItem];
        _confirmed = true;
        Application.RequestStop();
    }

    private static string GetProtocolLabel(string protocolKey) => protocolKey switch
    {
        "opcua" => "OPC UA",
        "mqtt" => "MQTT",
        "websocket" => "WebSocket",
        "modbus.tcp" => "Modbus TCP",
        "modbus.rtu" => "Modbus RTU",
        "siemens.s7" => "Siemens S7",
        "melsec" => "Mitsubishi MC",
        "omron" => "Omron FINS",
        "delta" => "Delta",
        "xinje" => "Xinje",
        _ => protocolKey
    };
}