# ADR-0003：命名空间与项目边界规范

**状态**：✅ 已采纳（Final，不可随意修改）  
**级别**：架构约束（Architectural Contract）  
**适用范围**：所有 Platform / Application / Modules / Host / Tests 项目  
**生效时间**：即刻

---

## 聚焦内容（Focus）

- BaseNamespace 固定与统一定义
- 目录结构与 RootNamespace 自动映射
- MSBuild 推导与防御规则
- 项目命名与命名空间边界原则
- 架构自动测试与 CI 校验

---

## 术语表（Glossary）

| 术语                | 定义                                               |
|---------------------|--------------------------------------------------|
| BaseNamespace       | 公司+产品根命名空间（如 `Zss.BilliardHall`）       |
| RootNamespace       | 项目根命名空间，由BaseNamespace及目录自动推导      |
| Directory.Build.props | MSBuild全局配置文件，统一定义BaseNamespace     |
| MSBuild 推导        | 通过 MSBuild 条件和目录映射自动赋值 RootNamespace  |

---

## 决策（Decision）

### 命名空间自动推导与一致性

- 所有项目必须通过 Directory.Build.props 定义 BaseNamespace，严禁手工指定
- 目录结构直接推导 RootNamespace，例如 `src/Modules/Orders` → `Zss.BilliardHall.Modules.Orders`
- Host/Module/Platform/Tests 的目录与 RootNamespace 尾部需严格一致
- 禁用常见错误命名（如 Common、Shared、Utils）
- 项目名=RootNamespace 尾部

### 防御规则

- 手动指定 RootNamespace 即构建失败
- 目录不符、命名空间不符的必须架构测试覆盖
- 禁止在项目中直接用字符串硬编码命名空间
- 所有命名空间规则均需自动化测试验证

---

## 快速参考和架构测试映射

| 约束编号     | 描述                                    | 层级 | 测试用例/自动化   | 章节      |
|--------------|-----------------------------------------|------|-------------------|-----------|
| ADR-0003.1   | 所有类型命名空间以 BaseNamespace 开头    | L1   | All_Types_Should_Start_With_Base_Namespace | 约束-自动推导  |
| ADR-0003.2   | Platform 类型以 Platform 命名空间为前缀   | L1   | Platform_Types_Should_Have_Platform_Namespace | 约束-自动推导  |
| ADR-0003.3   | Application 类型以 Application 命名空间为前缀 | L1   | Application_Types_Should_Have_Application_Namespace | 约束-自动推导  |
| ADR-0003.4   | Modules 类型对应 Modules.{Name} 命名空间 | L1   | Module_Types_Should_Have_Module_Namespace | 约束-自动推导  |
| ADR-0003.5   | Host 类型对应 Host.{Name} 命名空间       | L1   | Host_Types_Should_Have_Host_Namespace | 约束-自动推导  |
| ADR-0003.6   | Directory.Build.props 必须位于仓库根目录 | L1   | Directory_Build_Props_Should_Exist_At_Repository_Root | 防御规则      |
| ADR-0003.7   | Directory.Build.props 定义 BaseNamespace | L1   | Directory_Build_Props_Should_Define_Base_Namespace | 防御规则      |
| ADR-0003.8   | 项目命名需遵循命名空间映射               | L1   | All_Projects_Should_Follow_Namespace_Convention | 防御规则      |
| ADR-0003.9   | 不得出现不规范命名空间                   | L1   | Modules_Should_Not_Contain_Irregular_Namespace_Patterns | 防御规则 |

---

## 依赖与相关ADR

- ADR-0002：启动体系与多 Host 适配
- ADR-0004：包管理的层级约束映射
- ADR-0005：运行时分层与 Handler 注册映射

---

## 检查清单

- [ ] 是否用 Directory.Build.props 统一 BaseNamespace？
- [ ] 根命名空间是否由目录自动推导？
- [ ] 项目名与目录/二级命名空间是否严格一致？
- [ ] 全局无 Common、Shared、Utils 等命名空间？
- [ ] CI 与架构测试是否已自动检验命名空间合规？

---

## 扩展落地建议

- 推荐将 MSBuild 逻辑固化为模板，避免手工配置
- 新增模块/Host 自动生成标准目录与命名空间
- 在 Onboarding 文档与技术地图同步讲解

---

## 版本历史

| 版本 | 日期 | 变更摘要          |
|------|------|-------------------|
| 3.0  | 2026-01-22 | 完全去编号、结构升级  |
| 2.0  | 2026-01-20 | 机制细化            |
| 1.0  | 初版       |                    |

---

## 附件

- [ADR-0002 三层启动体系](ADR-0002-platform-application-host-bootstrap.md)
- [ADR-0004 中央包管理与依赖](ADR-0004-Cpm-Final.md)