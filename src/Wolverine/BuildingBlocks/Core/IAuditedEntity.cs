namespace Zss.BilliardHall.BuildingBlocks.Core;

/// <summary>
/// 基础审计实体接口 - 只包含时间戳
/// Basic audited entity interface - timestamps only
/// </summary>
public interface ICreationAuditedEntity
{
    /// <summary>
    /// 创建时间（UTC）
    /// Creation time (UTC)
    /// </summary>
    DateTimeOffset CreatedAt { get; }
}

/// <summary>
/// 修改审计实体接口 - 包含修改时间戳
/// Modification audited entity interface - includes modification timestamp
/// </summary>
public interface IModificationAuditedEntity
{
    /// <summary>
    /// 最后修改时间（UTC）
    /// Last modification time (UTC)
    /// </summary>
    DateTimeOffset? UpdatedAt { get; }
}

/// <summary>
/// 完整审计实体接口 - 包含时间戳和用户信息
/// Full audited entity interface - includes timestamps and user information
/// </summary>
public interface IFullAuditedEntity : ICreationAuditedEntity, IModificationAuditedEntity
{
    /// <summary>
    /// 创建人ID（可选）
    /// Creator user ID (optional)
    /// </summary>
    string? CreatedBy { get; }
    
    /// <summary>
    /// 最后修改人ID（可选）
    /// Last modifier user ID (optional)
    /// </summary>
    string? UpdatedBy { get; }
}
