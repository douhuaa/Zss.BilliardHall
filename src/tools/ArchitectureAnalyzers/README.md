# Zss.BilliardHall Architecture Analyzers

This package contains Roslyn analyzers that enforce architectural constraints defined in ADR-0005 (Application Interaction Model & Execution Boundaries).

## Purpose

These analyzers provide **Level 2 enforcement** (Semantic Semi-Automatic) for architectural rules that cannot be fully checked through static dependency analysis alone but require understanding of code semantics.

## Included Analyzers

### ADR0005_02: Endpoint Business Logic Analyzer

**Rule**: Endpoints should not contain business logic (ADR-0005.2)

**Detection**:
- Checks endpoint methods for indicators of business logic:
  - Excessive lines of code (>10 statements)
  - Conditional logic (if, switch)
  - Loops (for, foreach, while)
  - LINQ queries
  - Direct database operations

**Severity**: Warning

**Fix**: Move business logic to Handlers. Endpoints should only coordinate Handler calls.

### ADR0005_05: Cross-Module Call Analyzer

**Rule**: Modules should not have synchronous cross-module calls (ADR-0005.5)

**Detection**:
- Detects method invocations crossing module boundaries
- Allows approved patterns:
  - Calls to Platform layer
  - Calls to Contracts namespace
  - Messaging operations (Publish, Send, Invoke)

**Severity**: Warning

**Fix**: Use asynchronous messaging (domain events) or Application layer orchestration for cross-module communication.

### ADR0005_11: Structured Exception Analyzer

**Rule**: Handlers should use structured exceptions (ADR-0005.11)

**Detection**:
- Detects `throw new Exception()` in Handler classes
- Requires domain-specific exception types

**Severity**: Warning

**Fix**: Define and use domain-specific exception types (e.g., `DomainException`, `ValidationException`) instead of generic `System.Exception`.

## Usage

### Installation

The analyzers are automatically included when you build the solution. They are packaged and referenced by all module projects.

### Configuration

To suppress a specific analyzer warning:

```csharp
#pragma warning disable ADR0005_02
// Code that needs to be exempted
#pragma warning restore ADR0005_02
```

To disable an analyzer for a project, add to `.csproj`:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);ADR0005_02</NoWarn>
</PropertyGroup>
```

### Severity Overrides

You can override analyzer severity in `.editorconfig`:

```ini
# Make endpoint business logic a build error instead of warning
dotnet_diagnostic.ADR0005_02.severity = error

# Disable cross-module call analyzer
dotnet_diagnostic.ADR0005_05.severity = none
```

## Integration with Build

The analyzers run during:
- **Local builds**: `dotnet build`
- **IDE**: Real-time as you type (Visual Studio, Rider, VS Code with C# extension)
- **CI**: As part of the automated build pipeline

## Enforcement Levels

According to [ADR-0005 Enforcement Levels](../../docs/adr/ADR-0005-Enforcement-Levels.md):

- **Level 1 (Static)**: NetArchTest (architecture tests)
- **Level 2 (Semantic)**: These Roslyn Analyzers ← You are here
- **Level 3 (Manual)**: PR review and human judgment

## Known Limitations

These analyzers use heuristics and may produce:
- **False positives**: Flagging code that is actually compliant
- **False negatives**: Missing violations that are too complex to detect

**Important**: Analyzer warnings are advisory. If a warning is incorrect, you may suppress it with a comment explaining why. All suppressions should be reviewed during PR review.

## Development

### Building the Analyzers

```bash
cd src/tools/ArchitectureAnalyzers
dotnet build
```

### Testing the Analyzers

Create test projects in `src/tests/ArchitectureAnalyzers.Tests/` with positive and negative test cases.

### Adding New Analyzers

1. Create a new analyzer class implementing `DiagnosticAnalyzer`
2. Assign a unique diagnostic ID following the pattern `ADR0005_XX`
3. Update this README with analyzer documentation
4. Add tests for the new analyzer

## Related Documentation

- [ADR-0005: Application Interaction Model](../../docs/adr/ADR-0005-Application-Interaction-Model-Final.md)
- [ADR-0005 Enforcement Levels](../../docs/adr/ADR-0005-Enforcement-Levels.md)
- [ADR-0000: Architecture Tests](../../docs/adr/ADR-0000-architecture-tests.md)

## Support

For questions or issues with the analyzers:
1. Check if the warning is a false positive (can you suppress it?)
2. Review the related ADR documentation
3. Discuss in architecture review meetings
4. Open an issue if the analyzer has a bug
