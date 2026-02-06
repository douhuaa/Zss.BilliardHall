namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_004;

/// <summary>
/// ADR-004_2: 项目依赖管理约束（Rule）
/// 验证项目文件不手动指定包版本，所有包版本集中管理
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-004_2_1: 项目文件禁止手动指定包版本
/// - ADR-004_2_2: 所有使用的包必须在 CPM 中定义
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-004-Cpm-Final.md
/// </summary>
public sealed class ADR_004_2_Architecture_Tests
{
    /// <summary>
    /// ADR-004_2_1: 项目文件禁止手动指定包版本
    /// 验证项目文件中的 PackageReference 不得包含 Version 属性（§ADR-004_2_1）
    /// </summary>
    [Fact(DisplayName = "ADR-004_2_1: 项目文件不应手动指定包版本")]
    public void ADR_004_2_1_Projects_Should_Not_Specify_Package_Versions()
    {
        var root = TestEnvironment.RepositoryRoot;
        var projectFiles = Directory
            .GetFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(p => !p.Contains("/obj/") && !p.Contains("/bin/"))
            .ToList();

        var violations = new List<string>();

        foreach (var projectFile in projectFiles)
        {
            var doc = new XmlDocument();
            try
            {
                doc.Load(projectFile);
            }
            catch
            {
                continue;
            }

            var mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("msb", doc.DocumentElement!.NamespaceURI);

            // 检查 PackageReference 是否有 Version 属性
            var packageRefsWithVersion = doc.SelectNodes("//msb:PackageReference[@Version]", mgr);
            if (packageRefsWithVersion != null && packageRefsWithVersion.Count > 0)
            {
                foreach (XmlNode node in packageRefsWithVersion)
                {
                    var packageName = node.Attributes?["Include"]?.Value ?? "未知包";
                    var version = node.Attributes?["Version"]?.Value ?? "未知版本";
                    violations.Add($"  • {Path.GetFileName(projectFile)}: {packageName} (Version={version})");
                }
            }
        }

        var message = AssertionMessageBuilder.BuildWithViolations(
            ruleId: "ADR-004_2_1",
            summary: $"发现 {violations.Count} 个项目手动指定了包版本，应使用 CPM 统一管理",
            failingTypes: violations,
            remediationSteps: new[]
            {
                "在 Directory.Packages.props 中定义包版本: <PackageVersion Include=\"包名\" Version=\"版本号\" />",
                "从项目文件中移除 Version 属性，只保留 <PackageReference Include=\"包名\" />",
                "运行 dotnet restore 和 dotnet build 验证配置正确"
            },
            adrReference: "docs/adr/constitutional/ADR-004-Cpm-Final.md");
        (violations.Count == 0).Should().BeTrue(message);
    }

    /// <summary>
    /// ADR-004_2_2: 所有使用的包必须在 CPM 中定义
    /// 验证所有项目引用的包都在 Directory.Packages.props 中有定义（§ADR-004_2_2）
    /// </summary>
    [Fact(DisplayName = "ADR-004_2_2: Directory.Packages.props 应定义所有项目使用的包")]
    public void ADR_004_2_2_Directory_Packages_Props_Should_Define_All_Used_Packages()
    {
        var root = TestEnvironment.RepositoryRoot;
        var cpmFile = Path.Combine(root, "Directory.Packages.props");

        if (!File.Exists(cpmFile)) return;

        // 加载 CPM 中定义的包
        var doc = new XmlDocument();
        doc.Load(cpmFile);
        var packageVersions = doc.SelectNodes("//PackageVersion[@Include]");
        var cpmPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (packageVersions != null)
        {
            foreach (XmlNode packageVersion in packageVersions)
            {
                var packageName = packageVersion.Attributes?["Include"]?.Value;
                if (!string.IsNullOrEmpty(packageName))
                {
                    cpmPackages.Add(packageName);
                }
            }
        }

        // 检查所有项目引用的包
        var projectFiles = Directory
            .GetFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(p => !p.Contains("/obj/") && !p.Contains("/bin/"))
            .ToList();

        var missingPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var projectFile in projectFiles)
        {
            var projectDoc = new XmlDocument();
            try
            {
                projectDoc.Load(projectFile);
            }
            catch
            {
                continue;
            }

            var mgr = new XmlNamespaceManager(projectDoc.NameTable);
            mgr.AddNamespace("msb", projectDoc.DocumentElement!.NamespaceURI);

            var packageRefs = projectDoc.SelectNodes("//msb:PackageReference[@Include]", mgr);
            if (packageRefs == null) continue;

            foreach (XmlNode packageRef in packageRefs)
            {
                var packageName = packageRef.Attributes?["Include"]?.Value;
                if (string.IsNullOrEmpty(packageName)) continue;

                // 如果包在项目中引用但不在 CPM 中定义
                if (!cpmPackages.Contains(packageName))
                {
                    missingPackages.Add(packageName);
                }
            }
        }

        var message = AssertionMessageBuilder.Build(
            ruleId: "ADR-004_2_2",
            summary: $"发现 {missingPackages.Count} 个包在项目中使用但未在 Directory.Packages.props 中定义",
            currentState: $"缺失的包: {string.Join(", ", missingPackages)}",
            remediationSteps: new[]
            {
                "在 Directory.Packages.props 中为每个缺失的包添加版本定义",
                "使用格式: <PackageVersion Include=\"包名\" Version=\"版本号\" />",
                "将包添加到合适的分组（使用 Label 属性）"
            },
            adrReference: "docs/adr/constitutional/ADR-004-Cpm-Final.md");
        (missingPackages.Count == 0).Should().BeTrue(message);
    }
}
