using System.Reflection;
using Microsoft.Extensions.Configuration;
using Zss.BilliardHall.Modules.Members;
using Zss.BilliardHall.Modules.Orders;

namespace Zss.BilliardHall.Host.Web;

/// <summary>
/// 模块注册表 - Host 层负责决定加载哪些模块
/// 优势：
/// 1. 类型安全（通过 typeof 引用）
/// 2. IDE 可跟踪（F12 导航、重构支持）
/// 3. 编译时检查（模块缺失立即发现）
/// 4. 符合 ADR-002（Application 不依赖 Modules）
/// </summary>
public static class ModuleRegistry
{
    /// <summary>
    /// 所有可用模块的程序集列表
    /// 注意：顺序决定了模块的初始化顺序
    /// </summary>
    private static readonly Assembly[] AllModuleAssemblies =
    [
        typeof(MemberModule).Assembly,  // Members 模块
        typeof(OrderModule).Assembly    // Orders 模块
    ];

    /// <summary>
    /// 获取启用的模块程序集
    /// 支持通过配置控制启用/禁用（可选）
    /// </summary>
    public static Assembly[] GetEnabledAssemblies(IConfiguration configuration)
    {
        var enabledModuleNames = GetEnabledModuleNames(configuration);

        // 如果配置为空，默认启用所有模块
        if (enabledModuleNames.Length == 0)
            return AllModuleAssemblies;

        // 根据配置筛选启用的模块
        var enabledSet = new HashSet<string>(enabledModuleNames, StringComparer.OrdinalIgnoreCase);
        var enabledModules = AllModuleAssemblies
            .Where(a => enabledSet.Contains(a.GetName().Name!))
            .ToArray();

        // 验证配置的模块是否存在
        var foundNames = new HashSet<string>(enabledModules.Select(a => a.GetName().Name!), StringComparer.OrdinalIgnoreCase);
        var missingModules = enabledSet.Except(foundNames).ToArray();
        if (missingModules.Length > 0)
            throw new InvalidOperationException(
                $"配置中指定的模块不存在：{string.Join(", ", missingModules)}\n" +
                $"可用模块：{string.Join(", ", AllModuleAssemblies.Select(a => a.GetName().Name))}");

        return enabledModules;
    }

    private static string[] GetEnabledModuleNames(IConfiguration configuration)
    {
        // 支持两种配置方式：
        // 1. Modules:Enabled 数组（推荐）- 明确控制启用的模块
        // 2. Modules:Assemblies 数组（兼容旧格式）
        var section = configuration.GetSection("Modules:Enabled");
        if (section.Exists())
        {
            var names = section.Get<string[]>();
            if (names is { Length: > 0 })
                return NormalizeModuleNames(names);
        }

        // 兼容旧格式
        section = configuration.GetSection("Modules:Assemblies");
        if (section.Exists())
        {
            var names = section.Get<string[]>();
            if (names is { Length: > 0 })
                return NormalizeModuleNames(names);
        }

        var raw = configuration["Modules:Assemblies"];
        if (!string.IsNullOrWhiteSpace(raw))
            return NormalizeModuleNames(raw.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

        // 默认启用所有模块
        return [];
    }

    private static string[] NormalizeModuleNames(IEnumerable<string> names)
        => names
            .Where(static x => !string.IsNullOrWhiteSpace(x))
            .Select(static x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
}

