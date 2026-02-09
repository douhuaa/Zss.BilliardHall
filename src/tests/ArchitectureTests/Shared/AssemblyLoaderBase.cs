namespace Zss.BilliardHall.Tests.ArchitectureTests.Shared;

/// <summary>
/// 程序集加载器基类
/// 提供统一的程序集加载逻辑，消除 ModuleAssemblyData 和 HostAssemblyData 之间的代码重复
/// 
/// 设计原则：
/// - 使用 Lazy<T> 延迟加载避免重复初始化
/// - 使用 TestEnvironment 消除硬编码路径
/// - 提供灵活的路径解析策略
/// 
/// 重构说明：
/// 从 ModuleAssemblyData 和 HostAssemblyData 中提取公共逻辑（~70% 代码重复）
/// </summary>
public abstract class AssemblyLoaderBase
{
    /// <summary>
    /// 解析程序集路径的优先级候选列表
    /// </summary>
    /// <param name="projectDir">项目目录</param>
    /// <param name="projectName">项目名称</param>
    /// <param name="configuration">构建配置</param>
    /// <param name="tfms">目标框架列表</param>
    /// <returns>按优先级排序的 DLL 路径候选列表</returns>
    protected static List<string> ResolveAssemblyPathCandidates(
        string projectDir,
        string projectName,
        string configuration,
        string[] tfms)
    {
        var prioritized = new List<string>();

        // 1. 优先级候选：bin/{Configuration}/{TFM}/{ProjectName}.dll
        foreach (var tfm in tfms)
        {
            prioritized.Add(Path.Combine(projectDir, "bin", configuration, tfm, $"{projectName}.dll"));
            prioritized.Add(Path.Combine(projectDir, "obj", configuration, tfm, $"{projectName}.dll"));
        }

        // 2. Fallback 候选：全目录搜索
        var fallback = new List<string>();
        try
        {
            fallback.AddRange(Directory.GetFiles(projectDir, $"{projectName}.dll", SearchOption.AllDirectories));
            
            var binDir = Path.Combine(projectDir, "bin");
            if (Directory.Exists(binDir))
            {
                fallback.AddRange(Directory.GetFiles(binDir, $"{projectName}.dll", SearchOption.AllDirectories));
            }

            var objDir = Path.Combine(projectDir, "obj");
            if (Directory.Exists(objDir))
            {
                fallback.AddRange(Directory.GetFiles(objDir, $"{projectName}.dll", SearchOption.AllDirectories));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AssemblyLoaderBase] 搜索 DLL 时出错: {ex.Message}");
        }

        // 3. 合并并去重，保持优先级顺序
        var candidates = prioritized
            .Concat(fallback)
            .Where(p => !string.IsNullOrEmpty(p))
            .Select(Path.GetFullPath)
            .Distinct()
            .Where(File.Exists)
            .ToList();

        return candidates;
    }

    /// <summary>
    /// 加载单个程序集
    /// </summary>
    /// <param name="dllPath">DLL 文件路径</param>
    /// <param name="expectedName">期望的程序集名称（可选，用于验证）</param>
    /// <returns>加载的程序集，失败则返回 null</returns>
    protected static Assembly? LoadAssembly(string dllPath, string? expectedName = null)
    {
        try
        {
            var assembly = Assembly.LoadFrom(dllPath);
            var assemblyName = assembly.GetName().Name;

            Debug.WriteLine($"[AssemblyLoaderBase] 已加载程序集: {dllPath}, AssemblyName={assemblyName}");

            // 如果指定了期望名称，进行验证
            if (expectedName != null && assemblyName != expectedName)
            {
                Debug.WriteLine($"[AssemblyLoaderBase] 警告: 加载的程序集名称为 {assemblyName}，与期望名称 {expectedName} 不匹配");
            }

            return assembly;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[AssemblyLoaderBase] 无法加载程序集 {dllPath}: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// 从目录列表加载所有程序集
    /// </summary>
    /// <param name="directories">项目目录列表</param>
    /// <param name="configuration">构建配置</param>
    /// <param name="tfms">目标框架列表</param>
    /// <param name="nameValidator">程序集名称验证器（可选）</param>
    /// <returns>成功加载的程序集列表</returns>
    protected static List<Assembly> LoadAssembliesFromDirectories(
        IEnumerable<string> directories,
        string configuration,
        string[] tfms,
        Func<string, string, bool>? nameValidator = null)
    {
        var assemblies = new List<Assembly>();

        foreach (var directory in directories)
        {
            var projectName = Path.GetFileName(directory);
            var candidates = ResolveAssemblyPathCandidates(directory, projectName, configuration, tfms);

            if (!candidates.Any())
            {
                Debug.WriteLine($"[AssemblyLoaderBase] 未找到程序集输出: {projectName}，路径={directory}。请确保已构建（dotnet build）。");
                continue;
            }

            var selectedPath = candidates.First();
            var assembly = LoadAssembly(selectedPath, projectName);

            if (assembly != null)
            {
                // 如果提供了名称验证器，进行验证
                if (nameValidator == null || nameValidator(assembly.GetName().Name ?? "", projectName))
                {
                    assemblies.Add(assembly);
                }
            }
        }

        return assemblies;
    }

    /// <summary>
    /// 验证程序集列表是否为空，并在必要时抛出友好的错误消息
    /// </summary>
    /// <param name="assemblies">程序集列表</param>
    /// <param name="assemblyType">程序集类型描述（如 "模块"、"Host"）</param>
    protected static void ValidateAssembliesNotEmpty(IReadOnlyList<Assembly> assemblies, string assemblyType)
    {
        if (assemblies.Count == 0)
        {
            true.Should().BeFalse(
                $"❌ 未加载任何{assemblyType}程序集，架构测试失效。" +
                $"请先运行 `dotnet build` 或检查{assemblyType}输出路径/命名约定。");
        }
    }
}
