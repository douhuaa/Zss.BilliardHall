本项目采用“模块化单体 + 垂直切片架构”，所有开发、测试、部署需**严格遵循**架构决策记录（ADR）及技术约束。下方为统一的项目 instructions 草案，对开发流程、约束与协作标准进行前置声明和明确界定。建议将本 instructions 固化到仓库顶层或 docs 目录，所有开发/运维成员务必熟读，违例即视为架构违规。

---

## 1. 架构约束与 ADR

### 核心 ADR

| ADR 编号 | 描述 | 对应测试类 |
|----------|------|------------|
| ADR-0001 | 模块化单体 + 垂直切片 | `ADR_0001_Architecture_Tests` |
| ADR-0002 | Platform / Application / Host 三层启动体系 | `ADR_0002_Architecture_Tests` |
| ADR-0003 | 命名空间与项目边界规范 | `ADR_0003_Architecture_Tests` |
| ADR-0004 | 中央包管理（CPM） | `ADR_0004_Architecture_Tests` |
| ADR-0005 | 应用内交互模型与执行边界 | `ADR_0005_Architecture_Tests` |

> 所有架构规则均自动化校验，CI 失败即禁止合并。

### 违例标记

- PR/Commit 标题或描述中必须加 `[ARCH-VIOLATION][ADR-XXXX]`
- 破例需明确 **偿还时间**，CI 自动检测到期未修复将阻断合并。

---

## 文件与目录结构

- 目录约定（详见 ADR-0002/0003）：
  ```
  /src
      /Platform      → 技术基座
      /Application   → 系统装配层
      /Modules/XXX   → 业务功能分模块，垂直切片
      /Host/YYY      → 进程、协议外壳
      /Tests         → 与被测模块一一对应
  ```
- **子项目名称 = 目录尾部名称**，禁止随意命名或覆盖 RootNamespace。

---

## 提交与分支管理

- 所有提交信息需采用 [Conventional Commits](https://www.conventionalcommits.org/zh-hans/v1.0.0/)，如：
  ```
  feat(Members): 新增余额充值命令及 Handler
  fix(ADR-0001): 修复模块间非法依赖
  chore(deps): 升级 Platform 包版本
  ```
- 合并请求须包含：
    - 中文变更摘要
    - 涉及架构影响时，需清晰标注并说明影响范围
    - 如属于破例操作，PR/Commit Title 必须加 "[ARCH-VIOLATION]"，并在说明中给出偿还计划和失效期（详见 ADR-0001/0005）

---

## 模块与依赖约束

- **严格不得越界依赖**（详见 ADR-0003/0004）：
  - Platform → 自身（不依赖 Application/Modules/Host）
  - Application → Platform（不依赖 Host/Modules）
  - Modules → 仅依赖 Platform / BuildingBlocks / Contract，禁止直接引用其他业务模块
  - Host → 不包含业务逻辑

- 依赖包统一在 `Directory.Packages.props` 管理，禁止子项目私自指定或升级版本。

---

## 配置与机密管理

- 项目代码**严禁**硬编码密码、密钥、连接串
- 配置推荐使用 User Secrets 或 KeyVault/Config Server 等安全手段
- 配置变更需同步更新默认值文档与 onboarding 指南

---

## 测试与 CI/CD

- 架构校验、依赖分析、契约使用，均纳入 CI 自动化流程
- CI 不通过禁止合并
- 所有业务功能变更/新增必须配套测试用例（单元/集成测试）

---

## 破例与豁免

- 任何不可避免的架构豁免，**必须新增 ADR、标记 PR，并声明失效期**
- 破例需项目 Owner 和架构负责人共同审批

---

## 协作与沟通

- 变更、设计、技术决策务必在 Issues/PR/ADR 中留痕
- 对架构规范有质疑，可提 Issue 讨论，需达成团队共识

---

## 新成员指引

- 强烈建议新人先阅读本 instructions 及全部 ADR
- 推荐先了解项目前两层（Platform/Application），逐步进入业务模块开发

---
## 执行与落地原则
- 一一对应：每条 ADR 对应唯一测试类
- 可验证：架构规则转化为自动化测试
- CI 阻断：测试失败 = 构建失败 = PR 阻断
- 可追溯：输出违规 ADR + 修复建议
- 可演进：新增 ADR 必须新增对应测试类

**🚫 一切与上述 instructions 冲突的实践，默认无效；变更规范只能通过团队共识和 ADR 修订！**