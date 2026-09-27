using Terminal.Gui;
using AIoT.App.Themes;
using AppThemeManager = AIoT.App.Themes.ThemeManager;

namespace AIoT.App.Dialogs;

public sealed class InputBox : Dialog
{
    private readonly TextField _input;
    private bool _confirmed;

    public bool Confirmed => _confirmed;
    public string? Value => _input.Text;

    public InputBox(string title, string message, string initialValue, int width = 60)
    {
        var theme = AppThemeManager.Current;

        Title = $" {title} ";
        Width = width;
        Height = 9;
        ThemeStyler.ApplyToDialog(this, theme);

        var messageLabel = new Label
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1),
            Text = message
        };

        _input = new TextField
        {
            X = 1,
            Y = 2,
            Width = Dim.Fill(1),
            Text = initialValue
        };

        var ok = new Button
        {
            X = Pos.Center() - 10,
            Y = 5,
            Text = $"{theme.ButtonPrefix}OK{theme.ButtonSuffix}",
            IsDefault = true,
            ColorScheme = theme.ButtonColorScheme
        };

        var cancel = new Button
        {
            X = Pos.Center() + 2,
            Y = 5,
            Text = $"{theme.ButtonPrefix}Cancel{theme.ButtonSuffix}",
            ColorScheme = theme.ButtonColorScheme
        };

        ok.Accepting += (_, _) =>
        {
            _confirmed = true;
            Application.RequestStop();
        };

        cancel.Accepting += (_, _) =>
        {
            _confirmed = false;
            Application.RequestStop();
        };

        Add(messageLabel, _input, ok, cancel);
        _input.SetFocus();
    }
}
