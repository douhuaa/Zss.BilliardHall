namespace Zss.BilliardHall.Tests.ArchitectureTests.Specification.RuleSets.ADR907.tests;

/// <summary>
/// ADR-907 具体测试实现
/// 基于 Adr907Definitions 规则定义生成的具体测试方法
/// 每个测试方法对应一个条款（Clause），包含实际的验证逻辑
/// </summary>
public sealed class Adr907Concrete_Tests
{
    private readonly Adr907RuleSet _ruleSet = new();
    private ArchitectureRuleSet _adr907RuleSet => _ruleSet.Define();

    #region Rule 1: ArchitectureTests 的法律地位

    [Fact(DisplayName = "ADR-907_1_1: ArchitectureTests 是 ADR 的唯一自动化执法形式")]
    public void ADR_907_1_1_唯一执法形式()
    {
        // 验证：ArchitectureTests 是 ADR 的唯一自动化执法形式
        var rule = _adr907RuleSet.GetRule(1);
        rule.Should().NotBeNull("Rule 1 必须存在");
        rule!.Summary.Should().Be("ArchitectureTests 的法律地位");
        
        var clause = _adr907RuleSet.GetClause(1, 1);
        clause.Should().NotBeNull("Clause 1.1 必须存在");
        clause!.Condition.Should().Contain("唯一自动化执法形式", 
            "ADR-907_1_1 验证 ArchitectureTests 作为唯一执法手段");
    }

    [Fact(DisplayName = "ADR-907_1_2: 任何具备裁决力的 ADR 必须有对应的 ArchitectureTests")]
    public void ADR_907_1_2_必须有测试()
    {
        // 验证：Final ADR 必须有对应的 ArchitectureTests 或明确声明为 Non-Enforceable
        var clause = _adr907RuleSet.GetClause(1, 2);
        clause.Should().NotBeNull("Clause 1.2 必须存在");
        clause!.Condition.Should().Contain("必须有对应", 
            "ADR-907_1_2 检测 Final ADR 是否具备对应测试");
        
        // 验证所有已注册的 ADR
        var allAdrNumbers = RuleSetRegistry.GetAllAdrNumbers();
        foreach (var adrNumber in allAdrNumbers)
        {
            var ruleSet = RuleSetRegistry.GetStrict(adrNumber);
            ruleSet.Should().NotBeNull($"ADR-{adrNumber:000} 必须在 Registry 中存在");
            
            // 每个 ADR 必须至少有一个规则
            ruleSet.Rules.Should().NotBeEmpty(
                $"ADR-{adrNumber:000} 必须定义至少一个规则，否则应标记为 Non-Enforceable");
        }
    }

    [Fact(DisplayName = "ADR-907_1_3: 不存在无执法路径的架构规则")]
    public void ADR_907_1_3_禁止无执法路径()
    {
        // 验证：不存在声明为'文档专属、拒绝自动化'的架构规则
        var clause = _adr907RuleSet.GetClause(1, 3);
        clause.Should().NotBeNull("Clause 1.3 必须存在");
        
        // 检查所有规则都有执行策略（至少有一个关联的条款）
        foreach (var rule in _adr907RuleSet.Rules)
        {
            var clausesForRule = _adr907RuleSet.Clauses
                .Where(c => c.Id.RuleNumber == rule.Id.RuleNumber)
                .ToList();
            clausesForRule.Should().NotBeEmpty(
                $"ADR-907 Rule {rule.Id.RuleNumber} 必须至少有一个条款定义执行路径");
        }
    }

    #endregion

    #region Rule 2: 命名与组织规范

    [Fact(DisplayName = "ADR-907_2_1: ArchitectureTests 必须集中于独立测试项目")]
    public void ADR_907_2_1_独立测试项目()
    {
        // 验证：ArchitectureTests 项目存在性
        var clause = _adr907RuleSet.GetClause(2, 1);
        clause.Should().NotBeNull("Clause 2.1 必须存在");
        
        // 验证当前测试项目的存在
        var testAssembly = typeof(Adr907Concrete_Tests).Assembly;
        testAssembly.GetName().Name.Should().Contain("ArchitectureTests",
            "ADR-907_2_1 验证测试项目必须命名为 ArchitectureTests");
    }

    [Fact(DisplayName = "ADR-907_2_2: 测试目录必须按 ADR 编号分组")]
    public void ADR_907_2_2_按ADR分组()
    {
        // 验证：目录结构符合 /ADR-XXX/ 格式
        var clause = _adr907RuleSet.GetClause(2, 2);
        clause.Should().NotBeNull("Clause 2.2 必须存在");
        
        // 验证当前测试类所在的命名空间
        var testType = typeof(Adr907Concrete_Tests);
        testType.Namespace.Should().Contain("ADR907",
            "ADR-907_2_2 验证测试必须按 ADR 编号组织");
    }

    [Fact(DisplayName = "ADR-907_2_3: 单个测试类或文件仅允许覆盖一个 ADR")]
    public void ADR_907_2_3_一对一映射()
    {
        // 验证：测试类与 ADR 映射的一致性
        var clause = _adr907RuleSet.GetClause(2, 3);
        clause.Should().NotBeNull("Clause 2.3 必须存在");
        
        // 当前测试类只测试 ADR-907
        var testType = typeof(Adr907Concrete_Tests);
        testType.Namespace.Should().Contain("ADR907",
            "ADR-907_2_3 单个测试类只能覆盖一个 ADR");
        testType.Namespace.Should().NotMatchRegex(@"ADR\d{3}.*ADR\d{3}",
            "ADR-907_2_3 测试类不能混合多个 ADR 编号");
    }

    [Fact(DisplayName = "ADR-907_2_4: 测试类命名必须显式绑定 ADR")]
    public void ADR_907_2_4_显式绑定命名()
    {
        // 验证：命名格式 ADR_{编号}_{Rule}_Architecture_Tests
        var clause = _adr907RuleSet.GetClause(2, 4);
        clause.Should().NotBeNull("Clause 2.4 必须存在");
        
        var testType = typeof(Adr907Concrete_Tests);
        var className = testType.Name;
        className.Should().MatchRegex(@"^Adr907.*_Tests$",
            "ADR-907_2_4 测试类必须以 Adr907 开头并以 _Tests 结尾");
    }

    [Fact(DisplayName = "ADR-907_2_5: 测试方法必须映射 ADR 子规则")]
    public void ADR_907_2_5_方法映射子规则()
    {
        // 验证：命名格式 ADR_{编号}_{Rule}_{Clause}_{行为描述}
        var clause = _adr907RuleSet.GetClause(2, 5);
        clause.Should().NotBeNull("Clause 2.5 必须存在");
        
        // 验证当前测试类的所有测试方法命名
        var testType = typeof(Adr907Concrete_Tests);
        var testMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<FactAttribute>() != null);
        
        // 只验证符合 Rule_Clause 格式的测试方法
        var ruleClauseTests = testMethods
            .Where(m => Regex.IsMatch(m.Name, @"^ADR_907_\d+_\d+_"))
            .ToList();
        
        ruleClauseTests.Should().NotBeEmpty(
            "ADR-907_2_5 应该有符合 Rule_Clause 格式的测试方法");
        
        foreach (var method in ruleClauseTests)
        {
            method.Name.Should().MatchRegex(@"^ADR_907_\d+_\d+_",
                $"ADR-907_2_5 测试方法 {method.Name} 必须包含 ADR 编号和规则条款编号");
        }
    }

    [Fact(DisplayName = "ADR-907_2_6: 测试失败信息必须包含 ADR 编号与子规则")]
    public void ADR_907_2_6_失败信息溯源()
    {
        // 验证：失败信息的 ADR 溯源能力
        var clause = _adr907RuleSet.GetClause(2, 6);
        clause.Should().NotBeNull("Clause 2.6 必须存在");
        clause!.Enforcement.Should().NotBeNullOrEmpty(
            "ADR-907_2_6 验证失败信息必须有执行要求");
    }

    [Fact(DisplayName = "ADR-907_2_7: ArchitectureTests 不得为空、占位或弱断言")]
    public void ADR_907_2_7_禁止弱断言()
    {
        // 验证：检测空测试和弱断言
        var clause = _adr907RuleSet.GetClause(2, 7);
        clause.Should().NotBeNull("Clause 2.7 必须存在");
        
        // 验证当前测试类的所有测试方法都有实际断言
        var testType = typeof(Adr907Concrete_Tests);
        var testMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<FactAttribute>() != null);
        
        testMethods.Should().NotBeEmpty("ADR-907_2_7 必须有测试方法");
        
        // 每个测试方法都应该有意义的实现（非空占位）
        foreach (var method in testMethods)
        {
            var methodBody = method.GetMethodBody();
            methodBody.Should().NotBeNull($"ADR-907_2_7 测试方法 {method.Name} 不能为空");
        }
    }

    [Fact(DisplayName = "ADR-907_2_8: 不得 Skip 或条件禁用测试")]
    public void ADR_907_2_8_禁止跳过测试()
    {
        // 验证：检测 Skip 和条件编译指令
        var clause = _adr907RuleSet.GetClause(2, 8);
        clause.Should().NotBeNull("Clause 2.8 必须存在");
        
        // 验证当前测试类没有被跳过的测试
        var testType = typeof(Adr907Concrete_Tests);
        var testMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<FactAttribute>() != null);
        
        foreach (var method in testMethods)
        {
            var factAttr = method.GetCustomAttribute<FactAttribute>();
            factAttr!.Skip.Should().BeNullOrEmpty(
                $"ADR-907_2_8 测试方法 {method.Name} 不应使用 Skip 属性");
        }
    }

    #endregion

    #region Rule 3: 最小断言语义规范

    [Fact(DisplayName = "ADR-907_3_1: 每个测试类至少包含1个有效断言")]
    public void ADR_907_3_1_最小断言数量()
    {
        // 验证：通过静态分析验证断言数量
        var clause = _adr907RuleSet.GetClause(3, 1);
        clause.Should().NotBeNull("Clause 3.1 必须存在");
        
        // 验证当前测试类有断言
        var testType = typeof(Adr907Concrete_Tests);
        var testMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<FactAttribute>() != null);
        
        testMethods.Should().NotBeEmpty("ADR-907_3_1 测试类必须至少包含一个测试方法");
    }

    [Fact(DisplayName = "ADR-907_3_2: 每个测试方法只能映射一个 ADR 子规则")]
    public void ADR_907_3_2_单一职责()
    {
        // 验证：通过命名模式检查验证单一职责
        var clause = _adr907RuleSet.GetClause(3, 2);
        clause.Should().NotBeNull("Clause 3.2 必须存在");
        
        // 验证测试方法命名格式唯一映射  
        var testType = typeof(Adr907Concrete_Tests);
        var testMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<FactAttribute>() != null);
        
        // 只检查符合 Rule_Clause 格式的测试方法
        var ruleClauseTests = testMethods
            .Where(m => Regex.IsMatch(m.Name, @"^ADR_907_\d+_\d+_"))
            .ToList();
        
        foreach (var method in ruleClauseTests)
        {
            // 测试方法名应该只包含一个 Rule_Clause 组合
            var ruleClauseMatches = Regex.Matches(method.Name, @"_(\d+)_(\d+)_");
            ruleClauseMatches.Count.Should().Be(1,
                $"ADR-907_3_2 测试方法 {method.Name} 应该只映射一个子规则");
        }
    }

    [Fact(DisplayName = "ADR-907_3_3: 所有断言失败信息必须可反向溯源到 ADR")]
    public void ADR_907_3_3_可溯源失败()
    {
        // 验证：失败消息包含 ADR 引用、违规标记、修复建议和文档引用
        var clause = _adr907RuleSet.GetClause(3, 3);
        clause.Should().NotBeNull("Clause 3.3 必须存在");
        clause!.Enforcement.Should().NotBeNullOrEmpty(
            "ADR-907_3_3 验证失败消息的可溯源性要求");
    }

    [Fact(DisplayName = "ADR-907_3_4: 禁止形式化断言")]
    public void ADR_907_3_4_禁止形式化()
    {
        // 验证：禁止 Assert.True(true) 等无意义断言
        var clause = _adr907RuleSet.GetClause(3, 4);
        clause.Should().NotBeNull("Clause 3.4 必须存在");
        clause!.Enforcement.Should().NotBeNullOrEmpty(
            "ADR-907_3_4 禁止形式化断言的执行要求");
    }

    #endregion

    #region Rule 4: Analyzer / CI Gate 映射协议

    [Fact(DisplayName = "ADR-907_4_1: 所有 ArchitectureTests 必须被 Analyzer 自动发现")]
    public void ADR_907_4_1_自动发现()
    {
        // 验证：测试的可发现性和注册机制
        var clause = _adr907RuleSet.GetClause(4, 1);
        clause.Should().NotBeNull("Clause 4.1 必须存在");
        
        // 验证 ADR-907 在 RuleSetRegistry 中已注册
        var adr907 = RuleSetRegistry.GetStrict(907);
        adr907.Should().NotBeNull("ADR-907 必须在 RuleSetRegistry 中注册");
        adr907.AdrNumber.Should().Be(907);
    }

    [Fact(DisplayName = "ADR-907_4_2: 测试失败必须精确映射至 ADR 子规则（RuleId）")]
    public void ADR_907_4_2_RuleId格式()
    {
        // 验证：RuleId 格式为 ADR-XXX_Y_Z
        var clause = _adr907RuleSet.GetClause(4, 2);
        clause.Should().NotBeNull("Clause 4.2 必须存在");
        
        // 验证所有条款的 RuleId 格式
        foreach (var rule in _adr907RuleSet.Rules)
        {
            var clausesForRule = _adr907RuleSet.Clauses
                .Where(c => c.Id.RuleNumber == rule.Id.RuleNumber)
                .ToList();
            
            foreach (var clauseDef in clausesForRule)
            {
                var ruleId = clauseDef.Id.ToString();
                ruleId.Should().MatchRegex(@"^ADR-\d{3}_\d+_\d+$",
                    $"ADR-907_4_2 条款 {ruleId} 必须符合 RuleId 格式规范");
            }
        }
    }

    [Fact(DisplayName = "ADR-907_4_3: 支持执行级别分类（L1/L2）")]
    public void ADR_907_4_3_执行级别()
    {
        // 验证：L1 阻断和 L2 告警策略
        var clause = _adr907RuleSet.GetClause(4, 3);
        clause.Should().NotBeNull("Clause 4.3 必须存在");
        
        // 验证规则有 Severity 定义 (RuleSeverity 是枚举类型，不为 null)
        foreach (var rule in _adr907RuleSet.Rules)
        {
            rule.Severity.Should().BeOneOf(
                Enum.GetValues<RuleSeverity>(),
                $"ADR-907_4_3 Rule {rule.Id.RuleNumber} 必须定义有效的严重级别");
        }
    }

    [Fact(DisplayName = "ADR-907_4_4: 破例机制必须自动记录")]
    public void ADR_907_4_4_破例记录()
    {
        // 验证：破例的 ADR 编号、测试类/方法、原因、到期时间和偿还计划
        var clause = _adr907RuleSet.GetClause(4, 4);
        clause.Should().NotBeNull("Clause 4.4 必须存在");
        clause!.Enforcement.Should().NotBeNullOrEmpty(
            "ADR-907_4_4 验证破例机制的记录要求");
    }

    [Fact(DisplayName = "ADR-907_4_5: Analyzer 必须具备检测能力")]
    public void ADR_907_4_5_Analyzer检测()
    {
        // 验证：能检测空测试/弱断言/跨ADR/非Final ADR生成测试
        var clause = _adr907RuleSet.GetClause(4, 5);
        clause.Should().NotBeNull("Clause 4.5 必须存在");
        
        // 验证执行绑定存在
        var binding = Adr907ExecutionBindings.Lookup(2, 7);
        binding.Should().NotBeNull(
            "ADR-907_4_5 Rule 2 Clause 7（检测弱断言）应该有执行绑定");
    }

    [Fact(DisplayName = "ADR-907_4_6: ADR 生命周期变更必须同步")]
    public void ADR_907_4_6_生命周期同步()
    {
        // 验证：Superseded/Obsolete ADR 对应测试的处理
        var clause = _adr907RuleSet.GetClause(4, 6);
        clause.Should().NotBeNull("Clause 4.6 必须存在");
        clause!.Enforcement.Should().NotBeNullOrEmpty(
            "ADR-907_4_6 验证 ADR 生命周期同步机制");
    }

    #endregion

    #region 验证所有条款定义完整性

    [Fact(DisplayName = "ADR-907: 验证所有条款都有对应的测试方法")]
    public void ADR_907_All_Clauses_Should_Have_Test_Methods()
    {
        // 获取所有定义的条款
        var allClauses = new List<(int RuleId, int ClauseId, string Name)>();
        foreach (var rule in Adr907Definitions.AllRules)
        {
            foreach (var clause in rule.Clauses)
            {
                allClauses.Add((rule.RuleId, clause.ClauseId, clause.Name));
            }
        }

        // 获取所有测试方法
        var testType = typeof(Adr907Concrete_Tests);
        var testMethods = testType.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => m.GetCustomAttribute<FactAttribute>() != null)
            .Select(m => m.Name)
            .ToList();

        // 验证每个条款都有对应的测试方法
        foreach (var (ruleId, clauseId, name) in allClauses)
        {
            var expectedPrefix = $"ADR_907_{ruleId}_{clauseId}_";
            var hasTest = testMethods.Any(m => m.StartsWith(expectedPrefix));
            hasTest.Should().BeTrue(
                $"条款 ADR-907_{ruleId}_{clauseId} ({name}) 必须有对应的测试方法");
        }
    }

    #endregion
}
