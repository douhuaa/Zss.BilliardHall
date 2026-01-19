# ADR-0001

## 模块化单体与垂直切片架构决策（Vertical Slice + Module Isolation）

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

- 按业务能力划分独立模块：如 Members、Orders、Payments
- 每个模块拥有独立程序集与逻辑边界
- 对外只暴露受控集成点，禁止其他模块访问内部实现
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
- 严禁将业务逻辑抽象为横向 Service

### 3. 模块通信约束

仅允许以下三种模块间通信方式：

1. 领域事件（Domain Events）
2. 数据契约（Contracts，只读稳定 DTO）
3. 原始类型 / 标准库类型

严禁行为：

- ❌ 跨模块引用 Entity / Aggregate / ValueObject
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

### 7. 自动化架构测试映射

| 约束项            | 校验方式            | 工具/方法                  |
|-------------------|--------------------|----------------------------|
| 模块隔离          | 类型依赖分析        | NetArchTest                |
| 垂直切片禁止分层   | 命名空间/文件夹规则  | NetArchTest                |
| 契约使用规则       | 类型依赖+语义分析    | NetArchTest+Roslyn Analyzer|
| Platform 层判定   | 代码扫描            | Roslyn Analyzer            |
| 架构测试入 CI 门禁 | 构建失败阻断         | CI Pipeline                |

### 8. 执行与豁免机制

- 所有约束均须自动化测试验证
- 架构测试失败 = 构建失败
- 特殊情况放行需通过 ADR 记录，PR 中标注 ARCH-VIOLATION，并设定偿还时间

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

## 附录

- [ADR-0002 Platform / Application / Host 三层启动体系](ADR-0002-platform-application-host-bootstrap.md)
- [ADR-0003 命名空间与项目边界规范](ADR-0003-namespace-rules.md)
- [ADR-0004 中央包管理与层级依赖规则](ADR-0004-Cpm-Final.md)
