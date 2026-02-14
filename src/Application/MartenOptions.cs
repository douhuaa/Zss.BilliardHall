namespace Zss.BilliardHall.Application;

/// <summary>
/// Marten 数据库配置选项
/// 使用 IOptions 模式管理配置，避免硬编码连接字符串
/// </summary>
public sealed class MartenOptions
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "Marten";

    /// <summary>
    /// PostgreSQL 连接字符串
    /// 应通过 User Secrets / KeyVault / 环境变量配置，禁止硬编码
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// 验证配置是否有效
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ConnectionString))
            throw new InvalidOperationException(
                $"Marten:ConnectionString 未配置。" +
                $"请使用 User Secrets (开发)、KeyVault (生产) 或环境变量配置数据库连接字符串。" +
                $"示例：dotnet user-secrets set \"Marten:ConnectionString\" \"Host=localhost;Port=5432;Database=xxx;Username=xxx;Password=xxx\"");
    }
}
