# ADR-0004：中央包管理（CPM）规范

**状态**：Final  
**级别**：宪法层  
**影响范围**：所有 Platform / Application / Modules / Host / Tests 项目  
**生效时间**：即刻

---

## 规则本体（Rule）

> **这是本 ADR 唯一具有裁决力的部分。**

### R1 CPM 启用

所有项目**必须**：
- 启用中央包管理（CPM）
- 在 `Directory.Packages.props` 中集中管理所有包版本
- 项目文件中**禁止**出现 `Version` 属性

### R2 层级依赖约束

各层**必须**遵守以下依赖约束：

| 层级        | 允许依赖包类型                                  | 禁止依赖           |
|-------------|-----------------------------------------------|--------------------|
| Platform    | 技术底座包（Logging、OpenTelemetry、基础异常处理等） | Application/Module/Host 的业务包 |
| Application | Wolverine、Marten、所有装配与 Pipeline          | Host/Http/业务模块 |
| Modules     | 业务依赖、DTO、协议、契约                       | Platform 内部包、Host、其它模块 |
| Host        | 仅调用 Platform+Application Bootstrapper        | 业务模块、Handler  |
| Tests       | 被测模块+Platform/Application                  | Host 内部实现      |

### R3 版本唯一性

同一个包**必须**：
- 在整个解决方案中只有一个版本
- 所有引用该包的项目使用相同版本
- 版本定义在 `Directory.Packages.props` 中

### R4 包分组管理

`Directory.Packages.props` **必须**：
- 按功能或技术栈分组
- 使用注释标记分组（如 `<!-- Logging -->`）
- 保持清晰的组织结构

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
| R4.1    | L1  | `CPM_Should_Be_Enabled` |
| R4.1    | L1  | `Repository_Should_Have_Directory_Packages_Props` |
| R4.1    | L1  | `Projects_Should_Not_Specify_Package_Versions` |
| R4.2    | L1  | `Layer_Package_Dependencies_Should_Be_Valid` |
| R4.2    | L1  | `Platform_Projects_Should_Not_Reference_Business_Packages` |
| R4.3    | L1  | `All_Test_Projects_Should_Use_Same_Test_Framework_Versions` |
| R4.3    | L1  | `Projects_Should_Not_Override_Central_Package_Versions` |
| R4.4    | L1  | `Directory_Packages_Props_Should_Contain_Package_Groups` |
| R4.4    | L1  | `Directory_Packages_Props_Should_Define_All_Used_Packages` |

### 测试位置

所有架构测试位于：`src/tests/ArchitectureTests/ADR/ADR_0004_Architecture_Tests.cs`

---

## 破例与归还（Exception）

> **破例不是逃避，而是债务。**

### 允许破例的前提

破例 **仅在以下情况允许**：

* 迁移期遗留代码（必须在 6 个月内归还）
* 第三方库的技术限制（需架构委员会审批）
* 实验性功能的隔离测试（需 Tech Lead 审批）

### 破例要求（不可省略）

每个破例 **必须**：

* 记录在 `docs/summaries/ARCH-VIOLATIONS.md`
* 指明 ADR-0004 + 规则编号（如 R4.1）
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
* Copilot prompts 文件（`docs/copilot/adr-0004.prompts.md`）
* 映射脚本
* README 导航

---

## 明确不管什么（Non-Goals）

> **防止 ADR 膨胀的关键段落。**

本 ADR **不负责**：

* 具体使用哪个第三方包 → ADR-300+ 技术层
* 包的升级策略 → 工程实践
* 依赖项漏洞扫描 → 安全团队
* 包许可证合规性 → 法务团队
* 教学示例和最佳实践 → `docs/copilot/adr-0004.prompts.md`

---

## 非裁决性参考（References）

> **仅供理解，不具裁决力。**

### 术语表

| 术语            | 定义说明 |
|----------------|--------------------------------------|
| CPM            | Central Package Management，中央包管理 |
| Directory.Packages.props | NuGet 配置文件，集中定义全局依赖包版本 |
| 传递依赖固定    | 通过 CPM 禁止传递依赖的漂移 |
| 层级依赖        | 不同层项目允许的包类型约束 |
| 包分组          | 依赖包按技术栈、场景分隔分组 |

### 相关 ADR

- [ADR-0000：架构测试与 CI 治理](../governance/ADR-0000-architecture-tests.md)
- [ADR-0001：模块化单体与垂直切片架构](ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [ADR-0002：Platform/Application/Host 启动体系](ADR-0002-platform-application-host-bootstrap.md)
- [ADR-0003：命名空间与项目边界规范](ADR-0003-namespace-rules.md)

### 辅导材料

- `docs/copilot/adr-0004.prompts.md` - 示例代码和常见问题

### Directory.Packages.props 示例

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- Logging -->
    <PackageVersion Include="Serilog" Version="3.1.1" />
    <PackageVersion Include="Serilog.Sinks.Console" Version="5.0.1" />
    
    <!-- OpenTelemetry -->
    <PackageVersion Include="OpenTelemetry" Version="1.7.0" />
    <PackageVersion Include="OpenTelemetry.Extensions.Hosting" Version="1.7.0" />
    
    <!-- Wolverine -->
    <PackageVersion Include="Wolverine" Version="2.4.0" />
    <PackageVersion Include="Wolverine.Http" Version="2.4.0" />
    
    <!-- Marten -->
    <PackageVersion Include="Marten" Version="7.0.0" />
    <PackageVersion Include="Marten.AspNetCore" Version="7.0.0" />
    
    <!-- Testing -->
    <PackageVersion Include="xunit" Version="2.6.6" />
    <PackageVersion Include="FluentAssertions" Version="6.12.0" />
    <PackageVersion Include="NetArchTest.Rules" Version="1.3.2" />
  </ItemGroup>
</Project>
```

### 项目文件示例

```xml
<!-- ✅ 正确：不指定 Version -->
<ItemGroup>
  <PackageReference Include="Serilog" />
  <PackageReference Include="Wolverine" />
</ItemGroup>

<!-- ❌ 错误：指定了 Version -->
<ItemGroup>
  <PackageReference Include="Serilog" Version="3.1.1" />
  <PackageReference Include="Wolverine" Version="2.4.0" />
</ItemGroup>
```

### 检查清单

开发时自检：

- [ ] 是否使用 Directory.Packages.props 管理所有包？
- [ ] 项目文件无手动 Version？
- [ ] 各层引入的依赖包完全符合层级规则？
- [ ] 所有项目依赖包都在集中声明？
- [ ] 相关架构测试和 CI 校验被正确拦截？

### 版本历史

| 版本 | 日期 | 变更摘要 |
|------|------|---------|
| 4.0 | 2026-01-24 | 采用终极模板，明确规则与执法分离 |
| 3.0 | 2026-01-22 | 结构升级、统一结构和映射 |
| 2.0 | 2026-01-20 | 分组细化，新增 CI 校验 |
| 1.0 | 初版 | 初始发布 |

---

# ADR 终极一句话定义

> **ADR 是系统的法律条文，不是架构师的解释说明。**
