# 全局 Using 语句迁移指南

## 概述

本文档记录了项目从传统 using 语句到全局 using 语句的迁移过程。

## 什么是全局 Using？

全局 using（C# 10+）允许在一个文件中声明 using 语句，这些声明会应用到整个项目。这可以：
- 减少每个文件中的重复代码
- 提高代码可读性
- 统一管理项目级别的命名空间依赖

## 迁移结果

### 创建的 GlobalUsings.cs 文件

我们为以下项目创建了 GlobalUsings.cs 文件：

1. **Platform** (`src/Platform/GlobalUsings.cs`)
2. **Application** (`src/Application/GlobalUsings.cs`)
3. **Members 模块** (`src/Modules/Members/GlobalUsings.cs`)
4. **Orders 模块** (`src/Modules/Orders/GlobalUsings.cs`)
5. **Web Host** (`src/Host/Web/GlobalUsings.cs`)
6. **Worker Host** (`src/Host/Worker/GlobalUsings.cs`)
7. **ArchitectureAnalyzers** (`src/tools/ArchitectureAnalyzers/GlobalUsings.cs`)
8. **AdrSemanticParser** (`src/tools/AdrSemanticParser/Zss.BilliardHall.AdrSemanticParser/GlobalUsings.cs`)
9. **AdrSemanticParser.Tests** (`src/tools/AdrSemanticParser/Tests/GlobalUsings.cs`)
10. **ArchitectureTests** (已存在)

### 迁移统计

- **创建的 GlobalUsings.cs 文件**：9 个新文件 + 1 个已存在
- **清理的文件**：9 个 .cs 文件
- **移除的重复 using 语句**：约 45 行
- **编译状态**：✅ 所有项目编译成功
- **测试状态**：✅ 所有测试通过

## 各项目的全局 Using 内容

### 1. Platform 和 Application 项目

这两个项目使用相同的全局 using，因为它们都是基础设施层：

```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Logging;
```

### 2. Members 和 Orders 模块

模块项目使用基本的系统命名空间：

```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Threading;
global using System.Threading.Tasks;
```

### 3. Host 项目（Web 和 Worker）

Host 项目包含应用程序引导命名空间：

```csharp
global using System;
global using System.Threading.Tasks;

global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;

global using Zss.BilliardHall.Platform;
global using Zss.BilliardHall.Application;
```

### 4. ArchitectureAnalyzers

分析器项目使用 Roslyn API：

```csharp
global using System;
global using System.Collections.Generic;
global using System.Collections.Immutable;
global using System.Linq;

global using Microsoft.CodeAnalysis;
global using Microsoft.CodeAnalysis.CSharp;
global using Microsoft.CodeAnalysis.CSharp.Syntax;
global using Microsoft.CodeAnalysis.Diagnostics;
```

### 5. AdrSemanticParser

ADR 解析器使用 Markdown 处理库：

```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;
global using System.Text.RegularExpressions;

global using Markdig;
global using Markdig.Syntax;
global using Markdig.Syntax.Inlines;

global using Zss.BilliardHall.AdrSemanticParser.Models;
```

### 6. AdrSemanticParser.Tests

测试项目使用测试框架：

```csharp
global using System;
global using System.Collections.Generic;
global using System.Linq;

global using Xunit;
global using FluentAssertions;

global using Zss.BilliardHall.AdrSemanticParser;
global using Zss.BilliardHall.AdrSemanticParser.Models;
```

## 清理的文件

### 示例 1: PlatformBootstrapper.cs

**清理前**：
```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace Zss.BilliardHall.Platform;
```

**清理后**：
```csharp
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;

namespace Zss.BilliardHall.Platform;
```

说明：Microsoft.Extensions.* 命名空间已在 GlobalUsings.cs 中定义，因此从文件中移除。保留了 OpenTelemetry 和 Serilog，因为这些是文件特定的依赖。

### 示例 2: Host/Web/Program.cs

**清理前**：
```csharp
using Serilog;
using Zss.BilliardHall.Application;
using Zss.BilliardHall.Platform;

var builder = WebApplication.CreateBuilder(args);
```

**清理后**：
```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);
```

说明：Platform 和 Application 命名空间已在 GlobalUsings.cs 中定义。

## 最佳实践

### 1. 选择全局 Using 的原则

只将以下类型的命名空间放入 GlobalUsings.cs：
- ✅ 项目中 80% 以上的文件都会使用的命名空间
- ✅ 基础系统命名空间（System, System.Collections.Generic 等）
- ✅ 项目级别的核心依赖（如 Microsoft.Extensions.*）
- ✅ 项目自己的基础命名空间

不要放入：
- ❌ 只在少数文件中使用的命名空间
- ❌ 第三方库的特定命名空间（除非广泛使用）
- ❌ 可能引起命名冲突的命名空间

### 2. 文件组织建议

```csharp
// GlobalUsings.cs - 按类别组织
// 系统命名空间
global using System;
global using System.Collections.Generic;
global using System.Linq;

// 第三方库
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;

// 项目命名空间
global using YourProject.Core;
global using YourProject.Infrastructure;
```

### 3. 清理现有文件

清理文件时遵循以下步骤：
1. 识别哪些 using 已在 GlobalUsings.cs 中定义
2. 从文件中移除这些 using
3. 保留文件特定的 using
4. 按字母顺序排列剩余的 using
5. 编译验证无错误

## 编译和测试结果

### 编译结果
```
✅ Platform: Build succeeded
✅ Application: Build succeeded
✅ Members: Build succeeded
✅ Orders: Build succeeded
✅ Web: Build succeeded
✅ Worker: Build succeeded
✅ ArchitectureAnalyzers: Build succeeded (8 warnings - 预先存在)
✅ AdrSemanticParser: Build succeeded
✅ AdrSemanticParser.Tests: Build succeeded (1 warning - 预先存在)
✅ ArchitectureTests: Build succeeded (7 warnings - 预先存在)
```

### 测试结果
```
✅ AdrSemanticParser.Tests: 25/25 tests passed
```

## 好处

1. **代码简洁**：每个文件的 using 语句减少 3-8 行
2. **统一管理**：项目级别依赖在一个地方管理
3. **易于理解**：文件顶部只显示文件特定的依赖
4. **减少冲突**：减少 merge conflict 的可能性
5. **提升性能**：编译器可以更好地优化

## 维护指南

### 添加新的全局 Using

当需要添加新的全局 using 时：
1. 确认这个命名空间会被大多数文件使用
2. 在对应项目的 GlobalUsings.cs 中添加
3. 重新编译验证
4. 考虑从现有文件中清理这个 using

### 移除全局 Using

如果某个全局 using 不再广泛使用：
1. 从 GlobalUsings.cs 中移除
2. 使用 IDE 的"添加缺失的 using"功能
3. 或使用以下命令查找需要添加 using 的文件：
   ```bash
   dotnet build 2>&1 | grep "CS0246\|CS0103"
   ```

## 故障排除

### 问题：编译错误 CS0234（命名空间不存在）

**原因**：GlobalUsings.cs 中引用了项目未引用的程序集。

**解决方案**：
1. 检查项目的 .csproj 文件
2. 确保引用了对应的 NuGet 包或项目
3. 或从 GlobalUsings.cs 中移除该 using

### 问题：命名冲突

**原因**：全局 using 中的类型与本地类型或其他命名空间冲突。

**解决方案**：
1. 使用完全限定名称（包含命名空间）
2. 或从 GlobalUsings.cs 中移除冲突的 using
3. 在特定文件中使用 using alias：
   ```csharp
   using MyType = Namespace1.Type;
   ```

## 总结

全局 using 迁移已成功完成！项目现在使用更现代的 C# 10 功能，代码更简洁、更易维护。

**统计数据**：
- 10 个项目已迁移
- 9 个新的 GlobalUsings.cs 文件
- 约 45 行重复代码被移除
- 0 个编译错误
- 所有测试通过

**下一步**：
- 持续优化：随着项目发展，定期审查全局 using 的合理性
- 团队培训：确保团队成员理解全局 using 的使用原则
- 代码审查：在 Code Review 中检查 using 语句的合理性
