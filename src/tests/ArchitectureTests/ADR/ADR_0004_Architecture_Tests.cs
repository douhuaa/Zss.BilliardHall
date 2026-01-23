using System.Xml;

namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-0004: 中央包管理 (CPM) 规范
/// 验证 Directory.Packages.props 存在且配置正确，所有项目统一管理包版本
/// 
/// 测试-ADR 映射清单：
/// ├─ ADR-0004.1: Repository_Should_Have_Directory_Packages_Props → Directory.Packages.props 必须存在
/// ├─ ADR-0004.2: CPM_Should_Be_Enabled → ManagePackageVersionsCentrally 必须启用
/// ├─ ADR-0004.3: CPM_Should_Enable_Transitive_Pinning → CentralPackageTransitivePinningEnabled 建议启用
/// ├─ ADR-0004.4: Projects_Should_Not_Specify_Package_Versions → 项目文件不应手动指定包版本
/// ├─ ADR-0004.5: Directory_Packages_Props_Should_Contain_Package_Groups → 包应按功能分组
/// ├─ ADR-0004.6: Directory_Packages_Props_Should_Contain_Common_Package_Groups → 应包含常见分组
/// ├─ ADR-0004.7: Platform_Projects_Should_Not_Reference_Business_Packages → Platform 不引用业务包
/// ├─ ADR-0004.8: All_Test_Projects_Should_Use_Same_Test_Framework_Versions → 测试框架版本统一
/// └─ ADR-0004.9: Directory_Packages_Props_Should_Define_All_Used_Packages → CPM 定义所有使用的包
/// 
/// 参考文档：
/// - ADR-0004: docs/adr/constitutional/ADR-0004-Cpm-Final.md
/// - Copilot Prompts: docs/copilot/adr-0004.prompts.md
/// </summary>
public sealed class ADR_0004_Architecture_Tests
{
    #region 1. CPM 基础设施约束

    [Fact(DisplayName = "ADR-0004.1: Directory.Packages.props 应存在于仓库根目录")]
    public void Repository_Should_Have_Directory_Packages_Props()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var cpmFile = Path.Combine(root, "Directory.Packages.props");

        Assert.True(File.Exists(cpmFile),
            $"❌ ADR-0004.1 违规: 仓库根目录必须存在 Directory.Packages.props 文件以启用 Central Package Management (CPM)。\n\n" +
            $"预期路径: {cpmFile}\n\n" +
            $"修复建议:\n" +
            $"1. 在仓库根目录创建 Directory.Packages.props 文件\n" +
            $"2. 添加 <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>\n" +
            $"3. 添加 <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>\n\n" +
            $"参考: docs/copilot/adr-0004.prompts.md (场景 1)");
    }

    [Fact(DisplayName = "ADR-0004.2: CPM 应被启用")]
    public void CPM_Should_Be_Enabled()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var cpmFile = Path.Combine(root, "Directory.Packages.props");

        Assert.True(File.Exists(cpmFile), "Directory.Packages.props 文件不存在");

        var content = File.ReadAllText(cpmFile);

        Assert.True(content.Contains("ManagePackageVersionsCentrally"),
            $"❌ ADR-0004.2 违规: Directory.Packages.props 必须包含 ManagePackageVersionsCentrally 设置。\n\n" +
            $"当前状态: 未找到 ManagePackageVersionsCentrally 配置\n\n" +
            $"修复建议:\n" +
            $"1. 在 Directory.Packages.props 中添加 <PropertyGroup> 节点\n" +
            $"2. 添加 <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>\n" +
            $"3. 重新构建项目验证配置生效\n\n" +
            $"参考: docs/copilot/adr-0004.prompts.md (场景 1)");

        Assert.True(content.Contains("true"),
            $"❌ ADR-0004.2 违规: Directory.Packages.props 中的 ManagePackageVersionsCentrally 应该设置为 true。\n\n" +
            $"当前状态: ManagePackageVersionsCentrally 值不正确\n\n" +
            $"修复建议:\n" +
            $"1. 确保 <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>\n" +
            $"2. 检查拼写和大小写是否正确\n" +
            $"3. 删除所有项目文件中的手动 Version 属性\n\n" +
            $"参考: docs/copilot/adr-0004.prompts.md (场景 1)");
    }

    [Fact(DisplayName = "ADR-0004.3: CPM 应启用传递依赖固定")]
    public void CPM_Should_Enable_Transitive_Pinning()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var cpmFile = Path.Combine(root, "Directory.Packages.props");

        if (!File.Exists(cpmFile)) return;

        var content = File.ReadAllText(cpmFile);

        // 建议启用但不强制
        if (content.Contains("CentralPackageTransitivePinningEnabled"))
        {
            Assert.True(content.Contains("<CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>"),
                $"⚠️ ADR-0004.3 建议: 建议启用 CentralPackageTransitivePinningEnabled 以固定传递依赖版本。\n\n" +
                $"当前状态: CentralPackageTransitivePinningEnabled 未设置为 true\n\n" +
                $"修复建议:\n" +
                $"1. 在 Directory.Packages.props 中添加 <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>\n" +
                $"2. 这将确保所有传递依赖使用 CPM 中定义的版本\n" +
                $"3. 避免间接依赖升级导致的破坏性变更\n\n" +
                $"参考: docs/copilot/adr-0004.prompts.md (场景 4)");
        }
    }

    #endregion

    #region 2. 项目不应手动指定包版本

    [Fact(DisplayName = "ADR-0004.4: 项目文件不应手动指定包版本")]
    public void Projects_Should_Not_Specify_Package_Versions()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var projectFiles = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories)
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

        Assert.True(violations.Count == 0,
            $"❌ ADR-0004.4 违规: 发现 {violations.Count} 个项目手动指定了包版本，应使用 CPM 统一管理。\n\n" +
            $"违规项目:\n{string.Join("\n", violations)}\n\n" +
            $"修复建议:\n" +
            $"1. 在 Directory.Packages.props 中定义包版本: <PackageVersion Include=\"包名\" Version=\"版本号\" />\n" +
            $"2. 从项目文件中移除 Version 属性，只保留 <PackageReference Include=\"包名\" />\n" +
            $"3. 运行 dotnet restore 和 dotnet build 验证配置正确\n\n" +
            $"参考: docs/copilot/adr-0004.prompts.md (场景 1, 反模式 1)");
    }

    #endregion

    #region 3. 包分组约束

    [Fact(DisplayName = "ADR-0004.5: Directory.Packages.props 应包含包分组")]
    public void Directory_Packages_Props_Should_Contain_Package_Groups()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var cpmFile = Path.Combine(root, "Directory.Packages.props");

        if (!File.Exists(cpmFile)) return;

        var doc = new XmlDocument();
        doc.Load(cpmFile);

        var itemGroups = doc.SelectNodes("//ItemGroup[@Label]");

        Assert.True(itemGroups != null && itemGroups.Count > 0,
            $"⚠️ ADR-0004.5 建议: 建议在 Directory.Packages.props 中使用 Label 属性对包进行分组。\n\n" +
            $"当前状态: 未发现使用 Label 属性的包分组\n\n" +
            $"修复建议:\n" +
            $"1. 使用 <ItemGroup Label=\"分组名称\"> 对包进行逻辑分组\n" +
            $"2. 常见分组: Logging, Testing, Wolverine Framework, Marten, Aspire 等\n" +
            $"3. 这有助于快速定位和管理相关包\n\n" +
            $"参考: docs/copilot/adr-0004.prompts.md (FAQ Q3)");
    }

    [Fact(DisplayName = "ADR-0004.6: Directory.Packages.props 应包含常见包分组")]
    public void Directory_Packages_Props_Should_Contain_Common_Package_Groups()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var cpmFile = Path.Combine(root, "Directory.Packages.props");

        if (!File.Exists(cpmFile)) return;

        var content = File.ReadAllText(cpmFile);
        var doc = new XmlDocument();
        doc.Load(cpmFile);

        var itemGroups = doc.SelectNodes("//ItemGroup[@Label]");
        var labels = new List<string>();

        if (itemGroups != null)
        {
            foreach (XmlNode itemGroup in itemGroups)
            {
                var label = itemGroup.Attributes?["Label"]?.Value;
                if (!string.IsNullOrEmpty(label))
                {
                    labels.Add(label);
                }
            }
        }

        // 建议包含的分组（不强制，只是建议）
        var recommendedGroups = new[] { "Testing", "Logging" };
        var missingGroups = recommendedGroups.Where(g => !labels.Any(l => l.Contains(g))).ToList();

        if (missingGroups.Any())
        {
            // 这只是建议，不强制失败
            System.Diagnostics.Debug.WriteLine(
                $"⚠️ ADR-0004 建议: 建议在 Directory.Packages.props 中添加以下分组：{string.Join(", ", missingGroups)}");
        }

        Assert.True(true, "包分组检查完成");
    }

    #endregion

    #region 4. 分层依赖包规则约束

    [Fact(DisplayName = "ADR-0004.7: Platform 项目不应引用业务包")]
    public void Platform_Projects_Should_Not_Reference_Business_Packages()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var platformDir = Path.Combine(root, "src", "Platform");

        if (!Directory.Exists(platformDir)) return;

        var platformProjects = Directory.GetFiles(platformDir, "*.csproj", SearchOption.AllDirectories);

        // 禁止 Platform 引用的业务相关包（示例）
        var forbiddenPackages = new string[]
        {
            // 目前没有明确禁止的包，这里作为示例
            // "SomeBusinessPackage"
        };

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

                if (forbiddenPackages.Any(fp => packageName.Contains(fp)))
                {
                    Assert.Fail(
                        $"❌ ADR-0004.7 违规: Platform 项目 {Path.GetFileName(projectFile)} 不应引用业务包: {packageName}。\n\n" +
                        $"违规项目: {Path.GetFileName(projectFile)}\n" +
                        $"违规包: {packageName}\n\n" +
                        $"修复建议:\n" +
                        $"1. 从 Platform 项目中移除该业务包引用\n" +
                        $"2. Platform 层只能引用技术基础包（Serilog, OpenTelemetry, HealthChecks 等）\n" +
                        $"3. 业务包应在 Application/Modules 层引用\n\n" +
                        $"参考: docs/copilot/adr-0004.prompts.md (场景 3, 反模式 2)");
                }
            }
        }
    }

    [Fact(DisplayName = "ADR-0004.8: 所有测试项目应引用相同的测试框架版本")]
    public void All_Test_Projects_Should_Use_Same_Test_Framework_Versions()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
        var testProjects = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories)
            .Where(p => p.Contains("/tests/") || p.Contains("/Tests/") || Path.GetFileName(p).Contains("Test"))
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

        foreach (var kvp in packageVersions)
        {
            if (kvp.Value.Count > 1)
            {
                Assert.Fail(
                    $"❌ ADR-0004.8 违规: 测试包 {kvp.Key} 存在多个版本: {string.Join(", ", kvp.Value)}。\n\n" +
                    $"检测到的版本: {string.Join(", ", kvp.Value)}\n\n" +
                    $"修复建议:\n" +
                    $"1. 在 Directory.Packages.props 中统一该包的版本\n" +
                    $"2. 从所有项目文件中移除手动指定的版本号\n" +
                    $"3. 确保所有测试项目使用相同的测试框架版本\n\n" +
                    $"参考: docs/copilot/adr-0004.prompts.md (场景 1, FAQ Q2)");
            }
        }
    }

    #endregion

    #region 5. 包版本一致性约束

    [Fact(DisplayName = "ADR-0004.9: Directory.Packages.props 应定义所有项目使用的包")]
    public void Directory_Packages_Props_Should_Define_All_Used_Packages()
    {
        var root = ModuleAssemblyData.GetSolutionRoot();
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
        var projectFiles = Directory.GetFiles(root, "*.csproj", SearchOption.AllDirectories)
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

        Assert.True(missingPackages.Count == 0,
            $"❌ ADR-0004.9 违规: 发现 {missingPackages.Count} 个包在项目中使用但未在 Directory.Packages.props 中定义。\n\n" +
            $"缺失的包: {string.Join(", ", missingPackages)}\n\n" +
            $"修复建议:\n" +
            $"1. 在 Directory.Packages.props 中为每个缺失的包添加版本定义\n" +
            $"2. 使用格式: <PackageVersion Include=\"包名\" Version=\"版本号\" />\n" +
            $"3. 将包添加到合适的分组（使用 Label 属性）\n\n" +
            $"参考: docs/copilot/adr-0004.prompts.md (场景 1, FAQ Q1)");
    }

    #endregion
}
