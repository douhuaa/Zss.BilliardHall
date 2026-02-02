# ADR 治理体系健康报告

**生成时间**：2026-01-26 10:43:45  
**仓库**：Zss.BilliardHall  
**报告版本**：1.0

---

## 执行摘要

本报告提供 ADR 治理体系的全面健康度评估，包括文档完整性、测试覆盖率、映射一致性等关键指标。

---

## ADR 文档统计

### 按层级分布

| 层级 | 编号范围 | 文档数量 | 占比 |
|-----|---------|---------|------|
| 宪法层 (constitutional) | 0001-0099 | 9 | 33.3% |
| 结构层 (structure) | 0100-0199 | 5 | 18.5% |
| 运行层 (runtime) | 0200-0299 | 4 | 14.8% |
| 技术层 (technical) | 0300-0399 | 4 | 14.8% |
| 治理层 (governance) | 0900-0999 | 5 | 18.5% |
| **总计** | | **27** | **100%** |

### 状态分布

| 状态 | 数量 | 说明 |
|-----|------|------|
| Draft | 0 | 草案阶段 |
| Accepted | 9 | 已接受 |
| Final | 14 | 已定稿 |
| Superseded | 0 | 已废弃 |

---

## 架构测试覆盖率

### 测试文件统计

| 指标 | 数量 | 覆盖率 |
|-----|------|--------|
| 标记【必须测试】的 ADR | 10 | - |
| 有测试文件的 ADR | 2 | 20% |
| 总测试文件数 | 26 | - |

❌ **状态**：测试覆盖率不足

---

## Copilot Prompts 映射

### Prompt 文件统计

| 指标 | 数量 | 映射率 |
|-----|------|--------|
| ADR 总数 | 27 | - |
| Prompt 文件数 | 26 | 96% |

⚠️ **状态**：Prompt 映射基本完整

---

## 编号与目录一致性

✅ **状态**：所有 ADR 编号、目录、内容一致

---

## 改进建议

### 高优先级

1. **补充架构测试**：为标记【必须测试】但缺少测试的 ADR 添加测试文件
2. **完善 Copilot Prompts**：为缺少 Prompt 的 ADR 创建场景化提示词

### 中优先级

1. **定期审查**：每月审查 Draft 状态的 ADR，推动其进入 Accepted 或 Final 状态
2. **清理孤立文件**：检查并处理孤立的测试和 Prompt 文件
3. **优化文档**：补充 ADR 示例代码和常见场景说明

### 低优先级

1. **统计分析**：添加 ADR 变更趋势分析
2. **自动化增强**：集成更多自动化检查到 CI/CD
3. **文档模板**：持续优化 ADR 和 Prompt 模板

---

## 工具使用指南

### 日常维护工具

1. **ADR 一致性检查**
   ```bash
   ./scripts/validate-adr-consistency.sh
   ```

2. **三位一体映射验证**
   ```bash
   ./scripts/validate-three-way-mapping.sh
   ```

3. **ADR 管理 CLI**
   ```bash
   # 创建新 ADR
   ./scripts/adr-cli.sh create constitutional "标题"
   
   # 查询下一个可用编号
   ./scripts/adr-cli.sh next-number structure
   
   # 列出 ADR
   ./scripts/adr-cli.sh list
   ```

4. **生成健康报告**
   ```bash
   ./scripts/generate-health-report.sh
   ```

### 问题诊断流程

1. 运行一致性检查发现问题
2. 根据报告定位具体文件
3. 使用 ADR CLI 工具修正
4. 重新运行验证确认修复

---

## 附录

### 相关资源

- [ADR 目录](./adr/README.md)
- [ADR 流程规范](./adr/governance/ADR-900-architecture-tests.md)
- [架构测试宪法](./adr/governance/ADR-900-architecture-tests.md)
- [Copilot 治理体系](./copilot/README.md)

### 下次报告

建议每月生成一次健康报告，跟踪改进进度。

---

**报告生成**：自动化工具 v1.0  
**维护者**：架构委员会
