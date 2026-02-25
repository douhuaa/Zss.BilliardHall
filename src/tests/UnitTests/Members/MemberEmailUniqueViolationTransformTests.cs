using Microsoft.AspNetCore.Http;
using Zss.BilliardHall.Modules.Members.Domain;
using Zss.BilliardHall.Modules.Members.Domain.Exceptions;
using Zss.BilliardHall.Modules.Members.Infrastructure.ExceptionTransforms;

namespace Zss.BilliardHall.Tests.UnitTests.Members;

public class MemberEmailUniqueViolationTransformTests
{
    private readonly MemberEmailUniqueViolationTransform _sut = new();

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
        var result = MemberEmailUniqueViolationTransform.IsEmailUniqueViolation(sqlState, constraintName);

        result.Should().Be(expected);
    }

    #endregion

    #region IsEmailUniqueViolation 通过异常链判定

    [Fact]
    public void IsEmailUniqueViolation_WithNonMatchingException_ReturnsFalse()
    {
        var ex = new InvalidOperationException("其他异常");

        var result = MemberEmailUniqueViolationTransform.IsEmailUniqueViolation(ex);

        result.Should().BeFalse();
    }

    [Fact]
    public void IsEmailUniqueViolation_WithInnerNonMatchingException_ReturnsFalse()
    {
        var inner = new InvalidOperationException("内部异常");
        var outer = new Exception("外部异常", inner);

        var result = MemberEmailUniqueViolationTransform.IsEmailUniqueViolation(outer);

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
    public void MemberEmailAlreadyExistsException_HasCorrectErrorCode()
    {
        var ex = new MemberEmailAlreadyExistsException();

        ex.ErrorCode.Should().Be(MemberErrorCodes.MemberEmailExists);
        ex.Message.Should().Be("会员邮箱已存在");
    }

    #endregion

    #region 翻译后 DomainException 映射为 409

    [Fact]
    public void MemberEmailAlreadyExistsException_MapsTo409WithStableErrorCode()
    {
        var mapper = new Zss.BilliardHall.Host.Web.ExceptionProblemDetailsMapper();
        var ex = new MemberEmailAlreadyExistsException();

        var result = mapper.Map(ex, "/api/members", includeExceptionDetail: false);

        result.Status.Should().Be(StatusCodes.Status409Conflict);
        result.Extensions["errorCode"].Should().Be(MemberErrorCodes.MemberEmailExists);
        result.Detail.Should().Be("会员邮箱已存在");
        result.Detail.Should().NotContain("mt_doc_member_uidx_email");
        result.Detail.Should().NotContain("23505");
    }

    #endregion
}
