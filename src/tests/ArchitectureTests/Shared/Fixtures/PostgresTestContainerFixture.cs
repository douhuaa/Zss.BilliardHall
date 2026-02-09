using DotNet.Testcontainers.Builders;
using Testcontainers.PostgreSql;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.Fixtures;

/// <summary>
/// PostgreSQL 测试容器 Fixture
/// 支持两种模式：
/// 1. CI 模式：从环境变量读取 POSTGRES_CONNECTION_STRING
/// 2. 本地模式：使用 Testcontainers 启动临时 PostgreSQL 容器
/// </summary>
public sealed class PostgresTestContainerFixture : IAsyncLifetime
{
    private PostgreSqlContainer? _container;
    private string? _connectionString;
    private readonly bool _useExternalDatabase;

    public string ConnectionString => _connectionString
        ?? throw new InvalidOperationException("连接字符串未初始化，请先调用 InitializeAsync");

    public PostgresTestContainerFixture()
    {
        // 优先使用 CI 提供的连接串
        var envConnectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING");
        _useExternalDatabase = !string.IsNullOrEmpty(envConnectionString);
        
        if (_useExternalDatabase)
        {
            _connectionString = envConnectionString;
        }
    }

    public async Task InitializeAsync()
    {
        if (_useExternalDatabase)
        {
            // 使用外部数据库，无需启动容器
            Console.WriteLine($"[PostgresTestContainerFixture] 使用外部数据库（来自环境变量）");
            return;
        }

        // 启动 Testcontainer
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:17-alpine")
            .WithDatabase("test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        try
        {
            Console.WriteLine("[PostgresTestContainerFixture] 启动 PostgreSQL 容器...");
            await _container.StartAsync();
            _connectionString = _container.GetConnectionString();
            Console.WriteLine($"[PostgresTestContainerFixture] 容器已启动，连接串: {MaskConnectionString(_connectionString)}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PostgresTestContainerFixture] 容器启动失败: {ex.Message}");
            throw;
        }
    }

    public async Task DisposeAsync()
    {
        if (_container != null)
        {
            try
            {
                Console.WriteLine("[PostgresTestContainerFixture] 停止 PostgreSQL 容器...");
                await _container.DisposeAsync();
                Console.WriteLine("[PostgresTestContainerFixture] 容器已停止");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[PostgresTestContainerFixture] 容器停止时发生错误: {ex.Message}");
                // 不重新抛出异常，避免影响测试清理
            }
        }
    }

    /// <summary>
    /// 屏蔽连接串中的敏感信息（用于日志输出）
    /// </summary>
    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return string.Empty;

        // 简单替换密码部分
        var parts = connectionString.Split(';');
        var masked = new List<string>();
        foreach (var part in parts)
        {
            if (part.Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
            {
                masked.Add("Password=***");
            }
            else
            {
                masked.Add(part);
            }
        }
        return string.Join(";", masked);
    }
}
