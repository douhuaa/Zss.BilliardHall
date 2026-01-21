# GitHub Copilot Instructions

This directory contains role-based instructions for GitHub Copilot to ensure consistent behavior aligned with the project's architecture governance.

## Purpose

These instruction files define **Copilot's personality and behavioral boundaries** in this repository. They answer: "What kind of assistant are you?"

This is separate from `docs/copilot/` which contains **detailed work manuals** answering: "How do you help with specific tasks?"

## Structure

```
.github/instructions/
  ├─ base.instructions.md                 ← Core behavior (always active)
  ├─ backend.instructions.md              ← Backend development
  ├─ testing.instructions.md              ← Test writing
  ├─ documentation.instructions.md        ← Documentation
  └─ architecture-review.instructions.md  ← PR reviews (highest risk)
```

## How This Works

### Base Instructions (Always Active)

`base.instructions.md` establishes Copilot's fundamental behavior:
- Respect ADRs as constitutional law
- Treat architecture tests as hard constraints
- Never invent rules or bypass CI
- Amplify understanding, don't replace it

### Role-Specific Instructions (Context-Dependent)

Additional instructions apply based on what you're working on:

| Working on... | Additional Instructions |
|---------------|------------------------|
| Handlers, Use Cases, Domain Models | `backend.instructions.md` |
| Unit Tests, Integration Tests | `testing.instructions.md` |
| ADRs, Guides, Prompts | `documentation.instructions.md` |
| PR Reviews, Architecture Evaluation | `architecture-review.instructions.md` |

## Relationship with docs/copilot/

These two systems work together but have different purposes:

| `.github/instructions/` | `docs/copilot/` |
|------------------------|-----------------|
| **WHO** Copilot is | **HOW** Copilot works |
| Personality & boundaries | Specific procedures |
| Behavioral constraints | Detailed scenarios |
| What to never do | What to do when |
| Rarely changes | Evolves with experience |

### Example

**Base Instruction says**:
> "Never introduce cross-module dependencies"

**Copilot Prompt (docs/copilot/adr-0001.prompts.md) says**:
> "When developer wants to access another module:
> - ✅ Use domain events: `await _eventBus.Publish(...)`
> - ✅ Use contracts: `await _queryBus.Send(...)`
> - ✅ Use primitives: `Guid memberId`
> - ❌ Don't use direct references"

## When to Update

### Update Base Instructions When:
- Project adopts a new architectural principle
- A fundamental constraint changes
- New "never do this" rules emerge

### Update Role-Specific Instructions When:
- A pattern becomes common enough to formalize
- A role's boundaries need clarification
- Risk levels change

### DON'T Update For:
- Specific examples (those go in `docs/copilot/`)
- Temporary exceptions
- Individual use cases

## Three-Layer Architecture Governance

```
┌─────────────────────────────────────────────────┐
│ Layer 1: ADR (docs/adr/)                        │
│ - Constitutional law                            │
│ - Architecture Decision Records                 │
│ - Highest authority                             │
└─────────────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────────────┐
│ Layer 2: Instructions (.github/instructions/)  │
│ - Copilot's personality                         │
│ - Behavioral boundaries                         │
│ - "What kind of assistant am I?"                │
└─────────────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────────────┐
│ Layer 3: Prompts (docs/copilot/)               │
│ - Detailed work manuals                         │
│ - Scenario-specific guidance                    │
│ - "How do I handle situation X?"                │
└─────────────────────────────────────────────────┘
              ↓
┌─────────────────────────────────────────────────┐
│ Enforcement: ArchitectureTests                  │
│ - Automated validation                          │
│ - CI/CD gates                                   │
│ - Non-negotiable                                │
└─────────────────────────────────────────────────┘
```

## Usage Guidelines

### For Developers

When interacting with Copilot:
1. Copilot will follow these instructions automatically
2. If you need specific help, reference the relevant `docs/copilot/` file
3. If Copilot seems overly cautious, that's intentional—it's protecting architecture

### For Maintainers

When updating instructions:
1. Ensure consistency across all files
2. Update version numbers and dates
3. Test changes don't contradict existing ADRs
4. Document significant changes in PR

## Critical Principle

> **Copilot is放大理解能力，不替代理解**
> (Copilot amplifies understanding, doesn't replace it)

These instructions ensure Copilot:
- ✅ Helps you understand ADRs faster
- ✅ Catches violations earlier
- ✅ Suggests compliant solutions
- ❌ Doesn't let you bypass learning
- ❌ Doesn't replace human judgment
- ❌ Doesn't override architecture tests

## Quick Reference

| Scenario | Check File |
|----------|------------|
| "Can I do X in my module?" | `backend.instructions.md` |
| "How should I test this?" | `testing.instructions.md` |
| "How should I document this?" | `documentation.instructions.md` |
| "Is this PR architecturally sound?" | `architecture-review.instructions.md` |
| "What are the core rules?" | `base.instructions.md` |

For detailed "how-to" guidance, always refer to `docs/copilot/` files.

## Versioning

Current version: 1.0
Last updated: 2026-01-21

Changes to these instructions should be rare and deliberate, as they define Copilot's fundamental behavior in this repository.
