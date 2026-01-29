# CI/CD 与工具使用 FAQ

> 📚 **根据 ADR-950 建立的 CI/CD 和工具相关常见问题解答**  
> ⚠️ **无裁决力声明**：本文档无架构裁决权，所有决策以 [ADR 正文](../adr/) 为准。

---

## 概述

本文档解答与 CI/CD 流程、架构测试、自动化工具相关的常见问题。

---

## CI/CD 流程

### Q: 为什么我的 PR 在 CI 中失败了，但本地测试通过？

**A**: 可能的原因：

1. **环境差异**：
   - CI 使用的 .NET 版本可能与本地不同
   - CI 运行时可能有不同的环境变量

2. **缓存问题**：
   ```bash
   # 清理本地缓存后重新运行
   dotnet clean
   dotnet restore --force
   dotnet test
   ```

3. **并行测试冲突**：
   - CI 可能并行运行测试
   - 确保测试之间没有共享状态

4. **架构测试差异**：
   ```bash
   # 本地运行完整的架构测试
   dotnet test src/tests/ArchitectureTests/
   ```

**参考 ADR**：[ADR-0360：CI/CD 管道标准化](../adr/technical/ADR-0360-ci-cd-pipeline-standardization.md)

---

### Q: 架构测试在 CI 中失败，如何快速定位问题？

**A**: 按以下步骤诊断：

**步骤 1：查看失败的测试名称**
```bash
# CI 输出示例
Failed   ADR_0001_Architecture_Tests.Modules_Should_Not_Reference_Other_Modules
```

**步骤 2：本地复现**
```bash
# 运行特定测试
dotnet test --filter "FullyQualifiedName~ADR_0001"

# 查看详细输出
dotnet test --filter "FullyQualifiedName~ADR_0001" --logger "console;verbosity=detailed"
```

**步骤 3：查阅相关 ADR**
- 测试名称包含 ADR 编号（如 `ADR_0001`）
- 查阅对应 ADR 了解违反的规则
- 查阅对应的 Prompt 文件了解修复方法

**步骤 4：使用诊断指南**
- 参考 `docs/copilot/architecture-test-failures.md`
- 查找类似的失败案例

**参考 ADR**：[ADR-0000：架构测试与 CI 治理宪法](../adr/constitutional/ADR-0000-architecture-test-ci-governance-constitution.md)

---

### Q: 如何跳过某个失败的架构测试？

**A**: **不建议跳过架构测试**。架构测试失败表示代码违反了 ADR 约束。

**正确做法**：
1. 修复代码以符合架构约束
2. 如果认为 ADR 约束不合理，提出 ADR 修订提案

**极端情况的临时方案**（需要架构委员会批准）：
```csharp
[Fact(Skip = "临时跳过：原因说明 - Issue #123")]
public void Some_Architecture_Test()
{
    // ...
}
```

⚠️ **警告**：跳过测试必须：
- 创建 Issue 追踪
- 在 PR 中标注 `[ARCH-VIOLATION]`
- 获得架构委员会批准
- 设定修复期限

**参考 ADR**：[ADR-0000：架构测试与 CI 治理宪法](../adr/constitutional/ADR-0000-architecture-test-ci-governance-constitution.md)

---

### Q: CI 构建时间太长，如何优化？

**A**: 优化策略：

**1. 使用缓存**：
```yaml
# GitHub Actions 示例
- uses: actions/cache@v3
  with:
    path: ~/.nuget/packages
    key: ${{ runner.os }}-nuget-${{ hashFiles('**/Directory.Packages.props') }}
```

**2. 并行运行测试**：
```bash
# 并行运行测试项目
dotnet test --parallel
```

**3. 只运行受影响的测试**：
```bash
# 根据变更文件判断需要运行的测试
if [[ $CHANGED_FILES == *"Modules/Orders"* ]]; then
  dotnet test tests/Modules.Orders.Tests/
fi
```

**4. 分阶段构建**：
- Stage 1：快速检查（lint, format）
- Stage 2：架构测试
- Stage 3：单元测试
- Stage 4：集成测试

**参考 ADR**：[ADR-0360：CI/CD 管道标准化](../adr/technical/ADR-0360-ci-cd-pipeline-standardization.md)

---

## 架构测试工具

### Q: NetArchTest 报告的类型名称看起来很奇怪，如何理解？

**A**: NetArchTest 使用完全限定类型名称（Fully Qualified Type Name）。

**示例**：
```
Zss.BilliardHall.Modules.Orders.UseCases.CreateOrder.CreateOrderHandler
```

**解读**：
- 命名空间：`Zss.BilliardHall.Modules.Orders.UseCases.CreateOrder`
- 类名：`CreateOrderHandler`
- 模块：`Orders`
- 用例：`CreateOrder`

**快速定位**：
```bash
# 使用 grep 查找类
grep -r "class CreateOrderHandler" src/
```

**参考 ADR**：[ADR-0003：命名空间与项目结构规范](../adr/constitutional/ADR-0003-namespace-rules.md)

---

### Q: 如何为新的 ADR 编写架构测试？

**A**: 按以下步骤：

**步骤 1：创建测试类**
```csharp
// src/tests/ArchitectureTests/ADR/ADR_XXXX_Architecture_Tests.cs
namespace ArchitectureTests.ADR;

/// <summary>
/// ADR-XXXX：[ADR 标题]
/// 验证 [具体约束]
/// </summary>
public class ADR_XXXX_Architecture_Tests
{
    // 测试方法
}
```

**步骤 2：编写测试方法**
```csharp
[Fact]
public void Rule_Description_Should_Be_Enforced()
{
    // Arrange & Act
    var result = Types.InAssembly(typeof(Program).Assembly)
        .That()
        .ResideInNamespace("YourNamespace")
        .Should()
        .MeetCustomRule(new YourCustomRule())
        .GetResult();

    // Assert
    Assert.True(result.IsSuccessful, 
        $"违规说明。违规类型：{string.Join(", ", result.FailingTypes?.Select(t => t.FullName) ?? [])}");
}
```

**步骤 3：运行验证**
```bash
dotnet test src/tests/ArchitectureTests/ --filter "FullyQualifiedName~ADR_XXXX"
```

**参考文档**：
- [架构测试编写案例](../cases/architecture-test-writing-case.md)
- [ADR-0000：架构测试与 CI 治理宪法](../adr/constitutional/ADR-0000-architecture-test-ci-governance-constitution.md)

---

### Q: 架构测试可以测试性能吗？

**A**: **不建议**。架构测试用于验证结构性约束，不用于性能测试。

**架构测试适用于**：
- ✅ 依赖方向
- ✅ 命名约定
- ✅ 模块隔离
- ✅ 类型组织

**性能测试应该**：
- ❌ 不在架构测试中
- ✅ 使用专门的性能测试工具（如 BenchmarkDotNet）
- ✅ 在独立的性能测试项目中

**参考 ADR**：[ADR-0000：架构测试与 CI 治理宪法](../adr/constitutional/ADR-0000-architecture-test-ci-governance-constitution.md)

---

## 自动化脚本

### Q: 验证脚本（`scripts/validate-*.sh`）失败，如何调试？

**A**: 按以下步骤调试：

**步骤 1：检查脚本权限**
```bash
# 确保脚本有执行权限
chmod +x scripts/validate-*.sh
```

**步骤 2：手动运行脚本**
```bash
# 运行脚本并查看输出
./scripts/validate-adr-structure.sh

# 查看详细输出
bash -x ./scripts/validate-adr-structure.sh
```

**步骤 3：检查依赖**
```bash
# 验证脚本依赖的工具是否安装
which grep awk sed

# 检查 .NET CLI
dotnet --version
```

**步骤 4：查看脚本源码**
- 脚本位于 `scripts/` 目录
- 通常包含详细的注释
- 可以根据需要修改或扩展

**参考 ADR**：[ADR-970：自动化工具日志集成标准](../adr/governance/ADR-970-automation-log-integration-standard.md)

---

### Q: 如何添加新的验证脚本？

**A**: 遵循以下模式：

**步骤 1：创建脚本**
```bash
#!/bin/bash
# scripts/validate-new-rule.sh
#
# 描述：验证新规则
# 用法：./scripts/validate-new-rule.sh
# 退出码：0=成功，1=失败

set -e

echo "开始验证新规则..."

# 验证逻辑
if [ condition ]; then
    echo "✅ 验证通过"
    exit 0
else
    echo "❌ 验证失败：原因"
    exit 1
fi
```

**步骤 2：添加到 CI**
```yaml
# .github/workflows/validate.yml
- name: Validate New Rule
  run: ./scripts/validate-new-rule.sh
```

**步骤 3：更新文档**
- 在 `scripts/README.md` 中添加脚本说明
- 更新相关的验证指南

**参考 ADR**：[ADR-970：自动化工具日志集成标准](../adr/governance/ADR-970-automation-log-integration-standard.md)

---

## 依赖管理

### Q: 为什么在项目文件中添加 NuGet 包时不能指定版本号？

**A**: 项目使用中央包管理（CPM）。

**原因**：
- 所有版本号统一在 `Directory.Packages.props` 中管理
- 避免版本冲突
- 便于统一升级

**正确做法**：

**步骤 1：在 Directory.Packages.props 中添加版本**
```xml
<PackageVersion Include="Newtonsoft.Json" Version="13.0.3" />
```

**步骤 2：在项目文件中引用（不带版本）**
```xml
<PackageReference Include="Newtonsoft.Json" />
```

**参考 ADR**：[ADR-0004：中央包管理与层级依赖规则](../adr/constitutional/ADR-0004-Cpm-Final.md)

---

### Q: 如何更新所有 NuGet 包到最新版本？

**A**: 谨慎更新，按以下步骤：

**步骤 1：检查过时的包**
```bash
dotnet list package --outdated
```

**步骤 2：更新 Directory.Packages.props**
```xml
<!-- 更新版本号 -->
<PackageVersion Include="Newtonsoft.Json" Version="13.0.4" />
```

**步骤 3：验证**
```bash
# 恢复依赖
dotnet restore

# 运行所有测试
dotnet test

# 运行架构测试
dotnet test src/tests/ArchitectureTests/
```

**步骤 4：检查安全漏洞**
```bash
dotnet list package --vulnerable
```

**参考 ADR**：[ADR-0004：中央包管理与层级依赖规则](../adr/constitutional/ADR-0004-Cpm-Final.md)

---

## Copilot 与 AI 工具

### Q: GitHub Copilot 建议的代码违反了架构约束，如何避免？

**A**: 使用 Copilot Instructions 和提示：

**方法 1：查阅 Copilot Prompts**
- 位于 `docs/copilot/` 目录
- 每个 ADR 都有对应的 Prompt 文件
- 包含常见错误和正确模式

**方法 2：使用 ADR 作为上下文**
```
// 提示 Copilot：
// 根据 ADR-0001，模块间不应直接引用
// 请使用领域事件或契约进行通信
```

**方法 3：运行架构测试验证**
```bash
# 接受 Copilot 建议后立即运行
dotnet test src/tests/ArchitectureTests/
```

**参考文档**：
- [AI 治理指南](../guides/ai-governance-guide.md)
- [ADR-0007：Agent 行为与权限宪法](../adr/constitutional/ADR-0007-agent-behavior-permission-constitution.md)

---

### Q: 如何让 Copilot 更好地理解项目架构？

**A**: 提供足够的上下文：

**1. 在代码中添加注释**：
```csharp
/// <summary>
/// 根据 ADR-0001，此 Handler 仅返回 ID
/// 不应返回业务数据
/// </summary>
public class CreateOrderHandler : IRequestHandler<CreateOrder, Guid>
{
    // ...
}
```

**2. 引用相关 ADR**：
```csharp
// 参考 ADR-0005：Command Handler 仅返回 void 或 ID
public Task<Guid> Handle(CreateOrder command, ...)
```

**3. 使用描述性命名**：
```csharp
// 清晰的命名有助于 Copilot 理解意图
public class MemberRegisteredIntegrationEvent { }  // 跨模块事件
public class MemberRegisteredDomainEvent { }       // 模块内部事件
```

**参考文档**：
- [AI 治理指南](../guides/ai-governance-guide.md)

---

## 故障排查

### Q: 编译通过但运行时出错，如何调试？

**A**: 常见原因和解决方法：

**原因 1：依赖注入配置缺失**
```bash
# 错误示例
System.InvalidOperationException: Unable to resolve service for type 'IOrderRepository'

# 解决：检查 DependencyInjection.cs
services.AddScoped<IOrderRepository, OrderRepository>();
```

**原因 2：事件处理器未注册**
```bash
# 检查事件处理器是否注册
services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(YourModule).Assembly));
```

**原因 3：配置文件缺失**
```bash
# 检查 appsettings.json 是否包含必需配置
{
  "ConnectionStrings": {
    "Default": "..."
  }
}
```

**调试步骤**：
1. 查看完整堆栈跟踪
2. 检查依赖注入配置
3. 启用详细日志
4. 使用调试器逐步执行

---

### Q: 测试在本地通过，但在 CI 中随机失败？

**A**: 可能是测试不稳定（Flaky Test）：

**常见原因**：
1. **时间相关**：使用 `DateTime.Now` 而非可控的时间
2. **并行冲突**：测试之间共享状态
3. **异步问题**：未正确等待异步操作完成
4. **数据库状态**：测试之间数据未清理

**解决方法**：
```csharp
// 不要使用 DateTime.Now
// ❌ 错误
var now = DateTime.Now;

// ✅ 正确：注入时间提供者
public class TimeProvider : ITimeProvider
{
    public DateTime Now => DateTime.UtcNow;
}

// 测试时使用 Mock
var timeProvider = Substitute.For<ITimeProvider>();
timeProvider.Now.Returns(new DateTime(2024, 1, 1));
```

**参考文档**：
- [测试框架指南](../guides/testing-framework-guide.md)

---

## 相关文档

- [ADR-0000：架构测试与 CI 治理宪法](../adr/constitutional/ADR-0000-architecture-test-ci-governance-constitution.md)
- [ADR-0360：CI/CD 管道标准化](../adr/technical/ADR-0360-ci-cd-pipeline-standardization.md)
- [ADR-970：自动化工具日志集成标准](../adr/governance/ADR-970-automation-log-integration-standard.md)
- [CI/CD 集成指南](../guides/ci-cd-integration-guide.md)
- [架构测试编写案例](../cases/architecture-test-writing-case.md)

---

**维护**：Tech Lead  
**最后更新**：2026-01-29  
**状态**：✅ Active
