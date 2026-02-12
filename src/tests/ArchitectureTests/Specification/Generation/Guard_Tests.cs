namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.Generation;

/// <summary>
/// Guard 辅助类单元测试
/// </summary>
public sealed class Guard_Tests
{
    #region NotNull Tests

    [Fact(DisplayName = "NotNull 应该对 null 参数抛出 ArgumentNullException")]
    public void NotNull_Should_Throw_For_Null_Parameter()
    {
        // Arrange
        string? nullValue = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() => Guard.NotNull(nullValue, "testParam"));
        Assert.Equal("testParam", exception.ParamName);
    }

    [Fact(DisplayName = "NotNull 应该返回非 null 参数")]
    public void NotNull_Should_Return_NonNull_Parameter()
    {
        // Arrange
        var value = "test";

        // Act
        var result = Guard.NotNull(value, "testParam");

        // Assert
        Assert.Equal("test", result);
    }

    #endregion

    #region NotNullOrWhiteSpace Tests

    [Theory(DisplayName = "NotNullOrWhiteSpace 应该对无效输入抛出异常")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void NotNullOrWhiteSpace_Should_Throw_For_Invalid_Input(string? input)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Guard.NotNullOrWhiteSpace(input, "testParam"));
        Assert.Equal("testParam", exception.ParamName);
        Assert.Contains("不能为 null 或空白", exception.Message);
    }

    [Theory(DisplayName = "NotNullOrWhiteSpace 应该返回有效字符串")]
    [InlineData("test")]
    [InlineData("a")]
    [InlineData(" test ")]
    [InlineData("test\n")]
    public void NotNullOrWhiteSpace_Should_Return_Valid_String(string input)
    {
        // Act
        var result = Guard.NotNullOrWhiteSpace(input, "testParam");

        // Assert
        Assert.Equal(input, result);
    }

    #endregion

    #region NotNullOrEmpty Tests

    [Theory(DisplayName = "NotNullOrEmpty 应该对无效输入抛出异常")]
    [InlineData(null)]
    [InlineData("")]
    public void NotNullOrEmpty_Should_Throw_For_Invalid_Input(string? input)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Guard.NotNullOrEmpty(input, "testParam"));
        Assert.Equal("testParam", exception.ParamName);
        Assert.Contains("不能为 null 或空", exception.Message);
    }

    [Theory(DisplayName = "NotNullOrEmpty 应该返回有效字符串（包括空白）")]
    [InlineData("test")]
    [InlineData(" ")]
    [InlineData("  ")]
    [InlineData("\t")]
    public void NotNullOrEmpty_Should_Return_Valid_String(string input)
    {
        // Act
        var result = Guard.NotNullOrEmpty(input, "testParam");

        // Assert
        Assert.Equal(input, result);
    }

    #endregion

    #region NotNullOrEmpty Collection Tests

    [Fact(DisplayName = "NotNullOrEmpty 应该对 null 集合抛出异常")]
    public void NotNullOrEmpty_Should_Throw_For_Null_Collection()
    {
        // Arrange
        IEnumerable<string>? nullCollection = null;

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Guard.NotNullOrEmpty(nullCollection, "testParam"));
        Assert.Equal("testParam", exception.ParamName);
        Assert.Contains("不能为 null 或空", exception.Message);
    }

    [Fact(DisplayName = "NotNullOrEmpty 应该对空集合抛出异常")]
    public void NotNullOrEmpty_Should_Throw_For_Empty_Collection()
    {
        // Arrange
        var emptyCollection = Array.Empty<string>();

        // Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => Guard.NotNullOrEmpty(emptyCollection, "testParam"));
        Assert.Equal("testParam", exception.ParamName);
    }

    [Fact(DisplayName = "NotNullOrEmpty 应该返回非空集合")]
    public void NotNullOrEmpty_Should_Return_NonEmpty_Collection()
    {
        // Arrange
        var collection = new[] { "item1", "item2" };

        // Act
        var result = Guard.NotNullOrEmpty(collection, "testParam");

        // Assert
        Assert.Equal(collection, result);
    }

    #endregion

    #region InRange Tests

    [Theory(DisplayName = "InRange 应该对超出范围的值抛出异常")]
    [InlineData(-1, 0, 10)]
    [InlineData(11, 0, 10)]
    [InlineData(100, 0, 10)]
    [InlineData(-100, 0, 10)]
    public void InRange_Should_Throw_For_OutOfRange_Value(int value, int min, int max)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Guard.InRange(value, min, max, "testParam"));
        Assert.Equal("testParam", exception.ParamName);
        Assert.Contains($"[{min}, {max}]", exception.Message);
    }

    [Theory(DisplayName = "InRange 应该返回范围内的值")]
    [InlineData(0, 0, 10)]
    [InlineData(5, 0, 10)]
    [InlineData(10, 0, 10)]
    [InlineData(-5, -10, 0)]
    public void InRange_Should_Return_Value_In_Range(int value, int min, int max)
    {
        // Act
        var result = Guard.InRange(value, min, max, "testParam");

        // Assert
        Assert.Equal(value, result);
    }

    #endregion

    #region GreaterThan Tests

    [Theory(DisplayName = "GreaterThan 应该对不符合条件的值抛出异常")]
    [InlineData(0, 0)]
    [InlineData(5, 5)]
    [InlineData(-1, 0)]
    [InlineData(10, 10)]
    public void GreaterThan_Should_Throw_For_Invalid_Value(int value, int minValue)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Guard.GreaterThan(value, minValue, "testParam"));
        Assert.Equal("testParam", exception.ParamName);
        Assert.Contains($"大于 {minValue}", exception.Message);
    }

    [Theory(DisplayName = "GreaterThan 应该返回符合条件的值")]
    [InlineData(1, 0)]
    [InlineData(6, 5)]
    [InlineData(100, 10)]
    public void GreaterThan_Should_Return_Valid_Value(int value, int minValue)
    {
        // Act
        var result = Guard.GreaterThan(value, minValue, "testParam");

        // Assert
        Assert.Equal(value, result);
    }

    #endregion

    #region GreaterThanOrEqual Tests

    [Theory(DisplayName = "GreaterThanOrEqual 应该对不符合条件的值抛出异常")]
    [InlineData(-1, 0)]
    [InlineData(4, 5)]
    [InlineData(9, 10)]
    public void GreaterThanOrEqual_Should_Throw_For_Invalid_Value(int value, int minValue)
    {
        // Act & Assert
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => Guard.GreaterThanOrEqual(value, minValue, "testParam"));
        Assert.Equal("testParam", exception.ParamName);
        Assert.Contains($"大于或等于 {minValue}", exception.Message);
    }

    [Theory(DisplayName = "GreaterThanOrEqual 应该返回符合条件的值")]
    [InlineData(0, 0)]
    [InlineData(5, 5)]
    [InlineData(10, 5)]
    [InlineData(100, 10)]
    public void GreaterThanOrEqual_Should_Return_Valid_Value(int value, int minValue)
    {
        // Act
        var result = Guard.GreaterThanOrEqual(value, minValue, "testParam");

        // Assert
        Assert.Equal(value, result);
    }

    #endregion
}
