# Composition Root 项目

## 目的

本项目作为 **Composition Root**，是唯一了解具体模块类型的地方，解决了以下问题：

1. **架构边界合规**：满足 ADR-002_3_3（Host 不依赖 Modules）
2. **类型安全**：保留显式模块注册的优势，编译时类型检查
3. **关注点分离**：Host 层专注进程管理，不需要知道具体有哪些业务模块

## 架构设计

```
┌─────────────────────────────────────┐
│          Host Layer                 │
│  • 仅依赖 Composition               │
│  • 不知道具体模块类型                │
└─────────────────────────────────────┘
                ↓
┌─────────────────────────────────────┐
│      Composition Root               │
│  • 唯一了解 Members、Orders 等模块   │
│  • 提供 GetEnabledModules() 方法    │
└─────────────────────────────────────┘
         ↓              ↓
┌─────────────┐  ┌─────────────┐
│   Members   │  │   Orders    │
│   Module    │  │   Module    │
└─────────────┘  └─────────────┘
```

## 职责

- **模块发现**：声明所有可用的业务模块
- **模块过滤**：根据配置返回启用的模块
- **类型屏蔽**：对外提供 `IModule[]`，隐藏具体类型

## 添加新模块

当添加新业务模块时，只需修改 `ModuleComposition.cs`：

```csharp
private static readonly IModule[] AllModules =
[
    new MemberModule(),
    new OrderModule(),
    new YourNewModule()  // 添加这里
];
```

同时在 `Composition.csproj` 中添加项目引用：

```xml
<ProjectReference Include="..\Modules\YourNewModule\YourNewModule.csproj"/>
```

## 相关 ADR

- [ADR-002：Platform / Application / Host 三层启动体系](../../docs/adr/constitutional/ADR-002-platform-application-host-bootstrap.md)
- [ADR-001：模块化单体与垂直切片架构](../../docs/adr/constitutional/ADR-001-modular-monolith-vertical-slice-architecture.md)
