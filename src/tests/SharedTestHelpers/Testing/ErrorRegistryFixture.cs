using Zss.BilliardHall.Platform.Errors;

namespace Zss.BilliardHall.Tests.SharedTestHelpers;

/// <summary>
/// 测试用 ErrorRegistry 初始化辅助器。
/// 提供针对 xUnit <c>IClassFixture&lt;ErrorRegistryFixture&gt;</c> 的生命周期管理，
/// 在构建时解冻并允许注册，在 Dispose 时移除本次添加的错误码并恢复原始冻结状态。
/// </summary>
/// <remarks>
/// <para>
/// <b>并发限制</b>：xUnit 默认在同一测试类内串行执行，但跨测试类可能并行运行。
/// 如需避免与其他使用 ErrorRegistryFixture 的测试类发生干扰，
/// 请将相关测试类置于同一 xUnit 测试集合（<c>[Collection]</c>）中，强制串行执行。
/// </para>
/// </remarks>
public sealed class ErrorRegistryFixture : IDisposable
{
    private readonly bool _wasFrozen;
    private readonly List<string> _addedCodes = new();

    public ErrorRegistryFixture()
    {
        _wasFrozen = ErrorRegistry.IsFrozen;
        // 确保 Registry 处于可注册状态
        if (_wasFrozen)
            ErrorRegistry.RestoreForTesting([], freeze: false);
    }

    /// <summary>
    /// 注册单个错误描述符（仅当该错误码尚未注册时生效，避免测试间干扰）。
    /// </summary>
    public void TryRegister(ErrorDescriptor descriptor)
    {
        if (IsRegistered(descriptor.Code))
            return;

        ErrorRegistry.Register(descriptor);
        _addedCodes.Add(descriptor.Code);
    }

    public void Dispose()
    {
        // 清理本 Fixture 注册的错误码，还原冻结状态
        ErrorRegistry.RestoreForTesting(_addedCodes, _wasFrozen);
    }

    private static bool IsRegistered(string code)
        => ErrorRegistry.Contains(code);
}
