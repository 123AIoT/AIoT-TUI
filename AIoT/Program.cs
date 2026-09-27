using Terminal.Gui;
using AIoT.App;
using AIoT.Cli;
using AIoT.Protocols;

namespace AIoT;

class Program
{
    static int Main(string[] args)
    {
        if (CliCommandRouter.IsCliMode(args))
        {
            InitializeProtocolPlugins();
            return RunCli(args);
        }

        try
        {
            InitializeProtocolPlugins();
#pragma warning disable IL2026 // Terminal.Gui Application.Init uses reflection and is not AOT-compatible
            Application.Init();
#pragma warning restore IL2026

            string? autoConnectUrl = null;
            string? configPath = null;

            for (int i = 0; i < args.Length; i++)
            {
                if ((args[i] == "--config" || args[i] == "-f") && i + 1 < args.Length)
                {
                    configPath = args[i + 1];
                    i++;
                }
                else if (args[i].EndsWith(".cfg", StringComparison.OrdinalIgnoreCase) ||
                         args[i].EndsWith(".aiot", StringComparison.OrdinalIgnoreCase) ||
                         (args[i].EndsWith(".json", StringComparison.OrdinalIgnoreCase) && File.Exists(args[i])))
                {
                    configPath = args[i];
                }
                else if ((args[i] == "--connect" || args[i] == "-c") && i + 1 < args.Length)
                {
                    autoConnectUrl = args[i + 1];
                    i++;
                }
                else if (args[i].StartsWith("opc.tcp://"))
                {
                    autoConnectUrl = args[i];
                }
                else if (args[i] == "--help" || args[i] == "-h")
                {
                    PrintUsage();
                    return 0;
                }
            }

            var mainWindow = new MainWindow();

            if (!string.IsNullOrEmpty(configPath))
            {
                if (!File.Exists(configPath))
                {
                    Console.Error.WriteLine($"Error: Configuration file not found: {configPath}");
                    Application.Shutdown();
                    return 1;
                }

                mainWindow.LoadConfigFromCommandLine(configPath);
            }
            else if (!string.IsNullOrEmpty(autoConnectUrl))
            {
                Console.Error.WriteLine(
                    $"Warning: Auto-connect via command-line URL ('{autoConnectUrl}') is not currently implemented. " +
                    "Please use a configuration file with an endpoint URL instead.");
            }

            Application.Run(mainWindow);
            mainWindow.Dispose();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fatal error: {ex.Message}");
            Console.Error.WriteLine(ex.StackTrace);
            return 1;
        }
        finally
        {
            Application.Shutdown();
        }

        return 0;
    }

    private static int RunCli(string[] args)
    {
        try
        {
            var command = CliCommandRouter.Parse(args, out var parseError);
            if (command == null)
            {
                Console.Error.WriteLine(parseError ?? "Invalid command arguments.");
                PrintUsage();
                return 2;
            }

            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            return CliCommandRouter.RunAsync(command, cts.Token).GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
            return 130;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"CLI error: {ex.Message}");
            return 1;
        }
    }

    private static void PrintUsage()
    {
        Console.WriteLine("aiot - Terminal-based industrial protocol client");
        Console.WriteLine();
        Console.WriteLine("Usage: aiot [options] [file]");
        Console.WriteLine("       aiot <read|write|browse|monitor> <endpoint> ... [--format text|json|ndjson]");
        Console.WriteLine("       endpoint examples: opc.tcp://127.0.0.1:4840, mqtt://127.0.0.1:1883, ws://127.0.0.1:8080/ws, modbus.tcp://127.0.0.1:502, modbus.rtu://COM3?baud=9600");
        Console.WriteLine("       more endpoints: s7://192.168.0.10:102?plc=s1200&rack=0&slot=1, melsec://192.168.0.11:6000, omron://192.168.0.12:9600, delta://192.168.0.13:502?series=dvp&station=1, xinje://192.168.0.14:502?series=xc&station=1");
        Console.WriteLine("       override: --protocol opcua|mqtt|websocket|modbus.tcp|modbus.rtu|siemens.s7|melsec|omron|delta|xinje  --station 1  --baud 9600");
        Console.WriteLine("                 --type bool|byte|sbyte|short|ushort|int|uint|long|ulong|float|double|string|word|dword|hex  --length <n>");
        Console.WriteLine("       mqtt opts: --client-id <id> --user <u> --pass <p> --qos 0|1|2 --retain --topic <name>");
        Console.WriteLine("       websocket opts: --ws-subprotocol <name> --ws-header k1:v1[;k2:v2] --ws-address <name>");
        Console.WriteLine($"       supported protocols: {string.Join(", ", CliCommandRouter.GetSupportedProtocols())}");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -f, --config <file>   Load configuration file (.cfg, .aiot, or .json)");
        Console.WriteLine("      --protocol <name> Force protocol selection for ambiguous endpoints");
        Console.WriteLine("      --station <id>    Modbus station id for Byoe-backed endpoints");
        Console.WriteLine("      --baud <rate>     Serial baud rate for Modbus RTU endpoints");
        Console.WriteLine("      --plc <type>      Siemens PLC type (s1200|s1500|s300|s400|s200|s200smart)");
        Console.WriteLine("      --rack <id>       Siemens rack id (default 0)");
        Console.WriteLine("      --slot <id>       Siemens slot id (default 0)");
        Console.WriteLine("      --type <name>     Type hint: bool|byte|sbyte|short|ushort|int|uint|long|ulong|float|double|string|word|dword|hex");
        Console.WriteLine("      --length <n>      Optional data length for string/hex or multi-register values");
        Console.WriteLine("  -h, --help            Show this help message");
        Console.WriteLine();
        Console.WriteLine("TUI note: In Connect dialog, Modbus RTU baud/station and Siemens plc/rack/slot");
        Console.WriteLine("          can be entered directly via dedicated fields.");
        Console.WriteLine();
        Console.WriteLine("Note: Direct server connection via --connect or opc.tcp:// URLs is not yet");
        Console.WriteLine("      implemented. Please create a configuration file with the server URL.");
        Console.WriteLine();
        Console.WriteLine("Examples:");
        Console.WriteLine("  aiot                           Start with empty configuration");
        Console.WriteLine("  aiot production.cfg            Load configuration file");
        Console.WriteLine("  aiot --config config.json      Load configuration file");
        Console.WriteLine();
    }

    private static void InitializeProtocolPlugins()
    {
        var primaryPluginsDir = Path.Combine(AppContext.BaseDirectory, "plugins");
        var fallbackPluginsDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "plugins"));
        var pluginsDir = Directory.Exists(primaryPluginsDir)
            ? primaryPluginsDir
            : fallbackPluginsDir;

        if (!Directory.Exists(primaryPluginsDir) && Directory.Exists(fallbackPluginsDir))
        {
            Console.Error.WriteLine($"[plugin] Using fallback plugin folder: {fallbackPluginsDir}");
        }

        var pluginsOnly = string.Equals(
            Environment.GetEnvironmentVariable("AIOT_PROTOCOL_PLUGINS_ONLY"),
            "1",
            StringComparison.OrdinalIgnoreCase);

        ProtocolClientFactory.Initialize(
            pluginsDir,
            includeBuiltins: !pluginsOnly,
            log: message => Console.Error.WriteLine($"[plugin] {message}"));
    }
}
