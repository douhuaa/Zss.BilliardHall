using Marten;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared.Fixtures;

/// <summary>
/// 共享测试 Fixture，用于集成测试
/// 提供：
/// - PostgreSQL 容器管理
/// - Marten DocumentStore（测试隔离的 schema）
/// - 轻量级 Host 和 HttpClient
/// </summary>
public sealed class SharedTestFixture : IAsyncLifetime
{
    private PostgresTestContainerFixture? _postgresFixture;
    private IDocumentStore? _documentStore;
    private IHost? _host;

    public IDocumentStore DocumentStore => _documentStore
        ?? throw new InvalidOperationException("DocumentStore 未初始化，请先调用 InitializeAsync");

    public IHost Host => _host
        ?? throw new InvalidOperationException("Host 未初始化，请先调用 InitializeAsync");

    public string ConnectionString => _postgresFixture?.ConnectionString
        ?? throw new InvalidOperationException("连接字符串未初始化");

    public async Task InitializeAsync()
    {
        Console.WriteLine("[SharedTestFixture] 开始初始化测试环境...");

        // 1. 初始化 PostgreSQL 容器
        _postgresFixture = new PostgresTestContainerFixture();
        await _postgresFixture.InitializeAsync();

        // 2. 创建 DocumentStore（使用唯一 schema）
        _documentStore = Factories.DocumentStoreFactory.Create(
            _postgresFixture.ConnectionString,
            schema: null, // 自动生成唯一 schema
            loggerFactory: null // 可选：注入 ILoggerFactory
        );

        Console.WriteLine($"[SharedTestFixture] DocumentStore 已创建，Schema: {_documentStore.Options.DatabaseSchemaName}");

        // 3. 创建测试 Host（可选，用于集成测试）
        _host = CreateTestHost(_documentStore);

        Console.WriteLine("[SharedTestFixture] 测试环境初始化完成");
    }

    public async Task DisposeAsync()
    {
        Console.WriteLine("[SharedTestFixture] 开始清理测试环境...");

        try
        {
            // 1. 释放 Host
            if (_host != null)
            {
                await _host.StopAsync();
                _host.Dispose();
                Console.WriteLine("[SharedTestFixture] Host 已释放");
            }

            // 2. 清理并释放 DocumentStore
            if (_documentStore != null)
            {
                try
                {
                    // 清理 Schema（可选）
                    // await _documentStore.CompletelyRemoveSchemaAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SharedTestFixture] 清理 Schema 时发生警告: {ex.Message}");
                }

                _documentStore.Dispose();
                Console.WriteLine("[SharedTestFixture] DocumentStore 已释放");
            }

            // 3. 释放 PostgreSQL 容器
            if (_postgresFixture != null)
            {
                await _postgresFixture.DisposeAsync();
            }

            Console.WriteLine("[SharedTestFixture] 测试环境清理完成");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SharedTestFixture] 清理过程中发生错误: {ex.Message}");
            // 不重新抛出异常，避免影响测试清理
        }
    }

    /// <summary>
    /// 创建测试用的 IHost
    /// </summary>
    private IHost CreateTestHost(IDocumentStore documentStore)
    {
        var hostBuilder = Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // 注册测试用的 DocumentStore（替换生产配置）
                services.AddSingleton(documentStore);

                // 可选：注册其他测试服务
                // services.AddWolverine(...);
                // services.AddScoped<IMyService, MockMyService>();
            })
            .ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddConsole();
                logging.SetMinimumLevel(LogLevel.Warning); // 测试时减少日志噪音
            });

        return hostBuilder.Build();
    }

    /// <summary>
    /// 获取服务实例
    /// </summary>
    public T GetService<T>() where T : notnull
    {
        return Host.Services.GetRequiredService<T>();
    }

    /// <summary>
    /// 创建服务 Scope
    /// </summary>
    public IServiceScope CreateScope()
    {
        return Host.Services.CreateScope();
    }

    /// <summary>
    /// 清空所有文档数据（测试之间的隔离）
    /// </summary>
    public async Task ClearAllDataAsync()
    {
        if (_documentStore != null)
        {
            await _documentStore.Advanced.Clean.DeleteAllDocumentsAsync();
        }
    }
}
