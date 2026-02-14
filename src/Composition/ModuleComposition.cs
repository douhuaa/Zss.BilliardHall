using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Zss.BilliardHall.Modules.Members;
using Zss.BilliardHall.Modules.Orders;
using Zss.BilliardHall.Platform.Contracts;

namespace Zss.BilliardHall.Composition;

/// <summary>
/// Composition Root - 唯一了解具体模块类型的地方
/// 职责：提供可用的模块实例，支持配置过滤
/// 设计：
/// - Composition 项目可以引用所有 Modules
/// - Host 项目仅引用 Composition，不直接引用 Modules
/// - 这样保持了类型安全，同时满足 ADR-002 的架构边界约束
/// </summary>
public static class ModuleComposition
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
    /// <param name="configuration">配置对象</param>
    /// <returns>根据配置启用的模块数组</returns>
    public static IModule[] GetEnabledModules(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

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
