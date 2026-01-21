using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Zss.BilliardHall.ArchitectureAnalyzers;

/// <summary>
/// ADR-0005.11: Handler 应使用结构化异常
/// Analyzer that detects usage of generic Exception instead of domain-specific exceptions
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class StructuredExceptionAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ADR0005_11";
    private const string Category = "Architecture";

    private static readonly LocalizableString Title = "Handler uses generic Exception instead of structured exception";
    private static readonly LocalizableString MessageFormat = 
        "Handler '{0}' throws generic Exception. Use domain-specific exception types (ADR-0005.11)";
    private static readonly LocalizableString Description = 
        "Handlers should throw structured, domain-specific exceptions (e.g., DomainException, ValidationException) " +
        "instead of generic System.Exception to enable proper error handling and domain semantics.";

    private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(
        DiagnosticId,
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
        context.RegisterSyntaxNodeAction(AnalyzeThrowStatement, SyntaxKind.ThrowStatement);
    }

    private void AnalyzeThrowStatement(SyntaxNodeAnalysisContext context)
    {
        var throwStatement = (ThrowStatementSyntax)context.Node;
        
        // Check if this is in a Handler
        if (!IsInHandler(throwStatement))
            return;

        // Check if throwing generic Exception
        if (throwStatement.Expression is ObjectCreationExpressionSyntax objectCreation)
        {
            var semanticModel = context.SemanticModel;
            var typeInfo = semanticModel.GetTypeInfo(objectCreation);
            
            if (typeInfo.Type != null)
            {
                var typeName = typeInfo.Type.ToDisplayString();
                
                // Check if it's throwing System.Exception (not a subclass)
                if (typeName == "System.Exception")
                {
                    var handlerName = GetContainingHandlerName(throwStatement);
                    var diagnostic = Diagnostic.Create(
                        Rule,
                        throwStatement.GetLocation(),
                        handlerName ?? "Unknown");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }

    private bool IsInHandler(SyntaxNode node)
    {
        // Check if we're in a class that looks like a Handler
        var classDeclaration = node.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDeclaration == null)
            return false;

        var className = classDeclaration.Identifier.Text;
        return className.EndsWith("Handler") || 
               className.EndsWith("CommandHandler") || 
               className.EndsWith("QueryHandler") || 
               className.EndsWith("EventHandler");
    }

    private string? GetContainingHandlerName(SyntaxNode node)
    {
        var methodDeclaration = node.Ancestors().OfType<MethodDeclarationSyntax>().FirstOrDefault();
        if (methodDeclaration != null)
        {
            var classDeclaration = methodDeclaration.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
            return classDeclaration?.Identifier.Text;
        }
        return null;
    }
}
