using FluentAssertions;
using Marten;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wolverine;
using Xunit;
using Zss.BilliardHall.BuildingBlocks.Contracts;
using Zss.BilliardHall.BuildingBlocks.Exceptions;
using Zss.BilliardHall.Modules.Members;
using Zss.BilliardHall.Modules.Members.DeductBalance;
using Zss.BilliardHall.Modules.Members.RegisterMember;
using Zss.BilliardHall.Modules.Members.TopUpBalance;

namespace Zss.BilliardHall.Wolverine.Bootstrapper.Tests.Members;

/// <summary>
/// Members 模块集成测试：使用真实 PostgreSQL 容器验证完整功能
/// Members module integration tests: Verify complete functionality using real PostgreSQL container
/// </summary>
[Trait("Category", "Integration")]
[Trait("Category", "RequiresDocker")]
[Collection("MembersModuleTests")]
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

        var uniquePhone = $"13800138{Random.Shared.Next(1000, 9999)}";
        var command = new RegisterMember(
            "张三",
            uniquePhone,
            "zhangsan@example.com",
            "password123"
        );

        // Act
        var result = await bus.InvokeAsync<Result<Guid>>(command);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeEmpty();

        var store = app.Services.GetRequiredService<IDocumentStore>();
        await using (var session = store.LightweightSession())
        {
            var member = await session.LoadAsync<Member>(result.Value);
            member.Should().NotBeNull();
            member!.Phone.Should().Be(uniquePhone);
        }

        await app.StopAsync();
    }

    [Fact]
    public async Task RegisterMember_WithDuplicatePhone_ShouldThrowDomainException()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        ConfigureTestBuilder(builder);

        await using var app = BootstrapperHost.BuildAppFromBuilder(builder);
        await app.StartAsync();

        var bus = app.Services.GetRequiredService<IMessageBus>();
        var store = app.Services.GetRequiredService<IDocumentStore>();

        var phone = $"13800138{Random.Shared.Next(1000, 9999)}";
        await using (var session = store.LightweightSession())
        {
            session.Store(Member.Register("已存在的会员", phone, string.Empty));
            await session.SaveChangesAsync();
        }

        var command = new RegisterMember(
            "张三",
            phone,
            "zhangsan@example.com",
            "password123"
        );

        // Act
        var act = async () => await bus.InvokeAsync<Result<Guid>>(command);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.Code.Should().Be(MemberErrorCodes.DuplicatePhone.Code);

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
        var store = app.Services.GetRequiredService<IDocumentStore>();

        var memberId = await CreateMemberAsync(store, balance: 100m);
        var command = new TopUpBalance(memberId, 200m, "支付宝");

        // Act
        var result = await bus.InvokeAsync<Result>(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await using (var session = store.LightweightSession())
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
        var store = app.Services.GetRequiredService<IDocumentStore>();

        var memberId = await CreateMemberAsync(store, balance: 100m);
        var command = new DeductBalance(memberId, 50m, "支付订单");

        // Act
        var result = await bus.InvokeAsync<Result>(command);

        // Assert
        result.IsSuccess.Should().BeTrue();

        await using (var session = store.LightweightSession())
        {
            var member = await session.LoadAsync<Member>(memberId);
            member.Should().NotBeNull();
            member!.Balance.Should().Be(50m);
        }

        await app.StopAsync();
    }

    [Fact]
    public async Task DeductBalance_WithInsufficientBalance_ShouldThrowDomainException()
    {
        // Arrange
        var builder = WebApplication.CreateBuilder();
        ConfigureTestBuilder(builder);

        await using var app = BootstrapperHost.BuildAppFromBuilder(builder);
        await app.StartAsync();

        var bus = app.Services.GetRequiredService<IMessageBus>();
        var store = app.Services.GetRequiredService<IDocumentStore>();

        var memberId = await CreateMemberAsync(store, balance: 30m);
        var command = new DeductBalance(memberId, 50m, "支付订单");

        // Act
        var act = async () => await bus.InvokeAsync<Result>(command);

        // Assert
        var ex = await act.Should().ThrowAsync<DomainException>();
        ex.Which.Code.Should().Be(MemberErrorCodes.InsufficientBalance.Code);

        await app.StopAsync();
    }

    private void ConfigureTestBuilder(WebApplicationBuilder builder)
    {
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = _fixture.ConnectionString
        });

        builder.Environment.EnvironmentName = "Testing";
    }

    private static async Task<Guid> CreateMemberAsync(
        IDocumentStore store,
        decimal balance,
        string? phone = null)
    {
        var member = Member.Register(
            "测试会员",
            phone ?? $"13800138{Random.Shared.Next(1000, 9999)}",
            string.Empty,
            MemberTier.Regular,
            balance: balance,
            points: 0
        );

        await using var session = store.LightweightSession();
        session.Store(member);
        await session.SaveChangesAsync();
        return member.Id;
    }
}
