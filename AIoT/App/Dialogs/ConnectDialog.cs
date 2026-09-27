using Terminal.Gui;
using AIoT.App.Themes;
using AIoT.OpcUa;
using AppThemeManager = AIoT.App.Themes.ThemeManager;
namespace AIoT.App.Dialogs;

/// <summary>
/// Dialog for entering protocol connection details.
/// Uses Terminal.Gui v2 styling with theme support.
/// </summary>
public class ConnectDialog : Dialog
{
    private const long ConnectHotkeyGuardMs = 350;
    private readonly List<string> _protocolKeys;
    private string _selectedProtocol;
    private readonly Label _protocolValueLabel;
    private readonly Button _protocolPickButton;
    private readonly TextField _endpointField;
    private readonly NumericUpDown<int> _publishIntervalField;
    private readonly RadioGroup _authTypeRadio;
    private readonly Label _usernameLabel;
    private readonly TextField _usernameField;
    private readonly Label _passwordLabel;
    private readonly TextField _passwordField;
    private bool _confirmed;
    private readonly Label _authLabel;
    private readonly Label _endpointHintLabel;
    private readonly Label _siemensOptionsLabel;
    private readonly TextField _plcField;
    private readonly TextField _rackField;
    private readonly TextField _slotField;
    private readonly Label _rtuOptionsLabel;
    private readonly TextField _rtuBaudField;
    private readonly TextField _rtuStationField;
    private readonly Label _mqttOptionsLabel;
    private readonly TextField _mqttClientIdField;
    private readonly TextField _mqttQosField;
    private readonly TextField _mqttRetainField;
    private readonly TextField _mqttTopicField;
    private readonly Label _webSocketOptionsLabel;
    private readonly TextField _webSocketSubProtocolField;
    private readonly TextField _webSocketAddressField;
    private readonly TextField _webSocketHeaderField;
    private string? _lastProtocolForDefaults;
    private long _connectGuardUntilTick;

    public string EndpointUrl => _endpointField.Text?.Trim() ?? string.Empty;
    public bool Confirmed => _confirmed;
    public int PublishingInterval => _publishIntervalField.Value;
    public string SelectedProtocol => _selectedProtocol;
    public AuthenticationType SelectedAuthType =>
        _authTypeRadio.SelectedItem == 1 ? AuthenticationType.UserName : AuthenticationType.Anonymous;
    public string? Username => SelectedAuthType == AuthenticationType.UserName
        ? _usernameField.Text?.Trim() : null;
    public string? Password => SelectedAuthType == AuthenticationType.UserName
        ? _passwordField.Text : null;
    public string SiemensPlc => (_plcField.Text ?? "s1200").Trim();
    public byte SiemensRack => byte.TryParse((_rackField.Text ?? "0").Trim(), out var rack) ? rack : (byte)0;
    public byte SiemensSlot => byte.TryParse((_slotField.Text ?? "0").Trim(), out var slot) ? slot : (byte)0;
    public int ModbusRtuBaudRate => int.TryParse((_rtuBaudField.Text ?? "9600").Trim(), out var baud) ? baud : 9600;
    public byte ModbusRtuStation => byte.TryParse((_rtuStationField.Text ?? "1").Trim(), out var station) ? station : (byte)1;
    public string MqttClientId => (_mqttClientIdField.Text ?? string.Empty).Trim();
    public int MqttQos => int.TryParse((_mqttQosField.Text ?? "0").Trim(), out var qos) ? qos : 0;
    public bool MqttRetain => bool.TryParse((_mqttRetainField.Text ?? "false").Trim(), out var retain) && retain;
    public string MqttTopic => (_mqttTopicField.Text ?? string.Empty).Trim();
    public string WebSocketSubProtocol => (_webSocketSubProtocolField.Text ?? string.Empty).Trim();
    public string WebSocketDefaultAddress => (_webSocketAddressField.Text ?? string.Empty).Trim();
    public string WebSocketHeader => (_webSocketHeaderField.Text ?? string.Empty).Trim();

    public ConnectDialog(
        string? initialEndpoint = null,
        int publishingInterval = 250,
        AuthenticationType authType = AuthenticationType.Anonymous,
        string? username = null,
        IReadOnlyList<string>? availableProtocols = null,
        string? selectedProtocol = null)
    {
        var theme = AppThemeManager.Current;

        _protocolKeys = BuildProtocolKeys(availableProtocols);

        _selectedProtocol = ProtocolDataTypeCatalog.NormalizeProtocolKey(selectedProtocol);
        if (string.IsNullOrWhiteSpace(_selectedProtocol))
            _selectedProtocol = "opcua";

        var yOffset = 0;

        Title = " Connect to Server ";
        Width = 60;
        Height = 20;

        ThemeStyler.ApplyToDialog(this, theme);

        var protocolLabel = new Label
        {
            X = 1,
            Y = 1,
            Text = "Protocol:"
        };

        _protocolValueLabel = new Label
        {
            X = 11,
            Y = 1,
            Width = 16,
            Text = GetProtocolLabel(_selectedProtocol)
        };

        _protocolPickButton = new Button
        {
            X = 29,
            Y = 1,
            Width = 9,
            Height = 1,
            Text = "_Pick",
            ColorScheme = theme.ButtonColorScheme
        };
        _protocolPickButton.Accepting += (_, _) => ShowProtocolPicker();

        var endpointLabel = new Label
        {
            X = 1,
            Y = 3 + yOffset,
            Text = "Endpoint:"
        };

        _endpointField = new TextField
        {
            X = 1,
            Y = 4 + yOffset,
            Width = Dim.Fill(1),
            Text = initialEndpoint ?? string.Empty
        };

        _endpointHintLabel = new Label
        {
            X = 1,
            Y = 5 + yOffset,
            Width = Dim.Fill(1),
            ColorScheme = theme.MainColorScheme,
            Text = string.Empty
        };

        _siemensOptionsLabel = new Label
        {
            X = 1,
            Y = 9,
            Text = "Siemens options (plc/rack/slot):",
            Visible = false
        };

        _plcField = new TextField
        {
            X = 1,
            Y = 10,
            Width = 14,
            Text = "s1200",
            Visible = false
        };

        _rackField = new TextField
        {
            X = 17,
            Y = 10,
            Width = 8,
            Text = "0",
            Visible = false
        };

        _slotField = new TextField
        {
            X = 27,
            Y = 10,
            Width = 8,
            Text = "0",
            Visible = false
        };

        _rtuOptionsLabel = new Label
        {
            X = 1,
            Y = 9,
            Text = "RTU options (baud/station):",
            Visible = false
        };

        _rtuBaudField = new TextField
        {
            X = 1,
            Y = 10,
            Width = 14,
            Text = "9600",
            Visible = false
        };

        _rtuStationField = new TextField
        {
            X = 17,
            Y = 10,
            Width = 8,
            Text = "1",
            Visible = false
        };

        _mqttOptionsLabel = new Label
        {
            X = 1,
            Y = 9,
            Text = "MQTT options (clientId/qos/retain/topic):",
            Visible = false
        };

        _mqttClientIdField = new TextField
        {
            X = 1,
            Y = 10,
            Width = 18,
            Text = "aiot-tui",
            Visible = false
        };

        _mqttQosField = new TextField
        {
            X = 21,
            Y = 10,
            Width = 4,
            Text = "0",
            Visible = false
        };

        _mqttRetainField = new TextField
        {
            X = 27,
            Y = 10,
            Width = 8,
            Text = "false",
            Visible = false
        };

        _mqttTopicField = new TextField
        {
            X = 1,
            Y = 11,
            Width = Dim.Fill(1),
            Text = "",
            Visible = false
        };

        _webSocketOptionsLabel = new Label
        {
            X = 1,
            Y = 9,
            Text = "WebSocket options (subprotocol/address/header):",
            Visible = false
        };

        _webSocketSubProtocolField = new TextField
        {
            X = 1,
            Y = 10,
            Width = 18,
            Text = string.Empty,
            Visible = false
        };

        _webSocketAddressField = new TextField
        {
            X = 21,
            Y = 10,
            Width = 14,
            Text = "$last",
            Visible = false
        };

        _webSocketHeaderField = new TextField
        {
            X = 1,
            Y = 11,
            Width = Dim.Fill(1),
            Text = string.Empty,
            Visible = false
        };

        var intervalLabel = new Label
        {
            X = 1,
            Y = 6,
            Text = "Publishing Interval (ms):"
        };

        _publishIntervalField = new NumericUpDown<int>
        {
            X = 1,
            Y = 7,
            Width = 20,
            Value = publishingInterval,
            Increment = 100
        };

        var intervalHintLabel = new Label
        {
            X = 1,
            Y = 8,
            Text = "How often the server sends data updates (100-10000)",
            ColorScheme = theme.MainColorScheme
        };

        // Authentication section
        _authLabel = new Label
        {
            X = 1,
            Y = 9,
            Text = "Authentication:"
        };

        _authTypeRadio = new RadioGroup
        {
            X = 1,
            Y = 10,
            RadioLabels = ["Anonymous", "Username/Password"],
            Orientation = Orientation.Horizontal,
            SelectedItem = authType == AuthenticationType.UserName ? 1 : 0
        };

        _usernameLabel = new Label
        {
            X = 1,
            Y = 11,
            Text = "Username:",
            Visible = authType == AuthenticationType.UserName
        };

        _usernameField = new TextField
        {
            X = 12,
            Y = 11,
            Width = Dim.Fill(1),
            Text = username ?? string.Empty,
            Visible = authType == AuthenticationType.UserName
        };

        _passwordLabel = new Label
        {
            X = 1,
            Y = 12,
            Text = "Password:",
            Visible = authType == AuthenticationType.UserName
        };

        _passwordField = new TextField
        {
            X = 12,
            Y = 12,
            Width = Dim.Fill(1),
            Secret = true,
            Visible = authType == AuthenticationType.UserName
        };

        _authTypeRadio.SelectedItemChanged += (_, _) =>
        {
            var showCredentials = _authTypeRadio.SelectedItem == 1;
            _usernameLabel.Visible = showCredentials;
            _usernameField.Visible = showCredentials;
            _passwordLabel.Visible = showCredentials;
            _passwordField.Visible = showCredentials;
        };

        // Default button highlighted with amber
        var defaultButtonScheme = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(theme.Accent, theme.Background),
            Focus = new Terminal.Gui.Attribute(theme.AccentBright, theme.Background),
            HotNormal = new Terminal.Gui.Attribute(theme.Accent, theme.Background),
            HotFocus = new Terminal.Gui.Attribute(theme.AccentBright, theme.Background),
            Disabled = new Terminal.Gui.Attribute(theme.MutedText, theme.Background)
        };

        var connectButton = new Button
        {
            X = Pos.Center() - 10,
            Y = Pos.AnchorEnd(1),
            Text = $"{theme.ButtonPrefix}Connect{theme.ButtonSuffix}",
            IsDefault = true,
            ColorScheme = defaultButtonScheme
        };

        connectButton.Accepting += (_, _) =>
        {
            if (Environment.TickCount64 < _connectGuardUntilTick)
                return;

            if (ValidateInput())
            {
                _confirmed = true;
                Application.RequestStop();
            }
        };

        var cancelButton = new Button
        {
            X = Pos.Center() + 4,
            Y = Pos.AnchorEnd(1),
            Text = $"{theme.ButtonPrefix}Cancel{theme.ButtonSuffix}",
            ColorScheme = theme.ButtonColorScheme
        };

        cancelButton.Accepting += (_, _) =>
        {
            _confirmed = false;
            Application.RequestStop();
        };

        Add(protocolLabel, _protocolValueLabel, _protocolPickButton,
            endpointLabel, _endpointField, _endpointHintLabel,
            intervalLabel, _publishIntervalField, intervalHintLabel,
            _siemensOptionsLabel, _plcField, _rackField, _slotField,
            _rtuOptionsLabel, _rtuBaudField, _rtuStationField,
            _mqttOptionsLabel, _mqttClientIdField, _mqttQosField, _mqttRetainField, _mqttTopicField,
            _webSocketOptionsLabel, _webSocketSubProtocolField, _webSocketAddressField, _webSocketHeaderField,
            _authLabel, _authTypeRadio,
            _usernameLabel, _usernameField, _passwordLabel, _passwordField,
            connectButton, cancelButton);

        UpdateAuthVisibility();
        ApplyProtocolDefaults();

        KeyDown += OnDialogKeyDown;

        _endpointField.SetFocus();
    }

    private void OnDialogKeyDown(object? sender, Key key)
    {
        if (key.KeyCode == KeyCode.P)
        {
            ShowProtocolPicker();
            key.Handled = true;
        }
    }

    private void ShowProtocolPicker()
    {
        using var picker = new ProtocolPickerDialog(_protocolKeys, _selectedProtocol);
        Application.Run(picker);

        if (!picker.Confirmed || string.IsNullOrWhiteSpace(picker.SelectedProtocol))
            return;

        _selectedProtocol = ProtocolDataTypeCatalog.NormalizeProtocolKey(picker.SelectedProtocol);
        _protocolValueLabel.Text = GetProtocolLabel(_selectedProtocol);
        UpdateAuthVisibility();
        ApplyProtocolDefaults();
        _connectGuardUntilTick = Environment.TickCount64 + ConnectHotkeyGuardMs;
    }

    private static List<string> BuildProtocolKeys(IReadOnlyList<string>? availableProtocols)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (availableProtocols != null)
        {
            foreach (var protocol in availableProtocols)
            {
                var normalized = ProtocolDataTypeCatalog.NormalizeProtocolKey(protocol);
                if (!string.IsNullOrWhiteSpace(normalized))
                    set.Add(normalized);
            }
        }

        set.Add("mqtt");
        set.Add("websocket");
        set.Add("modbus.tcp");
        set.Add("modbus.rtu");
        set.Add("siemens.s7");
        set.Add("melsec");
        set.Add("omron");
        set.Add("delta");
        set.Add("xinje");
        set.Add("opcua");

        return set
            .OrderBy(static p => p, StringComparer.OrdinalIgnoreCase)
            .ToList();
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

    private void UpdateAuthVisibility()
    {
        var isOpcUa = SelectedProtocol == "opcua";
        var isSiemens = SelectedProtocol == "siemens.s7";
        var isRtu = SelectedProtocol == "modbus.rtu";
        var isMqtt = SelectedProtocol == "mqtt";
        var isWebSocket = SelectedProtocol == "websocket";

        _authLabel.Visible = isOpcUa;
        _authTypeRadio.Visible = isOpcUa;

        var showCredentials = isOpcUa && _authTypeRadio.SelectedItem == 1;
        _usernameLabel.Visible = showCredentials;
        _usernameField.Visible = showCredentials;
        _passwordLabel.Visible = showCredentials;
        _passwordField.Visible = showCredentials;

        _siemensOptionsLabel.Visible = isSiemens;
        _plcField.Visible = isSiemens;
        _rackField.Visible = isSiemens;
        _slotField.Visible = isSiemens;

        _rtuOptionsLabel.Visible = isRtu;
        _rtuBaudField.Visible = isRtu;
        _rtuStationField.Visible = isRtu;

        _mqttOptionsLabel.Visible = isMqtt;
        _mqttClientIdField.Visible = isMqtt;
        _mqttQosField.Visible = isMqtt;
        _mqttRetainField.Visible = isMqtt;
        _mqttTopicField.Visible = isMqtt;

        _webSocketOptionsLabel.Visible = isWebSocket;
        _webSocketSubProtocolField.Visible = isWebSocket;
        _webSocketAddressField.Visible = isWebSocket;
        _webSocketHeaderField.Visible = isWebSocket;

        ApplyProtocolDefaults();
    }

    private void ApplyProtocolDefaults()
    {
        var protocolChanged = !string.Equals(_lastProtocolForDefaults, SelectedProtocol, StringComparison.Ordinal);

        var (endpointTemplate, endpointHint, addressHint) = SelectedProtocol switch
        {
            "mqtt" => ("mqtt://127.0.0.1:1883", "Example: mqtt://127.0.0.1:1883", "Example addresses: sensors/temp, line1/status"),
            "websocket" => ("ws://127.0.0.1:8080/ws", "Example: ws://127.0.0.1:8080/ws", "Example addresses: line1/status, $last"),
            "modbus.tcp" => ("modbus.tcp://127.0.0.1:502", "Example: modbus.tcp://127.0.0.1:502", "Example addresses: 100, 101"),
            "modbus.rtu" => ("modbus.rtu://COM3?baud=9600&station=1", "Example: modbus.rtu://COM3?baud=9600&station=1", "Example addresses: 100, 101"),
            "siemens.s7" => ("s7://192.168.0.10:102?plc=s1200&rack=0&slot=1", "Example: s7://192.168.0.10:102?plc=s1200&rack=0&slot=1", "Example addresses: DB1.0, M100"),
            "melsec" => ("melsec://192.168.0.11:6000", "Example: melsec://192.168.0.11:6000", "Example addresses: D100, M100"),
            "omron" => ("omron://192.168.0.12:9600", "Example: omron://192.168.0.12:9600", "Example addresses: D100, CIO0.00"),
            "delta" => ("delta://192.168.0.13:502?series=dvp&station=1", "Example: delta://192.168.0.13:502?series=dvp&station=1", "Example addresses: D100, M100"),
            "xinje" => ("xinje://192.168.0.14:502?series=xc&station=1", "Example: xinje://192.168.0.14:502?series=xc&station=1", "Example addresses: D100, X0"),
            _ => ("opc.tcp://localhost:4840", "Example: opc.tcp://localhost:4840", "Example addresses: ns=2;s=Counter")
        };

        _endpointHintLabel.Text = endpointHint;
        if (!string.IsNullOrWhiteSpace(_endpointHintLabel.Text))
            _endpointHintLabel.Text = $"{_endpointHintLabel.Text}   (Alt+P 选择协议)";

        if (SelectedProtocol == "siemens.s7")
        {
            _siemensOptionsLabel.Text = "Siemens options (plc/rack/slot):";
        }

        if (SelectedProtocol == "modbus.rtu")
        {
            _rtuOptionsLabel.Text = "RTU options (baud/station):";
        }

        if (SelectedProtocol == "mqtt")
        {
            _mqttOptionsLabel.Text = "MQTT options (clientId/qos/retain/topic):";
        }

        if (SelectedProtocol == "websocket")
        {
            _webSocketOptionsLabel.Text = "WebSocket options (subprotocol/address/header):";
        }

        if (protocolChanged || string.IsNullOrWhiteSpace(_endpointField.Text))
        {
            _endpointField.Text = endpointTemplate;
        }

        if (protocolChanged && SelectedProtocol == "mqtt")
        {
            _mqttClientIdField.Text = "aiot-tui";
            _mqttQosField.Text = "0";
            _mqttRetainField.Text = "false";
            _mqttTopicField.Text = "sensors/temp";
        }

        if (protocolChanged && SelectedProtocol == "websocket")
        {
            _webSocketSubProtocolField.Text = string.Empty;
            _webSocketAddressField.Text = "$last";
            _webSocketHeaderField.Text = string.Empty;
        }

        _lastProtocolForDefaults = SelectedProtocol;
    }

    private bool ValidateInput()
    {
        var serverAddress = _endpointField.Text?.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(serverAddress))
        {
            MessageBox.ErrorQuery("Error", "Please enter a server address", "OK");
            return false;
        }

        if (SelectedProtocol == "opcua")
        {
            try
            {
                var uri = new Uri(EndpointUrl);
                if (string.IsNullOrEmpty(uri.Host))
                {
                    MessageBox.ErrorQuery("Error", "Invalid host in server address", "OK");
                    return false;
                }
            }
            catch
            {
                MessageBox.ErrorQuery("Error", "Invalid server address format", "OK");
                return false;
            }
        }

        if (SelectedProtocol == "websocket")
        {
            if (string.IsNullOrWhiteSpace(WebSocketDefaultAddress))
            {
                MessageBox.ErrorQuery("Error", "WebSocket default address cannot be empty", "OK");
                _webSocketAddressField.SetFocus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(WebSocketHeader))
            {
                var headerSegments = WebSocketHeader.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var segment in headerSegments)
                {
                    if (!segment.Contains(':'))
                    {
                        MessageBox.ErrorQuery("Error", "WebSocket header format must be key:value;key2:value2", "OK");
                        _webSocketHeaderField.SetFocus();
                        return false;
                    }
                }
            }
        }

        if (SelectedProtocol == "modbus.rtu")
        {
            if (ModbusRtuBaudRate <= 0)
            {
                MessageBox.ErrorQuery("Error", "Invalid RTU baud rate", "OK");
                _rtuBaudField.SetFocus();
                return false;
            }
        }

        var interval = _publishIntervalField.Value;
        if (interval < 100 || interval > 10000)
        {
            MessageBox.ErrorQuery("Error", "Publishing interval must be between 100 and 10000 ms", "OK");
            return false;
        }

        if (SelectedProtocol == "opcua" && SelectedAuthType == AuthenticationType.UserName)
        {
            var username = _usernameField.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrEmpty(username))
            {
                MessageBox.ErrorQuery("Error", "Please enter a username", "OK");
                _usernameField.SetFocus();
                return false;
            }

            var password = _passwordField.Text ?? string.Empty;
            if (string.IsNullOrEmpty(password))
            {
                MessageBox.ErrorQuery("Error", "Please enter a password", "OK");
                _passwordField.SetFocus();
                return false;
            }
        }

        if (SelectedProtocol == "siemens.s7")
        {
            var plc = SiemensPlc.ToLowerInvariant();
            var validPlc = plc is "s1200" or "s1500" or "s300" or "s400" or "s200" or "s200smart";
            if (!validPlc)
            {
                MessageBox.ErrorQuery("Error", "Invalid Siemens PLC type. Use s1200|s1500|s300|s400|s200|s200smart", "OK");
                _plcField.SetFocus();
                return false;
            }
        }

        if (SelectedProtocol == "mqtt")
        {
            if (MqttQos < 0 || MqttQos > 2)
            {
                MessageBox.ErrorQuery("Error", "Invalid MQTT QoS. Use 0|1|2", "OK");
                _mqttQosField.SetFocus();
                return false;
            }

            if (!bool.TryParse((_mqttRetainField.Text ?? string.Empty).Trim(), out _))
            {
                MessageBox.ErrorQuery("Error", "Invalid MQTT retain flag. Use true|false", "OK");
                _mqttRetainField.SetFocus();
                return false;
            }
        }

        return true;
    }
}
