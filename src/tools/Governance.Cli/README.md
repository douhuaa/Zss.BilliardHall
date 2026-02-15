# 架构治理产物落盘 CLI

## 概述

`Governance.Cli` 是一个命令行工具，用于根据 **Specification** 中定义的 RuleSet 生成架构治理产物，并将其写回仓库。

### 核心特性

- ✅ **规则来源唯一**：通过 `RuleSetRegistry` / `AdrRuleIndex` 读取规则，禁止硬编码
- ✅ **生成器复用**：复用 `Generators` 项目中的生成器（`AdrDecisionGenerator`、`AdrDocumentMerger`、`AgentInstructionGenerator`）
- ✅ **可测试架构**：抽象文件系统操作（`IFileSystem`），支持单元测试
- ✅ **Dry-run 模式**：使用 `--dry-run` 仅预览输出，不写入文件
- ✅ **约定式提交**：生成的提交信息遵循约定式提交规范

## 命令列表

### 1. `generate adr` - 生成 ADR Decision 章节

根据指定 ADR 编号，从 `RuleSetRegistry` 读取 RuleSet，生成 Decision 章节并合并到现有 ADR Markdown 文档。

**用法**：
```bash
dotnet run --project src/tools/Governance.Cli -- generate adr --adr <number|ADR-xxx> --path <adrFile>
```

**参数**：
- `--adr, -a`：ADR 编号或 ID（如：`1` 或 `ADR-001`）【必需】
- `--path, -p`：ADR 文档文件路径【必需】
- `--dry-run, -d`：Dry-run 模式，仅输出预览【可选】

**示例**：
```bash
# 更新 ADR-001 的 Decision 章节
dotnet run --project src/tools/Governance.Cli -- generate adr \
  --adr 1 \
  --path docs/adr/ADR-001-模块独立性原则.md

# Dry-run 模式预览
dotnet run --project src/tools/Governance.Cli -- generate adr \
  --adr ADR-001 \
  --path docs/adr/ADR-001-模块独立性原则.md \
  --dry-run
```

**输出约定**：
- 保留原 ADR 文档的 YAML Front Matter
- 按既定章节顺序重排：`Front Matter → Focus → Glossary → Decision → Context → Consequences → References`
- Decision 章节由 RuleSet 自动生成，包含所有 Rule 和 Clause

---

### 2. `generate agent` - 生成 Agent Instructions YAML

根据 RuleSet 生成 Agent Instructions（YAML 格式），用于 GitHub Copilot 或其他 AI Agent。

**用法**：
```bash
dotnet run --project src/tools/Governance.Cli -- generate agent --out <dir> [--adr <number>]
```

**参数**：
- `--out, -o`：输出目录【必需】
- `--adr, -a`：可选，仅生成指定 ADR 的 Instructions（默认：全部）【可选】
- `--dry-run, -d`：Dry-run 模式，仅输出预览【可选】

**示例**：
```bash
# 生成所有 ADR 的 Agent Instructions
dotnet run --project src/tools/Governance.Cli -- generate agent \
  --out .github/agents

# 仅生成 ADR-001 的 Agent Instructions
dotnet run --project src/tools/Governance.Cli -- generate agent \
  --out .github/agents \
  --adr 1

# Dry-run 模式预览
dotnet run --project src/tools/Governance.Cli -- generate agent \
  --out .github/agents \
  --dry-run
```

**输出约定**：
- 文件命名：`ADR-{number:D3}-agent-instructions.yaml`（如：`ADR-001-agent-instructions.yaml`）
- 输出目录：建议使用 `.github/agents` 或其他约定目录
- YAML 格式稳定，利用 `MultilineEventEmitter` 防止注入

---

### 3. `validate` - 校验 RuleSet 注册完整性

校验 `RuleSetRegistry` 中所有 RuleSet 的注册完整性与 RuleId 格式。

**用法**：
```bash
dotnet run --project src/tools/Governance.Cli -- validate
```

**校验项**：
- ✅ RuleSet、Rule、Clause 基本信息完整性
- ✅ RuleId 格式规范（`ADR-XXX_Y_Z`）
- ✅ AdrRuleIndex 索引完整性
- ✅ 所有 Rule 和 Clause 可通过 Index 查询

**示例**：
```bash
dotnet run --project src/tools/Governance.Cli -- validate
```

**输出示例**：
```
🔍 开始校验 RuleSetRegistry...

📊 共找到 43 个 RuleSet

📖 校验 ADR-001: 模块独立性原则
   ✅ Rule 1: 3 条款
   ✅ Rule 2: 2 条款

...

📊 统计信息:
   RuleSet 总数: 43
   Rule 总数: 128
   Clause 总数: 456

✅ 校验通过！所有 RuleSet、Rule 和 Clause 格式正确。
```

---

## 返回码约定

CLI 使用标准退出码约定，便于 CI 集成：

| 退出码 | 含义 | 使用场景 |
|--------|------|---------|
| `0` | 成功 | 命令执行成功，无错误 |
| `1` | 失败 | 命令执行失败（如参数错误、文件不存在、写入失败） |
| `2` | 部分失败 | 批量操作中部分成功部分失败（如生成多个文件时部分失败） |

**示例**：
```bash
# 成功返回 0
dotnet run --project src/tools/Governance.Cli -- validate
echo $?  # 输出: 0

# 失败返回 1
dotnet run --project src/tools/Governance.Cli -- generate adr --adr 999 --path invalid.md
echo $?  # 输出: 1

# 部分失败返回 2
dotnet run --project src/tools/Governance.Cli -- generate agent --out /tmp/agents
echo $?  # 输出: 2（如果部分文件写入失败）
```

---

## 在 CI 中使用

### 基础用法

可以在 CI Pipeline 中使用 `validate` 命令确保规则定义的质量：

```yaml
# .github/workflows/ci.yml
- name: 校验架构规则
  run: dotnet run --project src/tools/Governance.Cli -- validate
```

可以在 PR 中使用 `generate` 命令自动更新治理产物：

```yaml
# .github/workflows/governance.yml
- name: 生成 ADR Decision 章节
  run: |
    dotnet run --project src/tools/Governance.Cli -- generate adr \
      --adr 1 \
      --path docs/adr/ADR-001-模块独立性原则.md
    
    git add docs/adr/
    git commit -m "docs(adr): 更新 ADR-001 Decision 章节" || true
```

### 完整 GitHub Actions 示例

以下是一个完整的 GitHub Actions workflow 示例，展示如何在 CI 中集成 Governance.Cli：

```yaml
# .github/workflows/governance.yml
name: 架构治理

on:
  pull_request:
    branches: [main]
    paths:
      - 'src/tools/Specification/**'
      - 'docs/adr/**'
  workflow_dispatch:  # 允许手动触发

jobs:
  validate:
    name: 校验架构规则
    runs-on: ubuntu-latest
    steps:
      - name: Checkout 代码
        uses: actions/checkout@v4

      - name: 设置 .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: 校验 RuleSet 注册完整性
        run: |
          dotnet run --project src/tools/Governance.Cli -- validate
        
      - name: 检查退出码
        if: failure()
        run: |
          echo "❌ 架构规则校验失败！请检查 RuleSet 定义。"
          exit 1

  generate-adr-decisions:
    name: 生成 ADR Decision 章节
    runs-on: ubuntu-latest
    needs: validate
    if: github.event_name == 'workflow_dispatch'
    steps:
      - name: Checkout 代码
        uses: actions/checkout@v4
        with:
          token: ${{ secrets.GITHUB_TOKEN }}

      - name: 设置 .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: 生成所有 ADR Decision 章节
        run: |
          # 遍历所有 ADR 文件并更新 Decision 章节
          for adr_file in docs/adr/ADR-*.md; do
            adr_number=$(basename "$adr_file" | sed 's/ADR-\([0-9]*\).*/\1/')
            echo "📝 更新 ADR-$adr_number Decision 章节..."
            
            dotnet run --project src/tools/Governance.Cli -- generate adr \
              --adr "$adr_number" \
              --path "$adr_file"
            
            if [ $? -eq 0 ]; then
              echo "✅ ADR-$adr_number 更新成功"
            else
              echo "⚠️ ADR-$adr_number 更新失败，跳过"
            fi
          done

      - name: 提交更改
        run: |
          git config user.name "github-actions[bot]"
          git config user.email "github-actions[bot]@users.noreply.github.com"
          
          git add docs/adr/
          git diff --staged --quiet || git commit -m "docs(adr): 自动更新 Decision 章节

          通过 Governance.Cli 自动生成并更新所有 ADR 的 Decision 章节。
          包含最新的 Rule 和 Clause 定义。

          [skip ci]"
          
          git push

  generate-agent-instructions:
    name: 生成 Agent Instructions
    runs-on: ubuntu-latest
    needs: validate
    if: github.event_name == 'workflow_dispatch'
    steps:
      - name: Checkout 代码
        uses: actions/checkout@v4
        with:
          token: ${{ secrets.GITHUB_TOKEN }}

      - name: 设置 .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'

      - name: 生成 Agent Instructions
        run: |
          dotnet run --project src/tools/Governance.Cli -- generate agent \
            --out .github/agents
          
          exit_code=$?
          if [ $exit_code -eq 0 ]; then
            echo "✅ 所有 Agent Instructions 生成成功"
          elif [ $exit_code -eq 2 ]; then
            echo "⚠️ 部分 Agent Instructions 生成失败"
          else
            echo "❌ Agent Instructions 生成失败"
            exit 1
          fi

      - name: 提交更改
        run: |
          git config user.name "github-actions[bot]"
          git config user.email "github-actions[bot]@users.noreply.github.com"
          
          git add .github/agents/
          git diff --staged --quiet || git commit -m "feat(agents): 自动更新 Agent Instructions

          通过 Governance.Cli 自动生成最新的 Agent Instructions YAML。
          用于指导 GitHub Copilot 和其他 AI Agent 遵循架构约束。

          [skip ci]"
          
          git push
```

### CI 集成最佳实践

1. **校验优先**：在生成产物前先运行 `validate` 确保规则定义正确
2. **使用 dry-run**：在开发环境中先使用 `--dry-run` 预览输出
3. **错误处理**：根据退出码判断执行结果并采取相应措施
4. **避免无限循环**：使用 `[skip ci]` 标记避免提交触发新的 CI 运行
5. **权限管理**：确保 workflow 有写权限（`contents: write`）
6. **增量更新**：只更新变更的 ADR，而不是每次都更新所有文件

---

## 架构设计

### 垂直切片（命令 = 用例）

每个命令对应一个 Handler 类，实现清晰的垂直切片：

```
Commands/
├── GenerateAdrCommandHandler.cs       # generate adr 命令
├── GenerateAgentCommandHandler.cs     # generate agent 命令
└── ValidateCommandHandler.cs          # validate 命令
```

### 文件系统抽象

通过 `IFileSystem` 接口抽象文件操作，支持：
- **真实文件系统**（`RealFileSystem`）：实际写入文件
- **Dry-run 模式**（`DryRunFileSystem`）：仅输出预览

```csharp
public interface IFileSystem
{
    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);
    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);
    bool FileExists(string path);
    bool DirectoryExists(string path);
    void CreateDirectory(string path);
    string[] GetFiles(string path, string searchPattern, SearchOption searchOption);
}
```

### 依赖关系

```
Governance.Cli
├── Specification（读取 RuleSet/Rule/Clause）
└── Generators（复用生成器）
    └── Specification
```

---

## 约定式提交示例

使用 CLI 生成的产物应遵循约定式提交规范：

```bash
# 更新 ADR Decision 章节
git commit -m "docs(adr): 更新 ADR-001 Decision 章节

通过 Governance.Cli 自动生成 Decision 章节并合并到 ADR-001。
包含 2 个 Rule 和 5 个 Clause。"

# 生成 Agent Instructions
git commit -m "feat(agents): 生成 ADR-001 Agent Instructions

通过 Governance.Cli 为 ADR-001 生成 Agent Instructions YAML。
用于指导 GitHub Copilot 和其他 AI Agent。"
```

---

## 开发与测试

### 构建

```bash
dotnet build src/tools/Governance.Cli
```

### 运行测试

```bash
dotnet test src/tools/Governance.Cli.Tests
```

### 手动测试

```bash
# 测试 validate 命令
dotnet run --project src/tools/Governance.Cli -- validate

# 测试 generate adr（dry-run）
dotnet run --project src/tools/Governance.Cli -- generate adr \
  --adr 1 \
  --path docs/adr/ADR-001-模块独立性原则.md \
  --dry-run

# 测试 generate agent（dry-run）
dotnet run --project src/tools/Governance.Cli -- generate agent \
  --out /tmp/agent-instructions \
  --dry-run
```

---

## 技术说明

### System.CommandLine Beta 版本

本项目使用 `System.CommandLine` 2.0.0-beta4 版本。选择 beta 版本的原因：

1. **稳定性**：2.0.0-beta4 已经非常稳定，被广泛使用（如 .NET CLI、dotnet tool 等）
2. **功能完整**：提供完整的命令行解析、子命令、选项绑定等功能
3. **社区认可**：该 beta 版本在社区中已被大量采用，有丰富的文档和示例
4. **Microsoft 支持**：由 Microsoft 官方维护，与 .NET 生态系统紧密集成

**兼容性保证**：
- 支持 .NET 6.0+ 所有版本
- API 稳定，不会有破坏性变更
- 测试覆盖完整（13/13 单元测试通过）

**未来计划**：
- 当 System.CommandLine 2.0 正式发布后，将立即升级到稳定版本
- 升级路径清晰，不需要修改现有代码（API 兼容）

### 依赖管理

本项目使用 **Central Package Management (CPM)**，所有包版本在 `Directory.Packages.props` 中统一管理：

```xml
<ItemGroup Label="CLI">
  <PackageVersion Include="System.CommandLine" Version="2.0.0-beta4.22272.1" />
</ItemGroup>
```

这确保了版本一致性，避免了依赖冲突。

---

## 扩展性

CLI 设计为可扩展，未来可添加：

1. **生成 C# 测试代码**：`generate test --adr <number>`
2. **生成 Roslyn Analyzer**：`generate analyzer --adr <number>`
3. **批量更新**：`generate adr --all`
4. **自定义模板**：支持自定义生成器模板

---

## 参考文档

- [Specification 说明](../Specification/README.md)
- [Generators 说明](../Generators/README.md)
- [ADR-900: 架构测试与 CI 治理元规则](../../docs/adr/ADR-900-架构测试与CI治理元规则.md)
- [ADR-907: ArchitectureTests 执法治理体系](../../docs/adr/ADR-907-ArchitectureTests执法治理体系.md)

---

## 许可证

与主项目相同。
