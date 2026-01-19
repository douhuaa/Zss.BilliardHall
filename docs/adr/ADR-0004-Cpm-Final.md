# ADR-0004

## 中央包管理 (CPM) 规范（Final）

**状态**：✅ 已采纳（Final，不可随意修改）
**级别**：架构约束（Architectural Contract）
**适用范围**：所有 Platform / Application / Modules / Host / Tests 项目
**生效时间**：即刻

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

### 2.1 所有项目必须使用中央包管理

在 `Directory.Packages.props` 中定义版本：

* `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`
* `<CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>`

禁止单个项目手动指定版本。

### 2.2 分层依赖规则

| 层级          | 可用包类型                                    | 禁止依赖                             |
|-------------|------------------------------------------|----------------------------------|
| Platform    | Logging、Tracing、OpenTelemetry、ErrorModel | Application / Module / Host 的业务包 |
| Application | Wolverine、Marten、Pipeline、Policy         | HttpContext、Host 类型判断            |
| Modules     | 业务依赖、Application 服务                      | Platform 内部实现、Host、其他模块业务逻辑直接引用  |
| Host        | Platform / Application                   | 业务模块逻辑、Handler                   |
| Tests       | 被测模块 + Platform / Application            | Host 内部逻辑                        |

> 任何违规引用需在 CI 校验阶段失败。

### 2.3 包分组示例（参考现有 CPM）

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

### 2.4 防御性规则

1. CI 阶段检查：禁止项目手动覆盖包版本。
2. CI 阶段检查：禁止层级越界引用包。
3. 所有包升级必须修改 `Directory.Packages.props`，并经过架构审查。

---

## 3️⃣ 架构测试建议

使用 NetArchTest / Roslyn Analyzer：

* Platform 层项目不依赖 Application/Modules/Host 的业务包
* Application 层项目只依赖 Platform + 自身模块
* Modules 层项目不依赖其他模块业务逻辑
* Host 层项目只依赖 Platform + Application，不依赖业务模块 Handler
* Tests 层项目只依赖被测模块 + Platform / Application

示例伪代码：

```csharp
Types.InAssembly(typeof(SomeType).Assembly)
    .ShouldNot().HaveDependencyOn("Zss.BilliardHall.Modules.OtherModule")
    .GetResult().IsSuccessful.ShouldBeTrue();
```

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
