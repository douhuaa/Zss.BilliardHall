# ADR 终极模板使用说明

> **快速开始指南**：如何使用 ADR 终极模板编写和迁移 ADR

---

## 🎯 核心理念

> **ADR 是系统的法律条文，不是架构师的解释说明。**

---

## 📐 六段式结构

```markdown
1. Rule（规则本体｜裁决源）        ← 唯一具有裁决力的部分，≤ 1 页
2. Enforcement（执法模型）         ← 如何执行规则（测试、工具、流程）
3. Exception（破例与归还）         ← 破例条件和归还机制
4. Change Policy（变更政策）       ← ADR 修改和废弃流程
5. Non-Goals（明确不管什么）       ← 防止 ADR 膨胀
6. References（非裁决性）          ← 术语、示例、历史（无裁决力）
```

---

## 📚 文档导航

### 核心文档（必读）

| 文档 | 用途 | 字数 | 阅读时间 |
|------|------|------|---------|
| **[ADR-TEMPLATE-ULTIMATE.md](ADR-TEMPLATE-ULTIMATE.md)** | 终极模板本体 | 2,000 | 5 分钟 |
| **[ADR-TEMPLATE-GUIDE.md](ADR-TEMPLATE-GUIDE.md)** | 详细使用指南 | 8,000+ | 20 分钟 |
| **[ADR-TEMPLATE-MIGRATION.md](ADR-TEMPLATE-MIGRATION.md)** | 迁移指南和对比 | 10,000+ | 25 分钟 |

### 参考文档

| 文档 | 用途 |
|------|------|
| **[governance/ADR-0000-architecture-tests-v2.md](governance/ADR-0000-architecture-tests-v2.md)** | 迁移示例 |
| **[../summaries/adr-ultimate-template-implementation.md](../summaries/adr-ultimate-template-implementation.md)** | 实施总结 |
| **[../summaries/adr-ultimate-template-validation.md](../summaries/adr-ultimate-template-validation.md)** | 验证清单 |

---

## 🚀 快速开始

### 场景 1：编写新 ADR

**步骤**：

1. 复制 `ADR-TEMPLATE-ULTIMATE.md`
2. 阅读 `ADR-TEMPLATE-GUIDE.md`（至少前 3 节）
3. 填写六段式结构：
   - ✅ 先写 Rule（只写规则，≤ 1 页）
   - ✅ 再写 Enforcement（测试映射）
   - ✅ 然后写 Exception（破例机制）
   - ✅ 接着写 Change Policy（变更流程）
   - ✅ 标注 Non-Goals（不负责范围）
   - ✅ 最后写 References（术语、示例）
4. 使用质量检查清单验证（见 GUIDE 第 5 节）
5. 提交 PR

**注意**：

- ❌ 不要在 Rule 中写示例代码
- ❌ 不要用"建议/推荐/尽量"
- ❌ 不要写长背景故事
- ✅ 使用 MUST/MUST NOT
- ✅ Rule 段落 ≤ 1 页

---

### 场景 2：迁移现有 ADR

**步骤**：

1. 阅读 `ADR-TEMPLATE-MIGRATION.md`（特别是第 2-6 节）
2. 使用迁移清单（MIGRATION 第 4 节）
3. 按 6 个步骤迁移：
   - 步骤 1：提取硬性规则到 Rule
   - 步骤 2：构建执法映射到 Enforcement
   - 步骤 3：明确破例机制到 Exception
   - 步骤 4：定义变更政策到 Change Policy
   - 步骤 5：标注 Non-Goals
   - 步骤 6：整理 References
4. 使用质量检查清单验证（MIGRATION 第 7 节）
5. 提交 PR

**参考示例**：

- 查看 `governance/ADR-0000-architecture-tests-v2.md`
- 对比原文 `governance/ADR-0000-architecture-tests.md`

---

### 场景 3：审查 ADR

**步骤**：

1. 检查是否使用六段式结构
2. 检查 Rule 段落：
   - [ ] ≤ 1 页
   - [ ] 全部使用 MUST/MUST NOT
   - [ ] 没有示例代码
   - [ ] 没有"建议/推荐/尽量"
3. 检查 Enforcement 段落：
   - [ ] 标注了执行级别（L1/L2/L3）
   - [ ] 映射到具体测试
4. 检查 Exception 段落：
   - [ ] 明确破例前提
   - [ ] 明确破例要求
5. 检查 Change Policy 段落：
   - [ ] 明确变更流程
6. 检查 Non-Goals 段落：
   - [ ] 列出不负责的事项
7. 检查 References 段落：
   - [ ] 标注"非裁决性"

**完整清单**：见 `ADR-TEMPLATE-GUIDE.md` 第 5 节

---

## ⚠️ 写作红线（禁止事项）

**出现以下任一项，说明你在写"说明文"，不是 ADR：**

| 禁止事项 | 正确做法 |
|---------|---------|
| ❌ 长背景故事 | 背景放在 References |
| ❌ 方案对比表 | 方案对比放在 RFC 或历史文档 |
| ❌ 大段 Why 论证 | Why 放在 References |
| ❌ 示例代码 | 示例代码放在 `docs/copilot/*.prompts.md` |
| ❌ "建议 / 推荐 / 尽量" | 用 MUST / MUST NOT |
| ❌ 读者假设（"为了帮助理解…"） | ADR 不负责教学 |

**详细说明**：见 `ADR-TEMPLATE-GUIDE.md` 第 3 节

---

## 📋 质量检查清单

### Rule 段落

- [ ] 是否用 MUST / MUST NOT？
- [ ] 是否可判真伪？
- [ ] 是否独立于作者解释？
- [ ] 是否 ≤ 1 页？
- [ ] 是否没有"建议/推荐/尽量"？

### Enforcement 段落

- [ ] 是否标注了执行级别？
- [ ] 是否映射到具体测试/手段？
- [ ] 是否没有示例代码？

### Exception 段落

- [ ] 是否明确破例前提？
- [ ] 是否明确破例要求？
- [ ] 是否要求记录在 ARCH-VIOLATIONS.md？

### Change Policy 段落

- [ ] 是否明确变更流程？
- [ ] 是否明确失效机制？

### Non-Goals 段落

- [ ] 是否明确列出不负责的事项？
- [ ] 是否防止 ADR 膨胀？

### References 段落

- [ ] 是否标注"非裁决性"？
- [ ] 是否仅列出相关资料？

### 红线检查

- [ ] 是否没有长背景故事？
- [ ] 是否没有方案对比表？
- [ ] 是否没有大段 Why 论证？
- [ ] 是否没有示例代码？
- [ ] 是否没有"建议/推荐/尽量"？
- [ ] 是否没有读者假设？

**完整清单**：见 `ADR-TEMPLATE-GUIDE.md` 第 5 节

---

## 🎓 学习路径

### 初学者（0-1 小时）

1. **阅读**：`ADR-TEMPLATE-ULTIMATE.md`（5 分钟）
2. **阅读**：`ADR-TEMPLATE-GUIDE.md` 第 1-2 节（10 分钟）
3. **查看示例**：`governance/ADR-0000-architecture-tests-v2.md`（10 分钟）
4. **尝试编写**：选择一个简单的 ADR 练习（30 分钟）

### 中级用户（1-2 小时）

1. **完整阅读**：`ADR-TEMPLATE-GUIDE.md`（20 分钟）
2. **阅读**：`ADR-TEMPLATE-MIGRATION.md` 第 2-6 节（20 分钟）
3. **尝试迁移**：选择一个现有 ADR 练习迁移（40 分钟）
4. **使用清单**：验证迁移质量（20 分钟）

### 高级用户（2+ 小时）

1. **完整阅读**：所有文档（50 分钟）
2. **实战迁移**：迁移宪法层或治理层 ADR（60 分钟）
3. **质量审查**：审查其他人的 ADR（30 分钟）
4. **反馈优化**：提出改进建议（可选）

---

## 💡 常见问题

### Q1：必须立即使用终极模板吗？

**A**：

- ✅ 新 ADR：是，必须使用终极模板
- 📋 现有 ADR：可以渐进迁移，优先迁移宪法层和治理层

### Q2：Rule 段落一定要 ≤ 1 页吗？

**A**：是的。超过 1 页说明你在写设计说明，不是规则。应该：

- 提炼核心规则
- 将解释移至 References
- 将示例移至 Copilot prompts

### Q3：可以在 Rule 中写示例代码吗？

**A**：不可以。示例代码应该：

- 放在 `docs/copilot/*.prompts.md`（教学示例）
- 或放在 References 段落（参考示例）
- Rule 只写规则，不写示例

### Q4：Enforcement 段落必须有测试吗？

**A**：是的。规则如果无法执法，就不配存在。应该：

- L1：自动化测试（CI 阻断）
- L2：Analyzer / 启发式（人工复核）
- L3：Review / Checklist（架构裁决）

### Q5：可以省略 Exception 或 Non-Goals 段落吗？

**A**：不可以。即使没有内容，也要明确说明：

- Exception：说明"不允许破例"或明确破例条件
- Non-Goals：说明"本 ADR 不负责 X、Y、Z"

### Q6：References 段落有裁决力吗？

**A**：没有。References 仅供理解，不具裁决力。必须明确标注"非裁决性"。

---

## 🔗 相关资源

### 内部文档

- [ADR 主 README](README.md)
- [宪法层 README](constitutional/README.md)
- [治理层 README](governance/README.md)
- [Copilot 系统](../copilot/README.md)

### 实施文档

- [实施总结](../summaries/adr-ultimate-template-implementation.md)
- [验证清单](../summaries/adr-ultimate-template-validation.md)

### 测试

- 架构测试：`src/tests/ArchitectureTests/`
- 测试结果：77/77 通过 ✅

---

## ✅ 现在可以开始

你现在可以：

1. ✅ 使用终极模板编写新 ADR
2. ✅ 按迁移清单迁移现有 ADR
3. ✅ 拒绝不符合模板的 ADR

**记住**：凡是不适合放进这个模板的内容，都不配叫 ADR。

---

## 📞 获取帮助

### 遇到问题？

1. 查看 `ADR-TEMPLATE-GUIDE.md` 常见问题（第 7 节）
2. 查看 `ADR-TEMPLATE-MIGRATION.md` 常见问题（第 7 节）
3. 查看迁移示例 `governance/ADR-0000-architecture-tests-v2.md`
4. 咨询架构师或 Tech Lead

### 发现模板问题？

1. 在 GitHub Issues 中提出
2. 说明具体场景和问题
3. 提供改进建议

---

**最后更新**：2026-01-23  
**版本**：1.0  
**状态**：✅ 可正式使用
