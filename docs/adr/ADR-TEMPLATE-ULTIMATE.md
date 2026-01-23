# ADR-XXXX：〈裁决型标题〉

**状态**：Draft | Accepted | Final | Superseded  
**级别**：宪法层 | 决策层  
**影响范围**：〈明确列出模块 / 层 / 仓库〉  
**生效时间**：YYYY-MM-DD

---

## 1. Rule（规则本体｜裁决源）

> **这是本 ADR 唯一具有裁决力的部分。**

用**不可协商、可判真伪**的句子描述规则。

**必须满足：**

* 不依赖作者解释
* 不引用实现细节
* 不包含"建议 / 通常 / 尽量 / 可以"

**推荐格式：**

* ❗ MUST / MUST NOT / SHALL / SHALL NOT
* ❗ 违反即为架构违规

**示例：**

* Platform **MUST NOT** depend on Application or Host
* Modules **MUST NOT** reference each other directly
* All ADRs marked **【必须架构测试覆盖】** **MUST** have at least one architecture test

> Rule 段落 **≤ 1 页**，超过就是设计说明，不是 ADR。

---

## 2. Enforcement（执法模型）

> **规则如果无法执法，就不配存在。**

### 2.1 执行级别

| Level | 名称      | 执法方式               | 后果    |
| ----- | ------- | ------------------ | ----- |
| L1    | 静态可执行   | 自动化测试              | CI 阻断 |
| L2    | 语义半自动   | Analyzer / 启发式     | 人工复核  |
| L3    | 人工 Gate | Review / Checklist | 架构裁决  |

**每一条 Rule 必须标注 Level。**

---

### 2.2 测试映射（如适用）

> 仅列出**裁决级**测试，不列示例。

| Rule 编号    | 执行级 | 测试 / 手段                        |
| ---------- | --- | ------------------------------ |
| ADR-XXXX.1 | L1  | `XXX_Should_Not_Depend_On_YYY` |
| ADR-XXXX.2 | L2  | Roslyn Analyzer / Review       |
| ADR-XXXX.3 | L3  | ARCH-GATE                      |

---

## 3. Exception（破例与归还）

> **破例不是逃避，而是债务。**

### 3.1 允许破例的前提

破例 **仅在以下情况允许**：

* 技术限制
* 性能约束
* 迁移期遗留问题
* 外部系统强制约束

---

### 3.2 破例要求（不可省略）

每个破例 **必须**：

* 记录在 `ARCH-VIOLATIONS.md`
* 指明 ADR 编号 + Rule 编号
* 指定失效日期
* 给出归还计划

**未记录的破例 = 未授权架构违规。**

---

## 4. Change Policy（变更政策）

> **ADR 不是"随时可改"的文档。**

### 4.1 变更规则

* **宪法层 ADR**

  * 修改 = 架构修宪
  * 需要显式评审 + 全量回归测试

* **决策层 ADR**

  * 可 Superseded
  * 不得悄然修改 Rule

---

### 4.2 失效与替代

* Superseded ADR **必须**指向替代 ADR
* 不允许"隐性废弃"

---

## 5. Non-Goals（明确不管什么）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

* 代码风格
* 命名美学
* 教学示例
* 实现细节
* 临时约定

---

## 6. References（非裁决性）

> **仅供理解，不具裁决力。**

* 相关 ADR
* 规范文档
* 背景材料
* 历史讨论

---

# 附：ADR 写作红线（强制）

**出现以下任一项，说明你在写"说明文"，不是 ADR：**

* 长背景故事
* 方案对比表
* 大段 Why 论证
* 示例代码
* "建议 / 推荐 / 尽量"
* 读者假设（"为了帮助理解…"）

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
