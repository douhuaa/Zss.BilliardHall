using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace Zss.BilliardHall.ArchitectureAnalyzers;

/// <summary>
/// ADR-0005.2: Endpoint 不应包含业务逻辑
/// Analyzer that detects business logic in endpoint methods
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EndpointBusinessLogicAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ADR0005_02";
    private const string Category = "Architecture";
    
    // Configuration constants for scoring algorithm
    private const int BusinessLogicScoreThreshold = 10;
    private const int ManyStatementsScore = 5;
    private const int SomeStatementsScore = 2;
    private const int ConditionalScore = 3;
    private const int LoopScore = 4;
    private const int QueryScore = 3;
    private const int DbOperationScore = 2;
    private const int ManyStatementsThreshold = 10;
    private const int SomeStatementsThreshold = 5;

    private static readonly LocalizableString Title = "Endpoint contains business logic";
    private static readonly LocalizableString MessageFormat = 
        "Endpoint method '{0}' contains business logic. Endpoints should only coordinate Handler calls (ADR-0005.2)";
    private static readonly LocalizableString Description = 
        "Endpoints should be thin coordination layers that delegate to Handlers. " +
        "Business logic, conditionals, and data manipulation should be in Handlers.";

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
        context.RegisterSyntaxNodeAction(AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
    }

    private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
    {
        var methodDeclaration = (MethodDeclarationSyntax)context.Node;
        
        // Check if this is likely an endpoint method
        if (!IsLikelyEndpointMethod(methodDeclaration, context))
            return;

        // Check for business logic indicators
        var businessLogicScore = CalculateBusinessLogicScore(methodDeclaration);
        
        // If score exceeds threshold, report diagnostic
        if (businessLogicScore > BusinessLogicScoreThreshold)
        {
            var diagnostic = Diagnostic.Create(
                Rule,
                methodDeclaration.Identifier.GetLocation(),
                methodDeclaration.Identifier.Text);
            context.ReportDiagnostic(diagnostic);
        }
    }

    private bool IsLikelyEndpointMethod(MethodDeclarationSyntax method, SyntaxNodeAnalysisContext context)
    {
        // Check if method has common endpoint attributes
        var attributes = method.AttributeLists
            .SelectMany(al => al.Attributes)
            .Select(a => a.Name.ToString())
            .ToList();

        var endpointAttributes = new[] { "HttpGet", "HttpPost", "HttpPut", "HttpDelete", "HttpPatch", "MapGet", "MapPost" };
        
        if (attributes.Any(attr => endpointAttributes.Any(ea => attr.Contains(ea))))
            return true;

        // Check if method is in a class with Controller/Endpoint suffix
        var classDeclaration = method.Ancestors().OfType<ClassDeclarationSyntax>().FirstOrDefault();
        if (classDeclaration != null)
        {
            var className = classDeclaration.Identifier.Text;
            if (className.EndsWith("Controller") || className.EndsWith("Endpoint") || className.EndsWith("Endpoints"))
                return true;
        }

        return false;
    }

    private int CalculateBusinessLogicScore(MethodDeclarationSyntax method)
    {
        int score = 0;
        var body = method.Body;
        if (body == null)
            return 0;

        // Count lines of code (rough approximation)
        var statements = body.Statements.Count;
        if (statements > ManyStatementsThreshold)
            score += ManyStatementsScore;
        else if (statements > SomeStatementsThreshold)
            score += SomeStatementsScore;

        // Check for conditional logic (if, switch, etc.)
        var conditionals = body.DescendantNodes()
            .Count(n => n is IfStatementSyntax || n is SwitchStatementSyntax);
        score += conditionals * ConditionalScore;

        // Check for loops
        var loops = body.DescendantNodes()
            .Count(n => n is ForStatementSyntax || n is ForEachStatementSyntax || n is WhileStatementSyntax);
        score += loops * LoopScore;

        // Check for LINQ queries
        var queries = body.DescendantNodes()
            .Count(n => n is QueryExpressionSyntax);
        score += queries * QueryScore;

        // Check for database context usage (Select, Where, etc.)
        var dbOperations = body.DescendantNodes()
            .OfType<InvocationExpressionSyntax>()
            .Count(inv => {
                var methodName = inv.Expression.ToString();
                return methodName.Contains("Select") || methodName.Contains("Where") || 
                       methodName.Contains("FirstOrDefault") || methodName.Contains("Any");
            });
        score += dbOperations * DbOperationScore;

        return score;
    }
}
