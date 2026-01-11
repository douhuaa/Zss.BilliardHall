# Wolverine + 垂直切片架构项目

本目录包含基于 Wolverine 框架和垂直切片架构重构后的项目结构。

## 项目结构

```
src/Wolverine/
├── Directory.Build.props        # 通用 MSBuild 属性配置
├── Directory.Packages.props     # 中央包版本管理（CPM）
├── Bootstrapper/               # 启动项目（ASP.NET Core Web）
├── BuildingBlocks/             # 共享基础设施
│   ├── Contracts/              # Result、IIntegrationEvent、IClock
│   ├── Behaviors/              # Wolverine 中间件（待实现）
│   └── Exceptions/             # DomainException、NotFoundException
└── Modules/                    # 业务模块
    ├── Members/                # 会员管理模块
    ├── Tables/                 # 台球桌管理模块
    └── Sessions/               # 打球时段模块
```

## 已完成工作

### 1. 项目结构创建 ✅
- 创建了 Bootstrapper Web 项目
- 创建了 BuildingBlocks 基础设施项目（Contracts, Behaviors, Exceptions）
- 创建了核心业务模块项目（Members, Tables, Sessions）
- 创建了解决方案文件并添加了所有项目
- **新增**：添加了 `Directory.Build.props` 统一管理项目属性
- **新增**：添加了 `Directory.Packages.props` 实现中央包版本管理（CPM）

### 2. NuGet 包配置 ✅
使用中央包管理（CPM），所有包版本在 `Directory.Packages.props` 中统一管理：
- WolverineFx.Http 5.9.2
- Marten 8.17.0
- Marten.AspNetCore 8.17.0
- Serilog.AspNetCore 10.0.0

### 3. BuildingBlocks/Contracts ✅
已实现核心契约类型：
- `Result<T>` - 操作结果封装
- `IIntegrationEvent` - 集成事件接口
- `IClock` / `SystemClock` - 时间抽象

### 4. BuildingBlocks/Exceptions ✅
已实现异常类型：
- `DomainException` - 领域异常基类
- `NotFoundException` - 资源未找到异常

## 待实现工作

### 阶段一：完成 Bootstrapper 配置
- [ ] 实现 Program.cs 配置 Wolverine
- [ ] 配置 Marten 文档数据库
- [ ] 配置 Serilog 日志
- [ ] 添加 Wolverine HTTP 端点扫描

### 阶段二：实现 Members 模块示例
参考文档：`doc/04_模块设计/会员管理模块.md`

- [ ] 创建 Member 聚合根
- [ ] 实现 RegisterMember 功能切片
  - RegisterMember.cs (Command)
  - RegisterMemberHandler.cs (Handler)
  - RegisterMemberEndpoint.cs (HTTP Endpoint)
  - RegisterMemberValidator.cs (FluentValidation)
- [ ] 实现 GetMember 查询切片
- [ ] 添加集成事件（MemberRegistered）

### 阶段三：实现其他模块
- [ ] Tables 模块（台球桌管理）
- [ ] Sessions 模块（打球时段）

### 阶段四：测试与验证
- [ ] 创建集成测试项目
- [ ] 实现核心功能测试
- [ ] 验证应用启动
- [ ] 运行 CodeQL 扫描

## 架构参考文档

- [Wolverine模块化架构蓝图](../../doc/03_系统架构设计/Wolverine模块化架构蓝图.md)
- [Wolverine快速上手指南](../../doc/03_系统架构设计/Wolverine快速上手指南.md)
- [系统模块划分](../../doc/03_系统架构设计/系统模块划分.md)
- [会员管理模块设计](../../doc/04_模块设计/会员管理模块.md)

## 核心原则

1. **100% 垂直切片** - 按业务功能组织代码，不是技术层
2. **一个 Use Case = 一个文件夹** - Command + Handler + Endpoint + Validator 放在一起
3. **Handler 即 Application Service** - 不再需要单独的 Service 层
4. **直接使用 Marten** - 不使用 Repository 模式
5. **事件驱动通信** - 通过 Wolverine 消息总线实现模块间通信

## 下一步行动

1. 完成 Bootstrapper/Program.cs 的 Wolverine 配置
2. 实现 Members 模块的 RegisterMember 完整切片
3. 验证应用可以成功启动
4. 添加基本的集成测试

## 注意事项

- 本项目遵循文档中定义的 Wolverine + 垂直切片架构规范
- 拒绝传统的 Application/Domain/Infrastructure 分层
- 拒绝 Repository、UnitOfWork 等抽象模式
- 所有跨模块通信必须通过消息总线
