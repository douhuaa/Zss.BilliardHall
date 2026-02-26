using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Zss.BilliardHall.Platform.Errors;

public static class ErrorRegistry
{
    private static readonly ConcurrentDictionary<string, ErrorDescriptor> _errors = new();
    private static bool _frozen;

    public static void Register(ErrorDescriptor descriptor)
    {
        if (_frozen)
            throw new InvalidOperationException("ErrorRegistry 已冻结，禁止注册。");

        if (!_errors.TryAdd(descriptor.Code, descriptor))
            throw new InvalidOperationException($"错误码重复注册: {descriptor.Code}");
    }

    public static ErrorDescriptor Get(string code)
    {
        if (!_errors.TryGetValue(code, out var descriptor))
            throw new KeyNotFoundException($"未注册错误码: {code}");

        return descriptor;
    }

    public static IReadOnlyCollection<ErrorDescriptor> All => (IReadOnlyCollection<ErrorDescriptor>)_errors.Values;

    public static void Freeze() => _frozen = true;
}

