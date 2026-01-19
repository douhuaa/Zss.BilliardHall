# ADR-0003

## 命名空间与项目边界规范（Final）

**状态**：✅ 已采纳（Final，不可随意修改）
**级别**：架构约束（Architectural Contract）
**适用范围**：所有 Platform / Application / Modules / Host / Tests 项目
**生效时间**：即刻

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

## 3. 架构测试（建议纳入 CI）

### 3.1 Namespace 校验

使用 NetArchTest / Roslyn Analyzer：

* Platform 只依赖 Platform
* Application 只依赖 Platform
* Modules 只依赖 Application + Platform
* Host 只依赖 Platform + Application
* Tests 只依赖被测模块 + Platform

### 3.2 防止手动覆盖

```csharp
[Fact]
public void Project_Should_Not_Override_BaseNamespace()
{
    Types.InAssembly(typeof(SomeType).Assembly)
        .ShouldNot().HaveNamespaceNotStartingWith("Zss.BilliardHall")
        .GetResult().IsSuccessful.ShouldBeTrue();
}
```

---

## 4. 规范化命名与目录策略

* **项目名 = RootNamespace 尾部**（Host / Module / Test）
* **禁止在项目中硬编码 Namespace**
* **禁止使用 WebHost / Api / Misc 等随意命名**
* **Modules 必须完整 Vertical Slice**
* **Host 多实例是默认能力，不是扩展能力**

---

## 5. 例外与限制

* 特殊情况必须写 ADR 批准
* 禁止在 ADR-0003 外新增非标准目录映射
* 禁止在子项目私改 BaseNamespace

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
