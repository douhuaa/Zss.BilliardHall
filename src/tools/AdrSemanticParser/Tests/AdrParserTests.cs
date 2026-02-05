using FluentAssertions;
using Zss.BilliardHall.AdrSemanticParser.Models;

namespace Zss.BilliardHall.AdrSemanticParser.Tests;

public sealed class AdrParserTests
{
    private readonly AdrParser _parser;

    public AdrParserTests()
    {
        _parser = new AdrParser();
    }

    [Fact(DisplayName = "解析有效的 ADR 文档返回正确的 ID")]
    public void Parse_ValidAdr_ReturnsCorrectId()
    {
        // Arrange
        var markdown = @"# ADR-001：模块化单体架构

**状态**：✅ Final  
**级别**：架构约束

## 关系声明（Relationships）

**依赖（Depends On）**：无

**被依赖（Depended By）**：无

**替代（Supersedes）**：无

**被替代（Superseded By）**：无

**相关（Related）**：无
";

        // Act
        var result = _parser.Parse(markdown, "ADR-001-test.md");

        // Assert
        result.Id.Should().Be("ADR-001");
    }

    [Fact(DisplayName = "解析有效的 ADR 文档返回正确的标题")]
    public void Parse_ValidAdr_ReturnsCorrectTitle()
    {
        // Arrange
        var markdown = @"# ADR-001：模块化单体架构

**状态**：✅ Final  
";

        // Act
        var result = _parser.Parse(markdown, "ADR-001-test.md");

        // Assert
        result.Title.Should().Be("模块化单体架构");
    }

    [Fact(DisplayName = "解析包含元数据的 ADR 文档提取所有字段")]
    public void Parse_WithMetadata_ExtractsAllFields()
    {
        // Arrange
        var markdown = @"# ADR-940：测试 ADR

**状态**：✅ Accepted  
**级别**：治理层  
**适用范围**：所有模块  
**版本**：1.0  
**生效时间**：即刻

## 关系声明（Relationships）

**依赖（Depends On）**：无
";

        // Act
        var result = _parser.Parse(markdown, "ADR-940-test.md");

        // Assert
        result.Status.Should().Be("✅ Accepted");
        result.Level.Should().Be("治理层");
        result.Scope.Should().Be("所有模块");
        result.Version.Should().Be("1.0");
        result.EffectiveDate.Should().Be("即刻");
    }

    [Fact(DisplayName = "解析包含依赖关系的 ADR 文档正确提取")]
    public void Parse_WithDependencies_ExtractsCorrectly()
    {
        // Arrange
        var markdown = @"# ADR-001：测试 ADR

## 关系声明（Relationships）

**依赖（Depends On）**：
- [ADR-001：模块化单体架构](./ADR-001.md) - 基于模块隔离规则
- [ADR-002：平台架构](./ADR-002.md) - 依赖平台定义

**被依赖（Depended By）**：无
";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Relationships.DependsOn.Should().HaveCount(2);
        result.Relationships.DependsOn[0].Id.Should().Be("ADR-001");
        result.Relationships.DependsOn[0].Title.Should().Be("模块化单体架构");
        result.Relationships.DependsOn[0].Reason.Should().Be("基于模块隔离规则");
    }

    [Fact(DisplayName = "解析包含术语表的 ADR 文档提取术语")]
    public void Parse_WithGlossary_ExtractsTerms()
    {
        // Arrange
        var markdown = @"# ADR-001：测试 ADR

## 术语表（Glossary）

| 术语 | 定义 | 英文对照 |
|------|------|----------|
| 模块 | 独立的业务单元 | Module |
| 契约 | 数据传输对象 | Contract |
";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Glossary.Should().HaveCount(2);
        result.Glossary[0].Term.Should().Be("模块");
        result.Glossary[0].Definition.Should().Be("独立的业务单元");
        result.Glossary[0].EnglishTerm.Should().Be("Module");
    }

    [Fact(DisplayName = "解析包含快速参考表的 ADR 文档提取约束")]
    public void Parse_WithQuickReference_ExtractsConstraints()
    {
        // Arrange
        var markdown = @"# ADR-001：测试 ADR

## 快速参考表

| 约束编号 | 约束描述 | 测试方式 | 测试用例 | 必须遵守 |
|---------|---------|---------|---------|----------|
| ADR-001.1 | 模块不可相互引用 | L1 - NetArchTest | Modules_Should_Not_Reference | ✅ |
";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.QuickReference.Should().HaveCount(1);
        result.QuickReference[0].ConstraintId.Should().Be("ADR-001.1");
        result.QuickReference[0].Description.Should().Be("模块不可相互引用");
        result.QuickReference[0].IsMandatory.Should().BeTrue();
    }

    [Fact(DisplayName = "解析无关系声明的 ADR 文档返回空列表")]
    public void Parse_NoRelationships_ReturnsEmptyLists()
    {
        // Arrange
        var markdown = @"# ADR-001：测试 ADR

## 决策

这是一个没有关系声明的 ADR。
";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Relationships.DependsOn.Should().BeEmpty();
        result.Relationships.DependedBy.Should().BeEmpty();
        result.Relationships.Supersedes.Should().BeEmpty();
        result.Relationships.SupersededBy.Should().BeEmpty();
        result.Relationships.Related.Should().BeEmpty();
    }

    [Fact(DisplayName = "解析 null Markdown 抛出异常")]
    public void Parse_NullMarkdown_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _parser.Parse(null!));
    }

    [Fact(DisplayName = "解析空 Markdown 抛出异常")]
    public void Parse_EmptyMarkdown_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _parser.Parse(""));
    }

    [Fact(DisplayName = "解析无 ADR 编号的文档抛出异常")]
    public void Parse_NoAdrNumber_ThrowsException()
    {
        // Arrange - 没有 ADR 编号的文档
        var markdown = @"# 无效格式

**状态**：Final

## 决策

这是一个没有 ADR 编号的文档。
";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _parser.Parse(markdown));
        exception.Message.Should().Contain("Unable to extract ADR number");
    }

    [Fact(DisplayName = "解析非数字 ADR ID 的文档抛出异常")]
    public void Parse_NonNumericAdrId_ThrowsException()
    {
        // Arrange - ADR 编号不是数字
        var markdown = @"# ADR-XXXX：测试

**状态**：Final
";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _parser.Parse(markdown));
        exception.Message.Should().Contain("Unable to extract ADR number");
    }

    [Fact(DisplayName = "解析畸形 ADR ID 的文档抛出异常")]
    public void Parse_MalformedAdrId_ThrowsException()
    {
        // Arrange - 畸形的 ADR 编号
        var markdown = @"# ADR：测试（缺少编号）

**状态**：Final
";

        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() => _parser.Parse(markdown));
        exception.Message.Should().Contain("Unable to extract ADR number");
    }

    [Theory(DisplayName = "解析各种 ADR ID 格式正确提取")]
    [InlineData("# ADR-001：测试", "ADR-001")]  // 短格式
    [InlineData("# ADR-001：测试", "ADR-001")]  // 标准格式
    [InlineData("# ADR-12345：测试", "ADR-12345")]  // 长编号
    public void Parse_VariousAdrIdFormats_ExtractsCorrectly(string titleLine, string expectedId)
    {
        // Arrange
        var markdown = $@"{titleLine}

**状态**：Final
";

        // Act
        var result = _parser.Parse(markdown);

        // Assert
        result.Id.Should().Be(expectedId);
    }
}
