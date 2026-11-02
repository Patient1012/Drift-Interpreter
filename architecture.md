# Drift Interpreter Architecture

> Last updated: 2026-11-02, after adding source diagnostics, format specifiers, automatic `main`, and tests.
> This document describes the current implementation, not the intended design in `docs/Spec.md`.

## 1. Repository layout

```text
.
├── Drift.sln                     # Solution, references the interpreter and tests
├── README.md
├── architecture.md
├── TODO.md
├── audit.md
├── .gitignore
├── drift                         # Published self-contained Linux executable
├── examples/hello.drf            # Runnable example
├── src/DriftInterpreter/
│   ├── DriftInterpreter.csproj   # net10.0 console executable
│   ├── Program.cs                # CLI entry point
│   ├── Diagnostics/
│   │   ├── SourcePosition.cs
│   │   └── DriftException.cs
│   ├── Lexer/
│   │   ├── Lexer.cs
│   │   ├── Token.cs
│   │   └── TokenType.cs
│   ├── Parser/
│   │   └── Parser.cs
│   ├── AST/
│   │   ├── Expressions/
│   │   │   ├── Expression.cs
│   │   │   ├── LiteralExpression.cs
│   │   │   ├── TypedLiteralExpression.cs
│   │   │   ├── VariableExpression.cs
│   │   │   ├── UnaryExpression.cs
│   │   │   ├── BinaryExpression.cs
│   │   │   ├── CallExpression.cs
│   │   │   └── ArrayAccess.cs
│   │   └── Statements/
│   │       ├── Statement.cs
│   │       ├── VarDeclaration.cs
│   │       ├── AssignmentStatement.cs
│   │       ├── PrintStatement.cs
│   │       ├── CallStatement.cs
│   │       ├── ReturnStatement.cs
│   │       ├── FunctionDeclaration.cs
│   │       ├── IfStatement.cs
│   │       ├── WhileStatement.cs
│   │       ├── ArrayDeclaration.cs
│   │       └── ArrayAssignment.cs
│   ├── Semantic/
│   │   └── SemanticAnalyser.cs
│   └── Runtime/
│       ├── Interpreter.cs
│       ├── RuntimeEnvironment.cs
│       ├── DriftFormatter.cs
│       ├── Value.cs
│       ├── ValueType.cs
│       ├── ArrayValue.cs
│       ├── ReturnException.cs
│       └── Variable.cs
├── docs/
│   ├── Spec.md
│   ├── context.md
│   ├── Features.md
│   └── assets/ascii.txt
├── tests/DriftInterpreter.Tests/
│   ├── DriftInterpreter.Tests.csproj
│   ├── TestAssemblyInfo.cs
│   ├── TestRunner.cs
│   ├── CliTests.cs
│   ├── DiagnosticTests.cs
│   ├── EntryPointTests.cs
│   ├── FormattingTests.cs
│   └── InterpreterTests.cs
└── tools/tree-sitter-drift/
    ├── grammar.js
    └── package.json
```

The C# code does not declare any namespaces. All types live in the global namespace.

## 2. Build configuration

`src/DriftInterpreter/DriftInterpreter.csproj`:

- SDK: `Microsoft.NET.Sdk`
- Output: `Exe`
- Target framework: `net10.0`
- `ImplicitUsings`: enabled
- `Nullable`: enabled
- `RootNamespace`: `G_Sharp` (legacy value, unused because there are no namespaces)
- No NuGet package or project references.

The solution contains two projects: the console interpreter and `tests/DriftInterpreter.Tests`. The test project references the interpreter project and uses xUnit, `Microsoft.NET.Test.Sdk`, and `coverlet.collector`. There is no `global.json`, `Directory.Build.props`, or `NuGet.config`.

The .NET 10 SDK used during development is installed at `~/.dotnet`. The published `drift` executable is self-contained and does not require the SDK or runtime to be on `PATH`.

## 3. Execution pipeline

```text
Command line: drift <file.drf>
        |
        v
Program.Main
        |
        v
File.ReadAllText
        |
        v
new Lexer(source).Tokenize()       -> List<Token>
        |
        v
new Parser(tokens).Parse()         -> List<Statement>
        |
        v
new SemanticAnalyser().Analyse()   -> currently no-op
        |
        v
new Interpreter().Run()            -> executes statements
```

`Program.Main`:

- Prints usage and exits with code 1 when no file argument is given.
- Reads the file, reporting `IOException` and `UnauthorizedAccessException` cleanly.
- Runs the pipeline inside a `try/catch`.
- Catches `DriftException` and writes `file:line:column: error: message` to stderr.
- Falls back to `Error: ...` for unexpected non-language exceptions.
- Returns 0 on success and 1 on errors.

## 4. Lexer

Files: `Lexer/Lexer.cs`, `Lexer/Token.cs`, `Lexer/TokenType.cs`

### Behaviour

- Tracks a single integer `_position` into the source string.
- Skips whitespace.
- Skips `//` line comments and `/* ... */` block comments. Unterminated block comments produce an error.
- Identifiers start with a letter or `_`; subsequent characters may be letters, digits, or `_`.
- Keywords and type names are recognised in `ReadIdentifier`.
- Numbers support decimal integers and simple decimal fractions (`123`, `1.5`). No exponents or separators.
- Strings use double quotes and support escapes: `\n`, `\r`, `\t`, `\0`, `\\`, `\'`, `\"`.
- Character literals use single quotes and the same escapes.
- Multi-character operators: `==`, `!=`, `>=`, `<=`.
- Single-character operators: `+ - * / % { } = > < ; ( ) [ ] ,`.
- Unknown characters, unterminated strings, unterminated character literals, and unterminated block comments throw `LexerException` with a line/column position.
- Tokens contain `Type`, `Value`, and a `SourcePosition Location`.
- Line starts are indexed when the lexer is constructed, so token and error positions are computed without a second source scan.

### Recognised keywords

`var`, `const`, `println`, `readln`, `true`, `false`, `i8`–`i64`, `ui8`–`ui64`, `f16`–`f64`, `uf16`–`uf64`, `str`, `bool`, `char`, `void`, `if`, `elif`, `else`, `switch`, `case`, `while`, `for`, `do`, `break`, `return`.

### Not recognised

- `str16`.
- `%` is recognised, but additional compound assignment operators are not.
- Keywords for structs, hash maps, `foreach`, `in`, and `default`.
- `read` and `print`.

## 5. Parser

File: `Parser/Parser.cs`

`Parser` is a hand-written recursive-descent parser with a token list and `_position` index.

### Statement parsing

`Parse()` reads statements until EOF. `ParseStatement()` dispatches:

- `var` / `const`: `ParseVarDeclaration`.
- Type-led declaration with `=`: `ParseTypedVarDeclaration` (for example `i32 x = 5;`).
- Identifier followed by `=`: `ParseAssignment`.
- Identifier followed by `(`: `ParseCallStatement`.
- `println`: `ParsePrintln`.
- `if`: `ParseIf` with `elif`/`else`.
- `while`: `ParseWhile`.
- `return`: `ParseReturn`.
- Type + identifier + `(`: `ParseFunction`.
- Anything else: syntax error.

`ParseBlock()` consumes braces and returns a statement list. `Consume()` validates expected tokens and throws `ParserException` at the current token's line and column on failure.

Every parsed statement and expression is assigned the `SourcePosition` of its first relevant token. Runtime errors therefore point at the expression or statement that failed, not just at the program entry point.

`ParsePrintln()` accepts zero or more comma-separated arguments. Multiple arguments are interpreted as a format string followed by values.

### Expression precedence

Expressions are parsed with standard precedence levels:

```text
ParseExpression
└── ParseEquality       == !=
    └── ParseComparison < <= > >=
        └── ParseTerm   + -
            └── ParseFactor * / %
                └── ParseUnary  unary - !
                    └── ParsePrimary
```

`ParsePrimary` handles:

- Parenthesised expressions.
- Integer and floating-point literals.
- String, character, and boolean literals.
- Variable references.
- Function calls.
- Two-dimensional array access (`name[x, y]`).

The parser produces `LiteralExpression` with `LiteralKind.Integer`, `Float`, `String`, `Boolean`, or `Character`. `TypedLiteralExpression` remains unused.

### AST nodes produced

Produced: all statement nodes except `ArrayDeclaration` and `ArrayAssignment`; all expression nodes except `TypedLiteralExpression`.

Array declaration and assignment syntax are not yet produced by the parser. The AST and runtime support exists but is currently unreachable from source.

## 6. AST

```text
Statement (abstract)
├── VarDeclaration
├── AssignmentStatement
├── PrintStatement
├── CallStatement
├── ReturnStatement
├── FunctionDeclaration
├── IfStatement
├── WhileStatement
├── ArrayDeclaration
└── ArrayAssignment

Expression (concrete base class)
├── LiteralExpression
├── TypedLiteralExpression
├── VariableExpression
├── UnaryExpression
├── BinaryExpression
├── CallExpression
└── ArrayAccess
```

`Statement` and `Expression` both expose a mutable `SourcePosition Location`; the parser fills it in for each node. AST nodes are otherwise simple data containers. Most fields are public mutable fields; `VarDeclaration` uses get-only properties.

`IfStatement` stores a condition, then-branch, named-tuple elif branches, and an optional else branch.

`FunctionDeclaration` stores the return type, parameter names (types are discarded by the parser), and body.

### Diagnostics and source locations

`Diagnostics/SourcePosition.cs` defines a 1-based line/column plus a source offset. `SourcePosition.None` represents an unknown location.

`Diagnostics/DriftException.cs` defines the shared `DriftException` base class, a `DiagnosticPhase` enum, and the lexer, parser, semantic, and runtime subclasses. `ToDiagnosticString(file)` formats a message as `file:line:column: error: message`. `Program.Main` catches this type so all language errors use the same format.

## 7. Semantic analysis

`Semantic/SemanticAnalyser.cs` is still a placeholder. `Analyse()` accepts the program and does nothing. It is called by the pipeline so a real semantic phase can be added later without changing `Program.cs`.

## 8. Interpreter and runtime

Files: `Runtime/Interpreter.cs`, `Runtime/RuntimeEnvironment.cs`, `Runtime/Value.cs`, `Runtime/ValueType.cs`, `Runtime/ArrayValue.cs`, `Runtime/ReturnException.cs`, `Runtime/Variable.cs`

### State

`Interpreter` owns:

- `Dictionary<string, FunctionDeclaration> functions` for top-level functions.
- `RuntimeEnvironment globals` for global variables.
- `RuntimeEnvironment environment` for the current scope.
- An optional `entryPoint` function and a flag recording whether it has already run.

`RuntimeEnvironment` is a dictionary with an optional parent. Lookups walk up the chain; declarations go into the current environment. This provides block and function scope.

`Variable` remains unused; the runtime uses `Value`.

### Execution

`Run()` clears per-run state, registers all top-level function declarations so calls can appear before declarations, and identifies a case-insensitive `main` entry point. It then executes non-declaration statements in the global environment. If a `main` entry point exists, takes no parameters, and was not called explicitly during top-level execution, it is invoked automatically. A `main` with parameters, or two declarations whose names differ only by case, produces a runtime diagnostic.

`Execute(Statement)` handles:

- `PrintStatement`: evaluates its arguments and writes with `Console.WriteLine`. A string first argument is treated as a format string; see section 8.1.
- `IfStatement`: evaluates the condition, then executes the selected branch in a child environment.
- `WhileStatement`: executes the body in a fresh child environment on each iteration.
- `ArrayDeclaration`: creates a 2D `ArrayValue` in the current environment.
- `VarDeclaration`: evaluates the initialiser, coerces it to the declared type, and defines a `Value`.
- `ArrayAssignment`: writes an element after checking that the target is an array.
- `CallStatement`: evaluates the call expression.
- `FunctionDeclaration`: registers the function.
- `AssignmentStatement`: finds the variable in the environment chain, rejects constants, coerces the value to the variable's type, and stores it.
- `ReturnStatement`: throws `ReturnException`.

### Expressions

`Evaluate(Expression)` handles:

- Typed literals: integer literals become `long`, float literals become `double`, and strings, booleans, and characters keep their types.
- Variable lookup through the environment chain.
- Unary `-` for numeric values and `!` for booleans.
- Two-dimensional array access.
- Binary arithmetic, comparisons, equality, and string concatenation with `+`.
- Function calls.

Numeric binary operators work on integral values as `long` and on any floating value as `double`. Division or modulus by zero raises a `RuntimeException` at the operator's location. Equality compares numeric values across widths; strings, booleans, and characters use object equality.

### 8.1 Output formatting

`Runtime/DriftFormatter.cs` implements printf-style formatting for `println`. `%%` emits a literal percent sign; `%i`/`%d`, `%f`, `%s`, `%c`, `%b`, and `%v` consume one argument each. Missing, extra, mistyped, or unknown format arguments raise `RuntimeException` at the format string's location.

### Functions and scope

- Top-level functions are registered before execution, allowing forward calls.
- A function call checks the argument count, evaluates the arguments, creates a new environment whose parent is the global environment, binds parameter names to inferred `Value` objects, and executes the body.
- `ReturnException` transfers the return value. Non-void functions must return a value; void functions must not return one.
- Return values are coerced to the declared return type.
- Function bodies do not see caller-local variables; nested blocks can see their enclosing function scope.
- A zero-parameter `main` is invoked automatically after top-level statements, with case-insensitive name matching.
- Explicit `main` calls are tracked by function identity, so a program that calls `main()` itself does not run it twice.

### Type model

`ValueType` contains signed/unsigned integers, signed/unsigned floats, `String`, `Bool`, `Char`, `Array`, `Function`, and `Null`.

`ParseValueType` maps all declared type names to their corresponding `ValueType`:

- `i8`–`i64` and `ui8`–`ui64` map separately.
- `f16`–`f64` and `uf16`–`uf64` map separately.
- `str` → `String`, `bool` → `Bool`, `char` → `Char`, `void` → `Null`.

`CoerceValue` converts literals and expression results to the declared type:

- Integral target types use checked range conversion.
- Float target types use `Half`, `float`, or `double`.
- Strings, booleans, characters, arrays, and null require matching runtime values.
- Out-of-range and invalid conversions produce language-level errors.

`Value` stores `object? Data`, `ValueType Type`, `IsConst`, and `WasModified`. The old "never mutated" check has been removed.

`ArrayValue` is a fixed-size 2D array with a flat `object[]` using row-major indexing. It is not yet reachable from parsed source.

## 9. Component dependencies

```text
Program
├── Lexer ────────────> Token, TokenType, SourcePosition, LexerException
├── Parser ───────────> Token, TokenType, AST, ParserException
├── SemanticAnalyser ─> AST
└── Interpreter ──────> AST, RuntimeEnvironment, Value, ValueType,
                        ArrayValue, ReturnException, DriftFormatter,
                        RuntimeException

Parser ──────────────> Token, TokenType, AST, ParserException
Interpreter ─────────> AST, runtime values/environments
Lexer ───────────────> Token, TokenType, SourcePosition, LexerException
```

All components are instantiated directly. There are no interfaces, namespaces, or dependency injection.

## 10. Testing architecture

`tests/DriftInterpreter.Tests` is an xUnit project targeting `net10.0` and referencing the interpreter project. Test parallelisation is disabled because the helper redirects `Console.Out` to capture program output.

`TestRunner` lexes, parses, and executes source strings. `FormattingTests`, `EntryPointTests`, `DiagnosticTests`, and `InterpreterTests` cover format specifiers, automatic and explicit `main`, lexer/parser/runtime locations, precedence, functions, scope, comments, and control flow. `CliTests` invokes `Program.Main` against temporary files to verify exit codes, usage output, file-read errors, and the source-located diagnostic format. The main project grants test access through `InternalsVisibleTo`.

Run the suite with:

```sh
dotnet test Drift.sln
```

There is still no CI workflow.

## 11. Tooling

`tools/tree-sitter-drift/` is independent syntax-highlighting tooling. It does not currently cover all implemented syntax and is not part of the C# build.

## 12. Known limitations

- Arrays cannot yet be declared or assigned from parsed source.
- `for`, `switch`, `do`, and `break` are lexed but not parsed.
- `readln`, `read`, and `print` are not implemented.
- Function parameter types are discarded by the parser, so parameter binding is not type-checked.
- Diagnostics store a start position but not a full source span or end position.
- `TypedLiteralExpression`, `Variable`, `ValueType.Function`, and some token kinds are still unused.
- No namespaces.
- No CI workflow.
