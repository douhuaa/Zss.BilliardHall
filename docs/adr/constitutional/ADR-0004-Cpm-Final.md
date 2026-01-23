# ADR-0004：中央包管理（CPM）规范

**状态**：✅ 已采纳（Final，不可随意修改）  
**级别**：架构约束（Architectural Contract）  
**适用范围**：所有 Platform / Application / Modules / Host / Tests 项目  
**生效时间**：即刻  

---

## 本章聚焦内容（Focus）

本 ADR 是**静态结构层**的核心文档，聚焦于：

1. **Directory.Packages.props 集中管理**：如何统一管理所有包版本
2. **层级依赖规则**：各层允许和禁止的依赖包类型
3. **包分组策略**：如何组织和管理包列表
4. **防御性规则**：如何防止手动覆盖包版本
5. **架构测试映射**：如何自动化校验依赖规则

**不涉及**：
- ❌ 启动体系职责（见 ADR-0002）
- ❌ 命名空间规范（见 ADR-0003）
- ❌ 运行时交互模型（见 ADR-0005）
- ❌ 模块内部组织（见 ADR-0001）

---

## 术语表（Glossary）

| 术语                              | 定义                                                                 |
|-----------------------------------|----------------------------------------------------------------------|
| CPM（Central Package Management） | 中央包管理，统一管理所有项目的包版本                                 |
| Directory.Packages.props          | NuGet 中央包管理配置文件，定义所有包版本                             |
| 传递依赖固定                      | CentralPackageTransitivePinningEnabled，固定传递依赖版本             |
| 层级依赖规则                      | 各层（Platform/Application/Modules/Host）允许的依赖包类型            |
| 包分组                            | 按功能或技术栈对包进行逻辑分组                                       |

---

## 1️⃣ 背景与目标

在中大型系统中，包版本漂移、依赖混乱是架构事故高发区。中央包管理（CPM）可以：

* 保证所有子项目统一使用同一版本
* 避免间接依赖升级导致破坏性变更
* 支撑 Host / Application / Platform / Modules 分层架构约束
* 支撑 CI 自动校验

**目标**：

1. 所有项目统一管理包版本，禁止手动覆盖
2. 明确每个层级允许使用的依赖包列表
3. 支持跨模块、跨 Host 可安全升级
4. 与 ADR-0002 / ADR-0003 配合，保证启动三层 + 命名空间 + 依赖管理完整性

---

## 2️⃣ 核心约束

### 2.1 所有项目必须使用中央包管理 **【必须架构测试覆盖】**

在 `Directory.Packages.props` 中定义版本：

* `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`
* `<CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>`

禁止单个项目手动指定版本。

### 2.2 分层依赖规则 **【必须架构测试覆盖】**

| 层级          | 可用包类型                                    | 禁止依赖                             |
|-------------|------------------------------------------|----------------------------------|
| Platform    | Logging、Tracing、OpenTelemetry、ErrorModel | Application / Module / Host 的业务包 |
| Application | Wolverine、Marten、Pipeline、Policy         | HttpContext、Host 类型判断            |
| Modules     | 业务依赖、Application 服务                      | Platform 内部实现、Host、其他模块业务逻辑直接引用  |
| Host        | Platform / Application                   | 业务模块逻辑、Handler                   |
| Tests       | 被测模块 + Platform / Application            | Host 内部逻辑                        |

> 任何违规引用需在 CI 校验阶段失败。

### 2.3 包分组示例（参考现有 CPM） **【必须架构测试覆盖】**

```xml
<ItemGroup Label="Aspire">
    <PackageVersion Include="Aspire.Hosting.PostgreSQL" Version="13.1.0" />
    <PackageVersion Include="Aspire.Hosting.Testing" Version="13.1.0" />
</ItemGroup>
<ItemGroup Label="Wolverine Framework">
    <PackageVersion Include="WolverineFx.Http" Version="5.9.2" />
    <PackageVersion Include="WolverineFx.Http.FluentValidation" Version="5.9.2" />
</ItemGroup>
<ItemGroup Label="Marten Document Database">
    <PackageVersion Include="Marten" Version="8.17.0" />
</ItemGroup>
<ItemGroup Label="Logging">
    <PackageVersion Include="Serilog.AspNetCore" Version="10.0.0" />
</ItemGroup>
<ItemGroup Label="Testing">
    <PackageVersion Include="xunit" Version="2.9.3" />
</ItemGroup>
```

> 可根据项目需要调整包列表，但必须严格遵循层级依赖规则。

### 2.4 防御性规则 **【必须架构测试覆盖】**

1. CI 阶段检查：禁止项目手动覆盖包版本。
2. CI 阶段检查：禁止层级越界引用包。
3. 所有包升级必须修改 `Directory.Packages.props`，并经过架构审查。

---

## 3️⃣ 强化与测试

所有依赖规则必须通过自动化架构测试验证。

**架构测试详见**：[ADR-0000：架构测试与 CI 治理](ADR-0000-architecture-tests.md)

**核心测试用例**：
- Platform 层不依赖 Application/Modules/Host 的业务包
- Application 层只依赖 Platform + 自身模块
- Modules 层不依赖其他模块业务逻辑
- Host 层只依赖 Platform + Application
- Tests 层只依赖被测模块 + Platform / Application
- 所有项目未手动指定包版本

---

## 4️⃣ 例外与限制

* 特殊情况必须提交 ADR 批准
* 禁止在子项目中私改包版本或添加未批准依赖
* 禁止跨层级调用违禁包

---

## 5️⃣ 最终裁决

* ADR-0004 与 ADR-0002 / ADR-0003 配套，形成完整启动 + 命名空间 + 包管理约束体系
* 所有项目必须遵循中央包管理和层级依赖规则
* CI 校验失败视为架构缺陷
* 任何绕过行为均视为违规

---

## 6️⃣ 后续行动建议

1. 将 ADR-0004 纳入仓库文档目录
2. 将现有 `Directory.Packages.props` 固定为模板，不允许随意更改
3. 在 CI 中加入自动依赖校验
4. 将包管理规则写入团队 onboarding 文档

✅ ADR-0004 Final 完整版完毕

---

## 与其他 ADR 关系（Related ADRs）

| ADR        | 关系                                           |
|------------|------------------------------------------------|
| ADR-0000   | 定义本 ADR 的自动化测试机制                    |
| ADR-0001   | 定义模块组织，本 ADR 定义模块依赖规则          |
| ADR-0002   | 配合定义层级依赖方向                           |
| ADR-0003   | 定义命名空间规范                               |
| ADR-0005   | 定义运行时交互模型                             |

**依赖关系**：
- 本 ADR 定义依赖包管理规范
- ADR-0002 的层级约束需要本 ADR 的依赖规则支撑
- 所有层级的包依赖必须符合本 ADR 的规则

---

## 快速参考表（Quick Reference Table）

| 约束编号 | 约束描述 | 必须测试 | 测试覆盖 | ADR 章节 |
|---------|---------|---------|---------|---------|
| ADR-0004.1 | Directory.Packages.props 应存在于仓库根目录 | ✅ | Repository_Should_Have_Directory_Packages_Props | 2.1, 2.4, 3 |
| ADR-0004.2 | CPM 应被启用 (ManagePackageVersionsCentrally=true) | ✅ | CPM_Should_Be_Enabled | 2.1, 2.4, 3 |
| ADR-0004.3 | CPM 应启用传递依赖固定 (CentralPackageTransitivePinningEnabled) | ✅ | CPM_Should_Enable_Transitive_Pinning | 2.1, 3 |
| ADR-0004.4 | 项目文件不应手动指定包版本 | ✅ | Projects_Should_Not_Specify_Package_Versions | 2.1, 2.4, 3 |
| ADR-0004.5 | Directory.Packages.props 应包含包分组 (Label) | ✅ | Directory_Packages_Props_Should_Contain_Package_Groups | 2.3, 3 |
| ADR-0004.6 | Directory.Packages.props 应包含常见包分组 | ✅ | Directory_Packages_Props_Should_Contain_Common_Package_Groups | 2.3, 3 |
| ADR-0004.7 | Platform 项目不应引用业务包 | ✅ | Platform_Projects_Should_Not_Reference_Business_Packages | 2.2, 2.4, 3 |
| ADR-0004.8 | 所有测试项目应使用相同的测试框架版本 | ✅ | All_Test_Projects_Should_Use_Same_Test_Framework_Versions | 2.4, 3 |
| ADR-0004.9 | Directory.Packages.props 应定义所有项目使用的包 | ✅ | Directory_Packages_Props_Should_Define_All_Used_Packages | 2.1, 2.4, 3 |

---

## 快速参考（Quick Reference）

### CPM 配置检查清单

- [ ] 是否启用了 `ManagePackageVersionsCentrally`？
- [ ] 是否启用了 `CentralPackageTransitivePinningEnabled`？
- [ ] 所有包版本是否在 Directory.Packages.props 中定义？
- [ ] 项目文件是否未手动指定版本号？

### 层级依赖检查清单

| 层级        | 允许依赖的包类型                              |
|-------------|-----------------------------------------------|
| Platform    | Logging、Tracing、OpenTelemetry、ErrorModel   |
| Application | Wolverine、Marten、Pipeline、Policy           |
| Modules     | 业务依赖、Application 服务                    |
| Host        | Platform / Application（只调用 Bootstrapper） |
| Tests       | 被测模块 + Platform / Application             |

### 常见错误

| 错误                              | 正确做法                          |
|-----------------------------------|-----------------------------------|
| 项目中指定 Version                | 删除 Version，使用 CPM            |
| Platform 依赖 Application 的包    | 移除违规依赖                      |
| Module 直接依赖其他 Module        | 通过契约或事件通信                |
| Host 直接依赖 Module Handler      | 只依赖 Platform + Application     |
