using System.Text.RegularExpressions;
using Terminal.Gui;
using AIoT.App.Themes;
using AppThemeManager = AIoT.App.Themes.ThemeManager;

namespace AIoT.App.Dialogs;

public sealed class MonitorPointDialog : Dialog
{
    private readonly TextField _addressField;
    private readonly Label _dataTypeValueLabel;
    private readonly Button _dataTypePickButton;
    private readonly TextField _dataLengthField;
    private readonly string _protocol;
    private readonly string _defaultDataType;
    private readonly List<string> _supportedDataTypes;
    private string _selectedDataType;
    private bool _confirmed;

    public bool Confirmed => _confirmed;
    public string Address => (_addressField.Text ?? string.Empty).Trim();
    public string DataType => _selectedDataType;
    public int? DataLength => int.TryParse((_dataLengthField.Text ?? string.Empty).Trim(), out var length) && length > 0 ? length : null;

    public MonitorPointDialog(string title, string protocol, string? address = null, string? dataType = null, int? dataLength = null)
    {
        var theme = AppThemeManager.Current;
        _protocol = ProtocolDataTypeCatalog.NormalizeProtocolKey(protocol);
        _defaultDataType = ProtocolDataTypeCatalog.GetDefaultDataType(_protocol);
        _supportedDataTypes = ProtocolDataTypeCatalog.GetSupportedDataTypes(_protocol).ToList();
        _selectedDataType = string.IsNullOrWhiteSpace(dataType) ? _defaultDataType : dataType.Trim().ToLowerInvariant();
        if (!_supportedDataTypes.Contains(_selectedDataType, StringComparer.OrdinalIgnoreCase))
            _selectedDataType = _defaultDataType;

        Title = $" {title} ";
        Width = 64;
        Height = 14;
        ThemeStyler.ApplyToDialog(this, theme);

        var protocolLabel = new Label
        {
            X = 1,
            Y = 1,
            Width = Dim.Fill(1),
            Text = $"Protocol: {protocol}"
        };

        var addressLabel = new Label
        {
            X = 1,
            Y = 3,
            Text = "Address:"
        };

        _addressField = new TextField
        {
            X = 12,
            Y = 3,
            Width = Dim.Fill(1),
            Text = address ?? string.Empty
        };

        var typeLabel = new Label
        {
            X = 1,
            Y = 5,
            Text = "Data Type:"
        };

        _dataTypeValueLabel = new Label
        {
            X = 12,
            Y = 5,
            Width = 16,
            Text = _selectedDataType
        };

        _dataTypePickButton = new Button
        {
            X = 29,
            Y = 5,
            Width = 9,
            Height = 1,
            Text = "Pick(T)",
            ColorScheme = theme.ButtonColorScheme
        };
        _dataTypePickButton.Accepting += (_, _) => ShowDataTypePicker();

        var lengthLabel = new Label
        {
            X = 1,
            Y = 7,
            Text = "Length:"
        };

        _dataLengthField = new TextField
        {
            X = 40,
            Y = 7,
            Width = 10,
            Text = dataLength?.ToString() ?? string.Empty
        };

        var hintLabel = new Label
        {
            X = 1,
            Y = 9,
            Width = Dim.Fill(1),
            Text = "Tip: string/hex or multi-register values usually need Length.",
            ColorScheme = theme.MainColorScheme
        };

        var okButton = new Button
        {
            X = Pos.Center() - 10,
            Y = 11,
            Text = $"{theme.ButtonPrefix}OK{theme.ButtonSuffix}",
            IsDefault = true,
            ColorScheme = theme.ButtonColorScheme
        };

        okButton.Accepting += (_, _) =>
        {
            if (!ValidateInput())
                return;

            _confirmed = true;
            Application.RequestStop();
        };

        var cancelButton = new Button
        {
            X = Pos.Center() + 2,
            Y = 11,
            Text = $"{theme.ButtonPrefix}Cancel{theme.ButtonSuffix}",
            ColorScheme = theme.ButtonColorScheme
        };

        cancelButton.Accepting += (_, _) =>
        {
            _confirmed = false;
            Application.RequestStop();
        };

        Add(protocolLabel, addressLabel, _addressField, typeLabel, _dataTypeValueLabel, _dataTypePickButton, lengthLabel, _dataLengthField, hintLabel, okButton, cancelButton);
        KeyDown += OnDialogKeyDown;
        _addressField.SetFocus();
    }

    private void OnDialogKeyDown(object? sender, Key key)
    {
        if (key.KeyCode == KeyCode.T)
        {
            ShowDataTypePicker();
            key.Handled = true;
        }
    }

    private void ShowDataTypePicker()
    {
        using var picker = new DataTypePickerDialog("Select Data Type", _protocol, _supportedDataTypes, _selectedDataType);
        Application.Run(picker);

        if (!picker.Confirmed || string.IsNullOrWhiteSpace(picker.SelectedDataType))
            return;

        _selectedDataType = picker.SelectedDataType;
        _dataTypeValueLabel.Text = _selectedDataType;
    }

    private bool ValidateInput()
    {
        if (string.IsNullOrWhiteSpace(Address))
        {
            MessageBox.ErrorQuery("Error", "Address cannot be empty", "OK");
            _addressField.SetFocus();
            return false;
        }

        if (!string.IsNullOrWhiteSpace(_dataLengthField.Text))
        {
            if (!int.TryParse((_dataLengthField.Text ?? string.Empty).Trim(), out var length) || length <= 0)
            {
                MessageBox.ErrorQuery("Error", "Length must be a positive integer", "OK");
                _dataLengthField.SetFocus();
                return false;
            }
        }

        if ((DataType == "string" || DataType == "hex") && DataLength is null)
        {
            MessageBox.ErrorQuery("Error", "String/hex monitor points require Length", "OK");
            _dataLengthField.SetFocus();
            return false;
        }

        if (!IsAddressValidForProtocol(Address))
        {
            MessageBox.ErrorQuery("Error", GetAddressValidationMessage(), "OK");
            _addressField.SetFocus();
            return false;
        }

        return true;
    }

    private bool IsAddressValidForProtocol(string address) => _protocol switch
    {
        "modbus.tcp" or "modbus.rtu" => Regex.IsMatch(address, @"^(?:s=\d+;)?\d+(?:\.\d+)?$", RegexOptions.IgnoreCase),
        "siemens.s7" => Regex.IsMatch(address, @"^(?:DB\d+\.\d+(?:\.\d+)?|[MIQ]\d+(?:\.\d+)?)$", RegexOptions.IgnoreCase),
        "melsec" or "delta" or "xinje" => Regex.IsMatch(address, @"^[A-Z]{1,4}\d+(?:\.\d+)?$", RegexOptions.IgnoreCase),
        "omron" => Regex.IsMatch(address, @"^(?:D|W|H|AR|CIO)\d+(?:\.\d+)?$", RegexOptions.IgnoreCase),
        "mqtt" or "websocket" => !string.IsNullOrWhiteSpace(address) && !address.Any(char.IsWhiteSpace),
        _ => true
    };

    private string GetAddressValidationMessage() => _protocol switch
    {
        "modbus.tcp" or "modbus.rtu" => "Invalid address. Examples: 100, 100.0, s=2;100",
        "siemens.s7" => "Invalid address. Examples: DB1.0, M100, I0.0",
        "melsec" => "Invalid address. Examples: D100, M100, X0",
        "omron" => "Invalid address. Examples: D100, CIO0.00, W100",
        "delta" => "Invalid address. Examples: D100, M100, X0.0",
        "xinje" => "Invalid address. Examples: D100, M100, X0",
        "mqtt" => "Invalid topic. Topic cannot be empty or contain spaces",
        "websocket" => "Invalid address. Address cannot be empty or contain spaces",
        _ => "Invalid address"
    };
}
