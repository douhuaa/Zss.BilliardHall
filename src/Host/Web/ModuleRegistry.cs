using Zss.BilliardHall.Modules.Members;
using Zss.BilliardHall.Modules.Orders;
using Zss.BilliardHall.Platform.Contracts;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// 模块注册表 - Host 层显式决定加载哪些模块
/// 无反射、无运行时扫描、完全显式
/// 冻结规范：所有新增模块必须在这里声明
/// </summary>
public static class ModuleRegistry
{
    /// <summary>
    /// 所有可用的模块实例
    /// 冻结：添加新模块时，只需在此数组中实例化
    /// </summary>
    private static readonly IModule[] AllModules =
    [
        new MemberModule(),
        new OrderModule()
    ];

    /// <summary>
    /// 获取启用的模块
    /// </summary>
    public static IModule[] GetEnabledModules(IConfiguration configuration)
    {
        var enabledNames = ReadEnabledModuleNames(configuration);

        // 如果未配置，返回全部
        if (enabledNames.Length == 0)
            return AllModules.ToArray();

        var enabledSet = enabledNames.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var result = AllModules
            .Where(m => enabledSet.Contains(m.Name))
            .ToArray();

        ValidateNoMissing(enabledSet, result);
        return result;
    }

    private static void ValidateNoMissing(HashSet<string> enabledSet, IModule[] result)
    {
        var found = result.Select(m => m.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var missing = enabledSet.Except(found).ToArray();

        if (missing.Length == 0)
            return;

        throw new InvalidOperationException(
            $"配置中指定的模块不存在：{string.Join(", ", missing)}\n" +
            $"可用模块：{string.Join(", ", AllModules.Select(m => m.Name))}");
    }

    private static string[] ReadEnabledModuleNames(IConfiguration configuration)
    {
        // 支持多种配置方式
        var enabled = configuration.GetSection("Modules:Enabled").Get<string[]>();
        if (enabled is { Length: > 0 })
            return Normalize(enabled);

        var raw = configuration["Modules:Enabled"];
        if (!string.IsNullOrWhiteSpace(raw))
            return Normalize(raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

        return [];
    }

    private static string[] Normalize(IEnumerable<string> names)
        => names
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}
