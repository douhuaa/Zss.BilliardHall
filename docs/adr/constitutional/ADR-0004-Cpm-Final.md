# ADR-0004：中央包管理（CPM）规范

**状态**：✅ 已采纳（Final，不可随意修改）  
**级别**：架构约束（Architectural Contract）  
**适用范围**：所有 Platform / Application / Modules / Host / Tests 项目  
**生效时间**：即刻

---

## 聚焦内容（Focus）

- 所有依赖包通过 Directory.Packages.props 集中管理
- 层级依赖规则细化：Platform、Application、Modules、Host 各自明确边界
- 包分组策略及管理
- 强防御：禁止项目文件手动指定包版本
- 自动化测试与 CI 校验内建

---

## 术语表（Glossary）

| 术语                       | 定义                               |
|--------------------------|----------------------------------|
| CPM                      | Central Package Management，中央包管理 |
| Directory.Packages.props | NuGet 配置文件，集中定义全局依赖包版本           |
| 传递依赖固定                   | 通过 CPM 禁止传递依赖的漂移                 |
| 层级依赖                     | 不同层项目允许的包类型约束                    |
| 包分组                      | 依赖包按技术栈、场景分隔分组                   |

---

## 决策（Decision）

### 包集中管理与防御

- 必须启用 CPM，禁用项目手动指定 Version
- 所有包版本及分组写入 Directory.Packages.props
- 任何项目文件出现 Version 即构建失败
- 层级依赖关系必须反映体系结构：

| 层级          | 允许依赖包类型                               | 禁止依赖                         |
|-------------|---------------------------------------|------------------------------|
| Platform    | 技术底座包（Logging、OpenTelemetry、基础异常处理等）  | Application/Module/Host 的业务包 |
| Application | Wolverine、Marten、所有装配与 Pipeline       | Host/Http/业务模块               |
| Modules     | 业务依赖、DTO、协议、契约                        | Platform 内部包、Host、其它模块       |
| Host        | 仅调用 Platform+Application Bootstrapper | 业务模块、Handler                 |
| Tests       | 被测模块+Platform/Application             | Host 内部实现                    |

### 包分组管理

- 所有包按功能或部门分组，集中表述
- 用 `Label` 注释分组
- 定期归档未使用包（自动检测）

### 防御规则

- 任何层级越界依赖包，CI 自动阻断
- 违反上述规则，PR 自动拒绝

---

## 快速参考和架构测试映射

| 约束编号       | 描述                            | 层级 | 测试用例/自动化                                                  | 章节   |
|------------|-------------------------------|----|-----------------------------------------------------------|------|
| ADR-0004.1 | 必须启用 CPM                      | L1 | CPM_Should_Be_Enabled                                     | 管理规则 |
| ADR-0004.2 | Directory.Packages.props 强制存在 | L1 | Repository_Should_Have_Directory_Packages_Props           | 管理规则 |
| ADR-0004.3 | 不允许项目文件手动指定版本                 | L1 | Projects_Should_Not_Specify_Package_Versions              | 防御规则 |
| ADR-0004.4 | 层级依赖必须严格遵守                    | L1 | Layer_Package_Dependencies_Should_Be_Valid                | 层级依赖 |
| ADR-0004.5 | 包分组规范                         | L1 | Directory_Packages_Props_Should_Contain_Package_Groups    | 分组规则 |
| ADR-0004.6 | 测试项目使用相同测试框架版本                | L1 | All_Test_Projects_Should_Use_Same_Test_Framework_Versions | 防御规则 |
| ADR-0004.7 | 包依赖集中声明                       | L1 | Directory_Packages_Props_Should_Define_All_Used_Packages  | 分组管理 |
| ADR-0004.8 | Platform 不得依赖业务包              | L1 | Platform_Projects_Should_Not_Reference_Business_Packages  | 层级依赖 |
| ADR-0004.9 | 禁止私自覆盖中央包版本                   | L1 | Projects_Should_Not_Override_Central_Package_Versions     | 防御规则 |

---

## 依赖与相关ADR

- ADR-0002：层级装配分界的前提
- ADR-0003：命名空间与依赖一致性
- ADR-0005：运行时依赖语义补充
- ADR-0000：自动化测试强关联

---

## 检查清单

- [ ] 是否使用 Directory.Packages.props 管理所有包？
- [ ] 项目文件无手动 Version？
- [ ] 各层引入的依赖包完全符合层级规则？
- [ ] 所有项目依赖包都在集中声明？
- [ ] 相关架构测试和 CI 校验被正确拦截？

---

## 扩展落地建议

- 将 CPM 配置做成模板，开新项目/新域可快速复用
- 自动扫描未用包并归档避免依赖膨胀
- 团队会议定期梳理架构包分层变更
- 升级或迁移包需先归档再上线

---

## 版本历史

| 版本  | 日期         | 变更摘要         |
|-----|------------|--------------|
| 3.0 | 2026-01-22 | 结构升级、统一结构和映射 |
| 2.0 | 2026-01-20 | 分组细化，新增CI校验  |
| 1.0 | 初版         | 初始发布         |

---

## 附件

- [ADR-0002 三层启动体系规范](ADR-0002-platform-application-host-bootstrap.md)
- [ADR-0003 命名空间标准](ADR-0003-namespace-rules.md)
