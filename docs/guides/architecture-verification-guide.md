# 架构自动化验证系统（Architecture Automation Verification System）

> ⚠️ **无裁决力声明**：本文档仅供参考，不具备架构裁决权。
> 所有架构决策以相关 ADR 正文为准。详见 [ADR 目录](adr/README.md)。

## 概述

本文档说明如何通过自动化手段确保架构约束得到严格执行，避免"文档归文档，落地靠人喊"的情况。

## 三层防御体系

根据 [ADR-900](../adr/governance/ADR-900-architecture-tests.md)
和 [ADR-905 执行级别分类](../adr/ADR-905-enforcement-level-classification.md)，我们实施了三层自动化防御体系：

### 🛡️ Level 1: 静态可执行（Static Enforceable）

**工具**: NetArchTest + xUnit

**覆盖范围**: 84+ 架构测试

**执行时机**:

- 本地开发: `dotnet test src/tests/ArchitectureTests`
- CI/CD: 自动运行于每次 push 和 PR
- Pre-push hook: 可选的本地 Git 钩子

**特点**:

- ✅ 零容忍：测试失败 = 构建失败 = PR 阻断
- ✅ 全自动：无需人工判断
- ✅ 快速反馈：~1 秒完成所有架构测试

**测试覆盖**:

- ADR-900: 架构测试元规则（4 个测试）
- ADR-0001: 模块化单体与垂直切片（22 个测试）
- ADR-0002: Platform/Application/Host 三层启动体系（26 个测试）
- ADR-0003: 命名空间与项目边界规范（18 个测试）
- ADR-0004: 中央包管理规范（18 个测试）
- ADR-0005: 应用内交互模型与执行边界（24 个测试）

**查看测试详情**:

```bash
cd src/tests/ArchitectureTests
dotnet test --list-tests
```

---

### 🔬 Level 2: 语义半自动（Semantic Semi-Auto）

**工具**: Roslyn Analyzer（自定义分析器）

**状态**: ✅ 已实现（本 PR）

**执行时机**:

- IDE 实时分析（Visual Studio, Rider, VS Code）
- 编译时检查：`dotnet build`
- CI/CD：作为构建流程的一部分

**实现的分析器**:

1. **ADR0005_02**: Endpoint 业务逻辑检查
  - 检测过长方法体（>10 行）
  - 检测条件逻辑（if/switch）
  - 检测循环和 LINQ 查询
  - 检测直接数据库操作

2. **ADR0005_05**: 跨模块调用检查
  - 检测模块间直接方法调用
  - 允许 Platform 层调用
  - 允许 Contracts 命名空间
  - 允许消息传递（Publish/Send/Invoke）

3. **ADR0005_11**: 结构化异常检查
  - 检测 `throw new Exception()`
  - 要求使用领域特定异常类型

**特点**:

- ⚠️ 警告级别：不阻断编译，但需人工审查
- 🤖 实时反馈：在 IDE 中边写边检查
- 📊 启发式检测：可能存在误报，需要判断

**配置分析器**:

在项目中禁用特定分析器：

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);ADR0005_02</NoWarn>
</PropertyGroup>
```

抑制特定代码的警告：

```csharp
#pragma warning disable ADR0005_02
// 需要豁免的代码
#pragma warning restore ADR0005_02
```

在 `.editorconfig` 中调整严重级别：

```ini
# 将警告提升为错误
dotnet_diagnostic.ADR0005_02.severity = error

# 禁用分析器
dotnet_diagnostic.ADR0005_05.severity = none
```

**查看分析器文档**:

```bash
cat src/tools/ArchitectureAnalyzers/README.md
```

---

### 👨‍💼 Level 3: 人工 Gate（Manual Gate）

**工具**: PR 模板 + Code Review + 架构会议

**状态**: ✅ 已实施

**执行流程**:

1. **PR 提交阶段**
  - 开发者填写 `.github/PULL_REQUEST_TEMPLATE.md`
  - 根据 ADR-900，应声明架构测试状态
  - 如有架构破例，应详细说明

2. **自动检查阶段**
  - CI 运行 Level 1 测试
  - 如果测试失败但 PR 声明"无违规" → 自动拒绝
  - 如果 PR 声明"有破例" → 触发人工审查

3. **人工审查阶段**
  - 架构师 Code Review
  - 检查破例理由是否充分
  - 评估影响范围和归还计划
  - 决定批准或拒绝

4. **记录归档阶段**
  - 所有破例记录在 `docs/summaries/arch-violations.md`
  - 包含违规详情、理由、归还计划
  - 便于后续审计和偿还追踪

**人工审查要点**:

- 是否有明确的远程调用契约？
- 是否处理了超时/降级？
- 是否真的需要强一致性？
- Saga 补偿逻辑是否完整？
- 业务逻辑是否集中在一个 Handler？

**查看 PR 模板**:

```bash
cat .github/PULL_REQUEST_TEMPLATE.md
```

**查看架构违规记录**:

```bash
cat docs/summaries/arch-violations.md
```

---

## CI/CD 集成

### GitHub Actions 工作流

**文件**: `.github/workflows/architecture-tests.yml`

**触发条件**:

- Push 到 `main` 分支
- 针对 `main` 分支的 Pull Request

**工作流步骤**:

1. ✅ Checkout 代码
2. ✅ 设置 .NET 10.0 SDK
3. ✅ 恢复依赖（`dotnet restore`）
4. ✅ 构建解决方案（Release 配置）
5. ✅ 运行架构测试
6. ✅ 生成测试报告
7. ✅ 上传测试结果

**失败处理**:

- 架构测试失败 → CI 返回非零退出码
- PR 状态检查失败 → GitHub 阻止合并
- 测试报告作为 artifact 保存，便于审查

**查看最近的 CI 运行**:

```bash
# 使用 GitHub CLI
gh run list --workflow=architecture-tests.yml

# 查看特定运行的日志
gh run view <run-id> --log
```

---

## 本地开发工作流

### 标准开发流程

1. **编写代码**
  - IDE 实时显示 Roslyn Analyzer 警告
  - 根据警告调整代码结构

2. **提交前检查**
   ```bash
   # 构建并运行架构测试
   dotnet build
   dotnet test src/tests/ArchitectureTests
   
   # 如果测试失败，修复违规后重新测试
   ```

3. **推送代码**
  - 如果安装了 pre-push hook，自动运行架构测试
  - 测试通过才允许 push

4. **创建 PR**
  - 填写 PR 模板中的架构违规声明
  - CI 自动运行完整测试套件

### 安装 Pre-push Hook

**Linux/macOS**:

```bash
# 复制 hook 脚本
cp scripts/pre-push-hook.sh .git/hooks/pre-push

# 添加执行权限
chmod +x .git/hooks/pre-push
```

**Windows**:

```powershell
# Copy the PowerShell script
Copy-Item scripts\pre-push-hook.ps1 .git\hooks\

# Create a shell wrapper
@"
#!/bin/sh
exec pwsh -NoProfile -ExecutionPolicy Bypass -File "`$(dirname "`$0")/pre-push-hook.ps1"
"@ | Out-File -FilePath .git\hooks\pre-push -Encoding ASCII

# Make the wrapper executable (if using Git Bash or WSL)
chmod +x .git/hooks/pre-push
```

**验证安装**:

```bash
# 测试 hook 是否工作
git push --dry-run
```

---

## 故障排除

### 问题：本地测试通过，但 CI 失败

**可能原因**:

- 本地使用 Debug 配置，CI 使用 Release 配置
- 本地未运行完整构建

**解决方法**:

```bash
# 使用与 CI 相同的配置
dotnet clean
dotnet build -c Release
dotnet test src/tests/ArchitectureTests -c Release --no-build
```

### 问题：Roslyn Analyzer 报告误报

**解决方法**:

1. 确认是否真的是误报
2. 如果确认是误报，使用 `#pragma warning disable` 抑制
3. 在代码注释中说明为什么是误报
4. 在 PR 中说明情况

**示例**:

```csharp
// 这个 endpoint 方法确实很简单，只是参数多导致代码行数超标
// 实际上只是简单的参数转发给 Handler
#pragma warning disable ADR0005_02
public async Task<IResult> CreateOrder(
    string customerId,
    string productId,
    int quantity,
    string address,
    // ... 更多参数
) {
    return await _mediator.Send(new CreateOrderCommand(
        customerId, productId, quantity, address, ...
    ));
}
#pragma warning restore ADR0005_02
```

### 问题：架构测试运行很慢

**解决方法**:

1. 架构测试本身很快（~1 秒），慢的是构建
2. 使用增量构建：
   ```bash
   dotnet build  # 首次构建
   # 修改代码后
   dotnet build  # 只构建变更部分
   dotnet test src/tests/ArchitectureTests --no-build
   ```
3. 在 CI 中启用构建缓存

---

## 监控与报告

### 查看架构健康度

**本地查看**:

```bash
# 运行架构测试并生成详细报告
dotnet test src/tests/ArchitectureTests \
    --logger "trx;LogFileName=architecture-test-results.trx" \
    --logger "html;LogFileName=architecture-test-results.html"

# 查看报告
open TestResults/architecture-test-results.html
```

**CI 中查看**:

1. 进入 PR 页面
2. 查看 "Checks" 标签页
3. 点击 "Architecture Tests" 查看详细结果
4. 下载 test results artifact 获取完整报告

### 关键指标

- **架构测试通过率**: 目标 100%
- **当前豁免数量**: 见 `docs/summaries/arch-violations.md`
- **Analyzer 警告数量**: 使用 `dotnet build` 查看
- **跨模块依赖数量**: 应为 0（由 ADR-0001 测试保证）

---

## 持续改进

### 定期评审（每季度）

1. **评审豁免项**
  - 检查 `arch-violations.md` 中的记录
  - 是否已按计划偿还？
  - 是否需要延期？

2. **评审架构测试**
  - 是否需要新的测试？
  - 是否有测试过于严格？
  - 是否有新的架构约束需要自动化？

3. **评审 Roslyn Analyzer**
  - 误报率是否可接受？
  - 是否需要调整检测逻辑？
  - 是否需要新的分析器？

4. **更新文档**
  - 记录架构演进
  - 更新最佳实践
  - 分享案例学习

### 团队培训

**新人入职**:

1. 阅读本文档
2. 阅读 [ADR-900](../adr/governance/ADR-900-architecture-tests.md)
3. 运行一次架构测试
4. 安装 pre-push hook
5. 尝试故意违反一条规则，观察反馈

**定期分享**:

- 架构违规案例分析
- Analyzer 误报处理经验
- 架构测试最佳实践

---

## 总结

通过三层自动化防御体系，我们实现了：

✅ **Level 1（静态）**: 84+ 架构测试，零容忍执行  
✅ **Level 2（语义）**: 3 个 Roslyn Analyzer，实时反馈  
✅ **Level 3（人工）**: PR 模板 + Code Review，处理复杂场景

这确保了：

- 📋 文档不是"历史说明书"，是可执行规范
- 🤖 90% 的架构违规被自动拦截
- 👨‍💼 10% 的复杂场景有人工决策流程
- 📊 所有破例有记录、有追踪、有归还计划

**架构约束不再"靠人喊"，而是靠自动化执行！**

---

## 相关文档

- [ADR-900: 架构测试与 CI 治理](../adr/governance/ADR-900-architecture-tests.md)
- [ADR-0005: 应用内交互模型](../adr/ADR-0005-Application-Interaction-Model-Final.md)
- [ADR-905 执行级别分类](../adr/ADR-905-enforcement-level-classification.md)
- [Architecture Tests README](../../src/tests/ArchitectureTests/README.md)
- [Roslyn Analyzers README](../../src/tools/ArchitectureAnalyzers/README.md)
- [CI/CD 集成指南](ci-cd-guide.md)
