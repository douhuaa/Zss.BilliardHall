using FluentAssertions;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wolverine;
using Xunit;
using Zss.BilliardHall.Modules.Members;
using Zss.BilliardHall.Modules.Members.RegisterMember;
using Zss.BilliardHall.Modules.Members.TopUpBalance;
using Zss.BilliardHall.Modules.Members.DeductBalance;

namespace Zss.BilliardHall.Wolverine.Bootstrapper.Tests.Members;

/// <summary>
/// Members 模块集成测试：使用真实 PostgreSQL 容器验证完整功能
/// Members module integration tests: Verify complete functionality using real PostgreSQL container
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "RequiresDocker")]
public class MembersModuleTests : IClassFixture<PostgresFixture>
{
    private readonly PostgresFixture _fixture;

    public MembersModuleTests(PostgresFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task RegisterMember_WithValidData_ShouldSucceed()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        ConfigureTestBuilder(builder);
        
        await using var app = BootstrapperHost.BuildAppFromBuilder(builder);
        await app.StartAsync();

        var bus = app.Services.GetRequiredService<IMessageBus>();
        var command = new RegisterMember(
            "张三",
            "13800138000",
            "zhangsan@example.com",
            "password123"
        );

        // Act
        var result = await bus.InvokeAsync<BuildingBlocks.Contracts.Result<Guid>>(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        // Verify member was persisted
        var documentStore = app.Services.GetRequiredService<IDocumentStore>();
        using var session = documentStore.LightweightSession();
        var member = await session.LoadAsync<Member>(result.Value);
        
        member.Should().NotBeNull();
        member!.Name.Should().Be("张三");
        member.Phone.Should().Be("13800138000");
        member.Email.Should().Be("zhangsan@example.com");
        member.Tier.Should().Be(MemberTier.Regular);
        member.Balance.Should().Be(0);
        member.Points.Should().Be(0);

        await app.StopAsync();
    }

    [Fact]
    public async Task RegisterMember_WithDuplicatePhone_ShouldFail()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        ConfigureTestBuilder(builder);
        
        await using var app = BootstrapperHost.BuildAppFromBuilder(builder);
        await app.StartAsync();

        var bus = app.Services.GetRequiredService<IMessageBus>();
        var documentStore = app.Services.GetRequiredService<IDocumentStore>();

        // Create existing member
        using (var session = documentStore.LightweightSession())
        {
            var existingMember = new Member
            {
                Id = Guid.NewGuid(),
                Phone = "13800138000",
                Name = "已存在的会员",
                Tier = MemberTier.Regular,
                RegisteredAt = DateTimeOffset.UtcNow
            };
            session.Store(existingMember);
            await session.SaveChangesAsync();
        }

        var command = new RegisterMember(
            "张三",
            "13800138000", // 相同手机号
            "zhangsan@example.com",
            "password123"
        );

        // Act
        var result = await bus.InvokeAsync<BuildingBlocks.Contracts.Result<Guid>>(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("已注册");

        await app.StopAsync();
    }

    [Fact]
    public async Task TopUpBalance_WithValidAmount_ShouldSucceed()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        ConfigureTestBuilder(builder);
        
        await using var app = BootstrapperHost.BuildAppFromBuilder(builder);
        await app.StartAsync();

        var bus = app.Services.GetRequiredService<IMessageBus>();
        var documentStore = app.Services.GetRequiredService<IDocumentStore>();

        // Create member
        var memberId = Guid.NewGuid();
        using (var session = documentStore.LightweightSession())
        {
            var member = new Member
            {
                Id = memberId,
                Phone = "13800138001",
                Name = "测试会员",
                Balance = 100m,
                Tier = MemberTier.Regular,
                RegisteredAt = DateTimeOffset.UtcNow
            };
            session.Store(member);
            await session.SaveChangesAsync();
        }

        var command = new TopUpBalance(memberId, 200m, "支付宝");

        // Act
        var result = await bus.InvokeAsync<BuildingBlocks.Contracts.Result>(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify balance was updated
        using (var session = documentStore.LightweightSession())
        {
            var member = await session.LoadAsync<Member>(memberId);
            member.Should().NotBeNull();
            member!.Balance.Should().Be(300m);
        }

        await app.StopAsync();
    }

    [Fact]
    public async Task DeductBalance_WithSufficientBalance_ShouldSucceed()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        ConfigureTestBuilder(builder);
        
        await using var app = BootstrapperHost.BuildAppFromBuilder(builder);
        await app.StartAsync();

        var bus = app.Services.GetRequiredService<IMessageBus>();
        var documentStore = app.Services.GetRequiredService<IDocumentStore>();

        // Create member with balance
        var memberId = Guid.NewGuid();
        using (var session = documentStore.LightweightSession())
        {
            var member = new Member
            {
                Id = memberId,
                Phone = "13800138002",
                Name = "测试会员",
                Balance = 100m,
                Tier = MemberTier.Regular,
                RegisteredAt = DateTimeOffset.UtcNow
            };
            session.Store(member);
            await session.SaveChangesAsync();
        }

        var command = new DeductBalance(memberId, 50m, "支付订单");

        // Act
        var result = await bus.InvokeAsync<BuildingBlocks.Contracts.Result>(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        // Verify balance was deducted
        using (var session = documentStore.LightweightSession())
        {
            var member = await session.LoadAsync<Member>(memberId);
            member.Should().NotBeNull();
            member!.Balance.Should().Be(50m);
        }

        await app.StopAsync();
    }

    [Fact]
    public async Task DeductBalance_WithInsufficientBalance_ShouldFail()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        ConfigureTestBuilder(builder);
        
        await using var app = BootstrapperHost.BuildAppFromBuilder(builder);
        await app.StartAsync();

        var bus = app.Services.GetRequiredService<IMessageBus>();
        var documentStore = app.Services.GetRequiredService<IDocumentStore>();

        // Create member with insufficient balance
        var memberId = Guid.NewGuid();
        using (var session = documentStore.LightweightSession())
        {
            var member = new Member
            {
                Id = memberId,
                Phone = "13800138003",
                Name = "测试会员",
                Balance = 30m,
                Tier = MemberTier.Regular,
                RegisteredAt = DateTimeOffset.UtcNow
            };
            session.Store(member);
            await session.SaveChangesAsync();
        }

        var command = new DeductBalance(memberId, 50m, "支付订单");

        // Act
        var result = await bus.InvokeAsync<BuildingBlocks.Contracts.Result>(command);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("余额不足");

        await app.StopAsync();
    }

    private void ConfigureTestBuilder(WebApplicationBuilder builder)
    {
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = _fixture.ConnectionString
        });
        builder.Environment.EnvironmentName = Environments.Development;
    }
}
