using System.Reflection;
using Zss.BilliardHall.Modules.Members;
using Zss.BilliardHall.Modules.Orders;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// 模块注册表 - Host 层负责决定加载哪些模块
/// </summary>
public static class ModuleRegistry
{
    /// <summary>
    /// 所有可用模块的程序集列表
    /// </summary>
    private static readonly Assembly[] AllModuleAssemblies =
    [
        typeof(MemberModule).Assembly,  // Members 模块
        typeof(OrderModule).Assembly    // Orders 模块
    ];

    /// <summary>
    /// 获取启用的模块程序集
    /// </summary>
    public static Assembly[] GetEnabledAssemblies(IConfiguration configuration)
    {
        var enabled = ReadEnabledModuleNames(configuration);
        if (enabled.Length == 0)
            return AllModuleAssemblies;

        var enabledSet = enabled.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var result = AllModuleAssemblies
            .Where(a =>
            {
                var shortName = a.GetName().Name!;
                var fullName = a.GetName().FullName;
                return enabledSet.Contains(shortName) || enabledSet.Contains(fullName);
            })
            .ToArray();

        ValidateNoMissing(enabledSet, result);

        return result;
    }


    private static void ValidateNoMissing(HashSet<string> enabledSet, Assembly[] result)
    {
        var found = result
            .SelectMany(a => new[] { a.GetName().Name!, a.GetName().FullName })
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var missing = enabledSet.Except(found).ToArray();
        if (missing.Length == 0)
            return;

        throw new InvalidOperationException(
            $"配置中指定的模块不存在：{string.Join(", ", missing)}\n" +
            $"可用模块：{string.Join(", ", AllModuleAssemblies.Select(a => a.GetName().Name))}");
    }

    private static string[] ReadEnabledModuleNames(IConfiguration configuration)
    {
        var enabled = configuration.GetSection("Modules:Enabled").Get<string[]>();
        if (enabled is { Length: > 0 })
            return Normalize(enabled);

        var assemblies = configuration.GetSection("Modules:Assemblies").Get<string[]>();
        if (assemblies is { Length: > 0 })
            return Normalize(assemblies);

        var raw = configuration["Modules:Assemblies"];
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
