# ADR-0003：命名空间与项目边界规范

**状态**：✅ 已采纳（Final，不可随意修改）  
**级别**：架构约束（Architectural Contract）  
**适用范围**：所有 Platform / Application / Modules / Host / Tests 项目  
**生效时间**：即刻  

---

## 本章聚焦内容（Focus）

本 ADR 是**静态结构层**的核心文档，聚焦于：

1. **BaseNamespace 固定规则**：如何统一定义根命名空间
2. **目录 → RootNamespace 自动推导**：如何根据物理目录自动推导命名空间
3. **MSBuild 策略**：如何通过 MSBuild 强制执行命名空间规则
4. **防御性规则**：如何防止手动覆盖和不规范命名
5. **架构测试映射**：如何自动化校验命名空间规范

**不涉及**：
- ❌ 启动体系职责（见 ADR-0002）
- ❌ 依赖包管理（见 ADR-0004）
- ❌ 运行时交互模型（见 ADR-0005）
- ❌ 模块内部组织（见 ADR-0001）

---

## 术语表（Glossary）

| 术语                  | 定义                                                                 |
|-----------------------|----------------------------------------------------------------------|
| BaseNamespace         | 根命名空间，由 CompanyNamespace + ProductNamespace 构成              |
| RootNamespace         | 项目的根命名空间，基于 BaseNamespace 和目录结构自动推导              |
| Directory.Build.props | MSBuild 全局属性文件，用于定义所有项目共享的配置                     |
| MSBuild 推导          | 通过 MSBuild 条件判断自动计算 RootNamespace                          |
| 防御性规则            | 构建时检查，防止手动覆盖或不规范命名                                 |

---

## 1. 背景与目标

在中大型系统中，命名空间不仅是 **代码组织工具**，更是 **架构边界和模块约束**：

* Solution 名与 Namespace 分离，避免随意迁移破坏架构
* Host、Application、Platform 不允许随意改名
* Modules 命名必须严格对应 Vertical Slice
* Tests 命名必须严格对应测试范围

**目标**：

1. 根命名空间由 `CompanyNamespace` + `ProductNamespace` 构成
2. 目录结构 → RootNamespace 的映射明确、可推导
3. 禁止手动覆盖 BaseNamespace
4. 支持 CI / 架构测试自动校验
5. 新人 / 新 Host / 新模块无法绕过

---

## 2. 核心约束（强制）

### 2.1 BaseNamespace 固定

**【必须架构测试覆盖】**

在 `Directory.Build.props` 中定义：

```xml
<PropertyGroup>
    <CompanyNamespace>Zss</CompanyNamespace>
    <ProductNamespace>BilliardHall</ProductNamespace>
    <BaseNamespace>$(CompanyNamespace).$(ProductNamespace)</BaseNamespace>
</PropertyGroup>
```

* **禁止单个项目覆盖 BaseNamespace**
* **禁止在代码或 props 中写死 `Zss.BilliardHall.xxx`**

---

### 2.2 目录 → RootNamespace 映射规则

**【必须架构测试覆盖】**

| 目录前缀                 | RootNamespace                     | 规则说明                  |
| -------------------- | --------------------------------- | --------------------- |
| `src/Platform`       | `$(BaseNamespace).Platform`       | 技术平台基座                |
| `src/Application`    | `$(BaseNamespace).Application`    | 系统装配层                 |
| `src/Modules/<Name>` | `$(BaseNamespace).Modules.<Name>` | 每个模块对应 Vertical Slice |
| `src/Host/<Name>`    | `$(BaseNamespace).Host.<Name>`    | 每个 Host 是进程外壳         |
| `src/Tests/<Name>`   | `$(BaseNamespace).Tests.<Name>`   | 测试项目按范围命名             |

> 注：Modules / Host / Tests 的 `<Name>` **必须等于目录最后一级名称**。

---

### 2.3 防御性规则

**【必须架构测试覆盖】**

1. **未推导出 RootNamespace → 构建失败**

```xml
<Target Name="FailIfRootNamespaceNotSet" BeforeTargets="PrepareForBuild" Condition="'$(RootNamespace)'==''">
    <Error Text="❌ RootNamespace 未能根据目录结构推导。
项目路径: $(MSBuildProjectDirectory)
请将项目放入 src/Platform / src/Application / src/Modules / src/Host / src/Tests 之一。" />
</Target>
```

2. **Host / Module / Tests 目录名与 RootNamespace 尾名必须一致**
3. **禁止手动修改 RootNamespace**
4. **禁止 Host / Module / Platform / Tests 越界依赖**
5. **CI 环境触发失败即禁止合并**

---

### 2.4 核心 MSBuild 推导逻辑（摘要）

* `_RootDirSlash` 用于统一路径前缀
* `_ProjDirSlash` 表示当前项目目录
* Platform / Application / Modules / Host / Tests 条件 PropertyGroup 自动推导 RootNamespace

示例（Platform）：

```xml
<PropertyGroup Condition="$([System.String]::Copy('$(_ProjDirSlash)').StartsWith('$(_PlatformPrefix)', System.StringComparison.OrdinalIgnoreCase))">
    <RootNamespace>$(BaseNamespace).Platform</RootNamespace>
</PropertyGroup>
```

> ADR 中不需要写全 MSBuild 逻辑，但要说明规则可自动验证。

---

## 3. 规范化命名与目录策略

**【必须架构测试覆盖】**

* **项目名 = RootNamespace 尾部**（Host / Module / Test）
* **禁止在项目中硬编码 Namespace**
* **禁止使用 WebHost / Api / Misc 等随意命名**
* **Modules 必须完整 Vertical Slice**
* **Host 多实例是默认能力，不是扩展能力**

---

## 4. 例外与限制

* 特殊情况必须写 ADR 批准
* 禁止在 ADR-0003 外新增非标准目录映射
* 禁止在子项目私改 BaseNamespace

---

## 5. 强化与测试

**【必须架构测试覆盖】**

所有命名空间规则必须通过自动化架构测试验证。

**架构测试详见**：[ADR-0000：架构测试与 CI 治理](ADR-0000-architecture-tests.md)

**核心测试用例**：
- 所有类型命名空间以 BaseNamespace 开头
- 命名空间与物理结构匹配
- 无不规范命名空间（Common、Shared、Utils）
- 项目未手动覆盖 RootNamespace
- Directory.Build.props 存在且定义 BaseNamespace
- 项目命名遵循约定

---

## 6. 最终裁决（Final）

* ADR-0003 与 ADR-0002 配套，**共同构成启动与命名空间宪法**
* RootNamespace 约束 + CI 校验 + 架构测试 = **不可违背**
* 违反本 ADR 的代码，**视为架构缺陷**

---

## 7. 后续行动建议

1. 将 ADR-0003 纳入仓库文档目录
2. 将 MSBuild 推导逻辑固定在 `Directory.Build.props`
3. 在 CI 中执行 RootNamespace 架构测试
4. 将命名规则写入团队 onboarding 文档

---

## 与其他 ADR 关系（Related ADRs）

| ADR        | 关系                                           |
|------------|------------------------------------------------|
| ADR-0000   | 定义本 ADR 的自动化测试机制                    |
| ADR-0001   | 定义模块组织，本 ADR 定义模块命名空间          |
| ADR-0002   | 配合定义启动体系的命名空间规范                 |
| ADR-0004   | 定义依赖包管理规则                             |
| ADR-0005   | 定义运行时交互模型                             |

**依赖关系**：
- 本 ADR 定义命名空间规范
- ADR-0002 引用本 ADR 的 BaseNamespace 定义
- 所有 ADR 都依赖本 ADR 的命名空间规则

---

## 快速参考表（Quick Reference Table）

| 约束编号 | 约束描述 | 必须测试 | 测试覆盖 | ADR 章节 |
|---------|---------|---------|---------|---------|
| ADR-0003.1 | 所有类型应以 BaseNamespace 开头 | ✅ | `All_Types_Should_Start_With_Base_Namespace` | 2.1, 5 |
| ADR-0003.2 | Platform 类型应在 Zss.BilliardHall.Platform 命名空间 | ✅ | `Platform_Types_Should_Have_Platform_Namespace` | 2.2, 5 |
| ADR-0003.3 | Application 类型应在 Zss.BilliardHall.Application 命名空间 | ✅ | `Application_Types_Should_Have_Application_Namespace` | 2.2, 5 |
| ADR-0003.4 | Module 类型应在 Zss.BilliardHall.Modules.{ModuleName} 命名空间 | ✅ | `Module_Types_Should_Have_Module_Namespace` | 2.2, 5 |
| ADR-0003.5 | Host 类型应在 Zss.BilliardHall.Host.{HostName} 命名空间 | ✅ | `Host_Types_Should_Have_Host_Namespace` | 2.2, 5 |
| ADR-0003.6 | Directory.Build.props 应存在于仓库根目录 | ✅ | `Directory_Build_Props_Should_Exist_At_Repository_Root` | 2.1, 2.3 |
| ADR-0003.7 | Directory.Build.props 应定义 BaseNamespace | ✅ | `Directory_Build_Props_Should_Define_Base_Namespace` | 2.1, 2.3 |
| ADR-0003.8 | 所有项目应遵循命名空间约定 | ✅ | `All_Projects_Should_Follow_Namespace_Convention` | 2.2, 3 |
| ADR-0003.9 | 模块不应包含不规范的命名空间模式 | ✅ | `Modules_Should_Not_Contain_Irregular_Namespace_Patterns` | 3 |

---

## 快速参考（Quick Reference）

### 命名空间检查清单

- [ ] 是否使用了 Directory.Build.props 定义 BaseNamespace？
- [ ] 项目的 RootNamespace 是否基于目录自动推导？
- [ ] 是否避免了手动覆盖 BaseNamespace？
- [ ] 是否避免了 Common、Shared、Utils 等不规范命名？

### 常见错误

| 错误                              | 正确做法                          |
|-----------------------------------|-----------------------------------|
| 手动指定 RootNamespace            | 删除手动指定，使用自动推导        |
| 使用 Zss.BilliardHall.Common      | 使用 Zss.BilliardHall.Platform    |
| 不同项目使用不同 BaseNamespace    | 统一使用 Directory.Build.props    |
| 命名空间与目录结构不一致          | 调整目录结构或命名空间            |

### MSBuild 推导规则总结

| 目录前缀                 | 自动推导的 RootNamespace          |
|--------------------------|-----------------------------------|
| `src/Platform`           | `Zss.BilliardHall.Platform`       |
| `src/Application`        | `Zss.BilliardHall.Application`    |
| `src/Modules/<Name>`     | `Zss.BilliardHall.Modules.<Name>` |
| `src/Host/<Name>`        | `Zss.BilliardHall.Host.<Name>`    |
| `src/Tests/<Name>`       | `Zss.BilliardHall.Tests.<Name>`   |
