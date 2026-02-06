namespace Zss.BilliardHall.Tests.ArchitectureTests.ADR_240;

/// <summary>
/// ADR-240: Handler 异常约束
/// 验证异常分类、可重试标记、命名空间约束
///
/// ADR 映射清单（ADR Mapping Checklist）：
/// ┌────────────────┬─────────────────────────────────────────────────────┬──────────┐
/// │ 测试方法         │ 对应 ADR 约束                                        │ RuleId   │
/// ├────────────────┼─────────────────────────────────────────────────────┼──────────┤
/// │ All_Custom_Exceptions_Should_Inherit_From_Structured_Exception_Base_Classes  │ Handler 仅抛出结构化异常     │ ADR-240_1_1 │
/// │ Retryable_Exceptions_Must_Be_Infrastructure_Exceptions                      │ 可重试异常必须是基础设施异常   │ ADR-240_2_1 │
/// │ Domain_Exceptions_Should_Not_Be_Retryable                                   │ 领域异常不可标记为可重试      │ ADR-240_2_1 │
/// │ Validation_Exceptions_Should_Not_Be_Retryable                               │ 验证异常不可标记为可重试      │ ADR-240_2_1 │
/// │ Exceptions_Should_Be_In_Correct_Namespaces                                  │ 异常类型必须在正确的命名空间  │ ADR-240_4_1 │
/// └────────────────┴─────────────────────────────────────────────────────┴──────────┘
///
/// 测试覆盖的 Rule：
/// - ADR-240_1：结构化异常要求（Rule）
/// - ADR-240_2：可重试标记约束（Rule）
/// - ADR-240_4：异常命名空间约束（Rule）
/// </summary>
public sealed class ADR_240_Architecture_Tests
{
    #region ADR-240_1: 结构化异常要求

    [Fact(DisplayName = "ADR-240_1_1: 所有自定义异常应继承自结构化异常基类")]
    public void All_Custom_Exceptions_Should_Inherit_From_Structured_Exception_Base_Classes()
    {
        // 获取所有项目程序集
        var assemblies = GetAllProjectAssemblies();

        var violations = new List<string>();

        foreach (var assembly in assemblies)
        {
            var customExceptions = Types
                .InAssembly(assembly)
                .That()
                .Inherit(typeof(Exception))
                .And()
                .DoNotInherit(typeof(DomainException))
                .And()
                .DoNotInherit(typeof(ValidationException))
                .And()
                .DoNotInherit(typeof(InfrastructureException))
                .And()
                .AreNotAbstract()
                .GetTypes()
                .Where(t => !t.FullName!.StartsWith("System"))
                .Where(t => !t.FullName!.StartsWith("Microsoft"))
                .Where(t => t.FullName!.Contains("Exception"))
                .ToList();

            foreach (var exception in customExceptions)
            {
                // 检查是否继承自三大基类之一
                var isDomainException = typeof(DomainException).IsAssignableFrom(exception);
                var isValidationException = typeof(ValidationException).IsAssignableFrom(exception);
                var isInfrastructureException = typeof(InfrastructureException).IsAssignableFrom(exception);

                if (!isDomainException && !isValidationException && !isInfrastructureException)
                {
                    violations.Add($"  • {exception.FullName}");
                }
            }
        }

        violations.Should().BeEmpty($"❌ ADR-240_1_1 违规: 存在未继承结构化异常基类的自定义异常\n\n" +
                        $"违规类型:\n{string.Join("\n", violations)}\n\n" +
                        $"问题分析:\n" +
                        $"自定义异常必须继承自以下三大基类之一：\n" +
                        $"1. DomainException - 业务规则违反\n" +
                        $"2. ValidationException - 输入数据验证失败\n" +
                        $"3. InfrastructureException - 技术依赖失败\n\n" +
                        $"修复建议：\n" +
                        $"1. 将异常类改为继承适当的基类\n" +
                        $"2. 例如：public class OrderCancelledException : DomainException\n" +
                        $"3. 确保异常分类符合业务语义\n\n" +
                        $"参考：docs/adr/runtime/ADR-240-handler-exception-constraints.md（§ADR-240_1_1）");
    }

    #endregion

    #region ADR-240_2: 可重试标记约束

    [Fact(DisplayName = "ADR-240_2_1: 可重试异常必须是基础设施异常")]
    public void Retryable_Exceptions_Must_Be_Infrastructure_Exceptions()
    {
        var assemblies = GetAllProjectAssemblies();

        var violations = new List<string>();

        foreach (var assembly in assemblies)
        {
            var retryableExceptions = Types
                .InAssembly(assembly)
                .That()
                .Inherit(typeof(Exception))
                .And()
                .ImplementInterface(typeof(IRetryable))
                .GetTypes();

            foreach (var exception in retryableExceptions)
            {
                // 检查是否继承自 InfrastructureException
                var isInfrastructureException = typeof(InfrastructureException).IsAssignableFrom(exception);

                if (!isInfrastructureException)
                {
                    violations.Add($"  • {exception.FullName}");
                }
            }
        }

        violations.Should().BeEmpty($"❌ ADR-240_2_1 违规: 可重试异常必须是基础设施异常\n\n" +
                        $"违规类型:\n{string.Join("\n", violations)}\n\n" +
                        $"问题分析:\n" +
                        $"实现 IRetryable 接口的异常必须继承自 InfrastructureException。\n" +
                        $"领域异常和验证异常不应该可重试，因为：\n" +
                        $"- 业务规则违反不会因重试而改变\n" +
                        $"- 输入错误不会因重试而修正\n\n" +
                        $"修复建议：\n" +
                        $"1. 将异常改为继承 InfrastructureException\n" +
                        $"2. 或移除 IRetryable 接口实现\n" +
                        $"3. 重新评估异常的业务语义分类\n\n" +
                        $"参考：docs/adr/runtime/ADR-240-handler-exception-constraints.md（§ADR-240_2_1）");
    }

    [Fact(DisplayName = "ADR-240_2_1: 领域异常不可标记为可重试")]
    public void Domain_Exceptions_Should_Not_Be_Retryable()
    {
        var assemblies = GetAllProjectAssemblies();

        var violations = new List<string>();

        foreach (var assembly in assemblies)
        {
            var domainExceptions = Types
                .InAssembly(assembly)
                .That()
                .Inherit(typeof(DomainException))
                .GetTypes();

            foreach (var exception in domainExceptions)
            {
                // 检查是否实现了 IRetryable
                var isRetryable = typeof(IRetryable).IsAssignableFrom(exception);

                if (isRetryable)
                {
                    violations.Add($"  • {exception.FullName}");
                }
            }
        }

        violations.Should().BeEmpty($"❌ ADR-240_2_1 违规: 领域异常不可标记为可重试\n\n" +
                        $"违规类型:\n{string.Join("\n", violations)}\n\n" +
                        $"问题分析:\n" +
                        $"领域异常表示业务规则违反，不会因重试而改变结果。\n" +
                        $"标记为可重试会导致无效的重试尝试和资源浪费。\n\n" +
                        $"修复建议：\n" +
                        $"1. 移除 IRetryable 接口实现\n" +
                        $"2. 如果是暂时性技术故障，应使用 InfrastructureException\n" +
                        $"3. 确保异常分类准确反映失败原因\n\n" +
                        $"参考：docs/adr/runtime/ADR-240-handler-exception-constraints.md（§ADR-240_2_1）");
    }

    [Fact(DisplayName = "ADR-240_2_1: 验证异常不可标记为可重试")]
    public void Validation_Exceptions_Should_Not_Be_Retryable()
    {
        var assemblies = GetAllProjectAssemblies();

        var violations = new List<string>();

        foreach (var assembly in assemblies)
        {
            var validationExceptions = Types
                .InAssembly(assembly)
                .That()
                .Inherit(typeof(ValidationException))
                .GetTypes();

            foreach (var exception in validationExceptions)
            {
                // 检查是否实现了 IRetryable
                var isRetryable = typeof(IRetryable).IsAssignableFrom(exception);

                if (isRetryable)
                {
                    violations.Add($"  • {exception.FullName}");
                }
            }
        }

        violations.Should().BeEmpty($"❌ ADR-240_2_1 违规: 验证异常不可标记为可重试\n\n" +
                        $"违规类型:\n{string.Join("\n", violations)}\n\n" +
                        $"问题分析:\n" +
                        $"验证异常表示输入数据错误，不会因重试而修正。\n" +
                        $"标记为可重试会导致无效的重试尝试。\n\n" +
                        $"修复建议：\n" +
                        $"1. 移除 IRetryable 接口实现\n" +
                        $"2. 客户端应修正输入后重新提交\n" +
                        $"3. 如果是暂时性技术故障，应使用 InfrastructureException\n\n" +
                        $"参考：docs/adr/runtime/ADR-240-handler-exception-constraints.md（§ADR-240_2_1）");
    }

    #endregion

    #region ADR-240_4: 异常命名空间约束

    [Fact(DisplayName = "ADR-240_4_1: 异常类型必须在正确的命名空间")]
    public void Exceptions_Should_Be_In_Correct_Namespaces()
    {
        var assemblies = GetAllProjectAssemblies();

        var violations = new List<string>();

        foreach (var assembly in assemblies)
        {
            var exceptions = Types
                .InAssembly(assembly)
                .That()
                .Inherit(typeof(Exception))
                .And()
                .AreNotAbstract()
                .GetTypes()
                .Where(t => !t.FullName!.StartsWith("System"))
                .Where(t => !t.FullName!.StartsWith("Microsoft"))
                .ToList();

            foreach (var exception in exceptions)
            {
                var namespacePart = exception.Namespace ?? "";

                // Platform 层异常应在 Platform.Exceptions
                if (exception.Assembly.GetName().Name == "Zss.BilliardHall.Platform")
                {
                    if (!namespacePart.EndsWith(".Exceptions"))
                    {
                        violations.Add($"  • {exception.FullName} (应在 *.Exceptions 命名空间)");
                    }
                }
                // 模块层异常应在 Modules.{ModuleName}.Exceptions
                else if (namespacePart.Contains(".Modules."))
                {
                    if (!namespacePart.EndsWith(".Exceptions"))
                    {
                        violations.Add($"  • {exception.FullName} (应在 *.Exceptions 命名空间)");
                    }
                }
            }
        }

        violations.Should().BeEmpty($"❌ ADR-240_4_1 违规: 异常类型命名空间不符合约定\n\n" +
                        $"违规类型:\n{string.Join("\n", violations)}\n\n" +
                        $"问题分析:\n" +
                        $"异常类型必须组织在 Exceptions 命名空间下，以保持一致性和可发现性。\n\n" +
                        $"正确的命名空间结构：\n" +
                        $"- Platform 层: Zss.BilliardHall.Platform.Exceptions\n" +
                        $"- 模块层: Zss.BilliardHall.Modules.{{ModuleName}}.Exceptions\n\n" +
                        $"修复建议：\n" +
                        $"1. 将异常类移至对应的 Exceptions 命名空间\n" +
                        $"2. 确保文件夹结构与命名空间一致\n\n" +
                        $"参考：docs/adr/runtime/ADR-240-handler-exception-constraints.md（§ADR-240_4_1）");
    }

    #endregion

    #region Helper Methods

    private static IEnumerable<Assembly> GetAllProjectAssemblies()
    {
        yield return typeof(Platform.PlatformBootstrapper).Assembly;
        yield return typeof(Application.ApplicationBootstrapper).Assembly;

        foreach (var moduleAssembly in ModuleAssemblyData.ModuleAssemblies)
        {
            yield return moduleAssembly;
        }

        foreach (var hostAssembly in HostAssemblyData.HostAssemblies)
        {
            yield return hostAssembly;
        }
    }

    #endregion
}
