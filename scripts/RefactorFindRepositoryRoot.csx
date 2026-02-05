#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.CodeAnalysis.CSharp, 4.8.0"
#r "nuget: Microsoft.CodeAnalysis.CSharp.Workspaces, 4.8.0"

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// 配置
var testDirectory = Path.Combine(Environment.CurrentDirectory, "src", "tests", "ArchitectureTests");
var targetNamespace = "Zss.BilliardHall.Tests.ArchitectureTests.Shared";
var replacementExpression = "TestEnvironment.RepositoryRoot";

Console.WriteLine("========================================");
Console.WriteLine("FindRepositoryRoot 自动化重构工具");
Console.WriteLine("========================================\n");

// 统计信息
int filesProcessed = 0;
int filesModified = 0;
int methodsRemoved = 0;
int callsReplaced = 0;
var modifiedFiles = new List<string>();

// 查找所有 C# 文件
var csFiles = Directory.GetFiles(testDirectory, "*.cs", SearchOption.AllDirectories)
    .Where(f => !f.Contains("TestEnvironment.cs")) // 排除 TestEnvironment 本身
    .ToList();

Console.WriteLine($"找到 {csFiles.Count} 个 C# 文件待处理\n");

foreach (var filePath in csFiles)
{
    filesProcessed++;
    var relativePath = Path.GetRelativePath(Environment.CurrentDirectory, filePath);
    
    try
    {
        var sourceCode = File.ReadAllText(filePath);
        var tree = CSharpSyntaxTree.ParseText(sourceCode);
        var root = tree.GetRoot();
        
        bool fileModified = false;
        var currentRoot = root;
        
        // 1. 查找并替换 FindRepositoryRoot() 调用
        var invocations = currentRoot.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Where(inv => 
            {
                var identifier = inv.Expression as IdentifierNameSyntax;
                return identifier?.Identifier.Text == "FindRepositoryRoot";
            })
            .ToList();
        
        if (invocations.Any())
        {
            Console.WriteLine($"处理: {relativePath}");
            Console.WriteLine($"  找到 {invocations.Count} 个 FindRepositoryRoot() 调用");
            
            foreach (var invocation in invocations)
            {
                var newExpression = SyntaxFactory.ParseExpression(replacementExpression)
                    .WithLeadingTrivia(invocation.GetLeadingTrivia())
                    .WithTrailingTrivia(invocation.GetTrailingTrivia());
                
                currentRoot = currentRoot.ReplaceNode(invocation, newExpression);
                callsReplaced++;
                fileModified = true;
            }
        }
        
        // 2. 查找并删除 FindRepositoryRoot 方法定义
        var methods = currentRoot.DescendantNodes()
            .OfType<MethodDeclarationSyntax>()
            .Where(m => m.Identifier.Text == "FindRepositoryRoot")
            .ToList();
        
        foreach (var method in methods)
        {
            // 保留注释分隔符（如果存在）
            var triviaToKeep = method.GetLeadingTrivia()
                .Where(t => t.IsKind(SyntaxKind.SingleLineCommentTrivia) && 
                           t.ToString().Contains("====="))
                .ToList();
            
            currentRoot = currentRoot.RemoveNode(method, SyntaxRemoveOptions.KeepNoTrivia);
            methodsRemoved++;
            fileModified = true;
            
            if (invocations.Any())
            {
                Console.WriteLine($"  删除 FindRepositoryRoot 方法定义");
            }
        }
        
        // 3. 确保有正确的 using 声明
        var compilationUnit = currentRoot as CompilationUnitSyntax;
        if (compilationUnit != null && fileModified)
        {
            var hasTargetUsing = compilationUnit.Usings
                .Any(u => u.Name?.ToString() == targetNamespace);
            
            if (!hasTargetUsing)
            {
                var newUsing = SyntaxFactory.UsingDirective(
                    SyntaxFactory.ParseName(targetNamespace))
                    .WithTrailingTrivia(SyntaxFactory.CarriageReturnLineFeed);
                
                var lastUsing = compilationUnit.Usings.LastOrDefault();
                if (lastUsing != null)
                {
                    // 在最后一个 using 之后添加
                    currentRoot = compilationUnit.InsertNodesAfter(lastUsing, new[] { newUsing });
                }
                else
                {
                    // 如果没有 using，在文件开头添加
                    currentRoot = compilationUnit.WithUsings(
                        SyntaxFactory.List(new[] { newUsing }));
                }
                
                Console.WriteLine($"  添加 using {targetNamespace}");
            }
        }
        
        // 4. 保存修改后的文件
        if (fileModified)
        {
            var newCode = currentRoot.ToFullString();
            File.WriteAllText(filePath, newCode, Encoding.UTF8);
            filesModified++;
            modifiedFiles.Add(relativePath);
            Console.WriteLine($"  ✓ 文件已更新\n");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ✗ 错误: {ex.Message}\n");
    }
}

// 输出统计信息
Console.WriteLine("========================================");
Console.WriteLine("重构完成统计");
Console.WriteLine("========================================");
Console.WriteLine($"处理文件数: {filesProcessed}");
Console.WriteLine($"修改文件数: {filesModified}");
Console.WriteLine($"替换调用数: {callsReplaced}");
Console.WriteLine($"删除方法数: {methodsRemoved}");
Console.WriteLine();

if (modifiedFiles.Any())
{
    Console.WriteLine("修改的文件列表:");
    foreach (var file in modifiedFiles)
    {
        Console.WriteLine($"  - {file}");
    }
}

Console.WriteLine("\n重构工具执行完成！");
