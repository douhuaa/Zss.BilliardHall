using Zss.BilliardHall.BuildingBlocks.Core;

namespace Zss.BilliardHall.Modules.Tables;

/// <summary>
/// Tables 模块错误描述符（示例）
/// Tables module error descriptors (example)
/// </summary>
internal static class TableErrorDescriptors
{
    private const string ModuleName = "Tables";

    // NotFound errors
    public static ErrorDescriptor TableNotFound(Guid tableId) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:NotFound.Table")
            .WithCategory(ErrorCategory.NotFound)
            .WithMessage("台球桌不存在: {TableId}")
            .AddContext("TableId", tableId)
            .Build();

    // Validation errors
    public static ErrorDescriptor InvalidCapacity(int capacity) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:Validation.InvalidCapacity")
            .WithCategory(ErrorCategory.Validation)
            .WithMessage("台球桌容量无效，必须在 1-6 之间，实际: {Capacity}")
            .AddContext("Capacity", capacity)
            .Build();

    public static ErrorDescriptor InvalidTableNumber(string number) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:Validation.InvalidTableNumber")
            .WithCategory(ErrorCategory.Validation)
            .WithMessage("台球桌编号无效: {Number}")
            .AddContext("Number", number)
            .Build();

    // Business rule errors
    public static ErrorDescriptor TableOccupied(Guid tableId, Guid sessionId) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:Business.TableOccupied")
            .WithCategory(ErrorCategory.Business)
            .WithMessage("台球桌已被占用: {TableId}, 会话: {SessionId}")
            .AddContext("TableId", tableId)
            .AddContext("SessionId", sessionId)
            .Build();

    public static ErrorDescriptor TableUnderMaintenance(Guid tableId) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:Business.TableUnderMaintenance")
            .WithCategory(ErrorCategory.Business)
            .WithMessage("台球桌维护中，暂不可用: {TableId}")
            .AddContext("TableId", tableId)
            .Build();

    // InvalidStatus errors
    public static ErrorDescriptor InvalidTableStatus(Guid tableId, string currentStatus, string expectedStatus) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:InvalidStatus.Table")
            .WithCategory(ErrorCategory.InvalidStatus)
            .WithMessage("台球桌状态不符，当前: {CurrentStatus}, 期望: {ExpectedStatus}")
            .AddContext("TableId", tableId)
            .AddContext("CurrentStatus", currentStatus)
            .AddContext("ExpectedStatus", expectedStatus)
            .Build();

    // Conflict errors
    public static ErrorDescriptor DuplicateTableNumber(string number) =>
        ErrorCodeBuilder.ForModule(ModuleName)
            .WithCode($"{ModuleName}:Conflict.DuplicateTableNumber")
            .WithCategory(ErrorCategory.Conflict)
            .WithMessage("台球桌编号已存在: {Number}")
            .AddContext("Number", number)
            .Build();
}

/// <summary>
/// Tables 模块领域异常
/// Tables module domain exception
/// </summary>
public sealed class TablesDomainException : ModuleDomainException
{
    public TablesDomainException(ErrorDescriptor errorDescriptor)
        : base(errorDescriptor)
    {
    }

    public TablesDomainException(ErrorDescriptor errorDescriptor, Exception innerException)
        : base(errorDescriptor, innerException)
    {
    }
}
