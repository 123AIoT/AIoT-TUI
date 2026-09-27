namespace AIoT.ByoeCliShell.Runtime;

internal interface IDisposableClient : IDisposable
{
    string Protocol { get; }
    string Read(string address, string type);
    void Write(string address, string value, string type);
}

internal sealed class ReflectionClient(string protocol, object instance, Action<object> disposeAction) : IDisposableClient
{
    public string Protocol { get; } = protocol;

    public string Read(string address, string type)
    {
        var normalized = type.Trim().ToLowerInvariant();

        return normalized switch
        {
            "bool" => ReadScalar("ReadBool", address),
            "float" => ReadScalar("ReadFloat", address),
            "string" => ReadString(address),
            _ => ReadScalar("ReadInt32", address)
        };
    }

    public void Write(string address, string value, string type)
    {
        var normalized = type.Trim().ToLowerInvariant();
        object? result;

        result = normalized switch
        {
            "bool" => InvokeWrite(address, bool.Parse(value)),
            "float" => InvokeWrite(address, float.Parse(value)),
            "string" => InvokeWrite(address, value),
            _ => InvokeWrite(address, int.Parse(value))
        };

        ReflectionHelper.EnsureOperationSuccess(result, "Write");
    }

    public void Dispose() => disposeAction(instance);

    private string ReadScalar(string methodName, string address)
    {
        var method = instance.GetType().GetMethod(methodName, [typeof(string)])
                     ?? throw new InvalidOperationException($"Read method {methodName}(string) not found.");

        var result = method.Invoke(instance, [address]);
        return ReadOperateResultContent(result);
    }

    private string ReadString(string address)
    {
        var method = instance.GetType().GetMethod("ReadString", [typeof(string)])
                     ?? instance.GetType().GetMethod("ReadString", [typeof(string), typeof(ushort), typeof(System.Text.Encoding)]);

        if (method is null)
            throw new InvalidOperationException("ReadString method not found.");

        object? result;
        if (method.GetParameters().Length == 1)
        {
            result = method.Invoke(instance, [address]);
        }
        else
        {
            result = method.Invoke(instance, [address, (ushort)64, System.Text.Encoding.ASCII]);
        }

        return ReadOperateResultContent(result);
    }

    private object? InvokeWrite(string address, object typedValue)
    {
        var method = instance.GetType().GetMethod("Write", [typeof(string), typedValue.GetType()])
                     ?? throw new InvalidOperationException($"Write(string,{typedValue.GetType().Name}) not found.");

        return method.Invoke(instance, [address, typedValue]);
    }

    private static string ReadOperateResultContent(object? result)
    {
        if (result is null)
            throw new InvalidOperationException("Read returned null.");

        var type = result.GetType();
        var isSuccess = (bool)(type.GetProperty("IsSuccess")?.GetValue(result)
                               ?? throw new InvalidOperationException("OperateResult has no IsSuccess."));

        if (!isSuccess)
        {
            var message = (string?)type.GetProperty("Message")?.GetValue(result) ?? "Read failed.";
            throw new InvalidOperationException(message);
        }

        var content = type.GetProperty("Content")?.GetValue(result);
        return content?.ToString() ?? string.Empty;
    }
}
