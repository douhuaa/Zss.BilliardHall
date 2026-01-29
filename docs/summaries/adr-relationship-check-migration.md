# ADR 关系一致性检查：从 Bash 到 .NET 架构测试的迁移

## 概述

本文档记录了将 ADR 关系双向一致性检查从 bash 脚本迁移到 .NET 架构测试的过程和原因。

## 背景

原有的 `scripts/check-relationship-consistency.sh` 脚本在 CI 环境中遇到了兼容性问题：
- 在 GitHub Actions 中静默失败（exit code 1 但无错误输出）
- 依赖 bash 特定行为（`set -eo pipefail`）
- 字符串解析逻辑复杂且易出错
- 难以调试和维护

## 解决方案：治理级别跃迁

这不是简单的"语言迁移"，而是**治理级别的跃迁**。

### 架构对比

| 维度 | Bash 脚本 | .NET 架构测试 |
|------|-----------|--------------|
| **Markdown 解析** | sed/grep 字符串匹配 | Markdig AST 解析 |
| **错误定位** | 模糊文本输出 | 精确到 ADR 和行号 |
| **类型安全** | 字符串地狱 | 强类型模型 |
| **可扩展性** | 接近 0 | 无限 |
| **治理可信度** | 低（脚本可能出错） | 高（编译时检查） |
| **团队可维护性** | 痛苦（bash 专家） | 正常（C# 开发者） |
| **调试体验** | 困难 | IDE 断点调试 |
| **CI 集成** | 独立步骤 | 统一测试框架 |

## 新架构

```
tests/ArchitectureTests/Adr/
├── AdrDocument.cs                      # 强类型 ADR 模型
├── AdrParser.cs                        # Markdig AST 解析器
├── AdrRepository.cs                    # ADR 文档扫描器
└── AdrRelationshipConsistencyTests.cs  # 治理测试（裁决）
```

### 职责分离

1. **AdrDocument** - 强类型模型
   - 表示一个 ADR 及其关系声明
   - 使用 `HashSet<string>` 存储关系
   - 提供清晰的属性访问

2. **AdrRepository** - 文档扫描
   - 扫描 `docs/adr/` 目录
   - 过滤无效文件（README、proposals）
   - 批量加载所有 ADR

3. **AdrParser** - AST 解析
   - 使用 Markdig 解析 Markdown
   - 支持中英文双语格式
   - 提取关系声明到强类型模型

4. **AdrRelationshipConsistencyTests** - 治理测试
   - 4 个独立测试方法
   - 直接 Assert，失败即裁决
   - 精确的错误消息

## 测试覆盖

新架构测试提供 4 个独立测试：

1. ✅ **DependsOn_Must_Be_Declared_Bidirectionally**
   - 验证：A 依赖 B ⇔ B 被 A 依赖

2. ✅ **DependedBy_Must_Be_Declared_Bidirectionally**
   - 验证：A 被 B 依赖 ⇔ B 依赖 A

3. ✅ **Supersedes_Must_Be_Declared_Bidirectionally**
   - 验证：A 替代 B ⇔ B 被 A 替代

4. ✅ **SupersededBy_Must_Be_Declared_Bidirectionally**
   - 验证：A 被 B 替代 ⇔ B 替代 A

## 使用方式

### 本地运行

```bash
# 运行所有 ADR 关系一致性测试
dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj \
  --filter "FullyQualifiedName~AdrRelationshipConsistencyTests"

# 运行单个测试
dotnet test --filter "DependsOn_Must_Be_Declared_Bidirectionally"
```

### CI 集成

GitHub Actions workflow 已更新为使用新的架构测试：

```yaml
- name: Check Bidirectional Consistency (Architecture Tests)
  run: |
    dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj \
      --filter "FullyQualifiedName~AdrRelationshipConsistencyTests" \
      --logger "console;verbosity=normal" \
      --configuration Release
```

## 优势

### 1. 精确的错误报告

**之前（bash）**：
```
❌ 依赖关系不一致：
   ADR-902 依赖 ADR-901
   但 ADR-901 未声明被 ADR-902 依赖
```

**现在（.NET）**：
```
❌ 依赖关系不一致：
   ADR-902 声明依赖 ADR-901
   但 ADR-901 未声明被 ADR-902 依赖
   修复：在 ADR-901.md 的 **Depended By** 中添加 ADR-902
   文件：/path/to/docs/adr/governance/ADR-902-xxx.md
```

### 2. 可扩展性

基于现有架构，可以轻松添加：
- 🔜 循环依赖检测
- 🔜 ADR 状态约束（只有 Final 状态才能被依赖）
- 🔜 关系图可视化（Mermaid/Graphviz）
- 🔜 JSON 报告导出
- 🔜 关系路径分析
- 🔜 孤立 ADR 检测

### 3. 统一测试框架

所有架构测试现在都在同一个测试项目中：
```bash
dotnet test src/tests/ArchitectureTests/
```

### 4. 更好的开发体验

- IDE 智能提示和导航
- 断点调试支持
- 单元测试运行器集成
- 代码覆盖率报告

## 迁移影响

### 移除的文件

- ❌ `scripts/check-relationship-consistency.sh` （已被架构测试替代）

### 保留的文件

- ✅ `scripts/verify-adr-relationships.sh` （验证关系声明章节存在）
- ✅ `scripts/detect-circular-dependencies.sh` （下一步迁移目标）
- ✅ `scripts/generate-adr-relationship-map.sh` （关系图生成）

### 新增的文件

- ✅ `src/tests/ArchitectureTests/Adr/AdrDocument.cs`
- ✅ `src/tests/ArchitectureTests/Adr/AdrParser.cs`
- ✅ `src/tests/ArchitectureTests/Adr/AdrRepository.cs`
- ✅ `src/tests/ArchitectureTests/Adr/AdrRelationshipConsistencyTests.cs`

## 下一步

1. **循环依赖检测**
   - 实现图算法检测环形依赖
   - 添加到架构测试中

2. **ADR 状态约束**
   - 解析 Front Matter 中的状态
   - 验证只有 Final 状态才能被依赖

3. **关系图可视化**
   - 导出 Mermaid 格式
   - 自动生成关系图文档

4. **工具化**
   - 打包为 `dotnet tool install -g adr-check`
   - 提供命令行接口

## 参考

- ADR-940：ADR 关系与溯源管理
- ADR-0000：架构测试与 CI 治理宪法
- [Markdig GitHub](https://github.com/xoofx/markdig)

---

**日期**: 2026-01-29  
**作者**: GitHub Copilot  
**审核**: douhuaa
