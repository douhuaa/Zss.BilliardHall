using FluentAssertions;
using Zss.BilliardHall.AdrSemanticParser.Models;

namespace Zss.BilliardHall.AdrSemanticParser.Tests;

public sealed class AdrSerializerTests
{
    private readonly AdrSerializer _serializer;

    public AdrSerializerTests()
    {
        _serializer = new AdrSerializer();
    }

    [Fact(DisplayName = "序列化有效模型返回 JSON")]
    public void Serialize_ValidModel_ReturnsJson()
    {
        // Arrange
        var model = new AdrSemanticModel
        {
            Id = "ADR-001",
            Title = "测试 ADR",
            Status = "Final",
            Relationships = new AdrRelationships()
        };

        // Act
        var json = _serializer.Serialize(model);

        // Assert
        json.Should().Contain("\"id\": \"ADR-001\"");
        json.Should().Contain("\"title\": \"测试 ADR\"");
        json.Should().Contain("\"status\": \"Final\"");
    }

    [Fact(DisplayName = "序列化包含关系的模型包含所有字段")]
    public void Serialize_WithRelationships_IncludesAllFields()
    {
        // Arrange
        var model = new AdrSemanticModel
        {
            Id = "ADR-001",
            Title = "测试",
            Relationships = new AdrRelationships
            {
                DependsOn = 
                [
                    new AdrReference 
                    { 
                        Id = "ADR-001", 
                        Title = "基础 ADR",
                        Reason = "测试原因" 
                    }
                ]
            }
        };

        // Act
        var json = _serializer.Serialize(model);

        // Assert
        json.Should().Contain("ADR-001");
        json.Should().Contain("基础 ADR");
        json.Should().Contain("测试原因");
    }

    [Fact(DisplayName = "反序列化有效 JSON 返回模型")]
    public void Deserialize_ValidJson_ReturnsModel()
    {
        // Arrange
        var json = @"{
            ""id"": ""ADR-001"",
            ""title"": ""测试 ADR"",
            ""status"": ""Final"",
            ""relationships"": {
                ""dependsOn"": [],
                ""dependedBy"": [],
                ""supersedes"": [],
                ""supersededBy"": [],
                ""related"": []
            }
        }";

        // Act
        var model = _serializer.Deserialize(json);

        // Assert
        model.Should().NotBeNull();
        model!.Id.Should().Be("ADR-001");
        model.Title.Should().Be("测试 ADR");
        model.Status.Should().Be("Final");
    }

    [Fact(DisplayName = "批量序列化多个模型返回数组")]
    public void SerializeBatch_MultipleModels_ReturnsArray()
    {
        // Arrange
        var models = new[]
        {
            new AdrSemanticModel 
            { 
                Id = "ADR-001", 
                Title = "First",
                Relationships = new AdrRelationships() 
            },
            new AdrSemanticModel 
            { 
                Id = "ADR-002", 
                Title = "Second",
                Relationships = new AdrRelationships() 
            }
        };

        // Act
        var json = _serializer.SerializeBatch(models);

        // Assert
        json.Should().StartWith("[");
        json.Should().EndWith("]");
        json.Should().Contain("ADR-001");
        json.Should().Contain("ADR-002");
    }

    [Fact(DisplayName = "序列化 null 模型抛出异常")]
    public void Serialize_NullModel_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _serializer.Serialize(null!));
    }

    [Fact(DisplayName = "反序列化 null JSON 抛出异常")]
    public void Deserialize_NullJson_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _serializer.Deserialize(null!));
    }

    [Fact(DisplayName = "反序列化空 JSON 抛出异常")]
    public void Deserialize_EmptyJson_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _serializer.Deserialize(""));
    }

    [Fact(DisplayName = "反序列化无效 JSON 抛出异常")]
    public void Deserialize_InvalidJson_ThrowsException()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => _serializer.Deserialize(invalidJson));
    }

    [Fact(DisplayName = "批量序列化空数组返回空数组")]
    public void SerializeBatch_EmptyArray_ReturnsEmptyArray()
    {
        // Arrange
        var models = Array.Empty<AdrSemanticModel>();

        // Act
        var json = _serializer.SerializeBatch(models);

        // Assert
        json.Should().Be("[]");
    }

    [Fact(DisplayName = "批量序列化 null 数组抛出异常")]
    public void SerializeBatch_NullArray_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _serializer.SerializeBatch(null!));
    }

    [Fact(DisplayName = "序列化最小化模型返回有效 JSON")]
    public void Serialize_MinimalModel_ReturnsValidJson()
    {
        // Arrange - 最小化的模型，只有必需字段
        var model = new AdrSemanticModel
        {
            Id = "ADR-001",
            Title = "Test",
            Relationships = new AdrRelationships()
        };

        // Act
        var json = _serializer.Serialize(model);

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("ADR-001");
        json.Should().Contain("Test");
    }
}
