# Drift Development Roadmap

> Updated: 2026-11-02.
> Build status: `dotnet build Drift.sln` succeeds with 0 warnings and 0 errors.
> Test status: `dotnet test Drift.sln` passes 26 tests.
> A self-contained Linux executable is available at `./drift`.

## Completed in this pass

- **C1 — .NET SDK:** .NET SDK 10.0.403 is installed at `~/.dotnet`. The project restores and builds.
- **C2 — Compilable solution:** Added `AST/Statements/IfStatement.cs`.
- **C3 — Runnable baseline:** Added typed literals and numeric coercion; `i8` and other declared widths now work.
- **C4 — Removed the "never mutated" failure:** `Interpreter.Run` no longer throws for unmodified non-const variables.
- **H1 — Typed literal values:** Supports integer literals, decimal floats, strings, booleans, and characters with declared-type coercion.
- **H2 — Function arguments and parameters:** Arguments are evaluated and bound to parameter names; argument count is checked.
- **H3 — Scope/environments:** Added `RuntimeEnvironment`; blocks and functions now have lexical scope.
- **H4 — Comments:** Line and block comments are supported, with unterminated-block-comment errors.
- **H5 — Expressions:** Added precedence, `*`, `/`, `%`, unary `-`, `!`, parentheses, and `!=`.
- **H6 — Source locations and diagnostics:** Tokens and AST nodes carry line/column positions. Lexer, parser, and runtime errors use structured `DriftException` types and print `file:line:column: error: message`.
- **H7 — Command-line execution:** `Program.Main` reads a `.drf` file, reports readable errors, and returns meaningful exit codes.
- **H8 — Test project:** Added `tests/DriftInterpreter.Tests` with xUnit coverage for lexing, parsing, diagnostics, formatting, entry points, and interpreter behaviour. `dotnet test Drift.sln` passes.
- **M2 — Type mapping (partial):** Unsigned signedness is no longer confused, and `char` is mapped. Type metadata is still duplicated.
- **M3 — Assignment checks (partial):** Assignment and return values are coerced to their declared types.
- **M4 — Boolean literals and condition checks:** `true`/`false` are lexed, and `if`/`while` require boolean conditions.
- **M7 — Debug output (partial):** Parser/interpreter debug output and the semantic-analyser print have been removed.
- **M9 — Automatic `main`:** A zero-parameter function named `main` runs automatically, matched case-insensitively. Explicit calls suppress the automatic run.
- **Format specifiers:** `println` supports `%i`/`%d`, `%f`, `%s`, `%c`, `%b`, `%v`, and `%%`, with argument-count and type diagnostics.
- **H10 — Specification updates:** `docs/Spec.md` now documents format specifiers, automatic `main`, and structured diagnostics.
- **L4 — Documentation locations:** `docs/context.md` now points to the root `architecture.md` and `TODO.md`.

## Critical

No critical items remain.

## High

### H9. Preserve function parameter types

- **Description:** Change `FunctionDeclaration.Parameters` from `List<string>` to structured parameter nodes containing both type and name. Validate argument types during calls.
- **Files/classes:** `Parser/Parser.cs`, `AST/Statements/FunctionDeclaration.cs`, `Runtime/Interpreter.cs`.
- **Reason:** The parser currently discards parameter types, so calls are not type-checked and parameter metadata is lost.
- **Expected outcome:** Parameter types are retained and enforced.
- **Dependencies:** H1, H2.

## Medium

### M1. Make arrays work end to end

- **Description:** Add array declaration and assignment parsing, validate dimensions and indices, support initialisers, and decide on rank support beyond 2D.
- **Files/classes:** `Parser/Parser.cs`, `AST/Statements/ArrayDeclaration.cs`, `AST/Statements/ArrayAssignment.cs`, `AST/Expressions/ArrayAccess.cs`, `Runtime/ArrayValue.cs`, `Runtime/Interpreter.cs`.
- **Reason:** AST, parser fragments, and runtime support exist, but source cannot currently create or assign arrays.
- **Expected outcome:** Declaring, reading, and writing arrays works with bounds and const checks.
- **Dependencies:** None.

### M2. Centralise the type model

- **Description:** Remove duplicated type-name mapping across the lexer, parser, `ValueType`, and interpreter. Add `str16` support if the specification keeps it.
- **Files/classes:** `Lexer/Lexer.cs`, `Lexer/TokenType.cs`, `Runtime/ValueType.cs`, `Runtime/Interpreter.cs`.
- **Reason:** Type knowledge is spread across several switch statements and can drift again.
- **Expected outcome:** One authoritative type mapping.
- **Dependencies:** H9.

### M3. Complete argument and array type checks

- **Description:** Type-check function arguments once parameter types are preserved, and validate array element types.
- **Files/classes:** `Runtime/Interpreter.cs`.
- **Reason:** Assignment and return checks are implemented, but arguments and arrays are not fully covered.
- **Expected outcome:** Type violations are rejected consistently.
- **Dependencies:** H9, M1.

### M5. Implement `for`, `switch`, and remaining control flow

- **Description:** Add real `for` parsing, switch/case/default, and decide on `do`/`break` behaviour.
- **Files/classes:** `Parser/Parser.cs`, new AST statement types, `Runtime/Interpreter.cs`.
- **Reason:** The lexer recognises `for`, `switch`, `case`, `do`, and `break`, but the parser does not implement them.
- **Expected outcome:** Documented control-flow forms parse and execute.
- **Dependencies:** None.

### M6. Implement `print` and input built-ins

- **Description:** Add `print` (no newline), `read`, and `readln`, and make output work for all supported value types.
- **Files/classes:** `Lexer/Lexer.cs`, `Lexer/TokenType.cs`, `Parser/Parser.cs`, `Runtime/Interpreter.cs`.
- **Reason:** Only `println` is implemented; `readln` is lexed but unused, and `read`/`print` are absent.
- **Expected outcome:** The documented I/O functions work.
- **Dependencies:** None.

### M7. Remove remaining dead code

- **Description:** Remove or connect `TypedLiteralExpression`, `Variable`, `ValueType.Function`, unused token kinds, and decide whether `SemanticAnalyser` remains a placeholder.
- **Files/classes:** Various AST/runtime/parser files.
- **Reason:** Debug output has been removed, but dead code still obscures the architecture.
- **Expected outcome:** The codebase reflects only active components.
- **Dependencies:** None.

### M8. Introduce namespaces and cleaner project boundaries

- **Description:** Add namespaces such as `Drift.Lexer`, `Drift.Parser`, and `Drift.Runtime`, and update `RootNamespace`.
- **Files/classes:** All C# files, `.csproj`.
- **Reason:** Global types named `Value`, `Token`, `Lexer`, and `Interpreter` are collision-prone.
- **Expected outcome:** Clear namespaces and explicit dependencies.
- **Dependencies:** M7.

## Low

### L1. Align the tree-sitter grammar with the interpreter

- **Description:** Add comments, chars, booleans, unsigned types, and current expression syntax to the grammar.
- **Files/classes:** `tools/tree-sitter-drift/grammar.js`.
- **Reason:** The grammar lags behind the interpreter.
- **Expected outcome:** Editor highlighting matches supported source.
- **Dependencies:** Stable syntax decisions.

### L2. Add CI for restore, build, and test

- **Description:** Add a CI workflow that runs `dotnet restore`, `dotnet build`, and `dotnet test`.
- **Files/classes:** New CI configuration.
- **Reason:** The project has local tests but no automated verification on changes.
- **Expected outcome:** Broken builds and regressions are caught automatically.
- **Dependencies:** None.

### L3. Keep feature documentation current

- **Description:** Update `docs/Features.md` and `README.md` as features move from planned to implemented.
- **Files/classes:** `docs/Features.md`, `README.md`.
- **Reason:** The checklist and README should reflect the current supported subset.
- **Expected outcome:** Documentation clearly separates implemented, partial, and planned features.
- **Dependencies:** None.
