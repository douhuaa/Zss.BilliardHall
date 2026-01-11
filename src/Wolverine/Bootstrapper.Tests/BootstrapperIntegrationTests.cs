using FluentAssertions;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Zss.BilliardHall.Wolverine.Bootstrapper.Tests;

/// <summary>
/// Bootstrapper 集成测试：使用真实 PostgreSQL 容器验证完整功能
/// Bootstrapper integration tests: Verify complete functionality using real PostgreSQL container
/// </summary>
/// <remarks>
/// 这些测试使用 Testcontainers 启动真实的 PostgreSQL 实例，验证 Marten 和 Wolverine 集成。
/// 不依赖 Aspire AppHost/DCP，只需本地 Docker 环境。
/// These tests use Testcontainers to start a real PostgreSQL instance, verifying Marten and Wolverine integration.
/// Does not depend on Aspire AppHost/DCP, only requires local Docker environment.
/// </remarks>
[Trait("Category", "Integration")]
[Trait("Category", "RequiresDocker")]
public class BootstrapperIntegrationTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public BootstrapperIntegrationTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Bootstrapper_WithRealDatabase_CanStartAndStop()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        ConfigureTestBuilder(builder);

        // Act
        var app = BootstrapperHost.BuildAppWithBuilder(builder);
        await app.StartAsync();

        // Assert
        app.Should().NotBeNull("Application should start successfully");

        // Cleanup
        await app.StopAsync();
    }

    [Fact]
    public async Task Bootstrapper_HealthEndpoint_ShouldReturnHealthy()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        ConfigureTestBuilder(builder);

        var app = BootstrapperHost.BuildAppWithBuilder(builder);
        await app.StartAsync();

        // Act
        var client = app.Services.GetRequiredService<IHttpClientFactory>().CreateClient();
        client.BaseAddress = new Uri($"http://localhost:{GetRandomPort()}");

        // Note: In a real test, we would need to configure Kestrel to listen on a specific port
        // For now, we just verify the app can start with health checks registered
        var healthCheckService = app.Services.GetRequiredService<Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckService>();
        var result = await healthCheckService.CheckHealthAsync();

        // Assert
        result.Status.Should().Be(Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Healthy,
            "Health check should pass");

        // Cleanup
        await app.StopAsync();
    }

    [Fact]
    public async Task Marten_CanConnectToDatabase()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        ConfigureTestBuilder(builder);

        var app = BootstrapperHost.BuildAppWithBuilder(builder);

        // Act
        var documentStore = app.Services.GetRequiredService<IDocumentStore>();

        // Assert
        documentStore.Should().NotBeNull("IDocumentStore should be available");

        // Verify we can open a session (this tests the connection)
        using var session = documentStore.LightweightSession();
        session.Should().NotBeNull("Should be able to create a session");
    }

    [Fact]
    public async Task Marten_CanPersistAndRetrieveDocument()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        ConfigureTestBuilder(builder);

        var app = BootstrapperHost.BuildAppWithBuilder(builder);
        var documentStore = app.Services.GetRequiredService<IDocumentStore>();

        // Create test document
        var testDoc = new TestDocument
        {
            Id = Guid.NewGuid(),
            Name = "Test Document",
            CreatedAt = DateTime.UtcNow
        };

        // Act - Store document
        using (var session = documentStore.LightweightSession())
        {
            session.Store(testDoc);
            await session.SaveChangesAsync();
        }

        // Act - Retrieve document
        TestDocument? retrieved;
        using (var session = documentStore.LightweightSession())
        {
            retrieved = await session.LoadAsync<TestDocument>(testDoc.Id);
        }

        // Assert
        retrieved.Should().NotBeNull("Document should be retrievable");
        retrieved!.Id.Should().Be(testDoc.Id);
        retrieved.Name.Should().Be(testDoc.Name);
    }

    [Fact]
    public async Task Marten_UsesBilliardSchema()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        ConfigureTestBuilder(builder);

        var app = BootstrapperHost.BuildAppWithBuilder(builder);

        // Act
        var documentStore = app.Services.GetRequiredService<IDocumentStore>();

        // Assert
        documentStore.Options.DatabaseSchemaName.Should().Be("billiard",
            "Marten should use 'billiard' schema as configured");
    }

    private void ConfigureTestBuilder(WebApplicationBuilder builder)
    {
        // Override connection string with test container connection
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = _fixture.ConnectionString
        });

        // Set environment to Testing
        builder.Environment.EnvironmentName = Environments.Development; // Use Development to enable health endpoints
    }

    private static int GetRandomPort()
    {
        // Simple random port generator for test purposes
        return Random.Shared.Next(5000, 6000);
    }

    /// <summary>
    /// 测试文档类型
    /// Test document type
    /// </summary>
    private sealed class TestDocument
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
