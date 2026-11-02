```
    ____       _ ______
   / __ \_____(_) __/ /_
  / / / / ___/ / /_/ __/
 / /_/ / /  / / __/ /_
/_____/_/  /_/_/  \__/
```
## About
Drift is a personal vibe coded programing language. It takes inspiration from C and modern languages like rust or zig. The first interpreter is written in C# but I plan to make a compiler written in C in the future.
The first interpreter has been mostly written with the free ChatGPT model in the browser but has been extensive modified along the way as I get better at programming. Drift is a personal project which is what I would
like in language regarding syntax. It is not designed to be the best and have the most efficient interpreter but is just a passion project.

Drift also tries to solve the infinite way of doing something in C, an example is input with functions like scanf and fget. In Drift there are 2 functions readln and read, read only reads words and stops at white spaces and readln reads everything the user inputs. This is much simpler. 

Drift is still a work in progress. The current interpreter supports a small subset of the language; see [docs/Features.md](docs/Features.md) and [TODO.md](TODO.md).

## Repository layout
- `src/` — C# interpreter source.
- `docs/` — Language specification and project documentation.
- `tools/tree-sitter-drift/` — Tree-sitter grammar for editor tooling.
- `examples/hello.drf` — Runnable example (format specifiers, `while`, `if/else`)
- `examples/format.drf` — Format specifiers (`%i`, `%s`, `%%`, `%v`)
- `examples/control.drf` — Control flow, recursion, `while` loops
- `examples/scope.drf` — Block scope and case-insensitive automatic `main`
- `examples/arithmetic.drf` — Arithmetic, precedence, modulo, boolean operators
- `examples/comments.drf` — Line and block comments
- `drift` — Published self-contained Linux executable.

## Running the executable
The prebuilt executable in the repository root is self-contained and does not require the .NET SDK at runtime:

```sh
./drift examples/hello.drf
```

Usage:

```text
./drift <file.drf>
```

The interpreter exits with code `0` on success and `1` on read, lexical, parse, or runtime errors. Errors are written to stderr with source locations:

```text
examples/broken.drf:3:9: error: Undefined variable 'missing'.
```

To rebuild the executable after changing the source:

```sh
dotnet publish src/DriftInterpreter.csproj \
  -c Release -r linux-x64 --self-contained true \
  -p:PublishSingleFile=true -p:AssemblyName=drift \
  -p:DebugType=None -p:DebugSymbols=false \
  -p:IncludeNativeLibrariesForSelfExtract=true \
  -o /tmp/drift-publish
cp /tmp/drift-publish/drift ./drift
chmod +x ./drift
```

The .NET 10 SDK is installed at `~/.dotnet`. If `dotnet` is not on your `PATH`, add it before building:

```sh
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$HOME/.dotnet:$PATH"
```

## Building and running from source
```sh
dotnet build Drift.sln
dotnet run --project src -- examples/hello.drf
```

## Running tests
```sh
dotnet test Drift.sln
```

The test project is in `tests/DriftInterpreter.Tests/` and covers lexing, parsing, diagnostics, formatting, entry points, and core interpreter behaviour.

## Formatting output
`println` accepts a format string followed by arguments. Supported specifiers:

| Specifier | Meaning |
| --- | --- |
| `%i`, `%d` | Integer |
| `%f` | Float or integer as a decimal |
| `%s` | String |
| `%c` | Character |
| `%b` | Boolean (`true` / `false`) |
| `%v` | Any value using the default representation |
| `%%` | Literal percent sign |

```drift
println("%s scored %i points (100%%)", "Noah", 42);
```

## Entry point
A zero-parameter function named `main` runs automatically. The name is matched case-insensitively, so `main`, `Main`, and `mAin` all work. If `main` is called explicitly, it is not run a second time. Top-level statements still execute before the automatic entry point. A `main` that declares parameters must be called explicitly and is not auto-run.

## Current language subset
Implemented:
- `var` / `const` declarations and typed declarations such as `i32 x = 5;`
- Signed and unsigned integer types, floats, `str`, `bool`, and `char`
- Arithmetic, comparison, equality, `!`, and string concatenation with `+`
- `if` / `elif` / `else` and `while`
- Functions with parameters, return values, and block/function scope
- Automatic, case-insensitive `main` entry point
- `println` with format specifiers
- Line and block comments
- Source-located lexical, parse, and runtime diagnostics
- Two-dimensional array AST/runtime storage (not yet reachable from source)

Not yet implemented:
- Array declarations/assignment from source
- `for`, `switch`, `do`, and `break`
- `print`, `read`, and `readln`
- Structs, hash maps, and `foreach`
- Function parameter type checking
- CI
