# 开发规范

本目录包含项目开发规范和最佳实践。

## 规范清单

| 规范 | 状态 | 说明 |
|------|------|------|
| [代码风格](代码风格.md) | ✅ 完成 | C# 代码规范、命名约定 |
| [切片约束](切片约束.md) | ✅ 完成 | 垂直切片架构约束 |
| [Saga 使用指南](Saga使用指南.md) | ✅ 完成 | 跨模块长事务编排 |
| [FluentValidation 集成指南](FluentValidation集成指南.md) | ✅ 完成 | 输入验证最佳实践 |
| [ServiceDefaults 集成指南](ServiceDefaults集成指南.md) | ✅ 完成 | Aspire 服务配置 |
| [级联消息与副作用](级联消息与副作用.md) | ✅ 完成 | Handler 返回值、IO 分离 |
| [日志规范](日志规范.md) | ✅ 完成 | 日志格式、级别定义 |
| [Git 分支规范](Git分支规范.md) | ✅ 完成 | 分支模型、提交规范 |
| [Code Review 流程](CodeReview流程.md) | ✅ 完成 | 代码审查流程、模板 |
| [Wolverine 端点约定](Wolverine端点约定.md) | ✅ 完成 | HTTP 端点配置约定 |

## 快速开始

安装代码格式化工具：
```bash
dotnet tool install --global dotnet-format
```

设置 Git 提交模板：
```bash
git config commit.template .gitmessage.txt
```

详见各规范文档。
