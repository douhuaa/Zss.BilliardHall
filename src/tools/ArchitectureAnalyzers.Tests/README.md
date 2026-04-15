# ArchitectureAnalyzers.Tests

用于验证 `src/tools/ArchitectureAnalyzers` 中 Roslyn Analyzer 的最小单元测试集。

当前覆盖：

- `ADR0240_12`：禁止 `DomainError` / `DomainException` 使用错误码魔法字符串。

运行：

```powershell
dotnet test E:\Abp\Zss.BilliardHall\src\tools\ArchitectureAnalyzers.Tests\ArchitectureAnalyzers.Tests.csproj
```

