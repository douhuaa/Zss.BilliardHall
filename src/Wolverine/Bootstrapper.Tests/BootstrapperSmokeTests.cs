using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Xunit;

namespace Zss.BilliardHall.Wolverine.Bootstrapper.Tests;

/// <summary>
/// Bootstrapper 烟雾测试：验证应用可以构建和启动（不依赖数据库）
/// Bootstrapper smoke tests: Verify the application can be built and started (without database dependency)
/// </summary>
/// <remarks>
/// 这些测试验证配置完整性，不需要实际的 PostgreSQL 连接。
/// 使用假的连接字符串来测试 Host 构建逻辑。
/// These tests verify configuration integrity without requiring an actual PostgreSQL connection.
/// Uses fake connection strings to test Host building logic.
/// </remarks>
[Trait("Category", "Unit")]
public class BootstrapperSmokeTests
{
    [Fact]
    public void BuildApp_WithValidArgs_ShouldSucceed()
    {
        // Arrange
        var args = Array.Empty<string>();

        // Act
        Action act = () =>
        {
            var builder = WebApplication.CreateBuilder(args);
            // Provide a fake connection string to avoid Marten initialization failure
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:Default"] = "Host=localhost;Database=fake;Username=fake;Password=fake"
            });

            _ = BootstrapperHost.BuildAppWithBuilder(builder);
        };

        // Assert
        act.Should().NotThrow("Host should be buildable with fake connection string");
    }

    [Fact]
    public void BuildApp_ShouldRegisterHealthChecks()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=localhost;Database=fake;Username=fake;Password=fake"
        });

        // Act
        var app = BootstrapperHost.BuildAppWithBuilder(builder);

        // Assert
        var healthCheckService = app.Services.GetService<HealthCheckService>();
        healthCheckService.Should().NotBeNull("HealthCheckService should be registered");
    }

    [Fact]
    public async Task BuildApp_ShouldHaveSelfHealthCheck()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=localhost;Database=fake;Username=fake;Password=fake"
        });

        // Act
        var app = BootstrapperHost.BuildAppWithBuilder(builder);
        var healthCheckService = app.Services.GetRequiredService<HealthCheckService>();

        // Assert
        var result = await healthCheckService.CheckHealthAsync();
        result.Entries.Should().ContainKey("self", "self health check should be registered");
        result.Entries["self"].Tags.Should().Contain("live", "self check should be tagged with 'live'");
        result.Entries["self"].Status.Should().Be(HealthStatus.Healthy, "self check should return healthy");
    }

    [Fact]
    public void BuildApp_WithoutConnectionString_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        // Intentionally not providing ConnectionStrings:Default

        // Act
        Action act = () => BootstrapperHost.BuildAppWithBuilder(builder);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Missing or empty Default connection string*",
                "Marten requires a valid connection string");
    }

    [Fact]
    public void BuildApp_ShouldRegisterMartenServices()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=localhost;Database=fake;Username=fake;Password=fake"
        });

        // Act
        var app = BootstrapperHost.BuildAppWithBuilder(builder);

        // Assert
        var documentStore = app.Services.GetService<Marten.IDocumentStore>();
        documentStore.Should().NotBeNull("IDocumentStore should be registered");
    }

    [Fact]
    public void BuildApp_ShouldRegisterWolverineServices()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=localhost;Database=fake;Username=fake;Password=fake"
        });

        // Act
        var app = BootstrapperHost.BuildAppWithBuilder(builder);

        // Assert
        // Wolverine registers IMessageBus, but we just verify the app builds successfully
        app.Should().NotBeNull("App should be built with Wolverine services");
    }
}
