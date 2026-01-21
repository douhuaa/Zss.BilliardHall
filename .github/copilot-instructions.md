本项目采用“模块化单体 + 垂直切片架构”，所有开发、测试、部署需**严格遵循**架构决策记录（ADR）及技术约束。如你执行结构优化，旧文档应系统性迁移、存档和替换，避免“文档分叉”和团队认知混乱。务求收敛至唯一权威入口，历史版本便于溯源。

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

### 启动三层职责

- **Platform**：技术世界初始化（Logging、Tracing、HealthChecks 等）
- **Application**：系统定义者（模块注册、业务能力拼装）
- **Host**：纯装配（选择运行形态，调用 Platform 和 Application）

唯一合法依赖方向：`Host → Application → Platform`。

反向依赖 = 架构违规。

---

## 文件与目录结构

- 目录约定（详见 ADR-0002）：
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

## 模块与依赖约束

- **严格不得越界依赖**（详见 ADR-0003/0004）：
  - Platform → 自身（不依赖 Application/Modules/Host）
  - Application → Platform（不依赖 Host/Modules）
  - Modules → 仅依赖 Platform / BuildingBlocks / Contract，禁止直接引用其他业务模块
  - Host → 不包含业务逻辑，仅引用 Application 和 Platform。

- Modules 必须完整 Vertical Slice。

---

## 测试与 CI/CD

- 架构校验、依赖分析、契约使用，均纳入 CI 自动化流程。
- CI 不通过禁止合并。
- 所有业务功能变更/新增必须配套测试用例（单元/集成测试）。
- 启动三层相关测试示例：
  ```csharp
  public class HostBoundaryTests
  {
      [Fact]
      public void Host_Should_Not_Contain_Business_Types() =>
          Types.InAssembly(typeof(Program).Assembly)
              .ShouldNot().HaveNamespaceContaining("Modules")
              .GetResult().IsSuccessful.ShouldBeTrue();
  }
  ```

---

## 执行与落地原则

- 一一对应：每条 ADR 对应唯一测试类。
- 可验证：架构规则转化为自动化测试。
- CI 阻断：测试失败 = 构建失败 = PR 阻断。
- 可追溯：输出违规 ADR + 修复建议。
- 可演进：新增 ADR 必须新增对应测试类。
- **Program.cs 不超过 50 行**：超过即设计失败。

---

## PR 提交与分支管理

- 标题和内容均采用简体中文
- 所有提交信息需采用 [Conventional Commits](https://www.conventionalcommits.org/zh-hans/v1.0.0/)，如：
  ```
  feat(Members): 新增余额充值命令及 Handler
  fix(ADR-0001): 修复模块间非法依赖
  chore(deps): 升级 Platform 包版本
  ```
- 合并请求须包含：
  - 变更摘要
  - 涉及架构影响时，需清晰标注并说明影响范围
  - 如属于破例操作，PR/Commit Title 必须加 "[ARCH-VIOLATION]"，并在说明中给出偿还计划和失效期（详见 ADR-0001/0005）