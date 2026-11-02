# Drift — Codebase Context

## 1. Project Overview

Drift is a programming language implemented in C#.

This repository contains the implementation of the Drift language, including its lexer, parser, AST, interpreter and supporting runtime functionality.

The language's syntax, behaviour, type system and design decisions are documented separately in `docs/Spec.md`.

**Important:** `docs/Spec.md` is the authoritative source for language specifications. Do not duplicate its contents here.

This document exists to help AI agents understand, navigate and safely modify the codebase.

## 2. Source of Truth

When working on Drift, follow this documentation hierarchy:

1. `docs/Spec.md` — Authoritative language specification.
2. Source code — Actual current implementation.
3. Tests — Expected behaviour of implemented functionality.
4. `docs/context.md` — Engineering guidance and repository conventions.
5. Other documentation — Supporting explanations and historical information.

If these sources disagree, investigate the discrepancy rather than silently choosing one.

Never assume a feature is implemented simply because it appears in `docs/Spec.md`.

## 3. Repository Structure

Before making changes, inspect the repository and understand its actual structure.

```text
.
├── Drift.sln                     # Solution file
├── README.md                     # Project overview
├── src/
│   └── DriftInterpreter/         # C# interpreter project
│       ├── Program.cs            # Entry point
│       ├── Diagnostics/          # Source positions and structured errors
│       ├── Lexer/                # Lexer, tokens and token types
│       ├── Parser/               # Parser
│       ├── AST/
│       │   ├── Expressions/      # Expression nodes
│       │   └── Statements/       # Statement nodes
│       ├── Semantic/             # Semantic analysis
│       └── Runtime/              # Interpreter, values, formatting and runtime state
├── tests/
│   └── DriftInterpreter.Tests/   # xUnit test suite
├── docs/
│   ├── Spec.md                   # Authoritative language specification
│   ├── context.md                # Engineering guidance (this document)
│   ├── Features.md               # Feature checklist
│   └── assets/                   # Documentation assets
└── tools/
    └── tree-sitter-drift/        # Tree-sitter grammar and tooling
```

Keep this section updated when the project structure changes significantly.

Do not reorganise the repository without a clear reason.

## 4. Architecture

Drift generally follows a traditional interpreter pipeline:

```text
Source Code
    |
    v
Lexer
    |
    v
Tokens
    |
    v
Parser
    |
    v
AST
    |
    v
Interpreter
    |
    v
Runtime
```

The agent must investigate how each stage is actually implemented.

### Lexer

Responsible for converting source text into tokens.

When modifying the lexer:
- Preserve consistent tokenisation behaviour.
- Handle whitespace and comments correctly.
- Ensure token positions are maintained where supported.
- Handle invalid characters appropriately.
- Keep token definitions consistent with the parser.

### Parser

Responsible for converting tokens into AST nodes.

When modifying the parser:
- Follow existing parsing conventions.
- Respect operator precedence and associativity.
- Handle invalid syntax gracefully.
- Avoid silently accepting malformed expressions.
- Keep parser behaviour consistent with the specification.

### AST

Represents the structure of parsed programs.

When modifying AST nodes:
- Follow existing inheritance and naming conventions.
- Keep nodes focused on representing syntax.
- Avoid putting interpreter-specific behaviour into AST classes unless the existing architecture explicitly requires it.
- Update all affected consumers when changing node structures.

### Interpreter

Responsible for executing AST nodes.

When modifying the interpreter:
- Preserve consistent evaluation behaviour.
- Handle runtime errors appropriately.
- Respect variable and function scope.
- Avoid duplicating evaluation logic.
- Keep runtime behaviour consistent with the language specification.

### Runtime

Responsible for supporting execution, including values, environments and built-in functionality.

Inspect existing runtime abstractions before introducing new ones.

Avoid implementing parallel systems for functionality that already exists.

## 5. C# Development Conventions

Use the conventions already established in the repository.

General preferences:

- Prefer readable, maintainable C#.
- Use descriptive names.
- Keep classes and methods focused.
- Avoid unnecessarily complicated abstractions.
- Avoid excessive one-line implementations.
- Use appropriate access modifiers.
- Avoid unnecessary dependencies.
- Handle exceptional conditions explicitly.
- Prefer existing project patterns over introducing new architectural styles.

Do not introduce architectural changes simply to follow personal preferences.

## 6. Error Handling

Errors should be handled consistently throughout the interpreter.

Distinguish between:

- Lexical errors.
- Syntax errors.
- Semantic errors, where implemented.
- Runtime errors.
- Internal implementation errors.

When modifying error handling:

- Preserve useful source location information.
- Provide understandable error messages.
- Avoid swallowing exceptions.
- Avoid exposing unnecessary internal stack traces to normal language users.
- Keep error reporting consistent.

Inspect the existing error-handling architecture before making changes.

## 7. Testing

Tests are important for preventing regressions in language behaviour.

Before modifying code:

1. Identify the relevant tests.
2. Understand existing expected behaviour.
3. Run the relevant tests where possible.

After modifying code:

1. Add tests for new functionality.
2. Add regression tests for bug fixes.
3. Run relevant existing tests.
4. Run the complete test suite when practical.
5. Build the solution.

Do not modify tests simply to make an incorrect implementation pass.

Tests should validate actual language behaviour, using `docs/Spec.md` as the reference for intended behaviour.

## 8. Making Changes

Follow an inspect-first development approach.

### Before implementation

- Read the relevant source files.
- Trace the affected functionality through the architecture.
- Identify dependencies and affected components.
- Check for existing implementations.
- Check the specification.
- Determine the smallest reasonable change.

### During implementation

- Keep changes focused.
- Preserve existing functionality.
- Reuse existing code where appropriate.
- Avoid unnecessary refactoring.
- Avoid unrelated modifications.
- Keep documentation and tests updated.

### After implementation

- Build the project.
- Run relevant tests.
- Review the diff.
- Check for unintended changes.
- Report any remaining problems.

## 9. Refactoring

Refactoring should preserve existing functionality.

Do not rewrite entire subsystems without a clear architectural reason.

When refactoring:

- Preserve public interfaces unless a change is necessary.
- Preserve language behaviour.
- Avoid combining unrelated changes.
- Keep changes reviewable.
- Ensure tests still pass.

If a larger architectural improvement is warranted, explain the proposed changes before implementing them.

## 10. Git Practices

Treat the existing Git history and working tree carefully.

- Never discard uncommitted user changes.
- Do not reset, force-push or rewrite history without explicit permission.
- Do not delete branches or tags without permission.
- Avoid committing unrelated changes.
- Do not automatically commit completed work unless requested.

Before substantial changes, inspect the current Git status.

## 11. Documentation

Keep documentation accurate and consistent.

- `docs/Spec.md` defines language behaviour.
- `docs/context.md` defines engineering guidance.
- `architecture.md` should describe the actual implementation.
- `TODO.md` should contain outstanding development tasks.

Do not duplicate the language specification in other documents.

Clearly distinguish implemented functionality from planned functionality.

## 12. AI Agent Behaviour

The agent should act as a careful development assistant rather than independently redesigning the language.

Priorities:

1. Understand the existing code.
2. Follow established architecture.
3. Respect `docs/Spec.md`.
4. Make minimal, appropriate changes.
5. Preserve existing functionality.
6. Test changes.
7. Explain significant decisions.

The agent should not:

- Invent existing classes or files.
- Assume planned features are implemented.
- Change language behaviour without checking `docs/Spec.md`.
- Perform large rewrites without approval.
- Remove functionality to simplify implementation.
- Introduce unnecessary dependencies.
- Make destructive Git changes.

For ambiguous or significant architectural decisions, discuss the options before proceeding.

## 13. Initial Repository Investigation

When first entering the repository, the agent should:

1. Read `docs/context.md`.
2. Read `docs/Spec.md`.
3. Inspect the complete repository structure.
4. Identify the actual execution pipeline.
5. Understand the responsibilities of important classes.
6. Identify existing tests and build commands.
7. Identify incomplete implementations and technical debt.
8. Document the findings in `architecture.md`.

Do not modify source code during this initial investigation unless explicitly instructed.

The objective is to establish an accurate understanding of the codebase before development begins.
