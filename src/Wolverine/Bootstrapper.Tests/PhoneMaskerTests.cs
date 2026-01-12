using FluentAssertions;
using Xunit;
using Zss.BilliardHall.BuildingBlocks.Contracts;

namespace Zss.BilliardHall.Wolverine.Bootstrapper.Tests;

/// <summary>
/// PhoneMasker 工具类单元测试
/// PhoneMasker utility unit tests
/// </summary>
public class PhoneMaskerTests
{
    [Theory]
    [InlineData("13800138000", "*******8000")]
    [InlineData("18912345678", "*******5678")]
    [InlineData("1234", "****")]  // 4位或更短，全部掩码
    [InlineData("12345", "*2345")]
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Mask_WithDefaultKeepLength_ShouldMaskCorrectly(string? input, string expected)
    {
        // Act
        var result = PhoneMasker.Mask(input!);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Mask_WithChinesePhoneNumber_ShouldKeepLast4Digits()
    {
        // Arrange - 中国标准手机号 11 位
        var phone = "13800138000";

        // Act
        var masked = PhoneMasker.Mask(phone);

        // Assert
        masked.Should().Be("*******8000");
        masked.Length.Should().Be(11);
    }

    [Theory]
    [InlineData("13800138000", 0, "***********")]  // 全部掩码
    [InlineData("13800138000", 2, "*********00")]   // 保留后2位
    [InlineData("13800138000", 4, "*******8000")]   // 保留后4位
    [InlineData("13800138000", 6, "*****138000")]   // 保留后6位
    [InlineData("13800138000", 20, "***********")]  // 保留位数超过长度，全部掩码
    public void Mask_WithCustomKeepLength_ShouldMaskCorrectly(string input, int keepDigits, string expected)
    {
        // Act
        var result = PhoneMasker.Mask(input, keepDigits);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void Mask_PreservesOriginalLength()
    {
        // Arrange
        var phone = "13800138000";

        // Act
        var masked = PhoneMasker.Mask(phone);

        // Assert
        masked.Length.Should().Be(phone.Length);
    }

    [Fact]
    public void Mask_ShouldNotRevealSensitiveDigits()
    {
        // Arrange
        var phone = "13800138000";

        // Act
        var masked = PhoneMasker.Mask(phone);

        // Assert
        masked.Should().StartWith("*******");
        masked.Should().NotContain("138");  // 前缀不应暴露
        masked.Should().EndWith("8000");    // 只保留后4位
    }
}
