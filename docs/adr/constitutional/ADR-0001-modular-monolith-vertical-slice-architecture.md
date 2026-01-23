# ADR-0001：模块化单体与垂直切片架构

**状态**：✅ 已采纳（Final，不可随意修改）  
**级别**：架构约束（Architectural Contract）  
**适用范围**：所有 Host、模块、测试、未来子系统  
**生效时间**：即刻  

---

## 本章聚焦内容（Focus）

本 ADR 是**静态结构层**的核心文档，聚焦于：

1. **功能模块的划分方式**：如何按业务能力划分独立模块
2. **垂直切片的组织原则**：模块内部如何以用例为单元组织代码
3. **模块隔离规则**：模块之间如何保持边界清晰
4. **契约使用规则**：模块间如何通过契约通信
5. **模块通信约束**：允许和禁止的模块间交互方式

**不涉及**：
- ❌ 系统启动和装配（见 ADR-0002）
- ❌ 命名空间规范（见 ADR-0003）
- ❌ 依赖包管理（见 ADR-0004）
- ❌ 运行时交互模型（见 ADR-0005）
- ❌ 架构测试机制（见 ADR-0000）

---

## 术语表（Glossary）

| 术语                  | 定义                                                                 |
|-----------------------|----------------------------------------------------------------------|
| 模块化单体            | 单一进程中，按业务能力划分的多个独立模块，物理隔离但逻辑解耦         |
| 垂直切片              | 以单一业务用例为单元，包含从 API 到数据库的完整实现                  |
| 模块隔离              | 模块之间不能直接引用内部实现，只能通过契约或事件通信                 |
| 契约（Contracts）     | 只读的、稳定的数据传输对象（DTO），用于模块间通信                    |
| 领域事件              | 模块内部发生的业务事实，可以被其他模块订阅                           |
| 垂直分层架构          | 传统的 Controller / Service / Repository 分层，本架构禁止使用        |

---

## 状态

**状态**：✅ 已采纳（Final，不可随意修改）  
**级别**：架构约束（Architectural Contract）  
**适用范围**：所有 Host、模块、测试、未来子系统  
**生效时间**：即刻

## 背景

随着业务复杂度提升，传统分层架构（Controller / Service / Repository）逐步暴露以下问题：

- 单一业务需求需跨层级修改，开发效率降低
- "共享领域模型/Service"带来模块间强耦合
- 架构约束缺乏自动化校验，易随时间腐化
- 架构原则仅靠文档难以约束工程实践，回溯困难

目标：构建长期可演进、易扩展、边界清晰的业务系统。

方案：

- 功能组织采用 **垂直切片架构**（Vertical Slice）
- 系统级约束强调 **模块隔离**（Module Isolation）
- 利用 **自动化架构测试** 进行强制校验

---

## 决策

### 1. 模块划分方式

- 按业务能力划分独立模块：如 Members、Orders、Payments **【必须架构测试覆盖】**
- 每个模块拥有独立程序集与逻辑边界 **【必须架构测试覆盖】**
- 对外只暴露受控集成点，禁止其他模块访问内部实现 **【必须架构测试覆盖】**
- 强调模块是工程隔离单元而非命名空间

结构示例：
```
Module Assembly
├─ Members
├─ Orders
└─ Payments
```

### 2. 功能实现方式：垂直切片

- 模块内部以用例（Use Case）为最小单元组织
- 每个切片包含：
  - Endpoint / API
  - Command / Query
  - Handler
  - 业务规则与校验
  - 持久化与集成逻辑
- 严禁将业务逻辑抽象为横向 Service **【必须架构测试覆盖】**

### 3. 模块通信约束

仅允许以下三种模块间通信方式：

1. 领域事件（Domain Events） **【必须架构测试覆盖】**
2. 数据契约（Contracts，只读稳定 DTO） **【必须架构测试覆盖】**
3. 原始类型 / 标准库类型

严禁行为：

- ❌ 跨模块引用 Entity / Aggregate / ValueObject **【必须架构测试覆盖】**
- ❌ 在契约中表达业务意图、决策字段
- ❌ 将数据契约演变为服务接口

说明：契约仅限数据传递，禁止用于业务决策。

### 4. 业务执行分工规范

| 角色                | 允许职责               | 禁止行为                   |
|---------------------|-----------------------|----------------------------|
| Command / Query     | 表达业务意图           | 包含业务逻辑               |
| Handler             | 业务规则与一致性判断     | 业务决策下沉到基础设施       |
| Domain Model        | 承载业务不变量          | 依赖其他模块实现            |

任何违反以上分工，视为架构违规。

### 5. Platform 层限制

- 仅提供技术能力，如日志、事务、异常处理、序列化等
- 禁止任何业务规则、业务判断
- 判定标准：如 Platform 层出现 `if (业务状态/含义)` 即认定违规

### 6. 数据契约（Contracts）使用规则

**白名单表格：**

| 使用场景                       | 允许使用 Contracts | 说明                      |
|--------------------------------|-------------------|---------------------------|
| Command Handler                | ❌                | 禁止业务驱动              |
| Query Handler                  | ✅                | 只读查询允许              |
| Endpoint / API                 | ✅                | 请求/响应与数据展示        |
| Projection / ReadModel         | ✅                | 视图模型允许              |
| Platform / Building Blocks     | ❌                | 禁止依赖 Contracts        |

**示例代码：**
```csharp
// 违规
public class CreateOrderHandler
{
    public CreateOrderHandler(IMemberQueries queries) { ... } // ❌ 不允许
}

// 合规
public class GetMemberQueryHandler
{
    public async Task<MemberDto> Handle(GetMemberById query) { ... } // ✅ 只读查询
}
```

**执行说明：**
- 类型依赖违规通过 NetArchTest 自动检测
- 语义级违规（如依赖契约字段做业务决策）须 Roslyn Analyzer/人工评审
- 架构测试纳入 CI，违规必修复，不可放行

### 7. 强化与测试

所有模块隔离规则必须通过自动化架构测试验证。

**架构测试详见**：[ADR-0000：架构测试与 CI 治理](ADR-0000-architecture-tests.md)

---

## 不可协商条款

1. 禁止模块直接引用其他模块实现
2. 所有隔离规则须自动化测试校验
3. 架构测试失败即构建失败
4. 契约不得驱动业务决策
5. ADR 为最终裁决依据

---

## 决策理由

目标：
- 降低架构腐化与熵增速度
- 架构决策可审核、可追溯、工程级可执行
- 以长期可演进为核心设计原则

---

## 参考资料

- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)
- [NetArchTest.Rules](https://github.com/BenMorris/NetArchTest)
- [Modular Monolith Architecture](https://www.kamilgrzybek.com/blog/posts/modular-monolith-primer)

---

## 与其他 ADR 关系（Related ADRs）

| ADR        | 关系                                           |
|------------|------------------------------------------------|
| ADR-0000   | 定义本 ADR 的自动化测试机制                    |
| ADR-0002   | 定义模块的启动和装配方式                       |
| ADR-0003   | 定义模块的命名空间规范                         |
| ADR-0004   | 定义模块的依赖包管理规则                       |
| ADR-0005   | 定义模块间的运行时通信模型                     |

**依赖关系**：
- 本 ADR 定义静态模块结构
- ADR-0002/0003/0004 定义如何组织和管理模块
- ADR-0005 定义模块如何在运行时协作

---

## 快速参考表（Quick Reference Table）

| 约束编号 | 约束描述 | 必须测试 | 测试覆盖 | ADR 章节 |
|---------|---------|---------|---------|----------|
| ADR-0001.1 | 模块不得相互引用（程序集级） | ✅ | ADR_0001_Architecture_Tests::Modules_Should_Not_Reference_Other_Modules | 1 |
| ADR-0001.2 | 模块项目文件不得引用其他模块 | ✅ | ADR_0001_Architecture_Tests::Module_Csproj_Should_Not_Reference_Other_Modules | 1 |
| ADR-0001.3 | 禁止传统分层命名空间 | ✅ | ADR_0001_Architecture_Tests::Modules_Should_Not_Contain_Traditional_Layering_Namespaces | 2 |
| ADR-0001.4 | 禁止 Service 后缀类 | ✅ | ADR_0001_Architecture_Tests::Modules_Should_Not_Contain_Service_Classes | 2 |
| ADR-0001.5 | Handler 必须在 UseCases 下 | ✅ | ADR_0001_Architecture_Tests::Handlers_Should_Be_In_UseCases_Namespace | 2 |
| ADR-0001.6 | 模块通信仅限三种方式 | 🔄 | 部分覆盖（需人工 Code Review） | 3 |
| ADR-0001.7 | Command Handler 不得依赖 Contracts | ✅ | 待补充（ADR-0005 覆盖） | 6 |

**图例说明**：
- ✅ 已自动化测试
- 🔄 部分自动化（需配合人工审查）
- ❌ 待补充测试
- 💡 无需自动化（概念性指导）

---

## 快速参考（Quick Reference）

### 模块划分检查清单

- [ ] 模块是否按业务能力划分？
- [ ] 模块是否拥有独立程序集？
- [ ] 模块是否只暴露受控集成点？
- [ ] 模块内部实现是否对外隐藏？

### 垂直切片检查清单

- [ ] 用例是否是最小组织单元？
- [ ] 切片是否包含完整的业务逻辑？
- [ ] 是否避免了横向 Service 抽象？
- [ ] Handler 是否承载业务规则？

### 契约使用检查清单

- [ ] 契约是否只用于数据传递？
- [ ] 契约是否避免了业务决策字段？
- [ ] Command Handler 是否不依赖 Contracts？
- [ ] Query Handler 是否可以返回 Contracts？

---

## 附录

- [ADR-0002 Platform / Application / Host 三层启动体系](ADR-0002-platform-application-host-bootstrap.md)
- [ADR-0003 命名空间与项目边界规范](ADR-0003-namespace-rules.md)
- [ADR-0004 中央包管理与层级依赖规则](ADR-0004-Cpm-Final.md)
- [ADR-0005 应用内交互模型与执行边界](ADR-0005-Application-Interaction-Model-Final.md)
