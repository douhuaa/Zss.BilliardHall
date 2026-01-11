# GitHub Actions Workflows

## CI Workflow (`ci.yml`)

自动化持续集成工作流，在每次 PR 和推送到 main 分支时触发。

### 功能

- ✅ 代码格式检查 (`dotnet format`)
- ✅ 编译构建 (Release 配置)
- ✅ 单元测试执行（带代码覆盖率）
- ✅ 测试结果上传

### 使用的 Actions

- `actions/checkout@v4` - 代码检出
- `actions/setup-dotnet@v4` - .NET 环境配置
- `actions/upload-artifact@v4` - 测试结果上传

### 注意事项

- 使用 .NET 10.0 预览版
- AppHost 集成测试在 CI 中跳过（需要 Aspire/DCP（Developer Control Plane）基础设施）
- 仅运行 ServiceDefaults 单元测试

### 相关文档

- [部署流程](../../doc/10_部署与运维/部署流程.md) - CI/CD 详细说明
- [CodeReview 流程](../../doc/06_开发规范/CodeReview流程.md) - PR 审查标准
