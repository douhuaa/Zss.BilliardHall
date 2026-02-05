namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR;

/// <summary>
/// ADR-121: 契约（Contract）与 DTO 命名组织规范
/// 验证契约/DTO 命名、命名空间组织、版本管理和模块隔离约束
///
/// 约束映射（对应 ADR-121 快速参考表）：
/// ┌──────────────┬─────────────────────────────────────────────────────────┬─────────┬───────────────────────────────────────────────────────┐
/// │ 约束编号     │ 描述                                                     │ 层级    │ 测试方法                                               │
/// ├──────────────┼─────────────────────────────────────────────────────────┼─────────┼───────────────────────────────────────────────────────┤
/// │ ADR-121.1    │ 跨模块契约类型必须以 `Dto` 或 `Contract` 结尾              │ L1      │ Contract_Types_Should_End_With_Dto_Or_Contract_Suffix │
/// │ ADR-121.2    │ 契约属性必须是只读的（record 或 init-only）                │ L1      │ Contracts_Should_Be_Immutable                         │
/// │ ADR-121.3    │ 契约不得包含业务方法                                      │ L1      │ Contracts_Should_Not_Contain_Business_Methods         │
/// │ ADR-121.4    │ 契约不得包含领域模型类型                                  │ L1      │ Contracts_Should_Not_Contain_Domain_Types             │
/// │ ADR-121.5    │ 契约必须位于 Contracts 命名空间下                         │ L1      │ Contracts_Should_Be_In_Contracts_Namespace            │
/// │ ADR-121.6    │ 契约命名空间必须与物理目录一致                            │ L2      │ Contract_Namespace_Should_Match_Directory             │
/// └──────────────┴─────────────────────────────────────────────────────────┴─────────┴───────────────────────────────────────────────────────┘
/// </summary>
public sealed class ADR_121_Architecture_Tests
{
    #region 1. 契约命名规则 (ADR-121.1)

    [Fact(DisplayName = "ADR-121_1_1: 位于 Contracts 命名空间的类型必须以 Dto 或 Contract 结尾")]
    public void Contract_Types_Should_End_With_Dto_Or_Contract_Suffix()
    {
        // 获取所有程序集（包括模块和 Host）
        var allAssemblies = ModuleAssemblyData.ModuleAssemblies
            .Concat(HostAssemblyData.HostAssemblies)
            .Concat(new[] { Assembly.Load("Platform"), Assembly.Load("Application") })
            .ToList();

        // 查找所有在 Contracts 命名空间的公共类型（排除接口和抽象类）
        var contractTypes = Types
            .InAssemblies(allAssemblies)
            .That()
            .ResideInNamespaceContaining(".Contracts")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes()
            .Where(t => !t.Name.StartsWith("<")) // 排除编译器生成的类型
            .ToList();

        foreach (var contractType in contractTypes)
        {
            var hasValidSuffix = contractType.Name.EndsWith("Dto") ||
                                 contractType.Name.EndsWith("Contract");

            var message = BuildWithAnalysis(
                ruleId: "ADR-121_1_1",
                summary: "契约类型缺少 'Dto' 或 'Contract' 后缀",
                currentState: $"违规类型: {contractType.FullName}",
                problemAnalysis: "所有位于 Contracts 命名空间的类型必须以 'Dto' 或 'Contract' 后缀结尾，以明确标识其为数据传输对象（契约）",
                remediationSteps: new[]
                {
                    $"如果是信息摘要，使用 'InfoDto' 或 'SummaryDto' 后缀：",
                    $"   - {contractType.Name}InfoDto",
                    $"   - {contractType.Name}SummaryDto",
                    $"如果是详细信息，使用 'DetailContract' 或 'Dto' 后缀：",
                    $"   - {contractType.Name}DetailContract",
                    $"   - {contractType.Name}Dto",
                    $"如果是列表项，使用 'ListDto' 后缀：",
                    $"   - {contractType.Name}ListDto"
                },
                adrReference: "docs/adr/structure/ADR-121-contract-dto-naming-organization.md");

            hasValidSuffix.Should().BeTrue(message);
        }
    }

    #endregion

    #region 2. 契约不可变性 (ADR-121.2)

    [Fact(DisplayName = "ADR-121_2_1: 契约属性必须是只读的")]
    public void Contracts_Should_Be_Immutable()
    {
        var allAssemblies = ModuleAssemblyData.ModuleAssemblies
            .Concat(HostAssemblyData.HostAssemblies)
            .Concat(new[] { Assembly.Load("Platform"), Assembly.Load("Application") })
            .ToList();

        var contractTypes = Types
            .InAssemblies(allAssemblies)
            .That()
            .ResideInNamespaceContaining(".Contracts")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes()
            .Where(t => t.Name.EndsWith("Dto") || t.Name.EndsWith("Contract"))
            .ToList();

        foreach (var contractType in contractTypes)
        {
            var properties = contractType.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                // 检查属性是否有 set 访问器
                var hasSetter = property.SetMethod != null && property.SetMethod.IsPublic;

                // 检查属性是否是 init-only（C# 9.0+）
                var isInitOnly = property.SetMethod != null &&
                                 property.SetMethod.ReturnParameter
                                     .GetRequiredCustomModifiers()
                                     .Any(m => m.Name == "IsExternalInit");

                // 属性必须是只读的（没有 setter）或者是 init-only
                var isImmutable = !hasSetter || isInitOnly;

                var message = BuildWithAnalysis(
                    ruleId: "ADR-121_2_1",
                    summary: "契约属性必须是只读的",
                    currentState: $"违规类型: {contractType.FullName}\n可变属性: {property.Name}",
                    problemAnalysis: $"契约必须是不可变的，以确保数据传输的安全性和一致性。属性 '{property.Name}' 具有公共 set 访问器，违反了不可变性约束。",
                    remediationSteps: new[]
                    {
                        $"推荐使用 record 类型（自动不可变）：",
                        $"   public record {contractType.Name}(/* 属性 */);",
                        $"或使用 init-only 属性：",
                        $"   public class {contractType.Name}",
                        $"   {{",
                        $"       public required {property.PropertyType.Name} {property.Name} {{ get; init; }}",
                        $"   }}",
                        $"或使用只读属性：",
                        $"   public {property.PropertyType.Name} {property.Name} {{ get; }}"
                    },
                    adrReference: "docs/adr/structure/ADR-121-contract-dto-naming-organization.md");

                isImmutable.Should().BeTrue(message);
            }
        }
    }

    #endregion

    #region 3. 契约无业务方法 (ADR-121.3)

    [Fact(DisplayName = "ADR-121_3_1: 契约不得包含业务方法")]
    public void Contracts_Should_Not_Contain_Business_Methods()
    {
        var allAssemblies = ModuleAssemblyData.ModuleAssemblies
            .Concat(HostAssemblyData.HostAssemblies)
            .Concat(new[] { Assembly.Load("Platform"), Assembly.Load("Application") })
            .ToList();

        var contractTypes = Types
            .InAssemblies(allAssemblies)
            .That()
            .ResideInNamespaceContaining(".Contracts")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes()
            .Where(t => t.Name.EndsWith("Dto") || t.Name.EndsWith("Contract"))
            .ToList();

        foreach (var contractType in contractTypes)
        {
            // 获取所有公共实例方法（排除属性的 get/set 和 Object 继承的方法）
            var methods = contractType
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName) // 排除属性访问器和事件访问器
                .ToList();

            // Record 类型会自动生成一些方法（如 Equals, GetHashCode, ToString, PrintMembers），需要排除
            var businessMethods = methods
                .Where(m => m.Name != "Equals")
                .Where(m => m.Name != "GetHashCode")
                .Where(m => m.Name != "ToString")
                .Where(m => m.Name != "PrintMembers")
                .Where(m => m.Name != "Deconstruct")
                .Where(m => !m.Name.StartsWith("<")) // 排除编译器生成的方法
                .Where(m => !m.Name.StartsWith("get_")) // 排除属性 getter
                .Where(m => !m.Name.StartsWith("set_")) // 排除属性 setter
                .ToList();

            // 允许的计算属性（只读属性，基于现有数据计算，无参数）
            var computedProperties = contractType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetMethod != null && p.SetMethod == null)
                .Select(p => p.GetMethod)
                .ToList();

            // 从业务方法中排除计算属性的 getter
            businessMethods = businessMethods
                .Where(m => !computedProperties.Contains(m))
                .ToList();

            if (businessMethods.Any())
            {
                var message = BuildWithAnalysis(
                    ruleId: "ADR-121_3_1",
                    summary: "契约包含业务方法",
                    currentState: $"违规类型: {contractType.FullName}\n业务方法: {string.Join(", ", businessMethods.Select(m => m.Name))}",
                    problemAnalysis: "契约应该是纯数据对象（Data Object），不应包含业务逻辑或判断方法。检测到的方法可能包含业务逻辑，违反了契约的纯数据约束。",
                    remediationSteps: new[]
                    {
                        "移除契约中的所有业务方法，只保留属性",
                        "允许的例外：计算属性（基于现有数据的只读属性）：",
                        "   public decimal TotalAmount => Items.Sum(i => i.Price); // ✅ 允许",
                        "业务逻辑应该在以下位置实现：",
                        "   - 判断逻辑 → 领域模型方法",
                        "   - 协调逻辑 → Handler",
                        "   - 验证逻辑 → Validator"
                    },
                    adrReference: "docs/adr/structure/ADR-121-contract-dto-naming-organization.md");

                true.Should().BeFalse(message);
            }
        }
    }

    #endregion

    #region 4. 契约不包含领域模型类型 (ADR-121.4)

    [Fact(DisplayName = "ADR-121_4_1: 契约不得包含领域模型类型")]
    public void Contracts_Should_Not_Contain_Domain_Types()
    {
        var allAssemblies = ModuleAssemblyData.ModuleAssemblies
            .Concat(HostAssemblyData.HostAssemblies)
            .Concat(new[] { Assembly.Load("Platform"), Assembly.Load("Application") })
            .ToList();

        var contractTypes = Types
            .InAssemblies(allAssemblies)
            .That()
            .ResideInNamespaceContaining(".Contracts")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes()
            .Where(t => t.Name.EndsWith("Dto") || t.Name.EndsWith("Contract"))
            .ToList();

        foreach (var contractType in contractTypes)
        {
            // 检查构造函数参数和公共属性
            var properties = contractType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var ctorParams = contractType
                .GetConstructors()
                .SelectMany(c => c.GetParameters())
                .Select(p => p.ParameterType)
                .Distinct()
                .ToList();

            var propertyTypes = properties
                .Select(p => p.PropertyType)
                .Distinct()
                .ToList();

            var allTypes = ctorParams
                .Concat(propertyTypes)
                .Distinct()
                .ToList();

            foreach (var type in allTypes)
            {
                var actualType = type;

                // 提取泛型参数（如 List<T> 中的 T）
                if (actualType.IsGenericType && !actualType.IsGenericTypeDefinition)
                {
                    var genericArgs = actualType.GetGenericArguments();
                    foreach (var arg in genericArgs)
                    {
                        CheckDomainType(arg, contractType);
                    }
                }
                else
                {
                    CheckDomainType(actualType, contractType);
                }
            }
        }

        static void CheckDomainType(Type type, Type contractType)
        {
            // 跳过系统类型和接口
            if (type.IsPrimitive ||
                type == typeof(string) ||
                type == typeof(decimal) ||
                type == typeof(DateTime) ||
                type == typeof(DateTimeOffset) ||
                type == typeof(TimeSpan) ||
                type == typeof(Guid) ||
                type.IsInterface ||
                type.IsValueType && type.Namespace?.StartsWith("System") == true)
            {
                return;
            }

            // 检查是否为领域模型类型
            if (type.Namespace != null)
            {
                var isDomainType = type.Namespace.Contains(".Domain") ||
                                   type.Namespace.Contains(".Entities") ||
                                   type.Name.EndsWith("Entity") ||
                                   type.Name.EndsWith("Aggregate") ||
                                   type.Name.EndsWith("ValueObject") ||
                                   type.Name.EndsWith("AggregateRoot");

                // 排除其他契约类型（允许契约嵌套）
                var isContractType = type.Name.EndsWith("Dto") || type.Name.EndsWith("Contract");

                if (isDomainType && !isContractType)
                {
                    var message = BuildWithAnalysis(
                        ruleId: "ADR-121_4_1",
                        summary: "契约包含领域模型类型",
                        currentState: $"违规契约: {contractType.FullName}\n领域模型类型: {type.FullName}",
                        problemAnalysis: "契约不得包含领域实体（Entity/Aggregate/ValueObject），只能包含原始类型、系统类型和其他契约类型（DTO）。",
                        remediationSteps: new[]
                        {
                            "将领域实体转换为简单 DTO：",
                            $"   - 创建对应的 DTO：{type.Name}Dto",
                            "   - 只包含需要传输的属性",
                            "或使用原始类型：",
                            $"   - Guid {type.Name}Id（而非 {type.Name} 对象）",
                            $"   - string {type.Name}Name（而非 {type.Name} 对象）",
                            "契约应该是数据快照，不包含领域行为"
                        },
                        adrReference: "docs/adr/structure/ADR-121-contract-dto-naming-organization.md");

                    true.Should().BeFalse(message);
                }
            }
        }
    }

    #endregion

    #region 5. 契约命名空间规范 (ADR-121.5)

    [Fact(DisplayName = "ADR-121_5_1: 契约必须位于 Contracts 命名空间下")]
    public void Contracts_Should_Be_In_Contracts_Namespace()
    {
        var allAssemblies = ModuleAssemblyData.ModuleAssemblies
            .Concat(HostAssemblyData.HostAssemblies)
            .Concat(new[] { Assembly.Load("Platform"), Assembly.Load("Application") })
            .ToList();

        // 查找所有以 Dto 或 Contract 结尾的类型
        var dtoTypes = Types
            .InAssemblies(allAssemblies)
            .That()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .GetTypes()
            .Where(t => t.Name.EndsWith("Dto") || t.Name.EndsWith("Contract"))
            .Where(t => t.Namespace != null)
            .Where(t => !t.Name.StartsWith("<")) // 排除编译器生成的类型
            .ToList();

        foreach (var dtoType in dtoTypes)
        {
            var ns = dtoType.Namespace!;

            // 允许的命名空间模式：
            // 1. Platform.Contracts.*
            // 2. Modules.{ModuleName}.Contracts.*
            // 3. Modules.{ModuleName}.Features.* (模块内 DTO，用于查询返回)
            var isInContractsNamespace = ns.Contains(".Contracts");
            var isInFeaturesNamespace = ns.Contains(".Features");

            // 如果是跨模块契约（在 Contracts 目录），必须包含 .Contracts
            // 如果是模块内 DTO（在 Features 目录），允许不包含 .Contracts
            var isValidNamespace = isInContractsNamespace || isInFeaturesNamespace;

            // 特别检查：如果类型名明确表示是跨模块契约（如 MemberInfoDto、OrderDetailContract），
            // 但不在 Contracts 命名空间，则违规
            var looksLikePublicContract = dtoType.IsPublic &&
                                          !dtoType.IsNested &&
                                          (dtoType.Name.Contains("Info") ||
                                           dtoType.Name.Contains("Detail") ||
                                           dtoType.Name.Contains("Summary") ||
                                           dtoType.Name.EndsWith("Contract"));

            if (looksLikePublicContract && !isInContractsNamespace)
            {
                var message = BuildWithAnalysis(
                    ruleId: "ADR-121_5_1",
                    summary: "公共契约未在 Contracts 命名空间下",
                    currentState: $"违规类型: {dtoType.FullName}\n当前命名空间: {ns}\n期望命名空间: Zss.BilliardHall.Platform.Contracts.* 或 Zss.BilliardHall.Modules.{{ModuleName}}.Contracts.*",
                    problemAnalysis: "该类型看起来是公共契约（名称包含 Info/Detail/Summary 或以 Contract 结尾），但未位于 Contracts 命名空间下，违反了契约组织规范。",
                    remediationSteps: new[]
                    {
                        "如果是跨模块契约，移动到 Platform.Contracts：",
                        "   namespace Zss.BilliardHall.Platform.Contracts.{ModuleName};",
                        "如果是模块内契约，移动到模块的 Contracts 目录：",
                        "   namespace Zss.BilliardHall.Modules.{ModuleName}.Contracts;",
                        "如果是模块内部使用的 DTO，可以保持在 Features 下，",
                        "   但应避免使用 Info/Detail/Summary/Contract 等公共契约命名模式"
                    },
                    adrReference: "docs/adr/structure/ADR-121-contract-dto-naming-organization.md");

                true.Should().BeFalse(message);
            }
        }
    }

    #endregion

    #region 6. 契约命名空间与目录一致性 (ADR-121.6 - L2 可选)

    // 注意：此测试为 L2 级别（建议约束），暂时注释，可根据项目需要启用
    // 原因：物理目录结构验证需要文件系统访问，可能在某些 CI 环境中不可用

    /*
    [Fact(DisplayName = "ADR-121_6_1: 契约命名空间应与物理目录一致 (L2)")]
    public void Contract_Namespace_Should_Match_Directory()
    {
        // 此测试需要文件系统访问权限
        // 建议在本地开发环境或特定 CI 流水线中启用
        // 实现思路：
        // 1. 遍历所有契约类型
        // 2. 获取其源文件路径
        // 3. 验证命名空间是否与目录路径匹配
    }
    */

    #endregion
}
