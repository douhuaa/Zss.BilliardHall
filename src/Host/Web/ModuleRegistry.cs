using System.Reflection;
using Zss.BilliardHall.Modules.Members;
using Zss.BilliardHall.Modules.Orders;
using Zss.BilliardHall.Platform.Contracts;

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
    public static Assembly[] GetEnabledAssemblies(IConfiguration configuration, Microsoft.Extensions.Logging.ILogger? logger = null)
    {
        var enabledModuleNames = GetEnabledModuleNames(configuration);

        Assembly[] result;

        // 如果配置为空，默认启用所有模块
        if (enabledModuleNames.Length == 0)
        {
            result = AllModuleAssemblies;
            logger?.LogInformation(
                "未配置 Modules:Enabled，启用所有 {Count} 个模块: {Modules}",
                result.Length,
                string.Join(", ", result.Select(a => a.GetName().Name)));
        }
        else
        {
            // 根据配置筛选启用的模块
            var enabledSet = new HashSet<string>(enabledModuleNames, StringComparer.OrdinalIgnoreCase);
            result = AllModuleAssemblies
                .Where(a => IsModuleEnabled(a, enabledSet))
                .ToArray();

            // 验证配置的模块是否存在
            var foundNames = new HashSet<string>(
                result.SelectMany(a => new[] { a.GetName().Name!, GetFullModuleName(a) }),
                StringComparer.OrdinalIgnoreCase);
            var missingModules = enabledSet.Except(foundNames).ToArray();
            if (missingModules.Length > 0)
                throw new InvalidOperationException(
                    $"配置中指定的模块不存在：{string.Join(", ", missingModules)}\n" +
                    $"可用模块（支持短名称或完整名称）：\n" +
                    string.Join("\n", AllModuleAssemblies.Select(a => $"  - {a.GetName().Name} 或 {GetFullModuleName(a)}")));

            logger?.LogInformation(
                "已启用 {EnabledCount}/{TotalCount} 个模块: {EnabledModules}",
                result.Length,
                AllModuleAssemblies.Length,
                string.Join(", ", result.Select(a => a.GetName().Name)));

            var disabledModules = AllModuleAssemblies.Except(result).ToArray();
            if (disabledModules.Length > 0)
            {
                logger?.LogDebug(
                    "已禁用 {DisabledCount} 个模块: {DisabledModules}",
                    disabledModules.Length,
                    string.Join(", ", disabledModules.Select(a => a.GetName().Name)));
            }
        }

        return result;
    }

    /// <summary>
    /// 判断模块是否在启用列表中
    /// 支持短名称（如 "Members"）和完整名称（如 "Zss.BilliardHall.Members"）
    /// </summary>
    private static bool IsModuleEnabled(Assembly assembly, HashSet<string> enabledSet)
    {
        var shortName = assembly.GetName().Name!;
        var fullName = GetFullModuleName(assembly);
        return enabledSet.Contains(shortName) || enabledSet.Contains(fullName);
    }

    /// <summary>
    /// 获取模块的完整名称（基于实际命名空间）
    /// 优先从程序集中获取模块类型的命名空间，回退到约定命名
    /// </summary>
    private static string GetFullModuleName(Assembly assembly)
    {
        // 尝试从程序集中查找 IModule 实现类型的命名空间
        try
        {
            var moduleType = assembly.GetTypes()
                .FirstOrDefault(t => t is { IsAbstract: false, IsInterface: false }
                    && typeof(IModule).IsAssignableFrom(t));

            if (moduleType?.Namespace != null)
                return moduleType.Namespace;
        }
        catch
        {
            // 如果无法加载类型，回退到约定命名
        }

        // 回退到约定命名：Zss.BilliardHall.{ShortName}
        var shortName = assembly.GetName().Name!;
        if (shortName.StartsWith("Zss.BilliardHall.", StringComparison.OrdinalIgnoreCase))
            return shortName;

        return $"Zss.BilliardHall.{shortName}";
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

