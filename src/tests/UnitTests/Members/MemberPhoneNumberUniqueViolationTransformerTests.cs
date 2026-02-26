using Npgsql;
using Zss.BilliardHall.Modules.Members.Exceptions;
using Zss.BilliardHall.Modules.Members.Infrastructure.ExceptionTransformers;

namespace Zss.BilliardHall.Tests.UnitTests.Members;

public sealed class MemberPhoneNumberUniqueViolationTransformerTests
{
    private static PostgresException CreatePostgresException(string? sqlState, string? constraintName)
        => new PostgresException(
            messageText: "test",
            severity: "ERROR",
            invariantSeverity: "ERROR",
            sqlState: sqlState ?? "00000",
            detail: null,
            hint: null,
            position: 0,
            internalPosition: 0,
            internalQuery: null,
            where: null,
            schemaName: null,
            tableName: null,
            columnName: null,
            dataTypeName: null,
            constraintName: constraintName,
            file: null,
            line: null,
            routine: null);

    private readonly MemberPhoneNumberUniqueViolationTransformer _sut = new();

    #region TryTransform 命中

    [Fact]
    public void TryTransform_WhenPhoneNumberUniqueViolation_ReturnsMemberPhoneNumberAlreadyExistsException()
    {
        var pg = CreatePostgresException("23505", "mt_doc_member_uidx_phonenumber");

        var result = _sut.TryTransform(pg);

        result.Should().BeOfType<MemberPhoneNumberAlreadyExistsException>();
    }

    [Fact]
    public void TryTransform_WhenPhoneNumberUniqueViolation_InnerExceptionIsOriginalPostgres()
    {
        var pg = CreatePostgresException("23505", "mt_doc_member_uidx_phonenumber");

        var result = _sut.TryTransform(pg);

        result!.InnerException.Should().BeSameAs(pg);
    }

    #endregion

    #region TryTransform 不命中

    [Theory]
    [InlineData("23505", "other_constraint")]
    [InlineData("23000", "mt_doc_member_uidx_phonenumber")]
    [InlineData("00000", "mt_doc_member_uidx_phonenumber")]
    public void TryTransform_WhenNotPhoneNumberUniqueViolation_ReturnsNull(string sqlState, string constraintName)
    {
        var pg = CreatePostgresException(sqlState, constraintName);

        var result = _sut.TryTransform(pg);

        result.Should().BeNull();
    }

    #endregion

    #region 异常契约

    [Theory]
    [InlineData("MEMBER_PHONE_NUMBER_EXISTS", "会员手机号已存在")]
    public void MemberPhoneNumberAlreadyExistsException_WithInnerException_HasCorrectErrorCodeAndMessage(
        string errorCode, string message)
    {
        var pg = CreatePostgresException("23505", "mt_doc_member_uidx_phonenumber");
        var ex = new MemberPhoneNumberAlreadyExistsException(innerException: pg);

        ex.ErrorCode.Should().Be(errorCode);
        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeSameAs(pg);
    }

    [Theory]
    [InlineData("MEMBER_PHONE_NUMBER_EXISTS", "会员手机号已存在")]
    public void MemberPhoneNumberAlreadyExistsException_WithoutInnerException_HasCorrectErrorCodeAndMessage(
        string errorCode, string message)
    {
        var ex = new MemberPhoneNumberAlreadyExistsException();

        ex.ErrorCode.Should().Be(errorCode);
        ex.Message.Should().Be(message);
        ex.InnerException.Should().BeNull();
    }

    #endregion
}
