using Zss.BilliardHall.Modules.Members.Domain;
using Zss.BilliardHall.Modules.Members.Exceptions;
using Zss.BilliardHall.Modules.Members.Infrastructure.ExceptionTranslators;

namespace Zss.BilliardHall.Tests.UnitTests.Members;

public class MemberEmailUniqueViolationTranslatorTests
{
    private readonly MemberEmailUniqueViolationTranslator _sut = new();

    #region IsEmailUniqueViolation 结构化参数判定（无需构造 PostgresException）

    [Theory]
    [InlineData("23505", "mt_doc_member_uidx_email", true)]
    [InlineData("23505", "other_constraint", false)]
    [InlineData("23000", "mt_doc_member_uidx_email", false)]
    [InlineData("00000", "mt_doc_member_uidx_email", false)]
    [InlineData(null, null, false)]
    [InlineData("23505", null, false)]
    [InlineData(null, "mt_doc_member_uidx_email", false)]
    public void IsEmailUniqueViolation_WithStructuredData_ReturnsExpected(
        string? sqlState, string? constraintName, bool expected)
    {
        var result = MemberEmailUniqueViolationTranslator.IsEmailUniqueViolation(sqlState, constraintName);

        result.Should().Be(expected);
    }

    #endregion

    #region IsEmailUniqueViolation 通过异常链判定

    [Fact]
    public void IsEmailUniqueViolation_WithNonMatchingException_ReturnsFalse()
    {
        var ex = new InvalidOperationException("其他异常");

        var result = MemberEmailUniqueViolationTranslator.IsEmailUniqueViolation(ex);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsEmailUniqueViolation_WithInnerNonMatchingException_ReturnsFalse()
    {
        var inner = new InvalidOperationException("内部异常");
        var outer = new Exception("外部异常", inner);

        var result = MemberEmailUniqueViolationTranslator.IsEmailUniqueViolation(outer);

        result.Should().BeFalse();
    }

    #endregion

    #region Translate 方法

    [Fact]
    public void Translate_WithNonMatchingException_ReturnsNull()
    {
        var ex = new InvalidOperationException("无关异常");

        var result = _sut.Translate(ex);

        result.Should().BeNull();
    }

    #endregion

    #region MemberErrorCodes 常量验证

    [Fact]
    public void MemberErrorCodes_MemberEmailExists_IsExpectedValue()
    {
        MemberErrorCodes.MemberEmailExists.Should().Be("MEMBER_EMAIL_EXISTS");
    }

    #endregion

    #region MemberEmailAlreadyExistsException 属性验证

    [Fact]
    public void MemberEmailAlreadyExistsException_HasCorrectErrorCodeAndMessage()
    {
        var ex = new MemberEmailAlreadyExistsException();

        ex.ErrorCode.Should().Be(MemberErrorCodes.MemberEmailExists);
        ex.Message.Should().Be("会员邮箱已存在");
    }

    #endregion
}

