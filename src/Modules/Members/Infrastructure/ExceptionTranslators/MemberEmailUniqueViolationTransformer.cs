using Npgsql;
using Zss.BilliardHall.Modules.Members.Exceptions;
using Zss.BilliardHall.Platform.Exceptions;
using Zss.BilliardHall.Platform.Infrastructure;

namespace Zss.BilliardHall.Modules.Members.Infrastructure.ExceptionTranslators;

/// <summary>
/// 会员邮箱唯一约束冲突转换器
/// </summary>
/// <remarks>
/// 职责：检测 PostgreSQL 唯一约束冲突（会员邮箱）并转换为 DomainException
/// 
/// 实现 IPostgresExceptionTransformer 接口，由 ExceptionTransformMiddleware 调用。
/// 确保 PostgresException 在到达 Web 层之前被转换为语义异常。
/// </remarks>
public sealed class MemberEmailUniqueViolationTransformer : IPostgresExceptionTransformer
{
    private const string EmailConstraintName = "mt_doc_member_uidx_email";
    private const string UniqueViolationSqlState = "23505";

    /// <inheritdoc />
    public DomainException? TryTransform(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);
        return IsEmailUniqueViolation(ex) ? new MemberEmailAlreadyExistsException() : null;
    }

    /// <summary>
    /// 检查是否为会员邮箱唯一约束冲突
    /// </summary>
    public static bool IsEmailUniqueViolation(string? sqlState, string? constraintName) =>
        sqlState == UniqueViolationSqlState && constraintName == EmailConstraintName;

    /// <summary>
    /// 从异常中检测是否为会员邮箱唯一约束冲突
    /// </summary>
    public static bool IsEmailUniqueViolation(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);
        var (sqlState, constraintName) = ExtractFromException(ex);
        return IsEmailUniqueViolation(sqlState, constraintName);
    }

    private static (string? SqlState, string? ConstraintName) ExtractFromException(Exception ex)
    {
        var pg = FindPostgresException(ex);
        return (pg?.SqlState, pg?.ConstraintName);
    }

    private static PostgresException? FindPostgresException(Exception? ex)
    {
        for (var current = ex; current is not null; current = current.InnerException)
        {
            if (current is PostgresException pg) return pg;
        }

        return null;
    }
}
