using Npgsql;
using Zss.BilliardHall.Modules.Members.Exceptions;
using Zss.BilliardHall.Modules.Members.Infrastructure.ExceptionTransformers;

namespace Zss.BilliardHall.Tests.UnitTests.Members;

public sealed class MemberEmailUniqueViolationTransformerTests
{
    private static PostgresException CreatePostgresException(string? sqlState, string? constraintName)
        => new(
            "test", "ERROR", "ERROR", sqlState ?? "00000",
            null, null, 0, 0, null, null, null, null, null, null,
            constraintName, null, null, null);

    private readonly MemberEmailUniqueViolationTransformer _sut = new();

    #region TryTransform 命中

    [Fact]
    public void TryTransform_WhenEmailUniqueViolation_ReturnsMemberEmailAlreadyExistsException()
    {
        var pg = CreatePostgresException("23505", "mt_doc_member_uidx_email");

        var result = _sut.TryTransform(pg);

        result.Should().BeOfType<MemberEmailAlreadyExistsException>();
    }

    [Fact]
    public void TryTransform_WhenEmailUniqueViolation_InnerExceptionIsOriginalPostgres()
    {
        var pg = CreatePostgresException("23505", "mt_doc_member_uidx_email");

        var result = _sut.TryTransform(pg);

        result!.InnerException.Should().BeSameAs(pg);
    }

    #endregion

    #region TryTransform 不命中

    [Theory]
    [InlineData("23505", "other_constraint")]
    [InlineData("23000", "mt_doc_member_uidx_email")]
    [InlineData("00000", "mt_doc_member_uidx_email")]
    public void TryTransform_WhenNotEmailUniqueViolation_ReturnsNull(string sqlState, string constraintName)
    {
        var pg = CreatePostgresException(sqlState, constraintName);

        var result = _sut.TryTransform(pg);

        result.Should().BeNull();
    }

    #endregion

    #region 异常契约

    [Theory]
    [InlineData("MEMBER_EMAIL_EXISTS", "会员邮箱已存在")]
    public void MemberEmailAlreadyExistsException_WithInnerException_HasCorrectErrorCodeAndMessage(
        string errorCode, string message)
    {
        var pg = CreatePostgresException("23505", "mt_doc_member_uidx_email");
        var ex = new MemberEmailAlreadyExistsException(innerException: pg);

        ex.ErrorCode.Should().Be(errorCode);
        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeSameAs(pg);
    }

    [Theory]
    [InlineData("MEMBER_EMAIL_EXISTS", "会员邮箱已存在")]
    public void MemberEmailAlreadyExistsException_WithoutInnerException_HasCorrectErrorCodeAndMessage(
        string errorCode, string message)
    {
        var ex = new MemberEmailAlreadyExistsException();

        ex.ErrorCode.Should().Be(errorCode);
        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeNull();
    }

    #endregion
}
