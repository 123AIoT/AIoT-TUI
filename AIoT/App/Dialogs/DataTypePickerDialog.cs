using System.Collections.ObjectModel;
using Terminal.Gui;
using AIoT.App.Themes;
using AppThemeManager = AIoT.App.Themes.ThemeManager;

namespace AIoT.App.Dialogs;

internal sealed class DataTypePickerDialog : Dialog
{
    private readonly ListView _listView;
    private readonly List<string> _types;
    private bool _confirmed;

    public bool Confirmed => _confirmed;
    public string? SelectedDataType { get; private set; }

    public DataTypePickerDialog(string title, string protocol, IReadOnlyList<string> supportedTypes, string? selectedType)
    {
        var theme = AppThemeManager.Current;

        Title = $" {title} ";
        Width = 42;
        Height = 18;
        ThemeStyler.ApplyToDialog(this, theme);

        var protocolLabel = new Label
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1),
            Text = $"Protocol: {protocol}"
        };

        var tipLabel = new Label
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(1),
            Text = "Use ↑/↓ to select, Enter to confirm",
            ColorScheme = theme.MainColorScheme
        };

        _types = supportedTypes
            .Select(t => t.Trim().ToLowerInvariant())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(t => t, StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (_types.Count == 0)
            _types.Add("bool");

        _listView = new ListView
        {
            X = 1,
            Y = 4,
            Width = Dim.Fill(1),
            Height = Dim.Fill(3),
            ColorScheme = theme.MainColorScheme
        };
        _listView.SetSource(new ObservableCollection<string>(_types));

        var initialIndex = _types.FindIndex(t => t.Equals(selectedType?.Trim(), StringComparison.OrdinalIgnoreCase));
        if (initialIndex >= 0)
            _listView.SelectedItem = initialIndex;

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

        Add(protocolLabel, tipLabel, _listView, okButton, cancelButton);
        _listView.SetFocus();
    }

    private void Confirm()
    {
        if (_listView.SelectedItem < 0 || _listView.SelectedItem >= _types.Count)
            return;

        SelectedDataType = _types[_listView.SelectedItem];
        _confirmed = true;
        Application.RequestStop();
    }
}
