namespace AIoT.ByoeCliShell.Runtime;

internal interface IDisposableSimulator : IDisposable
{
    string Protocol { get; }
    void Write(string address, string value, string type);
}

internal sealed class ReflectionSimulator(string protocol, object instance, Action<object> disposeAction) : IDisposableSimulator
{
    public string Protocol { get; } = protocol;

    public void Write(string address, string value, string type)
    {
        var normalized = type.Trim().ToLowerInvariant();
        object? result;

        result = normalized switch
        {
            "bool" => InvokeWrite(address, bool.Parse(value)),
            "float" => InvokeWrite(address, float.Parse(value)),
            _ => InvokeWrite(address, int.Parse(value))
        };

        ReflectionHelper.EnsureOperationSuccess(result, "Simulator.Write");
    }

    public void Dispose() => disposeAction(instance);

    private object? InvokeWrite(string address, object typedValue)
    {
        var method = instance.GetType().GetMethod("Write", [typeof(string), typedValue.GetType()])
                     ?? throw new InvalidOperationException($"Simulator Write(string,{typedValue.GetType().Name}) not found.");

        return method.Invoke(instance, [address, typedValue]);
    }
}
