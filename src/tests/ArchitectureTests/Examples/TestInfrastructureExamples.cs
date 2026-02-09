using Zss.BilliardHall.Tests.ArchitectureTests.Shared.Fixtures;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared.Extensions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared.Factories;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Examples;

/// <summary>
/// 示例测试：演示如何使用新的测试工具类
/// </summary>
public class TestInfrastructureExamples
{
    [Fact(DisplayName = "示例：使用 PostgresTestContainerFixture")]
    public async Task Example_PostgresTestContainerFixture()
    {
        // Arrange: 创建 PostgreSQL 容器 Fixture
        var fixture = new PostgresTestContainerFixture();
        await fixture.InitializeAsync();

        try
        {
            // Act: 获取连接字符串
            var connectionString = fixture.ConnectionString;

            // Assert: 连接字符串不应为空
            connectionString.Should().NotBeNullOrWhiteSpace();
            Console.WriteLine($"✅ 连接字符串已获取（已屏蔽敏感信息）");
        }
        finally
        {
            // Cleanup: 释放资源
            await fixture.DisposeAsync();
        }
    }

    [Fact(DisplayName = "示例：使用 DocumentStoreFactory 创建隔离的 DocumentStore")]
    public async Task Example_DocumentStoreFactory_CreateIsolatedStore()
    {
        // Arrange: 创建数据库容器
        var postgresFixture = new PostgresTestContainerFixture();
        await postgresFixture.InitializeAsync();

        try
        {
            // Act: 使用工厂创建 DocumentStore（自动生成唯一 schema）
            using var store = DocumentStoreFactory.Create(postgresFixture.ConnectionString);

            // Assert: 验证 store 和 schema
            store.Should().NotBeNull();
            store.Options.DatabaseSchemaName.Should().StartWith("test_schema_");
            
            // 验证连接是否正常
            var isConnected = await store.VerifyConnectionAsync();
            isConnected.Should().BeTrue();
            
            Console.WriteLine($"✅ DocumentStore 已创建，Schema: {store.Options.DatabaseSchemaName}");
        }
        finally
        {
            await postgresFixture.DisposeAsync();
        }
    }

    [Fact(DisplayName = "示例：使用 MartenTestExtensions 清理数据")]
    public async Task Example_MartenTestExtensions_ClearData()
    {
        // Arrange: 创建测试环境
        var postgresFixture = new PostgresTestContainerFixture();
        await postgresFixture.InitializeAsync();

        try
        {
            using var store = DocumentStoreFactory.Create(postgresFixture.ConnectionString);

            // 插入一些测试数据
            await using var session = store.LightweightSession();
            session.Store(new { Id = Guid.NewGuid(), Name = "测试文档" });
            await session.SaveChangesAsync();

            // Act: 清空所有数据
            await store.ClearAllDataAsync();

            // Assert: 验证数据已清空
            await using var verifySession = store.QuerySession();
            var count = verifySession.Query<object>().ToList().Count;
            count.Should().Be(0);
            
            Console.WriteLine("✅ 数据已成功清空");
        }
        finally
        {
            await postgresFixture.DisposeAsync();
        }
    }
}
