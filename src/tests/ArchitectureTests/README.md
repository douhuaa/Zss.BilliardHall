# ArchitectureTests（架构约束测试）

这组测试的目的不是“测试业务逻辑”，而是把**架构纪律**写成可执行规则：
- 防止模块之间互相引用导致耦合爆炸
- 防止在模块里回到传统三层（Application/Domain/Infrastructure）命名
- 防止出现“Repository/Service/Manager”等会鼓励贫血模型/分层思维的语义
- 明确 Modules 只能依赖 Platform（而不是 Host / Application）

> 这些测试要做到两件事：**有杀伤力**（能抓到违规）+ **低维护**（不依赖 bin 固定路径、不依赖开发机目录）。

---

## 运行方式

在解决方案根目录运行：

```powershell
# 先编译（保证模块程序集存在）
dotnet build

# 再跑架构测试
dotnet test .\src\tests\ArchitectureTests\ArchitectureTests.csproj
```

如需指定配置（CI 常用 Release）：

```powershell
dotnet build -c Release
$env:Configuration = "Release"
dotnet test .\src\tests\ArchitectureTests\ArchitectureTests.csproj -c Release
```

---

## 常见失败与排查

### 1) `System.InvalidOperationException: No data found for ...`
**含义**：xUnit 的数据源（ClassData/MemberData）在 Discover 阶段没有产出任何行。

在本项目里，最常见原因是：
- Modules 程序集没有被构建出来（`src/Modules/*/bin/.../*.dll` 不存在）
- 测试进程的工作目录找不到 `Zss.BilliardHall.slnx`，导致解决方案根目录识别失败

**处理步骤**：
1. 先跑 `dotnet build`（或 `dotnet build -c Release`）。
2. 确认存在类似文件：`src/Modules/Members/bin/Debug/net10.0/Members.dll`（具体 TFM 可能不同）。
3. 如果你在 Rider/VS 里运行，确保测试工作目录是解决方案根目录（或至少能够向上找到 `Zss.BilliardHall.slnx`）。

> 备注：如果你希望测试在“没构建模块 dll”时也能跑，可以将数据源改成“用 ProjectReference 直接加载”，但成本更高且会增加测试时间。当前策略是：明确失败并提示先 build。

### 2) “订单引用了会员，为什么测试能成功？”
这通常是**假绿**：例如只扫描 namespace / 只扫描某个程序集，但实际引用发生在另一个模块 dll 里。

本项目的做法是：
- 对每个 Modules 程序集逐个检查：**不允许依赖任何其他 Modules.
- 再用 csproj 层面补一道：`ProjectReference` 不允许跨模块。

因此，只要：
- 你确实构建了最新 dll
- 并且 Orders/Members 的程序集都被加载

跨模块引用就会被抓到。

### 3) CI 上通过，本地失败（或反过来）
大概率是配置不同（Debug/Release）或 TFM 不一致。

建议：
- CI 里显式设置 `Configuration=Release`
- 本地想复现 CI 就用 `dotnet test -c Release`

---

## 规则总览（当前约束）

### A. 模块隔离（程序集级）
- `Zss.BilliardHall.Modules.*` **不得依赖**其他 `Zss.BilliardHall.Modules.*`

### B. 模块内命名空间约束（仅作用于 Modules）
- 禁止出现：`.Application/.Domain/.Infrastructure/.Repository/.Service/.Shared/.Common`

### C. 模块内“语义禁令”（仅作用于 Modules）
- 类型名禁止包含：`Repository/Service/Manager/Store`

### D. 模块依赖边界
- Modules 不得依赖 `Zss.BilliardHall.Application` 或 `Zss.BilliardHall.Host`
- 允许依赖 `Zss.BilliardHall.Platform`（以及未来可能的 BuildingBlocks）

### E. csproj 依赖约束（ProjectReference）
- Modules 的 csproj 不允许引用其他模块或非白名单项目

### F. 基础设施约束（Infrastructure）
- 仓库根目录必须存在 `Directory.Packages.props`（CPM 启用）
- 所有类型的命名空间必须以 `Zss.BilliardHall` 开头（RootNamespace 约定）
- 项目命名必须遵循 `Zss.BilliardHall.*` 约定

---

## CI 集成

架构测试已集成到 GitHub Actions CI 流程中（`.github/workflows/architecture-tests.yml`）：

- **触发条件**：Push 或 Pull Request 到 main 分支
- **测试环境**：Ubuntu Latest + .NET 10.0.x
- **失败阻断**：如果任何架构测试失败，CI 将返回非 0 退出码，阻止 PR 合并

CI 工作流程：
1. Checkout 代码
2. 设置 .NET SDK
3. 恢复依赖 (`dotnet restore`)
4. 构建所有项目（排除 docs）
5. 运行架构测试 (`dotnet test`)

你可以在本地模拟 CI 环境：
```bash
dotnet restore
dotnet build src/Platform/Platform.csproj
dotnet build src/Application/Application.csproj
dotnet build src/Modules/Members/Members.csproj
dotnet build src/Modules/Orders/Orders.csproj
dotnet build src/Host/Web/Web.csproj
dotnet build src/Host/Worker/Worker.csproj
dotnet build src/tests/ArchitectureTests/ArchitectureTests.csproj
dotnet test src/tests/ArchitectureTests/ArchitectureTests.csproj --no-build --verbosity normal
```

---

## 为什么不用硬编码 `bin/Debug/netX.Y` 路径？
因为：
- CI/Release 会变
- 多 TFM 会变
- IDE/命令行工作目录会变

测试只能依赖：
- 解决方案结构（`src/Modules`）
- 或已加载程序集

本项目采用折中：**从 solution root 出发定位 Modules 的输出 dll，并提供 Debug/Release + 多 TFM 的 fallback 扫描**。
