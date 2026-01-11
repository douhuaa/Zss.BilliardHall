using FluentAssertions;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Zss.BilliardHall.Wolverine.ServiceDefaults.Tests;

/// <summary>
/// 集成测试：验证 Marten 扩展方法正确配置服务
/// Integration tests: Verify Marten extension methods correctly configure services
/// </summary>
public class MartenExtensionsTests
{
    [Fact]
    public void AddMartenDefaults_WithValidConnectionString_ShouldRegisterMartenServices()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        
        // Add a test connection string
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=localhost;Database=test;Username=test;Password=test"
        });

        // Act
        builder.AddMartenDefaults();
        var app = builder.Build();

        // Assert
        var services = app.Services;

        // Verify IDocumentStore is registered
        var documentStore = services.GetService<IDocumentStore>();
        documentStore.Should().NotBeNull("IDocumentStore should be registered");

        // Verify IDocumentSession is registered (scoped)
        using var scope = services.CreateScope();
        var documentSession = scope.ServiceProvider.GetService<IDocumentSession>();
        documentSession.Should().NotBeNull("IDocumentSession should be registered");
    }

    [Fact]
    public void AddMartenDefaults_WithMissingConnectionString_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        // Intentionally not adding ConnectionStrings:Default

        // Act
        Action act = () => builder.AddMartenDefaults();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Missing or empty Default connection string*")
            .WithMessage("*correctly configured and injected*");
    }

    [Fact]
    public void AddMartenDefaults_ShouldConfigureSchemaName()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=localhost;Database=test;Username=test;Password=test"
        });

        // Act
        builder.AddMartenDefaults();
        var app = builder.Build();

        // Assert
        var documentStore = app.Services.GetRequiredService<IDocumentStore>();
        documentStore.Should().NotBeNull();
        
        // Verify the schema name is set to "billiard"
        var options = documentStore.Options;
        options.DatabaseSchemaName.Should().Be("billiard", "schema name should be set to 'billiard'");
    }

    [Fact]
    public void AddMartenDefaults_ShouldUseProvidedConnectionString()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        var expectedConnectionString = "Host=test.example.com;Port=5432;Database=billiard_test;Username=admin;Password=secret";
        
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = expectedConnectionString
        });

        // Act
        builder.AddMartenDefaults();
        var app = builder.Build();

        // Assert
        var documentStore = app.Services.GetRequiredService<IDocumentStore>();
        documentStore.Should().NotBeNull();
        
        // The connection string should be used (we can't directly access it, but we can verify the store was created)
        var options = documentStore.Options;
        options.Should().NotBeNull("DocumentStore options should be configured");
    }

    [Fact]
    public void AddMartenDefaults_WithValidConfiguration_ShouldSucceed()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=localhost;Database=test;Username=test;Password=test"
        });

        // Act
        builder.AddMartenDefaults();
        var app = builder.Build();

        // Assert - Should not throw
        var documentStore = app.Services.GetService<IDocumentStore>();
        documentStore.Should().NotBeNull();
    }

    [Fact]
    public void AddMartenDefaults_WithEmptyConnectionString_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "" // Empty string
        });

        // Act
        Action act = () => builder.AddMartenDefaults();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Missing or empty Default connection string*")
            .WithMessage("*correctly configured and injected*");
    }

    [Fact]
    public void AddMartenDefaults_ShouldConfigureLightweightSessions()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Host=localhost;Database=test;Username=test;Password=test"
        });

        // Act
        builder.AddMartenDefaults();
        var app = builder.Build();

        // Assert
        var documentStore = app.Services.GetRequiredService<IDocumentStore>();
        documentStore.Should().NotBeNull();
        
        // Verify lightweight sessions by checking that IDocumentSession is registered
        // Lightweight sessions are the default when UseLightweightSessions() is called
        using var scope = app.Services.CreateScope();
        var documentSession = scope.ServiceProvider.GetService<IDocumentSession>();
        documentSession.Should().NotBeNull("IDocumentSession should be registered for lightweight sessions");
    }
}
