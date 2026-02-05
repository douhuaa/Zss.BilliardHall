using System.Xml;
using FluentAssertions;
using Zss.BilliardHall.Tests.ArchitectureTests.Shared;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_004;

/// <summary>
/// ADR-004_3: 层级依赖与分组约束（Rule）
/// 验证包按功能分组、层级依赖规则、测试框架版本统一
///
/// 测试覆盖映射（严格遵循 ADR-907 v2.0 Rule/Clause 体系）：
/// - ADR-004_3_1: 包应按功能分组
/// - ADR-004_3_2: Platform 项目不引用业务包
/// - ADR-004_3_3: 测试框架版本统一
/// - ADR-004_3_4: 层级依赖规则
///
/// 关联文档：
/// - ADR: docs/adr/constitutional/ADR-004-Cpm-Final.md
/// </summary>
public sealed class ADR_004_3_Architecture_Tests
{
    /// <summary>
    /// ADR-004_3_1: 包应按功能分组
    /// 建议在 Directory.Packages.props 中使用 Label 属性对包进行逻辑分组（§ADR-004_3_1）
    /// 注意：此条款为建议性质（L2），不会阻断构建
    /// </summary>
    [Fact(DisplayName = "ADR-004_3_1: Directory.Packages.props 应包含包分组")]
    public void ADR_004_3_1_Directory_Packages_Props_Should_Contain_Package_Groups()
    {
        var root = TestEnvironment.RepositoryRoot;
        var cpmFile = Path.Combine(root, "Directory.Packages.props");

        if (!File.Exists(cpmFile)) return;

        var doc = new XmlDocument();
        doc.Load(cpmFile);

        var itemGroups = doc.SelectNodes("//ItemGroup[@Label]");

        (itemGroups != null && itemGroups.Count > 0).Should().BeTrue(
            $"⚠️ ADR-004_3_1 建议: 建议在 Directory.Packages.props 中使用 Label 属性对包进行分组。\n\n" +
            $"当前状态: 未发现使用 Label 属性的包分组\n\n" +
            $"修复建议：\n" +
            $"1. 使用 <ItemGroup Label=\"分组名称\"> 对包进行逻辑分组\n" +
            $"2. 常见分组: Logging, Testing, Wolverine Framework, Marten, Aspire 等\n" +
            $"3. 这有助于快速定位和管理相关包\n\n" +
            $"参考: docs/adr/constitutional/ADR-004-Cpm-Final.md §ADR-004_3_1");
    }

    /// <summary>
    /// ADR-004_3_2: Platform 项目不引用业务包
    /// 验证 Platform 层项目只能引用技术基础包（§ADR-004_3_2）
    /// </summary>
    [Fact(DisplayName = "ADR-004_3_2: Platform 项目不应引用业务包")]
    public void ADR_004_3_2_Platform_Projects_Should_Not_Reference_Business_Packages()
    {
        var root = TestEnvironment.RepositoryRoot;
        var platformDir = Path.Combine(root, "src", "Platform");

        if (!Directory.Exists(platformDir)) return;

        var platformProjects = Directory.GetFiles(platformDir, "*.csproj", SearchOption.AllDirectories);

        // 禁止 Platform 引用的业务相关包
        var forbiddenPackages = new string[] {
            "FluentValidation",
            "MediatR",
            "Wolverine",
            "Marten"
            // 可根据实际情况扩展
        };

        var violations = new List<string>();

        foreach (var projectFile in platformProjects)
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

            var packageRefs = doc.SelectNodes("//msb:PackageReference", mgr);
            if (packageRefs == null) continue;

            foreach (XmlNode packageRef in packageRefs)
            {
                var packageName = packageRef.Attributes?["Include"]?.Value;
                if (string.IsNullOrEmpty(packageName)) continue;

                if (forbiddenPackages.Any(fp => packageName.Contains(fp, StringComparison.OrdinalIgnoreCase)))
                {
                    violations.Add($"  • {Path.GetFileName(projectFile)}: {packageName}");
                }
            }
        }

        (violations.Count == 0).Should().BeTrue(
            $"❌ ADR-004_3_2 违规: Platform 项目不应引用业务包。\n\n" +
            $"违规项目:\n{string.Join("\n", violations)}\n\n" +
            $"修复建议：\n" +
            $"1. 从 Platform 项目中移除这些业务包引用\n" +
            $"2. Platform 层只能引用技术基础包（Serilog, OpenTelemetry, HealthChecks 等）\n" +
            $"3. 业务包应在 Application/Modules 层引用\n\n" +
            $"参考: docs/adr/constitutional/ADR-004-Cpm-Final.md §ADR-004_3_2");
    }

    /// <summary>
    /// ADR-004_3_3: 测试框架版本统一
    /// 验证所有测试项目使用相同版本的测试框架（§ADR-004_3_3）
    /// </summary>
    [Fact(DisplayName = "ADR-004_3_3: 所有测试项目应引用相同的测试框架版本")]
    public void ADR_004_3_3_All_Test_Projects_Should_Use_Same_Test_Framework_Versions()
    {
        var root = TestEnvironment.RepositoryRoot;
        var testProjects = Directory
            .GetFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(p => p.Contains("/tests/") ||
                        p.Contains("/Tests/") ||
                        Path.GetFileName(p).Contains("Test", StringComparison.OrdinalIgnoreCase))
            .Where(p => !p.Contains("/obj/") && !p.Contains("/bin/"))
            .ToList();

        if (testProjects.Count == 0) return;

        var testPackages = new[] { "xunit", "Microsoft.NET.Test.Sdk", "FluentAssertions" };
        var packageVersions = new Dictionary<string, HashSet<string>>();

        foreach (var projectFile in testProjects)
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

            var packageRefs = doc.SelectNodes("//msb:PackageReference", mgr);
            if (packageRefs == null) continue;

            foreach (XmlNode packageRef in packageRefs)
            {
                var packageName = packageRef.Attributes?["Include"]?.Value;
                var version = packageRef.Attributes?["Version"]?.Value;

                if (string.IsNullOrEmpty(packageName)) continue;
                if (!testPackages.Contains(packageName)) continue;

                if (!packageVersions.ContainsKey(packageName))
                {
                    packageVersions[packageName] = new HashSet<string>();
                }

                if (!string.IsNullOrEmpty(version))
                {
                    packageVersions[packageName].Add(version);
                }
            }
        }

        var violations = packageVersions.Where(kvp => kvp.Value.Count > 1).ToList();

        (violations.Count == 0).Should().BeTrue(
            $"❌ ADR-004_3_3 违规: 发现测试包存在多个版本。\n\n" +
            $"违规详情:\n{string.Join("\n", violations.Select(v => $"  • {v.Key}: {string.Join(", ", v.Value)}"))}\n\n" +
            $"修复建议：\n" +
            $"1. 在 Directory.Packages.props 中统一这些包的版本\n" +
            $"2. 从所有项目文件中移除手动指定的版本号\n" +
            $"3. 确保所有测试项目使用相同的测试框架版本\n\n" +
            $"参考: docs/adr/constitutional/ADR-004-Cpm-Final.md §ADR-004_3_3");
    }

    /// <summary>
    /// ADR-004_3_4: 层级依赖规则
    /// 验证各层级的依赖规则符合约束（§ADR-004_3_4）
    /// </summary>
    [Fact(DisplayName = "ADR-004_3_4: 层级依赖规则应符合约束")]
    public void ADR_004_3_4_Layered_Dependencies_Should_Follow_Rules()
    {
        // 此测试验证层级依赖的总体规则
        // 具体规则在 ADR-002 和 ADR-003 中有更详细的测试
        // 这里主要验证包管理角度的层级约束

        var root = TestEnvironment.RepositoryRoot;
        
        // 验证 Host 项目不直接依赖模块包
        var hostDir = Path.Combine(root, "src", "Host");
        if (Directory.Exists(hostDir))
        {
            var hostProjects = Directory.GetFiles(hostDir, "*.csproj", SearchOption.AllDirectories);
            
            foreach (var projectFile in hostProjects)
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

                var projectRefs = doc.SelectNodes("//msb:ProjectReference", mgr);
                if (projectRefs == null) continue;

                foreach (XmlNode projectRef in projectRefs)
                {
                    var include = projectRef.Attributes?["Include"]?.Value;
                    if (string.IsNullOrEmpty(include)) continue;

                    // Host 不应直接引用 Modules 目录下的项目
                    include.Contains("/Modules/", StringComparison.OrdinalIgnoreCase).Should().BeFalse(
                        $"❌ ADR-004_3_4 违规: Host 项目 {Path.GetFileName(projectFile)} 不应直接引用模块项目 {include}。\n\n" +
                        $"修复建议：\n" +
                        $"1. Host 应仅引用 Bootstrapper\n" +
                        $"2. 模块装配由 Application 层负责\n\n" +
                        $"参考: docs/adr/constitutional/ADR-004-Cpm-Final.md §ADR-004_3_4");
                }
            }
        }

        // 其他层级依赖验证在对应的 ADR 测试中进行
        true.Should().BeTrue("层级依赖规则验证通过");
    }
}
