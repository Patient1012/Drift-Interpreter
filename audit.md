# Drift Initial Project Audit

> Audit date: 2026-11-02  
> Branch: `master`  
> HEAD: `758630f` (`Update README.md`)  
> Scope: investigation and documentation only. No source code, tests, or specification files were modified.

> **Post-audit update (2026-11-02):** The critical items C1–C4, plus H1–H8 and H10, have since been implemented. This includes source-located diagnostics, format specifiers, automatic case-insensitive `main`, specification updates, and an xUnit test suite. The solution builds with 0 warnings/errors, `dotnet test Drift.sln` passes 26 tests, `./drift examples/hello.drf` runs, and `TODO.md` tracks the remaining work. The findings below describe the pre-implementation state and are retained as the audit record.

## 1. Environment verification

### Commands run

```text
$ command -v dotnet
(no output; exit 127)

$ dotnet --info
/bin/bash: line 1: dotnet: command not found

$ dotnet --list-sdks
/bin/bash: line 1: dotnet: command not found

$ dotnet --list-runtimes
/bin/bash: line 1: dotnet: command not found

$ git --version
git version 2.56.0

$ command -v nuget
(no output; exit 127)

$ nuget help
/bin/bash: line 1: nuget: command not found
```

### Host

```text
Linux VectorMachine 7.3.0-1-default #1 SMP PREEMPT_DYNAMIC Mon Oct 19 15:21:44 UTC 2026 x86_64
openSUSE Tumbleweed 20261030
```

### Configuration files found

- `Drift.sln`
- `src/DriftInterpreter/DriftInterpreter.csproj`

### Configuration files not found

- `global.json`
- `Directory.Build.props`
- `Directory.Packages.props`
- `NuGet.config` / `nuget.config`

### Result

The .NET environment is **not usable in this environment**:

- No `dotnet` executable is installed or on `PATH`.
- No SDK, runtime, or NuGet CLI could be queried.
- The project targets `net10.0`, so a .NET 10 SDK is required unless the target framework is deliberately changed.
- There is no `global.json`, so the SDK version is not pinned.
- The project has no NuGet dependencies, so dependency restore is not expected to be a blocker once an SDK is available.

`git` is installed and working. No software was installed or configured during this audit.

## 2. Build results

### Commands run

```text
$ dotnet restore
/bin/bash: line 1: dotnet: command not found
exit=127

$ dotnet build
/bin/bash: line 1: dotnet: command not found
exit=127

$ dotnet test
/bin/bash: line 1: dotnet: command not found
exit=127
```

### Result

Restore, build, and test **could not be executed** because the SDK is missing. This means there are no compiler results, warnings, or runtime results to report.

### Static build finding

A source inspection found a definite build blocker:

- `Parser.cs` declares `ParseIf()` returning `IfStatement`.
- `Parser.cs` constructs `new IfStatement(...)`.
- `Interpreter.cs` performs `statement is IfStatement`.
- No `IfStatement` class exists anywhere in the repository (there is no `IfStatement.cs` and no type definition).

This will produce a missing-type compilation error once a compiler is available. The project is therefore almost certainly **not building at HEAD**, even though the exact compiler output could not be captured.

### Project configuration

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>G_Sharp</RootNamespace>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
</Project>
```

Observations:

- `RootNamespace` still uses the legacy `G_Sharp` name, but no file declares a namespace, so the value currently has no effect.
- `Nullable` is enabled. Several assignments pass `object?` into `object` parameters, which should produce nullable warnings, not errors.
- No multiple target frameworks, configurations, or conditional compilation are present.

## 3. Test results

### Test discovery

No test project, test framework reference, test file, test runner configuration, or sample `.drf` corpus was found.

The only executable program is the hard-coded string in `src/DriftInterpreter/Program.cs`.

### Result

- Total tests: **0**
- Passing: **0**
- Failing: **0**
- Skipped: **0**
- `dotnet test`: could not run because the SDK is missing.

Even with an SDK installed, the solution contains no test project, so `dotnet test` would not exercise interpreter behaviour.

### Coverage assessment

Every component is untested:

- Lexer tokenisation and error handling.
- Parser statement and expression parsing.
- AST construction.
- Semantic analysis.
- Interpreter execution, control flow, type checks, and function calls.
- Runtime value and array behaviour.

## 4. Repository overview

### Tracked layout

```text
Drift.sln
README.md
docs/
  Spec.md
  context.md
  Features.md
  assets/ascii.txt
src/DriftInterpreter/
  DriftInterpreter.csproj
  Program.cs
  Lexer/{Lexer.cs, Token.cs, TokenType.cs}
  Parser/Parser.cs
  AST/Expressions/{Expression, LiteralExpression, TypedLiteralExpression,
                   VariableExpression, BinaryExpression, CallExpression,
                   ArrayAccess}.cs
  AST/Statements/{Statement, VarDeclaration, AssignmentStatement,
                  PrintStatement, CallStatement, ReturnStatement,
                  FunctionDeclaration, WhileStatement, ArrayDeclaration,
                  ArrayAssignment}.cs
  Semantic/SemanticAnalyser.cs
  Runtime/{Interpreter, Value, ValueType, ArrayValue,
           ReturnException, Variable}.cs
tools/tree-sitter-drift/{grammar.js, package.json}
```

The C# code is in a single project and a single global namespace. There are no `PackageReference` or `ProjectReference` entries.

### Code size

- 29 C# files.
- Approximately 1,242 lines of C#.

### Git state

The working tree contains uncommitted work from before and during the repository restructuring:

- Source and documentation paths are staged as renames.
- `obj/` build artifacts were removed from tracking.
- `Specification.md` is deleted in the working tree.
- `docs/Spec.md` is untracked and is identical to `HEAD:Specification.md`.
- `InterpreterInDrift/lexer.drf` is deleted in the working tree. Its committed content was a placeholder: `pub class Lexer { }`.
- `.gitignore` is untracked.
- `README.md` and `docs/context.md` have uncommitted edits.

These changes were preserved. No user changes were discarded.

### Documentation state

- `docs/Spec.md` is the authoritative language specification.
- `docs/context.md` provides engineering guidance.
- `docs/Features.md` is a short feature checklist.
- `README.md` contains the project overview and a basic build command.
- `docs/context.md` refers to `docs/architecture.md` and `docs/TODO.md`, but this audit was instructed to create the documents in the repository root. Those paths therefore need reconciliation later.

### Tooling

`tools/tree-sitter-drift/` contains a syntax grammar for editor tooling. It is not referenced by the C# solution and is not built or tested.

## 5. Architecture summary

The implementation is a conventional hand-written interpreter pipeline:

```text
Source string
   -> Lexer.Tokenize()        -> List<Token>
   -> Parser.Parse()          -> List<Statement>
   -> SemanticAnalyser.Analyse()
   -> Interpreter.Run()
```

### Component responsibilities

| Component | Files | Responsibility | Actual state |
|---|---|---|---|
| Entry point | `Program.cs` | Wire pipeline, supply source | Hard-coded sample; file input commented out |
| Lexer | `Lexer.cs`, `Token.cs`, `TokenType.cs` | Text to tokens | Integer/string/operator tokenisation; no comments, no positions |
| Parser | `Parser.cs` | Tokens to AST | Partial recursive descent; missing AST type; incomplete expressions |
| AST | `AST/**` | Syntax representation | Simple nodes; one required node missing; several unused nodes |
| Semantic analysis | `SemanticAnalyser.cs` | Validation/resolution | Placeholder that prints declarations |
| Interpreter | `Interpreter.cs` | Execute statements/evaluate expressions | Global variables, partial control flow, incomplete values/functions |
| Runtime | `Value`, `ValueType`, `ArrayValue`, `ReturnException`, `Variable` | Runtime data | `Value` and `ArrayValue` used; `Variable` unused; type mapping inconsistent |
| Tooling | `tools/tree-sitter-drift` | Editor grammar | Independent of interpreter; incomplete relative to specification |

### Execution details

- `Program.Main` creates a `Lexer`, `Parser`, `SemanticAnalyser`, and `Interpreter` directly.
- The interpreter stores functions and variables in two global dictionaries.
- There is no environment stack, call frame, block scope, or parameter binding.
- `ReturnException` implements function returns as non-local control flow.
- `SemanticAnalyser` has no effect on execution.
- Errors are plain `Exception` instances.
- Parser and interpreter contain unconditional `Console.WriteLine` debug output.

Full details are in `architecture.md`.

## 6. Specification implementation inventory

Status definitions:

- **Implemented:** Code exists and has been verified.
- **Partially implemented:** Some code exists, but the feature is incomplete or unverified.
- **Not implemented:** No meaningful implementation.
- **Unverified:** Code exists but cannot be executed in this environment.

Because no .NET runtime was available, **no feature could be verified by execution**. All statuses below are based on source inspection. Features described as partially implemented should be treated as unverified until a build and test run are possible.

| Specification area | Status | Evidence / notes |
|---|---|---|
| Basic statements and semicolon termination | Partially implemented | Statements are parsed; token kinds are consumed without validation |
| Brace-delimited blocks | Partially implemented | Function/if/while blocks parsed; no block scope |
| Single-line comments | Not implemented | Lexer has no comment handling |
| Multi-line comments | Not implemented | Lexer has no comment handling |
| Signed integer type names (`i8`–`i64`) | Partially implemented | Lexed and mapped, but all numeric literals evaluate to `int`, so only `i32` matches at runtime |
| Unsigned integer types (`ui8`–`ui64`) | Not implemented | Lexed, but `ParseValueType` maps them to signed `ValueType` values |
| Signed floating types (`f16`–`f64`) | Not implemented | Lexed, but no floating-point literals or arithmetic |
| Unsigned floating types (`uf16`–`uf64`) | Not implemented | Lexed, but mapped to signed float types; no floating-point literals |
| `bool` | Partially implemented | Type and runtime check exist; no `true`/`false` literals |
| `char` | Not implemented | Lexed as a type token; no character literals and no runtime `ValueType` mapping |
| `str` | Partially implemented | Type and string token exist; string literals throw in `int.Parse` |
| `str16` | Not implemented | Not recognised by lexer, parser, or runtime |
| Variable declarations | Partially implemented | `var` declarations parse and store; wide type support broken |
| Constants | Partially implemented | `const` stored and reassignment rejected; sample `const i8` fails on type matching |
| Assignment | Partially implemented | Identifier assignment works; no type check; arrays incomplete |
| `+`, `-` | Partially implemented | Parsed/evaluated for `int` only |
| `*`, `/`, `%` | Not implemented | `*` and `/` are lexed but not parsed/evaluated; `%` is not lexed |
| Operator precedence | Not implemented | Flat left-to-right chain |
| Parenthesised expressions | Not implemented | No grouping support |
| Comparison operators | Partially implemented | `<`, `>`, `<=`, `>=`, `==` work for `int`; `!=` is never lexed |
| `if` | Not implemented | Parser/interpreter code exists, but AST node is missing so the project cannot compile |
| `elif` | Not implemented | Parser code exists; blocked by missing `IfStatement` |
| `else` | Not implemented | Parser code exists; blocked by missing `IfStatement` |
| `switch` / `case` / `default` | Not implemented | Only `switch` and `case` keyword tokens exist |
| `while` | Partially implemented | Parser and interpreter exist; unverified |
| `for` | Not implemented | Parser aliases `for` to `ParseWhile`, which cannot parse the specified syntax |
| `do` | Not implemented | Keyword token only |
| `break` | Not implemented | Keyword token only |
| Function declarations | Partially implemented | Declarations and calls work mechanically; no parameter binding or hoisting |
| Function arguments and parameters | Not implemented | Arguments are parsed and ignored; only parameter names are stored |
| `return` | Partially implemented | Works inside a call via `ReturnException`; top-level return escapes as an exception |
| Void functions | Partially implemented | Return-value rules exist; no scope/parameters |
| Non-void functions | Partially implemented | Return type checked; no parameter binding; no implicit `main` invocation |
| `println` | Partially implemented | Prints `int` values; strings and other types fail during evaluation |
| `print` | Not implemented | No token or parser/interpreter support |
| `readln` | Not implemented | Keyword token only |
| `read` | Not implemented | Not recognised |
| Strong explicit type checking | Partially implemented | Declaration-time runtime checks only; assignments/parameters unchecked |
| Scope and environments | Not implemented | All variables are global; specification itself says scope is planned |
| Arrays | Partially implemented | AST and runtime support exist; parser branches are unreachable; no initialisers |
| Matrices | Not implemented | No implementation beyond the 2D array storage model |
| Structs | Not implemented | No token, AST, or runtime support |
| Hash maps | Not implemented | No token, AST, or runtime support |
| `foreach` | Not implemented | Not recognised |
| Error handling | Partially implemented | Raw exceptions; no source locations, phases, or recovery |
| Built-in functions | Not implemented | No built-in function mechanism; `println` is a statement keyword |
| String interpolation | Not implemented | Not referenced by the current specification text |
| Tree-sitter support | Partially implemented | Grammar exists, but is independent and incomplete |
| Example program in specification | Not implemented | Requires `if`, function parameters, return, string printing, and automatic `main` handling |

## 7. Confirmed bugs

All findings below are confirmed by source inspection. None could be confirmed by execution because no .NET SDK/runtime is available.

### B1. Missing `IfStatement` type prevents compilation

- **Where:** `Parser/Parser.cs` lines 140, 151, 181, 186; `Runtime/Interpreter.cs` line 83; `AST/Statements/` has no `IfStatement.cs`.
- **Problem:** `ParseIf()` returns and constructs `IfStatement`, and `Interpreter.Execute` pattern-matches it, but the class is not defined.
- **Impact:** Build failure. Conditional statements cannot exist.
- **Evidence:** Repository-wide search found only references, no declaration.

### B2. Default sample cannot run because of integer literal typing

- **Where:** `Program.cs` lines 14–22; `Runtime/Interpreter.cs` lines 171–174 and 120–131.
- **Problem:** `const i8 x = 16` is parsed as a `LiteralExpression` containing `"16"`. `Evaluate` always returns `int.Parse("16")`, producing an `int`. `TypeMatches(ValueType.I8, int)` returns false.
- **Impact:** The built-in sample throws `Cannot assign value of type 'Int32' to I8.`
- **Evidence:** `LiteralExpression` has no type information; `TypeMatches` requires `sbyte` for `I8`.

### B3. Valid programs fail on the "never mutated" check

- **Where:** `Runtime/Interpreter.cs` lines 6–15.
- **Problem:** After executing a program, `Run` throws for every non-const variable with `WasModified == false`. The exception message begins `Warning:`.
- **Impact:** A valid program such as `var i32 x = 5; println(x);` fails after printing. Mutability is permitted, not required, by the specification.
- **Evidence:** `WasModified` is only set by `AssignmentStatement`.

### B4. Unsigned types are silently treated as signed

- **Where:** `Runtime/Interpreter.cs` lines 57–60 and 66–68.
- **Problem:** `ui8` maps to `ValueType.I8`, `ui16` to `I16`, and so on; `uf*` maps to `F*`.
- **Impact:** Unsigned declarations use signed runtime types; range and type identity are lost.
- **Evidence:** `ValueType.UI*` and `UF*` exist but `ParseValueType` never returns them.

### B5. Strings and all non-integer literals are unsupported

- **Where:** `Runtime/Interpreter.cs` lines 171–174.
- **Problem:** Every `LiteralExpression` is evaluated with `int.Parse` regardless of whether the token was a string, and there are no float/char/bool literal nodes.
- **Impact:** `println("hello")` throws `FormatException`; floats, chars, and booleans cannot be initialised from literals.
- **Evidence:** The lexer has a `String` token, but `Evaluate` does not distinguish it.

### B6. Function arguments and parameters are ignored

- **Where:** `Parser/Parser.cs` lines 48–79; `Runtime/Interpreter.cs` lines 229–258.
- **Problem:** `FunctionDeclaration.Parameters` stores only parameter names, and `Evaluate(CallExpression)` never evaluates or binds `call.Arguments`.
- **Impact:** Functions cannot accept input. `add(2, 3)` executes the body with no parameters.
- **Evidence:** `CallExpression.Arguments` is never read by the interpreter.

### B7. All variables are global; function locals leak

- **Where:** `Runtime/Interpreter.cs` lines 3–4 and 120–131.
- **Problem:** One global `variables` dictionary is used for the whole program. Function bodies execute in the global environment.
- **Impact:** Block scope is absent. A local variable in a function remains visible afterward, and recursive or nested calls can overwrite caller state.
- **Evidence:** No environment/frame type exists.

### B8. Array syntax cannot be parsed

- **Where:** `Parser/Parser.cs` lines 311–334 and 255–275.
- **Problem:**
  - The second `if (Match(TokenType.Var))` branch is unreachable because the first `var` branch already returned.
  - Array declarations are intended to start with a type, not `var`.
  - The array-access branch in `ParsePrimary` is after an earlier identifier branch that always returns.
  - No parser path constructs `ArrayAssignment`.
- **Impact:** The array AST and runtime code are effectively unreachable; array declarations/reads/writes cannot be used.
- **Evidence:** `new ArrayAssignment` never occurs; `new ArrayDeclaration` occurs only in unreachable code.

### B9. Comments are not handled

- **Where:** `Lexer/Lexer.cs` lines 51–139.
- **Problem:** There is no comment handling. `//` becomes two `Slash` tokens followed by identifiers; `/*` is tokenised incorrectly.
- **Impact:** Any documented comment causes syntax errors.
- **Evidence:** Specification documents both comment forms; lexer has no branch for them.

### B10. Expression operators are missing and have no precedence

- **Where:** `Parser/Parser.cs` lines 218–240; `Runtime/Interpreter.cs` lines 197–227.
- **Problem:** The parser recognises only `+`, `-`, and comparisons. It builds a flat left-associative chain with no precedence. `*` and `/` are lexed but never parsed; `%` is not lexed at all.
- **Impact:** `1 + 2 * 3` parses as `(1 + 2)` and then fails at `*`; `10 % 3` cannot be tokenised.
- **Evidence:** Operator list in `ParseExpression` omits `Star`, `Slash`, and `%`.

### B11. `!=` can never be produced

- **Where:** `Lexer/Lexer.cs` lines 51–139; `TokenType.cs` line 69; `Parser/Parser.cs` line 232.
- **Problem:** `TokenType.NotEqual` is checked by the parser, but the lexer has no `!` case, so it throws `Unexpected character: !`.
- **Impact:** `!=` comparisons are unusable.
- **Evidence:** No `case '!'` in the lexer.

### B12. `for` is parsed as `while`

- **Where:** `Parser/Parser.cs` line 340.
- **Problem:** `if (Match(TokenType.For)) return ParseWhile();` consumes the `for` token as if it were `while`, then expects `(condition)` and a block.
- **Impact:** The specification's `for (init, condition, update)` syntax cannot parse.
- **Evidence:** No `ForStatement` or dedicated `ParseFor`.

### B13. Top-level `return` escapes as an uncaught exception

- **Where:** `Runtime/Interpreter.cs` lines 161–166; `ReturnException` is caught only in `Evaluate(CallExpression)`.
- **Problem:** `return` always throws `ReturnException`; outside a function call nothing catches it.
- **Impact:** A top-level return terminates with an internal exception rather than a language diagnostic.
- **Evidence:** `Run` has no handler for `ReturnException`.

### B14. Assignment and array writes skip type checking

- **Where:** `Runtime/Interpreter.cs` lines 133–160.
- **Problem:** `AssignmentStatement` replaces `Data` without checking `Type`. `ArrayAssignment` checks only that the target is an array; it does not check element type, bounds, or constness.
- **Impact:** Variables can change to incompatible runtime types.
- **Evidence:** `TypeMatches` is used only in `VarDeclaration` and function-return validation.

### B15. Array access has no bounds or existence checks

- **Where:** `Runtime/Interpreter.cs` lines 184–195; `Runtime/ArrayValue.cs` lines 14–17.
- **Problem:** The array dictionary lookup is unguarded, and `Index(x, y)` performs no bounds validation.
- **Impact:** Missing arrays throw `KeyNotFoundException`; out-of-range indices throw `IndexOutOfRangeException` instead of language-level errors.
- **Evidence:** Direct `variables[a.Name]` and direct `Data[array.Index(...)]`.

### B16. Parser can read past EOF instead of reporting a syntax error

- **Where:** `Parser/Parser.cs` lines 305–310, 342–344, 353–356.
- **Problem:** Several checks index `_position + 1` or `_position + 2` without ensuring those positions exist. `Match` indexes `_tokens[_position]` directly.
- **Impact:** Malformed input can throw `IndexOutOfRangeException` rather than a syntax diagnostic.
- **Evidence:** `ParseStatement` and `Match` have no bounds checks before indexing.

### B17. Unterminated strings are silently accepted

- **Where:** `Lexer/Lexer.cs` lines 219–231.
- **Problem:** `ReadString` stops at the source length without checking for a closing quote, then increments past the end.
- **Impact:** Invalid programs produce a `String` token instead of a lexical error.
- **Evidence:** The loop exits on `_position == _source.Length` and always returns a token.

### B18. Function calls before declarations fail; `main` is never invoked

- **Where:** `Runtime/Interpreter.cs` lines 6–15 and 149–152.
- **Problem:** Functions are registered only when their declaration is executed. There is no hoisting. Nothing automatically calls `main`.
- **Impact:** The specification's example program cannot run as written: it declares `main` but never invokes it, and its `checkScore` call depends on execution order.
- **Evidence:** `Run` simply executes top-level statements in order.

### B19. Parser debug output and interpreter debug output pollute program output

- **Where:** `Parser/Parser.cs` lines 43, 185, 220–221, 302–303; `Runtime/Interpreter.cs` lines 199–200.
- **Problem:** Debug statements execute unconditionally.
- **Impact:** Normal runs produce internal parser/expression traces mixed with program output.
- **Evidence:** Direct `Console.WriteLine` calls in parsing/evaluation paths.

## 8. Potential issues

These are not confirmed runtime bugs because execution was impossible, but they are credible risks in the current code.

- **`IfStatement` constructor contract is undefined:** The missing class must accept `(Expression condition, List<Statement> thenBranch, List<(Expression, List<Statement>)> elifBranches, List<Statement>? elseBranch)`. If the node is added with a different shape, parser/interpreter integration will need adjustment.
- **`ParseIf` token consumption:** The parser uses `Match` without consuming in several places and relies on the next `Advance` to consume the expected token. This works for the currently written sequence but is fragile and will fail silently if tokens are missing.
- **`ParseCall` null check:** `Advance() != null` in the argument loop is always true because `Advance` throws rather than returning `null`. The check gives a false sense of error handling.
- **`ParseExpression` loop termination:** If a future operator is lexed but not included in the parser's list, the parser returns a partial expression and leaves the operator for `ParseStatement`, producing a misleading "Unknown statement" error.
- **`ParsePrimary` duplicate identifier handling:** The second identifier block is unreachable. The array-access logic also uses `Match` followed by `Advance`, so even if reached it may consume the wrong tokens.
- **`TypeMatches` and `void`:** `void` maps to `ValueType.Null`; this works for `void` variable declarations only if the initialiser is exactly null, which no expression currently produces.
- **`Value.Data` nullability:** `Value` declares `object Data`, but `void` returns and failed evaluations can produce null. The nullable annotation does not match the actual runtime model.
- **`ArrayValue` is fixed to two dimensions:** Any future support for 1D or N-dimensional arrays will require a model change.
- **`FunctionDeclaration.Parameters` loses types:** Even after argument binding is added, the AST currently cannot validate parameter types.
- **`ReturnException` cost and diagnostics:** Using exceptions for normal control flow is workable for an interpreter but will make stack traces and error reporting noisy unless handled deliberately.
- **`SemanticAnalyser` gives false confidence:** It is called by the pipeline but performs no analysis.
- **No namespace and generic type names:** Moving toward libraries or tests will increase collision risk for names such as `Value`, `Variable`, `Token`, and `Interpreter`.
- **`RootNamespace` legacy value:** `G_Sharp` suggests stale project identity; it does not currently affect code but is misleading.
- **`docs/context.md` path references:** It points to `docs/architecture.md` and `docs/TODO.md`, while the audit files are in the repository root as requested.
- **Untracked specification:** `docs/Spec.md` is currently untracked while `Specification.md` is deleted in the working tree. A careless Git operation could lose the authoritative specification.
- **Tree-sitter grammar drift:** The grammar does not cover the same subset as the parser, so editor feedback could disagree with actual interpreter behaviour.

## 9. Technical debt

### Architecture and boundaries

- Single global namespace with no component boundaries.
- `Program` directly constructs every phase; no reusable pipeline API.
- Interpreter owns execution, type checking, value storage, function dispatch, and array access in one class.
- No environment abstraction, call frames, or scope model.
- Semantic analysis is a placeholder.
- No diagnostics abstraction, source spans, or error phases.
- No separation between runtime values and AST types beyond `object`.

### Correctness and maintainability

- Duplicated type mapping across lexer keywords, parser type set, `ValueType`, `ParseValueType`, and `TypeMatches`.
- Dead code: `TypedLiteralExpression`, `Variable`, `ValueType.Function`, several token kinds, `ArrayAssignment` (never produced), and part of `ArrayDeclaration` parsing.
- Inconsistent AST shape: public mutable fields in most nodes, get-only properties in `VarDeclaration`.
- Unconditional debug logging in parser and interpreter.
- Fixed 2D array representation with no abstraction for ranks.
- Raw `Exception` usage for lexical, syntax, and runtime errors.
- No tests, CI, or sample suite.

### Project and workflow

- No `global.json` to pin the required SDK.
- `net10.0` target with no SDK available in the current environment.
- No test project.
- No command-line execution path.
- No dependency or package management concerns because there are currently no packages.
- Legacy `RootNamespace` value.

## 10. Missing tests

There are no tests for any of the following:

- Lexer: whitespace, identifiers, keywords, numbers, strings, operators, EOF, invalid characters, unterminated strings, comments.
- Parser: each statement form, expression precedence, grouping, malformed input, EOF boundaries.
- AST: node construction and shape.
- Semantic analysis: currently no behaviour to test, but the intended phase is untested.
- Interpreter: variable declaration, constants, assignment, printing, arithmetic, comparison, if/elif/else, while, return, functions, parameters, scope, arrays.
- Runtime: `Value` type matching, `ArrayValue` indexing, `ReturnException`.
- End-to-end: hard-coded sample, file-based execution, error output, exit codes.
- Regression tests for every confirmed bug in section 7.
- Tooling: tree-sitter grammar parsing and highlighting.

## 11. Recommended development priorities

1. **Make the environment usable:** install/configure a .NET 10 SDK (or retarget deliberately), then confirm `dotnet --info`, restore, build, and test. (C1)
2. **Restore a compilable solution:** add the missing `IfStatement` node or remove its references. (C2)
3. **Establish a runnable smoke test:** make the default sample execute and print `16`. (C3)
4. **Remove the false "never mutated" failure** so valid programs can complete. (C4)
5. **Fix the value model:** typed literals for integers, floats, strings, booleans, and chars. (H1)
6. **Implement real scope and function arguments** before adding more control flow. (H2, H3)
7. **Add comments and complete expression parsing** to cover basic documented syntax. (H4, H5)
8. **Add diagnostics and remove debug output** so failures are understandable. (H6)
9. **Add file-based execution and a test project** to make future work verifiable. (H7, H8)
10. **Then expand arrays, control flow, I/O, and tooling** in the order listed in `TODO.md`.

## 12. Final assessment

### Answers

1. **Is the .NET environment working correctly?** No. `dotnet` is not installed or on `PATH`; no SDK/runtime/NuGet CLI could be queried.
2. **Does the project restore successfully?** Not verifiable. Restore could not run because the SDK is missing. There are no package references, so restore should be simple once an SDK exists.
3. **Does the project build successfully?** Not verifiable by command. Static inspection found a definite missing `IfStatement` type, so the current source is expected to fail compilation.
4. **Are the tests passing?** There are no tests. `dotnet test` could not run without the SDK.
5. **How is the codebase structured?** A single C# console project with lexer, parser, AST, placeholder semantic analyser, interpreter, and runtime value classes in a global namespace. The pipeline is manually wired in `Program.cs`.
6. **How much of the language specification is implemented?** Only a small, unverified subset: tokenisation of many keywords, partial declarations, `i32` arithmetic and comparisons, partial `println`, partial functions without parameters, and the skeleton of arrays. Conditionals do not compile; comments, strings, floats, chars, booleans, `for`, `switch`, structs, hash maps, scope, I/O built-ins, and most other specified features are absent or incomplete.
7. **What are the biggest confirmed bugs?** The missing `IfStatement` build blocker, integer-literal/type mismatch (including the default sample), the "never mutated" runtime failure, ignored function parameters, global-only scope, unreachable array parsing, and missing comments/operators.
8. **What are the biggest architectural concerns?** No scope/environment model, duplicated and inconsistent type mapping, a placeholder semantic phase, raw exception-based diagnostics, a monolithic interpreter, no test infrastructure, and an inability to execute source files.
9. **What should be worked on first?** Install the SDK, add the missing AST node, make the default sample run, remove the false mutation check, then build a typed value model and real scopes/function calls before expanding syntax.
10. **Is the project in a good state for continued development?** Not yet. The pipeline is understandable and small enough to evolve, but it is currently unbuildable and unverified, with no tests. It is a reasonable prototype foundation, not a stable base for feature work until the critical items above are resolved.

### Documentation created or updated

- `architecture.md` — actual implementation architecture.
- `TODO.md` — prioritised development roadmap.
- `audit.md` — this audit report.

No source code, test files, specification files, or existing user changes were modified during the audit.
