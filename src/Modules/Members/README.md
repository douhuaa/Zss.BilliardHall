# Members 模块

## 模块职责

管理台球馆会员的生命周期，包括：
- 会员注册
- 会员信息管理
- 会员查询

## 架构组织

本模块采用 **垂直切片架构**（Vertical Slice Architecture），按功能/用例组织代码：

```
Members/
├── Features/
│   ├── CreateMember/           # 创建会员切片
│   │   ├── CreateMemberCommand.cs
│   │   ├── CreateMemberCommandHandler.cs
│   │   └── CreateMemberEndpoint.cs
│   ├── GetMemberById/          # 查询会员切片
│   │   ├── GetMemberByIdQuery.cs
│   │   ├── GetMemberByIdQueryHandler.cs
│   │   ├── GetMemberByIdEndpoint.cs
│   │   └── MemberDto.cs
│   └── UpdateMember/           # 更新会员切片
│       └── ...
└── Members.csproj
```

## 垂直切片原则

### 每个切片（Feature）包含：
1. **Command/Query** - 表达业务意图
2. **Handler** - 业务规则与一致性判断
3. **Endpoint** - HTTP 请求/响应处理
4. **DTO** - 数据传输对象（如需要）

### 禁止的做法：
- ❌ 创建横向的 `Service` 层（如 `MemberService`）
- ❌ 创建 `Application/Domain/Infrastructure` 分层命名空间
- ❌ 创建 `Shared/Common` 文件夹（会导致横向抽象）
- ❌ Handler 之间直接相互调用

### 允许的做法：
- ✅ 在切片内包含所有必要的逻辑
- ✅ 使用领域事件进行切片间通信
- ✅ 通过 Mediator 发送 Command/Query
- ✅ 复制代码（DRY 不是最高原则）

## 模块间通信

### 对外暴露的接口
如果其他模块需要查询会员信息，应该：
1. 在 `Platform.Contracts` 中定义数据契约
2. 通过领域事件通知状态变更
3. 其他模块维护本地副本（Event Sourcing）

### 不允许的通信方式
- ❌ 直接引用 Members 模块的类型
- ❌ 共享聚合根/实体
- ❌ 跨模块调用 Handler

## 依赖规则

本模块只能依赖：
- `Platform` - 技术能力（日志、事务等）
- 标准库类型
- 必要的基础设施包（如 Marten、Wolverine）

不能依赖：
- ❌ 其他业务模块（Orders、Payments 等）
- ❌ Application 层
- ❌ Host 层

## 参考

- [ADR-0001: 模块化单体与垂直切片架构决策](/docs/adr/ADR-0001-modular-monolith-vertical-slice-architecture.md)
- [Vertical Slice Architecture](https://www.jimmybogard.com/vertical-slice-architecture/)
