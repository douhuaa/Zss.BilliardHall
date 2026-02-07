# 架构违规破例记录

> **用途**：记录已批准的临时性架构违规及其归还计划  
> **维护规范**：依据 [ADR-900 §3 破例治理](../adr/governance/ADR-900-architecture-tests.md)  
> **自动化**：CI 定期扫描过期破例，自动阻断构建

---

## 📋 当前有效破例

| ADR | 规则 | 到期版本 | 负责人 | 偿还计划 | 状态 |
|-----|------|---------|--------|---------|------|
| - | - | - | - | - | - |

**说明**：
- 🚧 Active：进行中
- ✅ Resolved：已解决
- 🔴 Overdue：已过期

---

## 📚 历史归档

### 已解决的破例

*暂无历史记录*

---

## 🔒 治理规范

根据 [ADR-900](../adr/governance/ADR-900-architecture-tests.md) 和 [ADR-905](../adr/governance/ADR-905-enforcement-level-classification.md)：

1. **所有破例必须记录**：任何未记录的违规将导致 CI 失败
2. **必需字段**：ADR 编号、规则描述、到期版本、负责人、偿还计划
3. **自动监控**：CI 每月第一次构建扫描此文件
4. **过期处理**：过期破例自动导致构建失败

---

## ➕ 添加破例

使用以下模板添加新的破例记录：

```markdown
| ADR-XXX_Y_Z | 规则简述 | vX.Y.Z | @username | 具体偿还计划 | 🚧 |
```

**示例**：
```markdown
| ADR-201_1_1 | Handler Scoped 生命周期 | v2.5.0 | @devteam | 迁移所有 Handler 至 Scoped | 🚧 |
```

---

**最后更新**：2026-02-07  
**维护责任**：Architecture Board
