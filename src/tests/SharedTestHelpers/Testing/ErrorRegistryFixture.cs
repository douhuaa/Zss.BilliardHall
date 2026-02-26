using System.Collections.Concurrent;
using System.Reflection;
using Zss.BilliardHall.Platform.Errors;

namespace Zss.BilliardHall.Tests.SharedTestHelpers;

/// <summary>
/// 测试用 ErrorRegistry 初始化辅助器。
/// 由于 ErrorRegistry 是静态单例，通过反射访问内部状态，实现测试间的隔离。
/// 使用方式：在 xUnit 测试类上实现 <c>IClassFixture&lt;ErrorRegistryFixture&gt;</c>，
/// 并在测试构造函数中调用 <see cref="TryRegister"/> 注册测试所需的错误码。
/// </summary>
public sealed class ErrorRegistryFixture : IDisposable
{
    private static readonly FieldInfo ErrorsField =
        typeof(ErrorRegistry).GetField("_errors", BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly FieldInfo FrozenField =
        typeof(ErrorRegistry).GetField("_frozen", BindingFlags.NonPublic | BindingFlags.Static)!;

    private readonly ConcurrentDictionary<string, ErrorDescriptor> _dict;
    private readonly bool _wasFrozen;
    private readonly List<string> _addedCodes = new();

    public ErrorRegistryFixture()
    {
        _dict = (ConcurrentDictionary<string, ErrorDescriptor>)ErrorsField.GetValue(null)!;
        _wasFrozen = (bool)FrozenField.GetValue(null)!;
        FrozenField.SetValue(null, false);
    }

    /// <summary>
    /// 注册单个错误描述符（仅当该错误码尚未注册时生效，避免测试干扰）。
    /// </summary>
    public void TryRegister(ErrorDescriptor descriptor)
    {
        if (_dict.TryAdd(descriptor.Code, descriptor))
            _addedCodes.Add(descriptor.Code);
    }

    public void Dispose()
    {
        foreach (var code in _addedCodes)
            _dict.TryRemove(code, out _);
        FrozenField.SetValue(null, _wasFrozen);
    }
}
