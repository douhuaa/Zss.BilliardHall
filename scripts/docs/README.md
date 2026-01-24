# 文档自动化脚本

本目录包含用于文档维护和健康度检查的自动化脚本。

## 可用脚本

### health-check.sh

**用途**：检查文档健康度，包括核心文档存在性、链接有效性等。

**运行方式**：

```bash
./scripts/docs/health-check.sh
```

**检查项目**：

1. ✅ 核心文档存在性
2. ✅ 内部链接有效性（计划中）
3. ✅ 文档结构完整性（计划中）
4. ✅ 文档更新时效性（计划中）
5. ✅ 内容质量（TODO/FIXME 标记等）（计划中）

**在 CI 中使用**：

```yaml
# .github/workflows/docs-health.yml
- name: Check Documentation Health
  run: ./scripts/docs/health-check.sh
```

**健康度评分**：

- 90-100: 优秀 (A)
- 75-89: 良好 (B)
- 60-74: 一般 (C)
- <60: 需要改进 (D)

## 计划中的脚本

### link-checker.sh

检查所有 Markdown 文件中的外部链接是否有效。

### docs-generator.sh

根据代码注释自动生成 API 文档。

### changelog-updater.sh

根据 git commits 自动更新 CHANGELOG.md。

## 贡献

欢迎提交新的自动化脚本！请确保：

- 脚本有清晰的注释
- 包含使用示例
- 更新本 README
