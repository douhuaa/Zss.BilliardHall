using Npgsql;
using Zss.BilliardHall.Modules.Members.Exceptions;
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

    private readonly Func<Exception, (string? SqlState, string? ConstraintName)> _extract;

    public MemberEmailUniqueViolationTranslator()
        : this(ExtractFromException)
    {
    }

    public MemberEmailUniqueViolationTranslator(
        Func<Exception, (string? SqlState, string? ConstraintName)> extract)
    {
        _extract = extract ?? throw new ArgumentNullException(nameof(extract));
    }

    public Exception? Translate(Exception ex)
    {
        ArgumentNullException.ThrowIfNull(ex);

        var (sqlState, constraintName) = _extract(ex);
        return IsEmailUniqueViolation(sqlState, constraintName)
            ? new MemberEmailAlreadyExistsException()
            : null;
    }

    public static bool IsEmailUniqueViolation(string? sqlState, string? constraintName) =>
        sqlState == UniqueViolationSqlState && constraintName == EmailConstraintName;

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
