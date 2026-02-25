using Npgsql;
using Zss.BilliardHall.Modules.Members.Domain.Exceptions;
using Zss.BilliardHall.Platform.Contracts;

namespace Zss.BilliardHall.Modules.Members.Infrastructure.ExceptionTranslators;

/// <summary>
/// 将 PostgreSQL 唯一约束冲突（会员邮箱）翻译为 DomainException
/// </summary>
/// <remarks>
/// 在 MemberModule.ConfigureServices 中注册为 IExceptionTranslator，
/// 由 GlobalExceptionMiddleware 在映射前调用。
/// Web 层映射器无需感知 Marten/Npgsql 异常类型。
/// </remarks>
public sealed class MemberEmailUniqueViolationTranslator : IExceptionTranslator
{
    private const string EmailConstraintName = "mt_doc_member_uidx_email";
    private const string UniqueViolationSqlState = "23505";

    /// <inheritdoc />
    public Exception? Translate(Exception ex) =>
        IsEmailUniqueViolation(ex) ? new MemberEmailAlreadyExistsException() : null;

    /// <summary>
    /// 核心判定逻辑（可单测）：是否为会员邮箱唯一约束冲突
    /// </summary>
    public static bool IsEmailUniqueViolation(Exception ex)
    {
        var pgEx = FindPostgresException(ex);
        return IsEmailUniqueViolation(pgEx?.SqlState, pgEx?.ConstraintName);
    }

    /// <summary>
    /// 根据结构化字段判定（用于单元测试，无需构造 PostgresException）
    /// </summary>
    public static bool IsEmailUniqueViolation(string? sqlState, string? constraintName) =>
        sqlState == UniqueViolationSqlState && constraintName == EmailConstraintName;

    private static PostgresException? FindPostgresException(Exception ex) =>
        ex switch
        {
            PostgresException pg => pg,
            _ when ex.InnerException != null => FindPostgresException(ex.InnerException),
            _ => null
        };
}
