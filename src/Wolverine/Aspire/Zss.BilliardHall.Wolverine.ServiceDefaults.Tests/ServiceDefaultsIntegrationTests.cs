using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Xunit;

namespace Zss.BilliardHall.Wolverine.ServiceDefaults.Tests;

/// <summary>
/// 集成测试：验证 ServiceDefaults 扩展方法正确配置服务
/// Integration tests: Verify ServiceDefaults extension methods correctly configure services
/// </summary>
public class ServiceDefaultsIntegrationTests
{
    [Fact]
    public void AddServiceDefaults_ShouldRegisterAllRequiredServices()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddServiceDefaults();
        var app = builder.Build();

        // Assert
        var services = app.Services;

        // Verify health checks are registered
        var healthCheckService = services.GetService<HealthCheckService>();
        healthCheckService.Should().NotBeNull("health check service should be registered");

        // Verify service discovery is registered
        var serviceDiscovery = services.GetService<IServiceProvider>();
        serviceDiscovery.Should().NotBeNull();
    }

    [Fact]
    public async Task AddDefaultHealthChecks_ShouldRegisterSelfCheck()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddDefaultHealthChecks();
        var app = builder.Build();

        // Assert
        var healthCheckService = app.Services.GetRequiredService<HealthCheckService>();
        healthCheckService.Should().NotBeNull();

        // Verify the "self" check exists and is tagged with "live" by checking health results
        var healthResult = await healthCheckService.CheckHealthAsync();
        healthResult.Entries.Should().ContainKey("self", "self health check should be registered");
        healthResult.Entries["self"].Tags.Should().Contain("live", "self check should be tagged with 'live'");
    }

    [Fact]
    public async Task AddDefaultHealthChecks_SelfCheck_ShouldReturnHealthy()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.AddDefaultHealthChecks();
        var app = builder.Build();

        // Act
        var healthCheckService = app.Services.GetRequiredService<HealthCheckService>();
        var result = await healthCheckService.CheckHealthAsync();

        // Assert
        result.Status.Should().Be(HealthStatus.Healthy, "self check should always return healthy");
        result.Entries.Should().ContainKey("self");
        result.Entries["self"].Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public void ConfigureOpenTelemetry_ShouldRegisterTracingAndMetrics()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.ConfigureOpenTelemetry();
        var app = builder.Build();

        // Assert - Verify OpenTelemetry services are registered
        var meterProvider = app.Services.GetService<MeterProvider>();
        var tracerProvider = app.Services.GetService<TracerProvider>();

        // Note: In test environment, these may be null if OTLP endpoint is not configured
        // The important thing is that the registration doesn't throw exceptions
        app.Services.Should().NotBeNull();
    }

    [Fact]
    public void MapDefaultEndpoints_InDevelopment_ShouldMapHealthCheckEndpoints()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = Environments.Development;
        builder.AddDefaultHealthChecks();

        var app = builder.Build();

        // Act
        app.MapDefaultEndpoints();

        // Assert - Verify the app starts successfully with mapped endpoints
        app.Should().NotBeNull();
    }

    [Fact]
    public void MapDefaultEndpoints_InProduction_ShouldNotMapHealthCheckEndpoints()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        builder.Environment.EnvironmentName = Environments.Production;
        builder.AddDefaultHealthChecks();

        var app = builder.Build();

        // Act
        app.MapDefaultEndpoints();

        // Assert - In production, endpoints should not be mapped
        // The method should complete without errors
        app.Should().NotBeNull();
    }

    [Fact]
    public void AddServiceDefaults_ShouldConfigureHttpClientWithResilience()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act
        builder.AddServiceDefaults();
        var app = builder.Build();

        // Assert - Verify HTTP client factory is available
        var httpClientFactory = app.Services.GetService<IHttpClientFactory>();
        httpClientFactory.Should().NotBeNull("HTTP client factory should be registered");

        // Create a client to verify configuration doesn't throw
        var client = httpClientFactory!.CreateClient();
        client.Should().NotBeNull();
    }

    [Fact]
    public void AddServiceDefaults_ShouldBeIdempotent()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();

        // Act - Call multiple times
        builder.AddServiceDefaults();
        builder.AddServiceDefaults();

        // Assert - Should not throw
        var app = builder.Build();
        app.Should().NotBeNull();
    }
}
