using Npgsql;
using Zss.BilliardHall.Modules.Members.Exceptions;
using Zss.BilliardHall.Platform.Exceptions;
using Zss.BilliardHall.Platform.Infrastructure;

namespace Zss.BilliardHall.Modules.Members.Infrastructure.ExceptionTranslators;

/// <summary>
/// 将 PostgreSQL 唯一约束冲突（会员邮箱）转换为 DomainException
/// </summary>
/// <remarks>
/// 在 MemberModule.ConfigureServices 中注册为 IPostgresExceptionTransformer，
/// 由 ExceptionTransformMiddleware 在 Wolverine pipeline 中调用。
/// 直接读取 SqlState 和 ConstraintName，无需遍历异常链。
/// </remarks>
public sealed class MemberEmailUniqueViolationTransformer : IPostgresExceptionTransformer
{
    private const string EmailConstraintName = "mt_doc_member_uidx_email";
    private const string UniqueViolationSqlState = "23505";

    public DomainException? TryTransform(PostgresException ex)
        => ex.SqlState == UniqueViolationSqlState && ex.ConstraintName == EmailConstraintName
            ? new MemberEmailAlreadyExistsException(innerException: ex)
            : null;
}
