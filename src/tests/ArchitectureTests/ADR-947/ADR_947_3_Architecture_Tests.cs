namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_947;

using Zss.BilliardHall.Tests.ArchitectureTests.Adr;

/// <summary>
/// ADR-947_3: 禁止显式循环声明（Rule）
/// 验证关系声明区不能同时出现 A→B 和 B→A 的循环依赖声明
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-947_3_1: 循环检测
///
/// 关联文档：
/// - ADR: docs/adr/governance/ADR-947-relationship-section-structure-parsing-safety.md
/// </summary>
public sealed class ADR_947_3_Architecture_Tests
{
    /// <summary>
    /// ADR-947_3_1: 循环检测
    /// 验证关系声明区不能同时声明 A→B 和 B→A，应使用单向声明+相关关系（§ADR-947_3_1）
    /// </summary>
    [Fact(DisplayName = "ADR-947_3_1: 禁止显式循环依赖声明")]
    public void ADR_947_3_1_Relationships_Must_Not_Have_Bidirectional_Dependencies()
    {
        var repoRoot = TestEnvironment.RepositoryRoot ?? throw new InvalidOperationException("未找到仓库根目录");
        var adrDirectory = Path.Combine(repoRoot, "docs/adr");

        var adrFiles = AdrFileFilter.GetAdrFiles(adrDirectory);

        // 构建所有 ADR 的依赖关系图
        var dependencies = new Dictionary<string, HashSet<string>>();

        foreach (var adrFile in adrFiles)
        {
            var fileName = Path.GetFileName(adrFile);
            var currentAdr = ExtractAdrNumber(fileName);
            if (string.IsNullOrEmpty(currentAdr))
            {
                continue;
            }

            var content = File.ReadAllText(adrFile);
            var relationshipsContent = ExtractRelationshipsSection(content);

            // 提取 "Depends On" 或 "依赖" 区域的依赖项
            var dependsOnPattern = @"\*\*(Depends On|依赖).*?\*\*.*?(?=\*\*|$)";
            var dependsOnMatch = Regex.Match(relationshipsContent, dependsOnPattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

            if (dependsOnMatch.Success)
            {
                var dependsOnContent = dependsOnMatch.Value;
                var dependencyNumbers = ExtractAdrNumbers(dependsOnContent);

                if (!dependencies.ContainsKey(currentAdr))
                {
                    dependencies[currentAdr] = new HashSet<string>();
                }

                foreach (var dep in dependencyNumbers)
                {
                    dependencies[currentAdr].Add(dep);
                }
            }
        }

        // 检测循环依赖
        var violations = new List<string>();

        foreach (var kvp in dependencies)
        {
            var adrA = kvp.Key;
            var depsOfA = kvp.Value;

            foreach (var adrB in depsOfA)
            {
                // 检查 B 是否也依赖 A
                if (dependencies.TryGetValue(adrB, out var depsOfB) && depsOfB.Contains(adrA))
                {
                    // 发现循环依赖 A→B 且 B→A
                    var cycle = $"ADR-{adrA} ↔ ADR-{adrB}";
                    if (!violations.Contains(cycle))
                    {
                        violations.Add(cycle);
                    }
                }
            }
        }

        violations.Should().BeEmpty(
            $"违反 ADR-947_3_1：检测到以下循环依赖声明：\n{string.Join("\n", violations)}\n" +
            "建议：使用单向声明 + 相关关系（Related）来表示双向关联。");
    }

    // ========== 辅助方法 ==========


    private static string ExtractAdrNumber(string fileName)
    {
        var match = Regex.Match(fileName, @"ADR-(\d{3,4})");
        return match.Success ? match.Groups[1].Value : string.Empty;
    }

    private static string ExtractRelationshipsSection(string content)
    {
        var pattern = @"##\s*(Relationships|关系声明).*?\n(.*?)(?=\n##\s|\z)";
        var match = Regex.Match(content, pattern, RegexOptions.Singleline | RegexOptions.IgnoreCase);

        return match.Success ? match.Groups[2].Value : string.Empty;
    }

    private static List<string> ExtractAdrNumbers(string content)
    {
        var pattern = @"ADR-(\d{3,4})";
        var matches = Regex.Matches(content, pattern);

        return matches.Select(m => m.Groups[1].Value).Distinct().ToList();
    }
}
