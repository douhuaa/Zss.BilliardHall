using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Zss.BilliardHall.ArchitectureAnalyzers;

/// <summary>
/// ADR-005.5: 模块间不应有未审批的同步调用
/// Analyzer that detects synchronous cross-module calls
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class CrossModuleCallAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ADR0005_05";
    private const string Category = "Architecture";

    // Configuration constants
    private const string ModuleNamespacePrefix = "Zss.BilliardHall.Modules.";
    private const string PlatformNamespacePrefix = "Zss.BilliardHall.Platform";
    private const string ContractsNamespacePart = ".Contracts";

    private static readonly LocalizableString Title = "Synchronous cross-module call detected";
    private static readonly LocalizableString MessageFormat = "Detected potential synchronous call to module '{0}' from module '{1}'. Use asynchronous messaging instead (ADR-005.5).";
    private static readonly LocalizableString Description = "Modules should communicate asynchronously through domain events or application layer orchestration. " + "Direct synchronous calls between modules violate isolation principles.";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId,
    Title,
    MessageFormat,
    Category,
    DiagnosticSeverity.Warning,
    isEnabledByDefault: true,
    description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
    }

    private void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
    {
        var invocation = (InvocationExpressionSyntax)context.Node;
        var semanticModel = context.SemanticModel;

        // Get the symbol being invoked
        var symbolInfo = semanticModel.GetSymbolInfo(invocation);
        if (symbolInfo.Symbol is not IMethodSymbol methodSymbol)
            return;

        // Get the namespace of the invoked method
        var targetNamespace = methodSymbol.ContainingNamespace?.ToDisplayString();
        if (string.IsNullOrEmpty(targetNamespace))
            return;

        // Get the namespace of the caller
        var callerNamespace = GetContainingNamespace(invocation);
        if (string.IsNullOrEmpty(callerNamespace))
            return;

        // Check if this is a cross-module call
        var targetModule = ExtractModuleName(targetNamespace);
        var callerModule = ExtractModuleName(callerNamespace);

        if (string.IsNullOrEmpty(targetModule) || string.IsNullOrEmpty(callerModule))
            return;

        if (targetModule != callerModule && !IsAllowedCrossModuleCall(methodSymbol, targetNamespace))
        {
            var diagnostic = Diagnostic.Create(Rule,
            invocation.GetLocation(),
            targetModule,
            callerModule);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private string? GetContainingNamespace(SyntaxNode node)
    {
        var namespaceDeclaration = node
            .Ancestors()
            .OfType<BaseNamespaceDeclarationSyntax>()
            .FirstOrDefault();
        return namespaceDeclaration?.Name.ToString();
    }

    private string? ExtractModuleName(string namespaceStr)
    {
        // Expected format: Zss.BilliardHall.Modules.{ModuleName}
        if (!namespaceStr.Contains(ModuleNamespacePrefix))
            return null;

        var parts = namespaceStr.Split('.');
        var moduleIndex = System.Array.IndexOf(parts, "Modules");
        if (moduleIndex >= 0 && moduleIndex + 1 < parts.Length)
            return parts[moduleIndex + 1];

        return null;
    }

    private bool IsAllowedCrossModuleCall(IMethodSymbol methodSymbol, string targetNamespace)
    {
        // Allow calls to Platform layer
        if (targetNamespace.StartsWith(PlatformNamespacePrefix))
            return true;

        // Allow calls to Contracts namespace (data contracts for cross-module communication)
        if (targetNamespace.Contains(ContractsNamespacePart))
            return true;

        // Allow calls to messaging infrastructure (Publish, Send from Wolverine)
        var methodName = methodSymbol.Name;
        if (methodName == "Publish" || methodName == "PublishAsync" || methodName == "Send" || methodName == "SendAsync" || methodName == "Invoke" || methodName == "InvokeAsync")
            return true;

        return false;
    }
}
