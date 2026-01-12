using Testcontainers.PostgreSql;
using Xunit;

namespace Zss.BilliardHall.Wolverine.Bootstrapper.Tests;

/// <summary>
/// PostgreSQL 测试容器 Fixture，用于集成测试
/// PostgreSQL test container fixture for integration tests
/// </summary>
/// <remarks>
/// 此 Fixture 使用 Testcontainers 在测试运行时自动启动和停止 PostgreSQL 容器。
/// 不依赖 Aspire AppHost 或外部 Docker Compose，仅需本地 Docker 环境。
/// This fixture uses Testcontainers to automatically start and stop a PostgreSQL container during test execution.
/// Does not depend on Aspire AppHost or external Docker Compose, only requires local Docker environment.
/// </remarks>
public sealed class PostgresFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;

    /// <summary>
    /// 获取 PostgreSQL 连接字符串
    /// Gets the PostgreSQL connection string
    /// </summary>
    public string ConnectionString { get; private set; } = string.Empty;

    /// <summary>
    /// 初始化测试容器（在测试类开始前执行）
    /// Initializes the test container (executed before test class starts)
    /// </summary>
    public async Task InitializeAsync()
    {
        // 注意：以下用户名和密码仅用于本地/CI 集成测试的临时 PostgreSQL 容器
        // Note: The following username and password are only for temporary PostgreSQL containers in local/CI integration tests
        // 容器由 Testcontainers 在测试期间创建，并通过 WithCleanUp(true) 在测试结束后自动清理
        // Containers are created by Testcontainers during tests and automatically cleaned up after tests via WithCleanUp(true)
        // 切勿在生产或持久化环境中复用这些凭据，它们不是实际业务数据库的账号
        // Never reuse these credentials in production or persistent environments; they are not real business database accounts
        _container = new PostgreSqlBuilder()
            .WithDatabase("billiard_test")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .WithCleanUp(true) // 自动清理容器
            .Build();

        await _container.StartAsync();
        ConnectionString = _container.GetConnectionString();
    }

    /// <summary>
    /// 清理测试容器（在测试类完成后执行）
    /// Cleans up the test container (executed after test class completes)
    /// </summary>
    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            await _container.DisposeAsync();
        }
    }
}
