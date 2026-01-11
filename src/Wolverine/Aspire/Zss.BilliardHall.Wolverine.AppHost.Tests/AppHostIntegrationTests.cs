using Aspire.Hosting.Testing;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Zss.BilliardHall.Wolverine.AppHost.Tests;

/// <summary>
/// AppHost 集成测试：验证应用能够通过 Aspire 编排正常启动，并且健康检查和基本端点可访问
/// AppHost Integration Tests: Verify the application can start properly through Aspire orchestration,
/// and health check + basic endpoints are accessible
/// </summary>
public class AppHostIntegrationTests
{
    /// <summary>
    /// 测试 AppHost 能够成功启动 Bootstrapper 服务
    /// Test that AppHost can successfully start the Bootstrapper service
    /// </summary>
    [Fact]
    public async Task AppHost_CanStartBootstrapperService()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Zss_BilliardHall_Wolverine_AppHost>();

        await using var app = await appHost.BuildAsync();

        // Act
        await app.StartAsync();

        // Assert - Application should start without throwing exceptions
        app.Should().NotBeNull();
    }

    /// <summary>
    /// 测试 Bootstrapper 健康检查端点响应正常
    /// Test that Bootstrapper health check endpoint responds correctly
    /// </summary>
    [Fact]
    public async Task Bootstrapper_HealthEndpoint_ReturnsHealthy()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Zss_BilliardHall_Wolverine_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Get HTTP client for the bootstrapper service
        var httpClient = app.CreateHttpClient("bootstrapper");

        // Act - Call health endpoint
        var response = await httpClient.GetAsync("/health");

        // Assert
        response.Should().NotBeNull();
        response.IsSuccessStatusCode.Should().BeTrue(
            "health endpoint should return success status code, but got {0}", 
            response.StatusCode);
    }

    /// <summary>
    /// 测试 Bootstrapper 根端点返回应用状态信息
    /// Test that Bootstrapper root endpoint returns application status information
    /// </summary>
    [Fact]
    public async Task Bootstrapper_RootEndpoint_ReturnsApplicationStatus()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder
            .CreateAsync<Projects.Zss_BilliardHall_Wolverine_AppHost>();

        await using var app = await appHost.BuildAsync();
        await app.StartAsync();

        // Get HTTP client for the bootstrapper service
        var httpClient = app.CreateHttpClient("bootstrapper");

        // Act - Call root endpoint
        var response = await httpClient.GetAsync("/");

        // Assert
        response.Should().NotBeNull();
        response.IsSuccessStatusCode.Should().BeTrue(
            "root endpoint should return success status code, but got {0}", 
            response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().NotBeNullOrWhiteSpace("root endpoint should return content");
        content.Should().Contain("Zss.BilliardHall.Bootstrapper", 
            "root endpoint should return application name");
        content.Should().Contain("Wolverine", 
            "root endpoint should mention Wolverine framework");
        content.Should().Contain("Marten", 
            "root endpoint should mention Marten framework");
    }
}
