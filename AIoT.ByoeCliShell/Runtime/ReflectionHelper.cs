using System.Reflection;

namespace AIoT.ByoeCliShell.Runtime;

internal static class ReflectionHelper
{
    public static object CreateInstance(string typeName, string assemblyName, object?[] args)
    {
        var type = Type.GetType($"{typeName}, {assemblyName}", throwOnError: true)!;
        return Activator.CreateInstance(type, args)
               ?? throw new InvalidOperationException($"Cannot instantiate type {typeName}.");
    }

    public static object? InvokeRequired(object instance, string methodName, params object?[] args)
    {
        var type = instance.GetType();
        var method = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance)
                     ?? throw new InvalidOperationException($"Method '{methodName}' not found on {type.FullName}.");

        var result = method.Invoke(instance, args);
        EnsureOperationSuccess(result, methodName);

        return result;
    }

    public static void EnsureOperationSuccess(object? result, string operationName)
    {
        if (result is null)
            return;

        var resultType = result.GetType();
        var isSuccessProperty = resultType.GetProperty("IsSuccess", BindingFlags.Public | BindingFlags.Instance);
        if (isSuccessProperty?.PropertyType != typeof(bool))
            return;

        var isSuccess = (bool)(isSuccessProperty.GetValue(result) ?? false);
        if (isSuccess)
            return;

        var message = resultType.GetProperty("Message", BindingFlags.Public | BindingFlags.Instance)?.GetValue(result)?.ToString();
        throw new InvalidOperationException(string.IsNullOrWhiteSpace(message)
            ? $"Operation '{operationName}' failed."
            : message);
    }

    public static void SetOptionalProperty(object instance, string propertyName, object value)
    {
        var property = instance.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (property is null || !property.CanWrite)
            return;

        var targetType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
        if (targetType.IsEnum && value is string str)
        {
            property.SetValue(instance, Enum.Parse(targetType, str, ignoreCase: true));
            return;
        }

        if (value.GetType() != targetType)
        {
            value = Convert.ChangeType(value, targetType);
        }

        property.SetValue(instance, value);
    }
}
