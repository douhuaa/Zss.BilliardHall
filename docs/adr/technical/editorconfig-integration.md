# EditorConfig 集成说明

**类型**：技术层实施指南  
**版本**：1.0  
**最后更新**：2026-01-24  
**适用范围**：开发环境配置

---

## 概述

本文档说明 `.editorconfig` 如何与 ADR 体系集成，以及在技术实施层面的最佳实践。

---

## 与 ADR 体系的集成关系

### 职责分离

```
┌─────────────────────────────────────────────────────────┐
│                   ADR 宪法层                             │
│  定义"什么是正确的架构"                                  │
│  - ADR-0001: 模块隔离                                    │
│  - ADR-0002: 三层启动                                    │
│  - ADR-0003: 命名空间规则                                │
│  - ADR-0004: 包管理                                      │
│  - ADR-0005: CQRS 模式                                   │
└─────────────────────────────────────────────────────────┘
                        │
        ┌───────────────┼───────────────┐
        ▼                               ▼
┌────────────────┐            ┌──────────────────┐
│  架构约束执行  │            │   格式风格辅助   │
│                │            │                  │
│  MSBuild 配置  │            │  .editorconfig   │
│  架构测试      │            │  格式化工具      │
│  CI 门禁       │            │  IDE 集成        │
└────────────────┘            └──────────────────┘
```

### 关键原则

1. **EditorConfig 仅处理格式，不涉及架构**
   - ✅ 缩进、换行、字符集 → EditorConfig
   - ❌ 命名空间、依赖、分层 → ADR + MSBuild + 架构测试

2. **架构约束必须可自动化验证**
   - EditorConfig 无法强制执行架构规则
   - 所有架构约束由架构测试验证
   - CI 确保未通过架构测试的代码无法合并

---

## 技术实施细节

### 1. 文件位置和优先级

```
Zss.BilliardHall/
├── .editorconfig          ← 仓库根目录（最高优先级）
├── Directory.Build.props  ← MSBuild 全局配置
├── Directory.Packages.props
└── src/
    ├── Platform/
    ├── Application/
    ├── Modules/
    └── Host/
```

**EditorConfig 查找顺序**：
1. 从当前文件向上查找 `.editorconfig`
2. 遇到 `root = true` 停止向上查找
3. 应用从根到当前目录的所有规则（就近优先）

**最佳实践**：
- ✅ 仅在仓库根目录放置一个 `.editorconfig`
- ✅ 设置 `root = true` 防止意外继承
- ❌ 避免在子目录创建 `.editorconfig`（除非有特殊需求）

### 2. 与 MSBuild 的协同

**EditorConfig** 和 **Directory.Build.props** 各司其职：

| 关注点 | EditorConfig | Directory.Build.props |
|--------|-------------|----------------------|
| 文件编码 | ✅ `charset = utf-8-bom` | - |
| 缩进和换行 | ✅ `indent_size = 4` | - |
| 命名空间 | ❌ | ✅ `RootNamespace` 自动推导 |
| 目标框架 | ❌ | ✅ `TargetFramework` |
| 语言版本 | ❌ | ✅ `LangVersion` |
| 包管理 | ❌ | ✅ `ManagePackageVersionsCentrally` |

**示例工作流**：

```csharp
// 1. 开发者创建新文件：src/Modules/Orders/OrderService.cs

// 2. EditorConfig 自动应用
//    - 字符集：UTF-8 with BOM
//    - 缩进：4 空格
//    - 换行符：CRLF

// 3. Directory.Build.props 自动推导
//    - RootNamespace: Zss.BilliardHall.Modules.Orders

// 4. 架构测试验证
//    - 命名空间是否正确
//    - 是否违反模块隔离
//    - Handler 模式是否合规

// 5. CI 门禁
//    - dotnet format --verify-no-changes
//    - dotnet test (包含架构测试)
```

### 3. IDE 集成最佳实践

#### Visual Studio 2022

**自动化配置**：

```xml
<!-- .editorconfig 配置自动应用 -->
<!-- 无需额外设置 -->

<!-- 推荐启用"保存时代码清理" -->
工具 → 选项 → 文本编辑器 → 代码清理
勾选"在保存时运行代码清理"
配置：包含"应用 .editorconfig 首选项"
```

**验证集成**：
1. 打开任意 `.cs` 文件
2. 底部状态栏应显示：`UTF-8 with signature` `CRLF` `空格: 4`
3. 修改缩进后保存，应自动格式化回 4 空格

#### JetBrains Rider

**配置验证**：

```
Settings (Ctrl+Alt+S) →
  Editor → Code Style
    ✓ Enable EditorConfig support
    ✓ EditorConfig: automatic detection of file encoding
```

**自动操作**：

```
Settings → Tools → Actions on Save
  ✓ Reformat code
  ✓ Optimize imports
  ✓ Cleanup code
```

#### Visual Studio Code

**必需扩展**：

```json
// .vscode/extensions.json（推荐团队配置）
{
  "recommendations": [
    "editorconfig.editorconfig",
    "ms-dotnettools.csharp",
    "ms-dotnettools.csdevkit"
  ]
}
```

**配置文件**：

```json
// .vscode/settings.json
{
  "editor.formatOnSave": true,
  "files.insertFinalNewline": true,
  "files.trimTrailingWhitespace": true,
  "[markdown]": {
    "files.trimTrailingWhitespace": false
  }
}
```

---

## CI/CD 集成

### GitHub Actions 示例

```yaml
# .github/workflows/code-quality.yml
name: Code Quality

on:
  pull_request:
    branches: [main]

jobs:
  format-check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      # EditorConfig 格式验证
      - name: Verify Code Format
        run: dotnet format --verify-no-changes
        continue-on-error: true  # 仅警告，不阻塞
      
      # 架构测试（必须通过）
      - name: Run Architecture Tests
        run: dotnet test src/tests/ArchitectureTests/
      
      # 单元测试
      - name: Run Unit Tests
        run: dotnet test
```

### 关键决策

**格式检查：警告 vs 失败**

```yaml
# 选项 1：格式问题仅警告（推荐）
- name: Format Check
  run: dotnet format --verify-no-changes
  continue-on-error: true

# 选项 2：格式问题导致失败（严格模式）
- name: Format Check
  run: dotnet format --verify-no-changes
  # 不设置 continue-on-error，失败即阻塞

# 选项 3：自动修复格式并提交（最用户友好）
- name: Auto Format
  run: |
    dotnet format
    if git diff --quiet; then
      echo "No formatting changes needed"
    else
      git config user.name "github-actions[bot]"
      git config user.email "github-actions[bot]@users.noreply.github.com"
      git commit -am "style: 自动格式化代码"
      git push
    fi
```

**推荐策略**：
- **格式问题**：警告或自动修复（用户友好）
- **架构测试**：必须通过（严格执行 ADR）

---

## 常见集成问题

### 问题 1：EditorConfig 与架构测试冲突

**症状**：EditorConfig 格式正确，但架构测试失败

**原因**：EditorConfig 不负责架构约束

**解决方案**：
1. 识别失败的架构测试
2. 查阅对应 ADR（ADR-0001 ~ 0005）
3. 修复架构违规，而非修改格式

**示例**：

```csharp
// ❌ EditorConfig 格式正确，但架构测试失败
namespace Zss.BilliardHall.Modules.Orders;  // ← 格式正确

using Zss.BilliardHall.Modules.Members.Domain;  // ← 违反 ADR-0001（跨模块引用）

// ✅ 修复方案：移除跨模块引用，使用契约或事件
using Zss.BilliardHall.Modules.Members.Contracts;  // ← 合规
```

### 问题 2：多个配置源冲突

**症状**：IDE 设置与 EditorConfig 不一致

**解决方案**：

**优先级顺序**：
1. `.editorconfig`（最高优先级，团队标准）
2. IDE 项目/解决方案配置
3. IDE 全局配置

**最佳实践**：
- ✅ 依赖 `.editorconfig` 作为唯一真实来源
- ✅ IDE 设置应该"启用 EditorConfig 支持"
- ❌ 避免在 IDE 中覆盖 EditorConfig 设置

### 问题 3：Git 换行符自动转换问题

**症状**：提交后换行符被自动修改

**解决方案**：

创建/更新 `.gitattributes`：

```gitattributes
# 默认自动检测文本文件并规范化
* text=auto

# C# 文件使用 CRLF（与 .editorconfig 一致）
*.cs text eol=crlf
*.csproj text eol=crlf
*.props text eol=crlf
*.targets text eol=crlf

# 文档文件使用 LF（与 .editorconfig 一致）
*.md text eol=lf
*.json text eol=lf
*.yml text eol=lf
*.yaml text eol=lf
*.sh text eol=lf

# 二进制文件不做转换
*.dll binary
*.exe binary
```

**配置 Git**：

```bash
# Windows 开发者
git config --global core.autocrlf true

# macOS/Linux 开发者
git config --global core.autocrlf input
```

---

## 最佳实践总结

### ✅ 推荐做法

1. **单一真实来源**：仓库根目录唯一 `.editorconfig`
2. **职责分离**：格式 → EditorConfig，架构 → ADR + 测试
3. **自动化优先**：IDE 自动应用，CI 自动验证
4. **用户友好**：格式问题自动修复或警告，架构问题必须修复
5. **文档同步**：变更 `.editorconfig` 必须更新文档

### ❌ 避免做法

1. **混淆职责**：在 EditorConfig 中尝试强制架构规则
2. **多配置文件**：子目录中创建额外 `.editorconfig`
3. **覆盖基准**：IDE 设置覆盖 EditorConfig
4. **忽略 CI**：跳过格式或架构验证
5. **文档脱节**：修改配置但不更新文档

---

## 版本历史

| 版本 | 日期 | 变更摘要 | 负责人 |
|------|------|---------|-------|
| 1.0  | 2026-01-24 | 创建 EditorConfig 集成技术指南 | 架构团队 |

---

## 相关文档

- [EditorConfig 详细文档](../../configuration/editorconfig.md)
- [配置文件索引](../../configuration/README.md)
- [.editorconfig 文件](../../../.editorconfig)
- [ADR-0900：ADR 新增与修订流程](../governance/ADR-0900-adr-process.md)
- [架构测试失败诊断](../../copilot/architecture-test-failures.md)

---

**本文档是 EditorConfig 在 Zss.BilliardHall 项目中的技术实施指南。**
