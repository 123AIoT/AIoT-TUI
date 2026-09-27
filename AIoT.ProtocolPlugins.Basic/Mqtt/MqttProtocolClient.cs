using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using AIoT.Protocols.Abstractions;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;

namespace AIoT.ProtocolPlugins.Basic.Mqtt;

public sealed class MqttProtocolClient : IProtocolClient
{
    private IMqttClient? _client;
    private string? _lastReceivedTopic;
    private Dictionary<string, string>? _extra;

    private readonly ConcurrentDictionary<string, ProtocolValue> _latestValues =
        new(StringComparer.OrdinalIgnoreCase);

    public string ProtocolName { get; private set; } = "MQTT";
    public bool IsConnected => _client?.IsConnected == true;

    public async Task<ProtocolResult> ConnectAsync(ProtocolConnectOptions options, CancellationToken ct = default)
    {
        if (!Uri.TryCreate(options.Endpoint, UriKind.Absolute, out var uri))
            return ProtocolResult.Fail("Invalid MQTT endpoint.");

        var scheme = uri.Scheme.ToLowerInvariant();
        if (scheme is not "mqtt" and not "mqtts")
            return ProtocolResult.Fail("Unsupported MQTT endpoint scheme. Use mqtt:// or mqtts://");

        var disconnect = await DisconnectAsync(ct);
        if (!disconnect.Success)
            return disconnect;

        _extra = options.Extra is null
            ? null
            : new Dictionary<string, string>(options.Extra, StringComparer.OrdinalIgnoreCase);

        var host = string.IsNullOrWhiteSpace(uri.Host) ? null : uri.Host;
        if (string.IsNullOrWhiteSpace(host))
            return ProtocolResult.Fail("Missing host for MQTT endpoint.");

        var port = uri.Port > 0 ? uri.Port : (scheme == "mqtts" ? 8883 : 1883);
        var useTls = scheme == "mqtts";

        var builder = new MqttClientOptionsBuilder()
            .WithTcpServer(host, port)
            .WithCleanSession();

        var clientId = GetExtra(options.Extra, "clientId");
        if (!string.IsNullOrWhiteSpace(clientId))
            builder.WithClientId(clientId);

        var username = options.Username ?? GetExtra(options.Extra, "username");
        var password = options.Password ?? GetExtra(options.Extra, "password");
        if (!string.IsNullOrWhiteSpace(username))
            builder.WithCredentials(username, password);

        if (useTls)
            builder.WithTlsOptions(_ => { });

        var mqttFactory = new MqttFactory();
        _client = mqttFactory.CreateMqttClient();
        _client.ApplicationMessageReceivedAsync += e =>
        {
            var payload = e.ApplicationMessage.PayloadSegment.Array is null
                ? string.Empty
                : Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);

            var value = new ProtocolValue(
                e.ApplicationMessage.Topic,
                payload,
                DateTime.UtcNow,
                "Good");

            _latestValues[e.ApplicationMessage.Topic] = value;
            _lastReceivedTopic = e.ApplicationMessage.Topic;
            return Task.CompletedTask;
        };

        try
        {
            await _client.ConnectAsync(builder.Build(), ct);
        }
        catch (Exception ex)
        {
            return ProtocolResult.Fail(ex.Message);
        }

        ProtocolName = "MQTT";
        return ProtocolResult.Ok();
    }

    public async Task<ProtocolResult> DisconnectAsync(CancellationToken ct = default)
    {
        if (_client == null)
            return ProtocolResult.Ok();

        try
        {
            if (_client.IsConnected)
                await _client.DisconnectAsync(cancellationToken: ct);
        }
        catch (Exception ex)
        {
            return ProtocolResult.Fail(ex.Message);
        }
        finally
        {
            _client.Dispose();
            _client = null;
            _latestValues.Clear();
            _lastReceivedTopic = null;
            _extra = null;
            ProtocolName = "MQTT";
        }

        return ProtocolResult.Ok();
    }

    public Task<ProtocolResult<ProtocolValue>> ReadAsync(string address, CancellationToken ct = default)
    {
        if (!IsConnected)
            return Task.FromResult(ProtocolResult<ProtocolValue>.Fail("Not connected"));

        if (!string.IsNullOrWhiteSpace(address) && _latestValues.TryGetValue(address, out var found))
            return Task.FromResult(ProtocolResult<ProtocolValue>.Ok(found));

        if (_lastReceivedTopic is not null && _latestValues.TryGetValue(_lastReceivedTopic, out var latest))
            return Task.FromResult(ProtocolResult<ProtocolValue>.Ok(latest));

        return Task.FromResult(ProtocolResult<ProtocolValue>.Fail("No cached MQTT message available yet."));
    }

    public async Task<ProtocolResult> WriteAsync(string address, string value, CancellationToken ct = default)
    {
        if (!IsConnected || _client == null)
            return ProtocolResult.Fail("Not connected");

        var topic = string.IsNullOrWhiteSpace(address) ? GetExtra(_extra, "topic") : address;
        if (string.IsNullOrWhiteSpace(topic))
            return ProtocolResult.Fail("MQTT write requires topic as address.");

        var qos = ParseQosFromExtra(_extra);
        var retain = ParseBoolFromExtra(_extra, "retain", false);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(value ?? string.Empty)
            .WithQualityOfServiceLevel(qos)
            .WithRetainFlag(retain)
            .Build();

        try
        {
            await _client.PublishAsync(message, ct);
            return ProtocolResult.Ok();
        }
        catch (Exception ex)
        {
            return ProtocolResult.Fail(ex.Message);
        }
    }

    public Task<ProtocolResult<IReadOnlyList<ProtocolNode>>> BrowseAsync(string? parent = null, CancellationToken ct = default)
    {
        IReadOnlyList<ProtocolNode> nodes = _latestValues.Keys
            .OrderBy(k => k, StringComparer.OrdinalIgnoreCase)
            .Select(k => new ProtocolNode(k, k, false))
            .ToList();

        return Task.FromResult(ProtocolResult<IReadOnlyList<ProtocolNode>>.Ok(nodes));
    }

    public async IAsyncEnumerable<ProtocolValue> SubscribeAsync(
        IReadOnlyList<string> addresses,
        int intervalMs = 1000,
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        if (!IsConnected || _client == null)
            yield break;

        var topics = addresses.Count > 0
            ? addresses.Where(a => !string.IsNullOrWhiteSpace(a)).Distinct(StringComparer.OrdinalIgnoreCase).ToList()
            : new List<string>();

        if (topics.Count == 0)
        {
            var defaultTopic = GetExtra(_extra, "topic");
            if (!string.IsNullOrWhiteSpace(defaultTopic))
                topics.Add(defaultTopic);
        }

        if (topics.Count == 0)
            yield break;

        var filters = topics.Select(t =>
        {
            var qos = ParseQos(t, out var topicOnly, _extra);
            return new MqttTopicFilterBuilder()
                .WithTopic(topicOnly)
                .WithQualityOfServiceLevel(qos)
                .Build();
        }).ToList();

        var subscribeOptions = new MqttClientSubscribeOptions
        {
            TopicFilters = filters
        };

        await _client.SubscribeAsync(subscribeOptions, ct);

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(Math.Max(100, intervalMs)));
        while (await timer.WaitForNextTickAsync(ct))
        {
            foreach (var raw in topics)
            {
                _ = ParseQos(raw, out var topic, _extra);
                if (_latestValues.TryGetValue(topic, out var value))
                    yield return value;
            }
        }
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }

    private static string? GetExtra(Dictionary<string, string>? extra, string key)
    {
        if (extra is null)
            return null;

        return extra.TryGetValue(key, out var value) ? value : null;
    }

    private static MqttQualityOfServiceLevel ParseQos(string topicOrTopicWithQos, out string topic, Dictionary<string, string>? extra)
    {
        topic = topicOrTopicWithQos;
        var idx = topicOrTopicWithQos.LastIndexOf(";qos=", StringComparison.OrdinalIgnoreCase);
        if (idx <= 0)
            return ParseQosFromExtra(extra);

        topic = topicOrTopicWithQos[..idx];
        var qosValue = topicOrTopicWithQos[(idx + 5)..];
        return qosValue switch
        {
            "1" => MqttQualityOfServiceLevel.AtLeastOnce,
            "2" => MqttQualityOfServiceLevel.ExactlyOnce,
            _ => ParseQosFromExtra(extra)
        };
    }

    private static MqttQualityOfServiceLevel ParseQosFromExtra(Dictionary<string, string>? extra)
    {
        var value = GetExtra(extra, "qos");
        return value switch
        {
            "1" => MqttQualityOfServiceLevel.AtLeastOnce,
            "2" => MqttQualityOfServiceLevel.ExactlyOnce,
            _ => MqttQualityOfServiceLevel.AtMostOnce
        };
    }

    private static bool ParseBoolFromExtra(Dictionary<string, string>? extra, string key, bool fallback)
    {
        var value = GetExtra(extra, key);
        return bool.TryParse(value, out var parsed) ? parsed : fallback;
    }
}
