using FluentAssertions;
using Zss.BilliardHall.Modules.Members.Domain;
using Zss.BilliardHall.Modules.Members.Exceptions;
using Zss.BilliardHall.Modules.Members.Infrastructure.ExceptionTranslators;

namespace Zss.BilliardHall.Tests.UnitTests.Members;

public sealed class MemberEmailUniqueViolationTranslatorTests
{
    #region IsEmailUniqueViolation（结构化数据）

    [Theory]
    [InlineData("23505", "mt_doc_member_uidx_email", true)]
    [InlineData("23505", "other_constraint", false)]
    [InlineData("23000", "mt_doc_member_uidx_email", false)]
    [InlineData("00000", "mt_doc_member_uidx_email", false)]
    [InlineData(null, null, false)]
    [InlineData("23505", null, false)]
    [InlineData(null, "mt_doc_member_uidx_email", false)]
    public void IsEmailUniqueViolation_WithStructuredData_ReturnsExpected(
        string? sqlState,
        string? constraintName,
        bool expected)
    {
        MemberEmailUniqueViolationTranslator
            .IsEmailUniqueViolation(sqlState, constraintName)
            .Should()
            .Be(expected);
    }

    #endregion

    #region IsEmailUniqueViolation（异常链，不命中）

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void IsEmailUniqueViolation_WithNonMatchingExceptionChain_ReturnsFalse(int variant)
    {
        var ex = variant switch
        {
            0 => new InvalidOperationException("其他异常"),
            1 => new Exception("外部异常", new InvalidOperationException("内部异常")),
            _ => throw new ArgumentOutOfRangeException(nameof(variant), variant, null)
        };

        MemberEmailUniqueViolationTranslator
            .IsEmailUniqueViolation(ex)
            .Should()
            .BeFalse();
    }

    #endregion

    #region Translate（命中/不命中）

    [Theory]
    [InlineData("23505", "mt_doc_member_uidx_email", typeof(MemberEmailAlreadyExistsException))]
    [InlineData("23505", "other_constraint", null)]
    [InlineData("23000", "mt_doc_member_uidx_email", null)]
    [InlineData(null, null, null)]
    public void Translate_WithStructuredExtractor_ReturnsExpected(
        string? sqlState,
        string? constraintName,
        Type? expectedExceptionType)
    {
        var sut = new MemberEmailUniqueViolationTranslator(_ => (sqlState, constraintName));

        var result = sut.Translate(new Exception("any"));

        if (expectedExceptionType is null)
        {
            result.Should().BeNull();
            return;
        }

        result.Should().BeOfType(expectedExceptionType);
    }

    [Fact]
    public void Translate_WithNonMatchingException_ReturnsNull()
    {
        var sut = new MemberEmailUniqueViolationTranslator();

        sut.Translate(new InvalidOperationException("无关异常"))
            .Should()
            .BeNull();
    }

    #endregion

    #region 常量与异常契约

    [Theory]
    [InlineData("MEMBER_EMAIL_EXISTS")]
    public void MemberErrorCodes_MemberEmailExists_IsExpectedValue(string expected)
    {
        MemberErrorCodes.MemberEmailExists.Should().Be(expected);
    }

    [Theory]
    [InlineData("MEMBER_EMAIL_EXISTS", "会员邮箱已存在")]
    public void MemberEmailAlreadyExistsException_HasCorrectErrorCodeAndMessage(string errorCode, string message)
    {
        var ex = new MemberEmailAlreadyExistsException();

        ex.ErrorCode.Should().Be(errorCode);
        ex.Message.Should().Be(message);
    }

    #endregion
}
