using System.Collections.Concurrent;

namespace Zss.BilliardHall.Platform.Errors;

/// <summary>
/// 全局错误码注册表（静态单例）。
/// 在应用启动阶段，通过 <see cref="IErrorModule"/> 实现类将各模块错误码注册到此表中，
/// 调用 <see cref="Freeze"/> 后不允许再注册新的错误码。
/// </summary>
/// <remarks>
/// <para>生命周期：应用启动时由各 <c>IErrorModule</c> 注册，<see cref="Freeze"/> 后只读。</para>
/// <para>线程安全：写操作基于 <see cref="ConcurrentDictionary"/>，<c>_frozen</c> 声明为 <c>volatile</c> 以保证可见性。</para>
/// <para>测试注意：静态状态在测试之间共享，架构测试应基于类型反射而非运行时注册状态。</para>
/// </remarks>
public static class ErrorRegistry
{
    private static readonly ConcurrentDictionary<string, ErrorDescriptor> _errors = new();
    private static volatile bool _frozen;

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

    public static IReadOnlyCollection<ErrorDescriptor> All => _errors.Values.ToList().AsReadOnly();

    public static void Freeze() => _frozen = true;

    /// <summary>仅供测试使用：当前是否已冻结。</summary>
    internal static bool IsFrozen => _frozen;

    /// <summary>
    /// 仅供测试使用：重置注册表状态，清除所有已注册的错误码并解除冻结。
    /// </summary>
    internal static void ResetForTesting()
    {
        _errors.Clear();
        _frozen = false;
    }

    /// <summary>
    /// 仅供测试使用：移除指定的错误码并（可选地）恢复冻结状态。
    /// </summary>
    internal static void RestoreForTesting(IEnumerable<string> codesToRemove, bool freeze)
    {
        foreach (var code in codesToRemove)
            _errors.TryRemove(code, out _);
        _frozen = freeze;
    }
}
