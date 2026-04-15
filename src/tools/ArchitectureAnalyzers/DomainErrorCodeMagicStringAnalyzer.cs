using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace Zss.BilliardHall.ArchitectureAnalyzers;

/// <summary>
/// ADR-240.1.2: 错误码必须引用常量，禁止魔法字符串。
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DomainErrorCodeMagicStringAnalyzer : DiagnosticAnalyzer
{
    public const string DiagnosticId = "ADR0240_12";

    private const string Category = "Architecture";
    private const string DomainErrorType = "Zss.BilliardHall.Platform.Errors.DomainError";
    private const string DomainExceptionType = "Zss.BilliardHall.Platform.Exceptions.DomainException";

    private static readonly LocalizableString Title = "Do not use magic string for error code";
    private static readonly LocalizableString MessageFormat = "{0} uses magic string error code '{1}'. Use *ErrorCodes constants instead (ADR-240.1.2).";
    private static readonly LocalizableString Description = "Error codes must be referenced from module error code constants to keep governance, registration checks, and refactoring safety.";

    private static readonly DiagnosticDescriptor Rule = new(
        DiagnosticId,
        Title,
        MessageFormat,
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true,
        description: Description);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(AnalyzeDomainErrorObjectCreation, SyntaxKind.ObjectCreationExpression);
        context.RegisterSyntaxNodeAction(AnalyzeDomainExceptionBaseInitializer, SyntaxKind.ConstructorDeclaration);
    }

    private static void AnalyzeDomainErrorObjectCreation(SyntaxNodeAnalysisContext context)
    {
        var creation = (ObjectCreationExpressionSyntax)context.Node;
        var argumentList = creation.ArgumentList;
        if (argumentList is null || argumentList.Arguments.Count < 1)
            return;

        var createdType = context.SemanticModel.GetTypeInfo(creation).Type;
        if (createdType?.ToDisplayString() != DomainErrorType)
            return;

        var firstArg = argumentList.Arguments[0].Expression;
        if (firstArg is not LiteralExpressionSyntax literal || !literal.IsKind(SyntaxKind.StringLiteralExpression))
            return;

        var value = literal.Token.ValueText;
        Report(context, literal.GetLocation(), "DomainError", value);
    }

    private static void AnalyzeDomainExceptionBaseInitializer(SyntaxNodeAnalysisContext context)
    {
        var ctor = (ConstructorDeclarationSyntax)context.Node;
        var initializer = ctor.Initializer;

        if (initializer is null || !initializer.IsKind(SyntaxKind.BaseConstructorInitializer))
            return;

        if (initializer.ArgumentList.Arguments.Count < 1)
            return;

        var symbol = context.SemanticModel.GetSymbolInfo(initializer).Symbol as IMethodSymbol;
        var containingType = symbol?.ContainingType;
        if (containingType?.ToDisplayString() != DomainExceptionType)
            return;

        var firstArg = initializer.ArgumentList.Arguments[0].Expression;
        if (firstArg is not LiteralExpressionSyntax literal || !literal.IsKind(SyntaxKind.StringLiteralExpression))
            return;

        var value = literal.Token.ValueText;
        Report(context, literal.GetLocation(), "DomainException", value);
    }

    private static void Report(SyntaxNodeAnalysisContext context, Location location, string usageType, string code)
    {
        var diagnostic = Diagnostic.Create(Rule, location, usageType, code);
        context.ReportDiagnostic(diagnostic);
    }
}
