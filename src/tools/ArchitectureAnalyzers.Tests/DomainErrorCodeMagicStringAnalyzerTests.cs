using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;
using Xunit;
using Zss.BilliardHall.ArchitectureAnalyzers;

namespace Zss.BilliardHall.Tools.ArchitectureAnalyzers.Tests;

public class DomainErrorCodeMagicStringAnalyzerTests
{
    [Fact]
    public async Task Should_Report_When_DomainError_Uses_String_Literal()
    {
        const string source = """
using Zss.BilliardHall.Platform.Errors;

public class Sample
{
    public void Run()
    {
        throw new DomainError("ORDERS_NOT_FOUND");
    }
}

namespace Zss.BilliardHall.Platform.Errors
{
    public class DomainError : System.Exception
    {
        public DomainError(string code) : base(code) {}
    }
}
""";

        var diagnostics = await GetDiagnosticsAsync(source);

        Assert.Contains(diagnostics, d => d.Id == DomainErrorCodeMagicStringAnalyzer.DiagnosticId);
    }

    [Fact]
    public async Task Should_Report_When_DomainException_Base_Uses_String_Literal()
    {
        const string source = """
using Zss.BilliardHall.Platform.Exceptions;

public sealed class MemberEmailExistsException : DomainException
{
    public MemberEmailExistsException() : base("MEMBER_EMAIL_EXISTS", "邮箱已存在") {}
}

namespace Zss.BilliardHall.Platform.Exceptions
{
    public abstract class DomainException : System.Exception
    {
        protected DomainException(string errorCode, string message) : base(message) {}
    }
}
""";

        var diagnostics = await GetDiagnosticsAsync(source);

        Assert.Contains(diagnostics, d => d.Id == DomainErrorCodeMagicStringAnalyzer.DiagnosticId);
    }

    [Fact]
    public async Task Should_Not_Report_When_Using_ErrorCodes_Constant()
    {
        const string source = """
using Zss.BilliardHall.Platform.Errors;

public static class OrdersErrorCodes
{
    public const string NotFound = "ORDERS_NOT_FOUND";
}

public class Sample
{
    public void Run()
    {
        throw new DomainError(OrdersErrorCodes.NotFound);
    }
}

namespace Zss.BilliardHall.Platform.Errors
{
    public class DomainError : System.Exception
    {
        public DomainError(string code) : base(code) {}
    }
}
""";

        var diagnostics = await GetDiagnosticsAsync(source);

        Assert.DoesNotContain(diagnostics, d => d.Id == DomainErrorCodeMagicStringAnalyzer.DiagnosticId);
    }

    private static async Task<IReadOnlyList<Diagnostic>> GetDiagnosticsAsync(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new[]
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
            MetadataReference.CreateFromFile(typeof(System.Runtime.GCSettings).Assembly.Location)
        };

        var compilation = CSharpCompilation.Create(
            assemblyName: "AnalyzerTests",
            syntaxTrees: new[] { syntaxTree },
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var analyzer = new DomainErrorCodeMagicStringAnalyzer();
        var analyzerOptions = new AnalyzerOptions(ImmutableArray<AdditionalText>.Empty);
        var compilationWithAnalyzers = compilation.WithAnalyzers(
            ImmutableArray.Create<DiagnosticAnalyzer>(analyzer),
            new CompilationWithAnalyzersOptions(analyzerOptions, onAnalyzerException: null, concurrentAnalysis: false, logAnalyzerExecutionTime: false));

        return await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync();
    }
}
