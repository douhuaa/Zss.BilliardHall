# EditorConfig 企业级配置文档

> ⚠️ **无裁决力声明**：本文档仅供参考，不具备架构裁决权。
> 所有架构决策以相关 ADR 正文为准。详见 [ADR 目录](../adr/README.md)。

**版本**：1.0  
**最后更新**：2026-01-24  
**适用范围**：Zss.BilliardHall 全项目  
**维护责任**：Tech Lead / 架构师

---

## 目录

- [概述](#概述)
- [核心原则](#核心原则)
- [配置规范详解](#配置规范详解)
- [变更流程](#变更流程)
- [常见问题排查](#常见问题排查)
- [IDE 和工具配置](#ide-和工具配置)
- [最佳实践](#最佳实践)

---

## 概述

`.editorconfig` 是项目的统一代码风格配置文件，定义了文件格式、缩进、换行等规范。本文件确保团队成员在不同编辑器和操作系统上保持一致的代码风格。

### 为什么需要 EditorConfig？

1. **一致性**：所有开发者使用相同的格式规范，减少 diff 噪音
2. **自动化**：IDE 自动应用配置，无需手动调整
3. **跨平台**：支持 Windows、Linux、macOS 等不同开发环境
4. **团队协作**：新成员无需学习复杂的代码风格指南

### 重要边界

⚠️ **EditorConfig 仅定义格式和风格，不包含架构约束**

- ✅ EditorConfig 管理：缩进、换行、字符集、文件格式
- ❌ EditorConfig 不管理：命名空间规则、分层依赖、包管理、CQRS 模式

架构约束由以下机制强制执行：

- **ADR-001**：模块化单体与垂直切片架构
- **ADR-002**：Platform/Application/Host 三层启动体系
- **ADR-003**：命名空间规则（通过 `Directory.Build.props` 强制）
- **ADR-004**：中央包管理（通过 `Directory.Packages.props` 强制）
- **ADR-005**：CQRS 和 Handler 模式
- **架构测试**：自动化验证架构约束合规性

---

## 核心原则

### 1. 仅控制格式，不涉及业务

EditorConfig 配置纯粹是技术性的：

- 文件编码（UTF-8、UTF-8 BOM）
- 换行符（LF、CRLF）
- 缩进（空格数量）
- 行尾处理（清理空格、添加换行）

### 2. 与 ADR 体系协同

本配置文件是 ADR 体系的补充，而非替代：

| 关注点  | 管理机制                 | 配置文件                           |
|------|----------------------|--------------------------------|
| 代码格式 | EditorConfig         | `.editorconfig`                |
| 命名空间 | MSBuild + ADR-003   | `Directory.Build.props`        |
| 依赖管理 | CPM + ADR-004       | `Directory.Packages.props`     |
| 架构约束 | 架构测试 + ADR-001~0005 | `src/tests/ArchitectureTests/` |

### 3. 跨平台兼容

配置考虑不同操作系统和开发环境：

- **Windows**：CRLF 换行符（C# 文件）
- **Linux/macOS**：LF 换行符（文档、脚本）
- **跨平台**：统一使用空格缩进，禁止 Tab

---

## 配置规范详解

### C# 源代码文件 (*.cs)

```ini
[*.cs]
charset = utf-8-bom          # UTF-8 with BOM（C# 编译器推荐）
indent_size = 4              # 4 空格缩进（.NET 标准）
end_of_line = crlf           # Windows 换行符（Visual Studio 默认）
```

**选择理由**：

- **UTF-8 BOM**：C# 编译器官方推荐，避免某些情况下的字符集识别问题
- **4 空格**：.NET 生态系统的事实标准，所有微软示例代码使用此缩进
- **CRLF**：Windows 是主要开发平台，Visual Studio 默认使用 CRLF

**注意事项**：

- Git 通过 `.gitattributes` 自动转换换行符，无需担心跨平台协作问题
- 如果团队主要在 Linux/macOS 开发，可考虑改为 LF，但需团队一致同意

### 文档文件 (*.md, *.json, *.yaml)

```ini
[*.md]
charset = utf-8              # UTF-8 without BOM（避免工具兼容性问题）
indent_size = 2              # 2 空格缩进（文档标准）
end_of_line = lf             # Unix 换行符（跨平台文档标准）
trim_trailing_whitespace = false  # 保留尾随空格（Markdown 换行需要）
max_line_length = 120        # 建议行宽（便于阅读）
```

**选择理由**：

- **无 BOM**：许多文档工具和解析器不兼容 BOM
- **2 空格**：Markdown、YAML、JSON 社区标准
- **LF**：文档通常跨平台共享，LF 是更广泛的标准
- **保留尾随空格**：Markdown 使用两个空格表示换行

### 项目配置文件 (*.csproj, *.props, *.targets)

```ini
[*.{csproj,props,targets}]
indent_size = 2              # 2 空格缩进（XML 标准）
end_of_line = crlf           # Windows 换行符（MSBuild 标准）
```

**选择理由**：

- **2 空格**：XML 文件通常使用更紧凑的缩进
- **CRLF**：MSBuild 和 Visual Studio 在 Windows 上运行，保持一致

### 脚本文件

```ini
[*.sh]                       # Bash 脚本
charset = utf-8
indent_size = 2
end_of_line = lf             # Unix 脚本必须使用 LF

[*.ps1]                      # PowerShell 脚本
charset = utf-8-bom
indent_size = 4
end_of_line = crlf           # Windows 脚本使用 CRLF
```

**选择理由**：

- **Shell 脚本 (LF)**：Unix/Linux 系统要求 LF 换行符
- **PowerShell (CRLF)**：Windows 脚本环境标准

---

## 变更流程

### 修改 .editorconfig 变更流程

#### 1. 评估变更级别

| 变更类型            | 级别  | 审批要求              | 公示期  |
|-----------------|-----|-------------------|------|
| 新增文件类型规范        | 技术层 | Tech Lead/架构师单人批准 | 无    |
| 修改现有规范（如改变缩进大小） | 技术层 | Tech Lead/架构师单人批准 | 无    |
| 涉及架构影响的变更       | 结构层 | Tech Lead/架构师     | 建议讨论 |

#### 2. 提交变更流程

```bash
# 1. 创建功能分支
git checkout -b config/update-editorconfig

# 2. 修改 .editorconfig 文件
# 编辑文件...

# 3. 更新此文档（editorconfig.md）
# 记录变更原因、影响范围、团队公告要点

# 4. 提交并创建 PR
git add .editorconfig docs/configuration/editorconfig.md
git commit -m "config(editorconfig): 描述变更内容"
git push origin config/update-editorconfig

# 5. PR 审查
# - 由 Tech Lead/架构师审查
# - 确保文档同步更新
# - 验证不影响现有代码构建

# 6. 合并后团队公告
# - 通知团队成员更新本地配置
# - 说明 IDE 可能需要重启
# - 提供迁移指南（如有必要）
```

#### 3. 变更检查清单

提交 PR 前必须完成：

- [ ] 变更原因明确记录在 PR 描述中
- [ ] 更新 `docs/configuration/editorconfig.md` 版本历史
- [ ] 验证不破坏现有代码构建
- [ ] 团队公告草稿准备完毕
- [ ] IDE 配置指南已更新（如有新工具要求）

---

## 常见问题排查

### 问题 1：EditorConfig 规则未生效

**症状**：保存文件时，IDE 没有应用预期的格式规则

**可能原因**：

1. IDE 未启用 EditorConfig 支持
2. IDE 缓存未刷新
3. 项目级别设置覆盖了 EditorConfig
4. `.editorconfig` 文件位置错误

**解决方案**：

**Visual Studio**：

```
工具 → 选项 → 文本编辑器 → 代码清理
确保勾选"在保存时运行代码清理"
并配置包含"应用 .editorconfig 首选项"
```

**JetBrains Rider**：

```
Settings → Editor → Code Style
确保勾选"Enable EditorConfig support"
使用 Ctrl+Alt+L 重新格式化代码
```

**VS Code**：

```
安装扩展：EditorConfig for VS Code
重启编辑器
验证状态栏显示"EditorConfig"标记
```

### 问题 2：文件换行符混乱

**症状**：Git 显示整个文件都有变更，实际只是换行符不同

**可能原因**：

1. `.gitattributes` 未配置或配置不当
2. 开发者在不同操作系统间切换
3. EditorConfig 换行符设置与实际文件不匹配

**解决方案**：

```bash
# 1. 检查 .gitattributes 配置
cat .gitattributes

# 应包含类似配置：
# * text=auto
# *.cs text eol=crlf
# *.md text eol=lf

# 2. 重新规范化仓库（谨慎操作）
git add --renormalize .
git commit -m "chore: 规范化换行符"

# 3. 团队成员同步
git pull
git reset --hard origin/main
```

### 问题 3：架构测试失败

**症状**：架构测试失败，提示命名空间或依赖违规

**重要**：EditorConfig 不负责架构约束！

**解决方案**：

1. **识别失败的测试**：
   ```bash
   dotnet test src/tests/ArchitectureTests/ --filter "ADR_003"
   ```

2. **查阅对应 ADR**：
  - 命名空间问题 → [ADR-003](../adr/constitutional/ADR-003-namespace-rules.md)
  - 依赖问题 → [ADR-001](../adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md)
  - 包管理问题 → [ADR-004](../adr/constitutional/ADR-004-Cpm-Final.md)

3. **参考 Copilot 指南**：
  - [架构测试失败诊断](../copilot/architecture-test-failures.md)
  - [ADR Prompts](../copilot/)

### 问题 4：缩进不一致

**症状**：同一文件中有些行是 Tab，有些是空格

**解决方案**：

**Visual Studio**：

```
编辑 → 高级 → 将空格转换为制表符（或反之）
Ctrl+K, Ctrl+D（格式化文档）
```

**Rider**：

```
Code → Reformat Code (Ctrl+Alt+L)
选择"Cleanup code"并应用
```

**VS Code**：

```
命令面板 → "Convert Indentation to Spaces"
保存文件触发自动格式化
```

### 问题 5：中文注释显示乱码

**症状**：中文注释在某些编辑器中显示为乱码或问号

**可能原因**：

1. 文件编码不是 UTF-8
2. 编辑器未正确检测编码
3. 缺少 BOM（C# 文件）

**解决方案**：

```bash
# 检查文件编码
file -i src/Platform/Platform.csproj

# 应显示：
# charset=utf-8

# 如果编码错误，转换文件：
iconv -f GBK -t UTF-8 file.cs > file_utf8.cs
mv file_utf8.cs file.cs

# 对于 C# 文件，确保有 BOM：
# Visual Studio 会自动添加
# 或使用工具：
# Tools → Options → Advanced Save Options → Encoding: UTF-8 with signature
```

---

## IDE 和工具配置

### Visual Studio 2022

**自动支持**：Visual Studio 原生支持 EditorConfig，无需额外配置

**推荐设置**：

1. **启用保存时代码清理**：
   ```
   工具 → 选项 → 文本编辑器 → C# → 代码样式 → 格式设置 → 常规
   勾选"保存时自动格式化"
   ```

2. **配置代码清理**：
   ```
   工具 → 选项 → 文本编辑器 → 代码清理
   配置文件 → 添加/编辑
   确保包含：
   - 应用 .editorconfig 首选项
   - 删除不必要的 using
   - 对 using 进行排序
   ```

3. **快捷键**：
  - `Ctrl+K, Ctrl+D`：格式化整个文档
  - `Ctrl+K, Ctrl+E`：运行代码清理

**验证配置**：

```
打开任意 .cs 文件
查看底部状态栏，应显示：
"UTF-8 with signature" "CRLF" "空格: 4"
```

### JetBrains Rider

**自动支持**：Rider 默认启用 EditorConfig 支持

**推荐设置**：

1. **验证 EditorConfig 启用**：
   ```
   Settings (Ctrl+Alt+S)
   → Editor → Code Style
   勾选"Enable EditorConfig support"
   ```

2. **配置保存时操作**：
   ```
   Settings → Tools → Actions on Save
   勾选：
   - Reformat code
   - Optimize imports
   - Cleanup code
   ```

3. **快捷键**：
  - `Ctrl+Alt+L`：重新格式化代码
  - `Ctrl+Alt+Enter`：代码清理

**验证配置**：

```
打开任意 .cs 文件
右下角应显示：
"EditorConfig" 标记（绿色勾号）
```

### Visual Studio Code

**需要扩展**：VS Code 需要安装 EditorConfig 扩展

**安装步骤**：

1. **安装 EditorConfig 扩展**：
   ```
   扩展市场搜索："EditorConfig for VS Code"
   安装扩展 ID: EditorConfig.EditorConfig
   ```

2. **安装 C# 扩展**（可选，用于 C# 语法支持）：
   ```
   扩展市场搜索："C#" 或 "C# Dev Kit"
   安装微软官方扩展
   ```

3. **配置 settings.json**：
   ```json
   {
     "editor.formatOnSave": true,
     "editor.insertSpaces": true,
     "files.eol": "\n",
     "files.encoding": "utf8",
     "files.insertFinalNewline": true,
     "files.trimTrailingWhitespace": true,
     "[markdown]": {
       "files.trimTrailingWhitespace": false
     }
   }
   ```

**验证配置**：

```
打开任意 .cs 文件
状态栏右下角应显示：
"EditorConfig" 标记
"UTF-8 with BOM" "CRLF" "Spaces: 4"
```

### 命令行工具

**EditorConfig 验证工具**：

```bash
# 安装 editorconfig-checker（跨平台）
npm install -g editorconfig-checker

# 验证项目中的所有文件
ec

# 验证特定文件
ec src/Platform/Platform.csproj

# CI 集成
ec || exit 1
```

**dotnet format**（C# 代码格式化）：

```bash
# 格式化整个解决方案
dotnet format

# 仅验证格式（不修改文件）
dotnet format --verify-no-changes

# 格式化特定项目
dotnet format src/Platform/Platform.csproj

# CI 集成
dotnet format --verify-no-changes || exit 1
```

---

## 最佳实践

### 1. 团队入职配置清单

新成员入职时，确保完成以下配置：

```markdown
- [ ] 克隆仓库：git clone <repo-url>
- [ ] 安装推荐 IDE（Visual Studio 2022 或 Rider）
- [ ] 验证 EditorConfig 自动应用（打开 .cs 文件检查格式）
- [ ] 配置 Git（user.name, user.email）
- [ ] 配置 Git autocrlf：
      Windows: git config --global core.autocrlf true
      macOS/Linux: git config --global core.autocrlf input
- [ ] 安装 dotnet SDK 10.0+
- [ ] 运行构建验证：dotnet build
- [ ] 运行测试验证：dotnet test
- [ ] 阅读核心 ADR（ADR-001 ~ ADR-005）
```

### 2. 代码审查注意事项

**不要关注的格式问题**（EditorConfig 自动处理）：

- ❌ 缩进是 2 空格还是 4 空格
- ❌ 换行符是 LF 还是 CRLF
- ❌ 文件末尾是否有空行
- ❌ 行尾是否有多余空格

**应该关注的问题**（EditorConfig 无法检测）：

- ✅ 命名空间是否符合 ADR-003
- ✅ 模块依赖是否违反 ADR-001
- ✅ 包引用是否符合 ADR-004
- ✅ Handler 模式是否符合 ADR-005
- ✅ 业务逻辑正确性
- ✅ 测试覆盖率

### 3. 持续集成检查

**推荐在 CI 中添加格式验证**：

```yaml
# .github/workflows/ci.yml
- name: Verify Code Format
  run: dotnet format --verify-no-changes
  
- name: Verify EditorConfig
  run: |
    npm install -g editorconfig-checker
    ec
```

**注意**：

- 格式检查应该是警告而非失败（避免阻塞合法 PR）
- 或在 PR 合并前自动修复格式（推荐）

### 4. 迁移现有代码

如果在已有代码库中引入 EditorConfig：

```bash
# 1. 备份当前代码
git checkout -b backup-before-editorconfig

# 2. 应用格式化
dotnet format

# 3. 检查变更范围
git diff --stat

# 4. 如果变更过大，分批提交
# 先提交关键模块
git add src/Platform/ src/Application/
git commit -m "style: 应用 EditorConfig 到 Platform 和 Application"

# 再提交其他模块
git add src/Modules/
git commit -m "style: 应用 EditorConfig 到 Modules"

# 5. 创建 PR 并通知团队
# 提醒团队成员在合并后立即 pull 并重启 IDE
```

### 5. 版本控制建议

**建议在 .gitattributes 中配置换行符规则**：

```gitattributes
# 自动检测文本文件并规范化换行符
* text=auto

# C# 源代码使用 CRLF
*.cs text eol=crlf
*.csproj text eol=crlf
*.props text eol=crlf
*.targets text eol=crlf

# 文档和配置使用 LF
*.md text eol=lf
*.json text eol=lf
*.yml text eol=lf
*.yaml text eol=lf
*.sh text eol=lf

# 二进制文件不做转换
*.dll binary
*.exe binary
*.png binary
*.jpg binary
```

---

## 版本历史

| 版本  | 日期         | 变更摘要                     | 负责人  |
|-----|------------|--------------------------|------|
| 1.0 | 2026-01-24 | 初始版本，企业级 EditorConfig 配置 | 架构团队 |

---

## 相关文档

### ADR 体系

- [ADR-001：模块化单体与垂直切片架构](../adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md)
- [ADR-002：Platform/Application/Host 三层启动体系](../adr/constitutional/ADR-002-platform-application-host-bootstrap.md)
- [ADR-003：命名空间规则](../adr/constitutional/ADR-003-namespace-rules.md)
- [ADR-004：中央包管理](../adr/constitutional/ADR-004-Cpm-Final.md)
- [ADR-005：CQRS 和 Handler 模式](../adr/constitutional/ADR-005-Application-Interaction-Model-Final.md)
- [ADR-900：架构测试与 CI 治理元规则](../adr/governance/ADR-900-architecture-tests.md)

### 配置文件

- [配置文件索引](README.md)
- [.editorconfig 文件](../../.editorconfig)
- [Directory.Build.props](../../Directory.Build.props)
- [Directory.Packages.props](../../Directory.Packages.props)

### Copilot 指南

- [架构测试失败诊断](../copilot/architecture-test-failures.md)
- [ADR-003 Prompts](../copilot/adr-003.prompts.md)
- [Copilot 指令索引](../copilot/README.md)

---

## 反馈与改进

如果发现配置问题或有改进建议：

1. **创建 Issue**：在 GitHub 仓库创建 Issue，使用标签 `config` 和 `editorconfig`
2. **提交 PR**：按照[变更流程](#变更流程)提交改进 PR
3. **团队讨论**：在团队会议中提出讨论

**联系方式**：

- 技术负责人：[姓名/联系方式]
- 架构委员会：[联系方式]

---

**本文档是 Zss.BilliardHall 项目 EditorConfig 配置的唯一权威说明。**
