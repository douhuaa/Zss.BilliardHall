using FluentAssertions;
using Zss.BilliardHall.AdrSemanticParser.Models;

namespace Zss.BilliardHall.AdrSemanticParser.Tests;

public class AdrSerializerTests
{
    private readonly AdrSerializer _serializer;

    public AdrSerializerTests()
    {
        _serializer = new AdrSerializer();
    }

    [Fact]
    public void Serialize_ValidModel_ReturnsJson()
    {
        // Arrange
        var model = new AdrSemanticModel
        {
            Id = "ADR-0001",
            Title = "测试 ADR",
            Status = "Final",
            Relationships = new AdrRelationships()
        };

        // Act
        var json = _serializer.Serialize(model);

        // Assert
        json.Should().Contain("\"id\": \"ADR-0001\"");
        json.Should().Contain("\"title\": \"测试 ADR\"");
        json.Should().Contain("\"status\": \"Final\"");
    }

    [Fact]
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
                        Id = "ADR-0001", 
                        Title = "基础 ADR",
                        Reason = "测试原因" 
                    }
                ]
            }
        };

        // Act
        var json = _serializer.Serialize(model);

        // Assert
        json.Should().Contain("ADR-0001");
        json.Should().Contain("基础 ADR");
        json.Should().Contain("测试原因");
    }

    [Fact]
    public void Deserialize_ValidJson_ReturnsModel()
    {
        // Arrange
        var json = @"{
            ""id"": ""ADR-0001"",
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
        model!.Id.Should().Be("ADR-0001");
        model.Title.Should().Be("测试 ADR");
        model.Status.Should().Be("Final");
    }

    [Fact]
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

    [Fact]
    public void Serialize_NullModel_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _serializer.Serialize(null!));
    }

    [Fact]
    public void Deserialize_NullJson_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _serializer.Deserialize(null!));
    }

    [Fact]
    public void Deserialize_EmptyJson_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => _serializer.Deserialize(""));
    }

    [Fact]
    public void Deserialize_InvalidJson_ThrowsException()
    {
        // Arrange
        var invalidJson = "{ this is not valid json }";

        // Act & Assert
        Assert.ThrowsAny<Exception>(() => _serializer.Deserialize(invalidJson));
    }

    [Fact]
    public void SerializeBatch_EmptyArray_ReturnsEmptyArray()
    {
        // Arrange
        var models = Array.Empty<AdrSemanticModel>();

        // Act
        var json = _serializer.SerializeBatch(models);

        // Assert
        json.Should().Be("[]");
    }

    [Fact]
    public void SerializeBatch_NullArray_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => _serializer.SerializeBatch(null!));
    }

    [Fact]
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
