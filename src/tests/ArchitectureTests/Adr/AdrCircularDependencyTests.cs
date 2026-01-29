using Xunit;

namespace Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-940: ADR 关系与溯源管理
/// 检测 ADR 依赖关系中的循环依赖
/// </summary>
public sealed class AdrCircularDependencyTests
{
    private readonly IReadOnlyDictionary<string, AdrDocument> _adrs;

    public AdrCircularDependencyTests()
    {
        var repoRoot = FindRepositoryRoot() 
            ?? throw new InvalidOperationException("未找到仓库根目录（无法定位 docs/adr 或 .git）");
        
        var adrPath = Path.Combine(repoRoot, "docs", "adr");
        var repo = new AdrRepository(adrPath);
        
        _adrs = repo.LoadAll().ToDictionary(a => a.Id);
    }

    /// <summary>
    /// ADR-940.4: ADR 依赖关系不得形成循环
    /// </summary>
    [Fact(DisplayName = "ADR-940.4: ADR 依赖关系不得存在循环")]
    public void ADR_Dependencies_Must_Not_Form_Cycles()
    {
        var cycles = DetectCycles();
        
        if (cycles.Count > 0)
        {
            var violations = cycles.Select(cycle => 
                $"❌ 检测到循环依赖：\n   " + string.Join(" → ", cycle) + " → " + cycle[0]
            );
            
            Assert.Fail($"发现 {cycles.Count} 个循环依赖：\n\n" + string.Join("\n\n", violations));
        }
    }

    /// <summary>
    /// 使用 DFS 检测循环依赖
    /// </summary>
    private List<List<string>> DetectCycles()
    {
        var cycles = new List<List<string>>();
        var visited = new HashSet<string>();
        var recursionStack = new HashSet<string>();
        var path = new List<string>();

        foreach (var adrId in _adrs.Keys)
        {
            if (!visited.Contains(adrId))
            {
                DfsDetectCycle(adrId, visited, recursionStack, path, cycles);
            }
        }

        return cycles;
    }

    /// <summary>
    /// 深度优先搜索检测循环
    /// </summary>
    private void DfsDetectCycle(
        string current,
        HashSet<string> visited,
        HashSet<string> recursionStack,
        List<string> path,
        List<List<string>> cycles)
    {
        visited.Add(current);
        recursionStack.Add(current);
        path.Add(current);

        if (_adrs.TryGetValue(current, out var adr))
        {
            foreach (var dependency in adr.DependsOn)
            {
                if (!_adrs.ContainsKey(dependency))
                    continue; // 跳过不存在的 ADR

                if (recursionStack.Contains(dependency))
                {
                    // 发现循环
                    var cycleStart = path.IndexOf(dependency);
                    if (cycleStart >= 0)
                    {
                        var cycle = path.Skip(cycleStart).ToList();
                        cycles.Add(cycle);
                    }
                }
                else if (!visited.Contains(dependency))
                {
                    DfsDetectCycle(dependency, visited, recursionStack, path, cycles);
                }
            }
        }

        path.RemoveAt(path.Count - 1);
        recursionStack.Remove(current);
    }

    /// <summary>
    /// 查找仓库根目录
    /// </summary>
    private static string? FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, "docs", "adr")) ||
                Directory.Exists(Path.Combine(currentDir, ".git")))
            {
                return currentDir;
            }
            
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
        
        return null;
    }
}
