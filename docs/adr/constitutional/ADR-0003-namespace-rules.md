# ADR-0003：命名空间与项目边界规范

**状态**：Final  
**级别**：宪法层  
**影响范围**：所有 Platform / Application / Modules / Host / Tests 项目  
**生效时间**：即刻

---

## 规则本体（Rule）

> **这是本 ADR 唯一具有裁决力的部分。**

### R3.1 BaseNamespace 定义

所有项目**必须**：
- 通过 `Directory.Build.props` 定义 BaseNamespace
- BaseNamespace = `Zss.BilliardHall`
- 不得在单个项目中覆盖 BaseNamespace

### R3.2 命名空间自动推导

项目 RootNamespace **必须**：
- 由 BaseNamespace + 目录路径自动推导
- Platform：`Zss.BilliardHall.Platform`
- Application：`Zss.BilliardHall.Application`
- Modules：`Zss.BilliardHall.Modules.{ModuleName}`
- Host：`Zss.BilliardHall.Host.{HostName}`

### R3.3 命名空间与目录一致性

命名空间**必须**：
- 与目录结构严格对应
- 所有类型的命名空间以 BaseNamespace 开头
- 禁止手动指定 RootNamespace（MSBuild 自动推导）

### R3.4 禁止不规范命名

**禁止**使用以下命名空间：
- `Common`
- `Shared`
- `Utils`
- `Helpers`
- 任何与目录结构不对应的命名空间

---

## 执法模型（Enforcement）

> **规则如果无法执法，就不配存在。**

### 执行级别

| 级别 | 名称      | 执法方式               | 后果    |
| ---- | ------- | ------------------ | ----- |
| L1   | 静态可执行   | 自动化测试（NetArchTest） | CI 阻断 |
| L2   | 语义半自动   | MSBuild 验证 | 构建失败  |
| L3   | 人工 Gate | Code Review / Checklist | 架构裁决  |

### 测试映射

| 规则编号 | 执行级 | 测试 / 手段                        |
| ------- | --- | ------------------------------ |
| R3.1    | L1  | `Directory_Build_Props_Should_Exist_At_Repository_Root` |
| R3.1    | L1  | `Directory_Build_Props_Should_Define_Base_Namespace` |
| R3.2    | L1  | `All_Types_Should_Start_With_Base_Namespace` |
| R3.2    | L1  | `Platform_Types_Should_Have_Platform_Namespace` |
| R3.2    | L1  | `Application_Types_Should_Have_Application_Namespace` |
| R3.2    | L1  | `Module_Types_Should_Have_Module_Namespace` |
| R3.2    | L1  | `Host_Types_Should_Have_Host_Namespace` |
| R3.3    | L1  | `All_Projects_Should_Follow_Namespace_Convention` |
| R3.4    | L1  | `Modules_Should_Not_Contain_Irregular_Namespace_Patterns` |

### 测试位置

所有架构测试位于：`src/tests/ArchitectureTests/ADR/ADR_0003_Architecture_Tests.cs`

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例 **仅在以下情况允许**：

* 迁移期遗留代码（必须在 6 个月内归还）
* 第三方库强制要求的命名空间（需架构委员会审批）
* 测试项目的特殊需求（需 Tech Lead 审批）

### 破例要求（不可省略）

每个破例 **必须**：

* 记录在 `docs/summaries/ARCH-VIOLATIONS.md`
* 指明 ADR-0003 + 规则编号（如 R3.2）
* 指定失效日期（不超过 6 个月）
* 给出归还计划（具体到季度）

**未记录的破例 = 未授权架构违规。**

---

## 变更政策（Change Policy）

> **ADR 不是"随时可改"的文档。**

### 变更规则

* **宪法层 ADR**（ADR-0001~0005）

  * 修改 = 架构修宪
  * 需要架构委员会 100% 同意
  * 需要 2 周公示期
  * 需要全量回归测试

### 失效与替代

* Superseded ADR **必须**：
  - 状态标记为 "Superseded by ADR-YYYY"
  - 指向替代 ADR
  - 保留在仓库中（不删除）
  - 移除或更新对应测试

* 不允许"隐性废弃"（偷偷删除或不标记状态）

### 同步更新

ADR 变更时 **必须** 同步更新：

* 架构测试代码
* Copilot prompts 文件（`docs/copilot/adr-0003.prompts.md`）
* 映射脚本
* README 导航

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

* 类型命名规范（如是否使用 I 前缀）→ `.editorconfig`
* 文件命名规范 → 团队约定
* 代码风格和格式化 → `.editorconfig`
* 具体 MSBuild 实现细节 → 工程实践
* 教学示例和最佳实践 → `docs/copilot/adr-0003.prompts.md`

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

### 术语表

| 术语            | 定义说明 |
|----------------|--------------------------------------|
| BaseNamespace  | 公司+产品根命名空间（如 `Zss.BilliardHall`） |
| RootNamespace  | 项目根命名空间，由 BaseNamespace 及目录自动推导 |
| Directory.Build.props | MSBuild 全局配置文件，统一定义 BaseNamespace |
| MSBuild 推导   | 通过 MSBuild 条件和目录映射自动赋值 RootNamespace |

### 相关 ADR

- [ADR-0000：架构测试与 CI 治理](../governance/ADR-0000-architecture-tests.md)
- [ADR-0001：模块化单体与垂直切片架构](ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0002：Platform/Application/Host 启动体系](ADR-0002-platform-application-host-bootstrap.md)
- [ADR-0004：中央包管理与依赖](ADR-0004-Cpm-Final.md)

### 辅导材料

- `docs/copilot/adr-0003.prompts.md` - 示例代码和常见问题

### Directory.Build.props 示例

```xml
<Project>
  <PropertyGroup>
    <CompanyNamespace>Zss</CompanyNamespace>
    <ProductNamespace>BilliardHall</ProductNamespace>
    <BaseNamespace>$(CompanyNamespace).$(ProductNamespace)</BaseNamespace>
  </PropertyGroup>
</Project>
```

### 项目 RootNamespace 示例

```xml
<!-- Platform 项目 -->
<PropertyGroup>
  <RootNamespace>$(BaseNamespace).Platform</RootNamespace>
</PropertyGroup>

<!-- Application 项目 -->
<PropertyGroup>
  <RootNamespace>$(BaseNamespace).Application</RootNamespace>
</PropertyGroup>

<!-- Module 项目 -->
<PropertyGroup>
  <RootNamespace>$(BaseNamespace).Modules.Members</RootNamespace>
</PropertyGroup>

<!-- Host 项目 -->
<PropertyGroup>
  <RootNamespace>$(BaseNamespace).Host.Web</RootNamespace>
</PropertyGroup>
```

### 命名空间映射示例

```
目录结构                      命名空间
/src/Platform/               Zss.BilliardHall.Platform
/src/Application/            Zss.BilliardHall.Application
/src/Modules/Members/        Zss.BilliardHall.Modules.Members
/src/Modules/Orders/         Zss.BilliardHall.Modules.Orders
/src/Host/Web/               Zss.BilliardHall.Host.Web
/src/Host/Worker/            Zss.BilliardHall.Host.Worker
```

### 检查清单

开发时自检：

- [ ] 是否用 Directory.Build.props 统一 BaseNamespace？
- [ ] 根命名空间是否由目录自动推导？
- [ ] 项目名与目录/二级命名空间是否严格一致？
- [ ] 全局无 Common、Shared、Utils 等命名空间？
- [ ] CI 与架构测试是否已自动检验命名空间合规？

### 版本历史

| 版本 | 日期 | 变更摘要 |
|------|------|---------|
| 4.0 | 2026-01-24 | 采用终极模板，明确规则与执法分离 |
| 3.0 | 2026-01-22 | 完全去编号、结构升级 |
| 2.0 | 2026-01-20 | 机制细化 |
| 1.0 | 初版 | 初始发布 |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
