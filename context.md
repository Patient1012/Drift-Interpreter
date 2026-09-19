# Drift — AI Agent Context

## 1. Project Overview

**Drift** is a custom programming language and interpreter written in **C#**.

The project was previously called **G-Sharp**, so some existing namespaces, folders, or references may still use the old name. Do not assume those should be changed automatically; inspect the current project first.

The language is designed to be:

* Strongly typed
* Explicit
* Predictable
* C/C#-inspired
* Relatively simple to parse and interpret
* Friendly to someone coming from C#

The current file extension for Drift source files is:

```text
.drf
```

The general execution pipeline is:

```text
Drift source code
       ↓
Lexer
       ↓
Tokens
       ↓
Parser
       ↓
AST
       ↓
Interpreter
       ↓
Runtime
```

---

# 2. Important AI Agent Rules

When modifying the Drift project:

### Inspect before changing

Always inspect the existing implementation before making changes.

Do not assume:

* A class exists
* A method has a particular signature
* A token already exists
* A parser method works a particular way
* A type is represented in a particular way
* A runtime feature has already been implemented

### Do not rewrite unrelated code

Only modify the files and methods required for the task.

Do not perform large refactors unless explicitly requested.

### Preserve existing comments

Existing comments, notes, TODOs, and explanatory code should be preserved unless they are directly related to the change.

### Explain implementation locations

When suggesting changes, clearly identify:

* File
* Class
* Method
* What needs to change
* Why it needs to change

For example:

```text
Parser.cs
└── ParseStatement()
    Add handling for the new statement type here.
```

### Build incrementally

After significant changes:

1. Build the project.
2. Fix compilation errors.
3. Run relevant tests/examples.
4. Only then continue with the next part.

Do not make many unrelated changes before checking whether the project still builds.

### Do not invent language syntax

Drift syntax should come from the existing language design.

If a requested feature requires syntax that has not been decided yet, identify the design decision rather than silently inventing syntax.

---

# 3. Language Design Philosophy

Drift should generally prefer explicit syntax.

For example:

```drift
i32 age = 15;
```

rather than relying on implicit type inference.

The language should feel familiar to someone who knows C or C#, while still being its own language.

Statements end with semicolons:

```drift
i32 x = 10;
i32 y = 20;
println("%i", x);
```

---

# 4. Comments

Comments should follow the syntax already implemented by the lexer.

Do not add a new comment syntax without first checking the current lexer and language design.

---

# 5. Primitive Types

The current planned/used primitive types include:

```text
str
char
bool

f8
f16
f32
f64

i8
i16
i32
i64

ui8
ui16
ui32
ui64
```

The exact implementation of some types may still be under development.

Do not assume every listed type is already fully implemented.

---

# 6. Variables

Variables are explicitly typed.

Example:

```drift
i32 age = 15;
str name = "Noah";
bool running = true;
f32 score = 98.75;
char grade = 'A';
```

The type comes before the variable name:

```drift
i32 number = 10;
```

Not:

```drift
number i32 = 10;
```

unless the language design is explicitly changed.

---

# 7. Constants

Constants use the `const` keyword.

Example:

```drift
const i32 maxScore = 100;
const str languageName = "Drift";
```

A constant should not be reassigned after initialization.

Example:

```drift
const i32 maxScore = 100;

maxScore = 200;
```

The second statement should produce an appropriate error.

---

# 8. Variable Scope

Variables are intended to be function-local unless Drift explicitly introduces another scope mechanism.

For example:

```drift
void main() {
    i32 number = 10;
    println("%i", number);
}
```

`number` should not automatically be accessible from an unrelated function.

Example:

```drift
void first() {
    i32 number = 10;
}

void second() {
    println("%i", number);
}
```

This should not be valid simply because `number` exists somewhere else in the program.

Scope should be represented by the interpreter's environment/scope system.

---

# 9. Functions

Functions have an explicit return type.

Example:

```drift
i32 add(i32 a, i32 b) {
    return a + b;
}
```

A function returning nothing uses `void`:

```drift
void main() {
    println("Hello");
}
```

Parameters are explicitly typed:

```drift
i32 multiply(i32 a, i32 b) {
    return a * b;
}
```

---

# 10. Main Function

The main entry point is intended to use:

```drift
void main() {
    println("Hello, Drift!");
}
```

The exact startup/runtime implementation may change as the interpreter develops.

---

# 11. Return Statements

A function returning a value should return a compatible value.

Example:

```drift
i32 add(i32 a, i32 b) {
    return a + b;
}
```

A `void` function can return without a value if supported by the current parser/interpreter:

```drift
void main() {
    return;
}
```

Do not assume return validation is already implemented if the current interpreter does not contain it.

---

# 12. Strings

Drift strings use double quotes.

Correct:

```drift
str name = "Noah";
str message = "Hello, world!";
```

A string literal must be enclosed in double quotes:

```drift
"Hello"
```

Do not write Drift string examples without quotation marks.

Incorrect:

```drift
str name = Noah;
```

Correct:

```drift
str name = "Noah";
```

Characters use single quotes:

```drift
char letter = 'A';
```

A character and a string are different types:

```drift
char letter = 'A';
str letterString = "A";
```

---

# 13. String Concatenation

String concatenation is allowed.

For example:

```drift
str name = "Noah";
str message = "Hello " + name;
```

However, string interpolation is intended to be a useful feature of the language.

---

# 14. String Interpolation

Drift supports interpolation using `$` inside a string.

Example:

```drift
str name = "Noah";

println($"Hello {name}");
```

Another example:

```drift
i32 score = 100;

println($"Score: {score}");
```

Interpolation should not be confused with the format-specifier system described below.

The two systems are separate language features.

---

# 15. `println()`

`println()` uses a **C-style format string**.

The first argument is always the format string.

Additional arguments provide the values consumed by the format specifiers.

The canonical form is:

```drift
println("format string", argument1, argument2);
```

For example:

```drift
str name = "Noah";

println("Name: %s", name);
```

The string:

```text
"Name: %s"
```

is a normal Drift string literal.

---

# 16. `println()` Format Specifiers

The currently defined format specifiers are:

| Specifier | Meaning                 |
| --------- | ----------------------- |
| `%i`      | Print as integer        |
| `%f`      | Print as floating-point |
| `%s`      | Print as string         |
| `%c`      | Print as character      |
| `%m`      | Print memory address    |
| `%n`      | Print variable name     |
| `%%`      | Print a literal `%`     |

---

# 17. `%i` — Integer Formatting

`%i` prints a value as an integer.

Example:

```drift
i32 age = 15;

println("Age: %i", age);
```

Output:

```text
Age: 15
```

A floating-point value can also be formatted using `%i`.

```drift
f32 number = 15.75;

println("%i", number);
```

Output:

```text
15
```

This **does not change the type of `number`**.

The variable remains:

```text
f32
```

Only the output representation changes.

---

# 18. Float-to-Integer Formatting

When `%i` formats a floating-point value, Drift should truncate toward zero.

Examples:

```drift
f32 a = 15.9;
f32 b = 15.1;
f32 c = -15.9;
f32 d = -15.1;

println("%i", a);
println("%i", b);
println("%i", c);
println("%i", d);
```

Expected output:

```text
15
15
-15
-15
```

This is formatting, not type conversion.

---

# 19. `%f` — Floating-Point Formatting

`%f` prints a value as floating-point.

Example:

```drift
f32 score = 98.75;

println("Score: %f", score);
```

An integer may also be formatted as a floating-point value:

```drift
i32 number = 15;

println("%f", number);
```

The output should represent the number as floating-point, such as:

```text
15.0
```

The original variable remains an `i32`.

---

# 20. `%s` — String Formatting

`%s` expects a string.

Example:

```drift
str name = "Noah";

println("Name: %s", name);
```

Output:

```text
Name: Noah
```

An incompatible value should produce a clear runtime error unless an explicit conversion rule exists.

For example:

```drift
i32 age = 15;

println("%s", age);
```

should not silently reinterpret the integer as a string unless the language explicitly defines that behaviour.

---

# 21. `%c` — Character Formatting

`%c` prints a character.

Example:

```drift
char grade = 'A';

println("Grade: %c", grade);
```

Output:

```text
Grade: A
```

A string should not automatically be accepted as a character unless Drift explicitly defines such a conversion.

---

# 22. `%n` — Variable Name

`%n` prints the **name of the variable**, rather than its value.

Example:

```drift
i32 score = 100;

println("%n", score);
```

Output:

```text
score
```

This means `%n` is different from:

```drift
println("%i", score);
```

which outputs:

```text
100
```

`%n` requires an identifier expression that refers to a variable.

This should be invalid:

```drift
println("%n", 100);
```

There is no variable name associated with the literal `100`.

This should also be invalid:

```drift
println("%n", score + 20);
```

because `score + 20` is an expression rather than a single variable identifier.

---

# 23. `%m` — Memory Address

`%m` represents a memory-address-style output.

Example conceptually:

```drift
i32 value = 100;

println("%m", value);
```

However, `%m` depends on Drift's own runtime and memory model.

Do **not** substitute a random C# object address for a Drift memory address.

If Drift does not yet have a defined memory model, `%m` should remain reserved or produce an appropriate error.

The implementation should not use arbitrary C# object identity as a fake Drift memory address.

---

# 24. `%%` — Literal Percent

`%%` outputs a literal `%`.

It does **not** consume an argument.

Example:

```drift
i32 score = 98;

println("Progress: 100%%, score: %i", score);
```

Output:

```text
Progress: 100%, score: 98
```

Only `%i` consumes an argument in this example.

---

# 25. Multiple Format Specifiers

Multiple values can be printed using one format string.

Example:

```drift
str name = "Noah";
i32 age = 15;
f32 score = 98.75;
char grade = 'A';

println("%s is %i years old and scored %f with grade %c", name, age, score, grade);
```

Format arguments are matched from left to right:

```text
%s → name
%i → age
%f → score
%c → grade
```

The order matters.

---

# 26. Format Argument Count

The number of arguments must match the number of **consuming** format specifiers.

For example:

```drift
str name = "Noah";

println("%s %i", name);
```

is invalid because `%i` does not have a corresponding argument.

This is also invalid:

```drift
str name = "Noah";
i32 age = 15;

println("%s", name, age);
```

There is an extra argument.

However, `%%` does not consume an argument:

```drift
i32 score = 98;

println("Score: %i%%", score);
```

This is valid.

The consuming specifier is:

```text
%i
```

while:

```text
%%
```

does not consume anything.

---

# 27. Unknown Format Specifiers

Unknown format specifiers should produce a clear error.

For example:

```drift
i32 value = 10;

println("%x", value);
```

`%x` is not currently a supported Drift format specifier.

Do not silently ignore unknown format specifiers.

---

# 28. Formatting Does Not Change Types

Format specifiers control **how a value is printed**.

They do not mutate the underlying Drift value.

Example:

```drift
f32 number = 15.75;

println("%i", number);
println("%f", number);
```

Output:

```text
15
15.75
```

`number` remains an `f32` after both calls.

Likewise:

```drift
i32 number = 15;

println("%f", number);
```

does not turn `number` into an `f32`.

This distinction is important:

```text
Formatting ≠ Type conversion
```

---

# 29. `println()` Implementation Architecture

Format processing should primarily live in the runtime/output formatting system rather than being scattered throughout the parser.

The intended flow is:

```text
println(formatString, arguments...)
             ↓
      Parse call expression
             ↓
      Interpreter evaluates arguments
             ↓
       println runtime function
             ↓
       Format-string parser
             ↓
      Format-specifier handling
             ↓
            Output
```

The parser should primarily understand that `println()` is a function call.

The runtime should handle the semantics of formatting.

---

# 30. Conditionals

Drift uses `if` for conditional execution.

Example:

```drift
if (score > 50) {
    println("Passed");
}
```

---

# 31. `elif`

Drift uses `elif` rather than `else if`.

Example:

```drift
if (score >= 90) {
    println("A");
} elif (score >= 80) {
    println("B");
} elif (score >= 70) {
    println("C");
} else {
    println("Below C");
}
```

Do not silently replace `elif` with `else if`.

The lexer and parser should treat `elif` as its own language construct if that is how the current implementation is designed.

---

# 32. Boolean Values

Boolean values are:

```drift
true
false
```

Example:

```drift
bool running = true;
bool finished = false;
```

Conditionals can use boolean expressions:

```drift
if (running) {
    println("Running");
}
```

---

# 33. Arrays

Arrays are intended to use explicit element types and dimensions.

Example:

```drift
i32 myarray[3,3];
```

This represents a two-dimensional array with dimensions `3 × 3`.

The exact array implementation should follow the current AST/interpreter design.

Do not invent additional array syntax without checking the existing implementation.

---

# 34. Structs

Drift supports the concept of structs for grouping related data.

Example:

```drift
struct Player {
    str name;
    i32 score;
}
```

A struct definition groups strongly typed fields.

Example fields:

```text
name → str
score → i32
```

Struct syntax should remain explicit and strongly typed.

---

# 35. Struct Parameters

A struct can be used as a function parameter.

Example:

```drift
void main(struct Player player) {
    println("Player: %s", player.name);
}
```

The exact member-access implementation depends on the current parser, AST, and runtime.

Do not assume member access is implemented simply because struct syntax exists.

---

# 36. Collections / Hashes

Collection and hash syntax is still evolving.

One proposed design uses `:` for key/value association and `{}` for collection literals.

For example, conceptually:

```drift
{
    "name": "Noah",
    "score": 100
}
```

However, this area should be treated as **unfinished language design** unless the current source code has already implemented it.

Do not build a large collection system based only on this example without confirming the current design.

---

# 37. Lexer

The lexer converts Drift source code into tokens.

It should be responsible for identifying things such as:

* Keywords
* Identifiers
* Numbers
* Strings
* Characters
* Operators
* Punctuation
* Format-string contents where appropriate
* Comments

The lexer should not perform interpreter/runtime behaviour.

For example, it should not decide that `%i` means "convert this value to an integer."

That belongs to the `println()` formatting/runtime system.

---

# 38. Parser

The parser converts tokens into the AST.

Typical parser responsibilities include:

* Expressions
* Statements
* Variable declarations
* Function declarations
* Function calls
* Conditionals
* Returns
* Struct declarations
* Arrays
* Blocks

The parser should validate syntax but should not contain large amounts of runtime behaviour.

---

# 39. AST

The AST represents the structure of the Drift program.

Possible AST concepts include:

```text
Program
FunctionDeclaration
VariableDeclaration
ConstantDeclaration
BlockStatement
IfStatement
ElifStatement
ReturnStatement
ExpressionStatement
CallExpression
BinaryExpression
LiteralExpression
IdentifierExpression
StructDeclaration
```

The exact class names must be taken from the existing codebase rather than assumed.

---

# 40. Interpreter

The interpreter evaluates the AST.

It is responsible for things such as:

* Evaluating expressions
* Executing statements
* Managing environments
* Calling functions
* Returning values
* Handling variables
* Executing conditionals
* Calling runtime functions

Runtime behaviour should remain separate from parsing logic where possible.

---

# 41. Environments / Scope

The interpreter should maintain environments representing variable scope.

Conceptually:

```text
Global Environment
        │
        ├── Function Environment
        │       ├── variable
        │       ├── variable
        │       └── parameter
        │
        └── Another Function Environment
```

When a function executes, its local variables should belong to that function's environment.

Nested blocks may eventually introduce additional scopes depending on the language design.

---

# 42. Runtime Errors

Errors should be clear and useful.

Examples include:

```text
Undefined variable: x
```

or:

```text
Type mismatch: expected i32, got str
```

For format strings, errors should identify the problem.

For example:

```text
Format argument mismatch: %i requires a compatible numeric value.
```

Avoid vague errors such as:

```text
Something went wrong.
```

---

# 43. Type Checking

Drift is intended to be strongly typed.

For example:

```drift
i32 number = 10;
```

should not accept an incompatible string:

```drift
i32 number = "hello";
```

unless the language explicitly defines an implicit conversion.

Type checking may occur during parsing, AST validation, interpretation, or a future semantic-analysis stage depending on the project's architecture.

Do not introduce a completely separate compiler/type-checking architecture unless requested.

---

# 44. Conversions vs Formatting

These concepts must remain separate.

A conversion changes the value/type representation.

Formatting changes how a value is displayed.

For example:

```drift
f32 number = 15.75;

println("%i", number);
```

does not convert `number` to an integer.

It only prints:

```text
15
```

The underlying value remains:

```text
15.75
```

and the type remains:

```text
f32
```

---

# 45. Naming

Use the existing project's naming conventions.

Likely C# conventions include:

```text
PascalCase
```

for classes and methods, and:

```text
camelCase
```

for local variables and parameters.

However, always inspect the existing code before introducing a different convention.

---

# 46. Old G-Sharp References

The project was previously called **G-Sharp**.

Old references may include:

```text
G-Sharp
G_Sharp
G.Sharp
G-Sharp.AST
```

Do not automatically rename all of them.

A namespace migration should be performed deliberately and only when appropriate.

---

# 47. Existing Parser Issues

Previous development has involved parser issues around:

* `ParseIf`
* `IsAtEnd`
* `Elif` token handling
* Statement parsing
* Token bounds
* AST statement handling

When fixing these, inspect the complete parser flow rather than patching only the line that generated the error.

For example, an `elif` problem may involve:

```text
Lexer
 ↓
Elif token
 ↓
Parser
 ↓
If/Elif AST
 ↓
Interpreter
```

Fix the appropriate layer rather than adding special cases to unrelated code.

---

# 48. Testing Philosophy

Every language feature should have small test programs.

For example, `println()` formatting should test:

```drift
void main() {
    str name = "Noah";
    i32 age = 15;
    f32 score = 98.75;
    char grade = 'A';

    println("Name: %s", name);
    println("Age: %i", age);
    println("Score: %f", score);
    println("Grade: %c", grade);
}
```

A multi-format test:

```drift
void main() {
    str name = "Noah";
    i32 age = 15;
    f32 score = 98.75;

    println("%s is %i years old and scored %f", name, age, score);
}
```

A percent test:

```drift
void main() {
    i32 score = 98;

    println("Progress: 100%%, score: %i", score);
}
```

A formatting-without-mutation test:

```drift
void main() {
    f32 number = 15.75;

    println("%i", number);
    println("%f", number);
}
```

Expected output:

```text
15
15.75
```

---

# 49. What AI Agents Should Not Do

Do not:

* Rewrite the entire interpreter to add a small feature.
* Replace the existing parser architecture without a reason.
* Invent syntax.
* Change `elif` into `else if`.
* Change `println()` back to a single-expression printing function.
* Treat `%i` as a permanent type conversion.
* Treat `%f` as a permanent type conversion.
* Use C# object addresses as Drift memory addresses.
* Remove existing comments without reason.
* Rename the entire project from G-Sharp to Drift automatically.
* Add implicit conversions without the language design explicitly allowing them.
* Assume an unfinished feature is implemented.
* Modify unrelated files simply to make the code "cleaner."

---

# 50. Canonical Drift Examples

A basic program:

```drift
void main() {
    println("Hello, Drift!");
}
```

Variables:

```drift
void main() {
    str name = "Noah";
    i32 age = 15;
    f32 score = 98.75;
    char grade = 'A';
    bool passed = true;

    println("Name: %s", name);
    println("Age: %i", age);
    println("Score: %f", score);
    println("Grade: %c", grade);
}
```

Conditional:

```drift
void main() {
    i32 score = 85;

    if (score >= 90) {
        println("Grade: A");
    } elif (score >= 80) {
        println("Grade: B");
    } else {
        println("Grade: C or below");
    }
}
```

Function:

```drift
i32 add(i32 a, i32 b) {
    return a + b;
}

void main() {
    i32 result = add(10, 20);

    println("Result: %i", result);
}
```

Struct:

```drift
struct Player {
    str name;
    i32 score;
}
```

Interpolation:

```drift
void main() {
    str name = "Noah";
    i32 score = 100;

    println($"Player {name} scored {score}");
}
```

Format specifiers:

```drift
void main() {
    str name = "Noah";
    i32 age = 15;
    f32 score = 98.75;
    char grade = 'A';

    println("%s", name);
    println("%i", age);
    println("%f", score);
    println("%c", grade);
    println("%n", score);
}
```

Multiple values:

```drift
void main() {
    str name = "Noah";
    i32 age = 15;
    f32 score = 98.75;

    println("%s is %i years old and scored %f", name, age, score);
}
```

---

# 51. Source of Truth

When there is a conflict between this document and the actual Drift implementation:

1. Inspect the current source code.
2. Determine what is actually implemented.
3. Treat explicit, recent language-design decisions as authoritative for intended syntax.
4. Do not silently change the language to match this document.
5. Update this context document if the language design has intentionally changed.

This file is intended to give AI agents enough context to work on Drift without repeatedly rediscovering the project's language design.

---

# 52. Current Core `println()` Rule

The most important rule for `println()` is:

```drift
println("format_string", arguments...);
```

The first argument is a **quoted Drift string literal** containing zero or more format specifiers.

Examples:

```drift
println("Hello");
println("Name: %s", name);
println("Age: %i", age);
println("Score: %f", score);
println("Grade: %c", grade);
println("Variable: %n", score);
println("Progress: 100%%");
```

Do not use:

```drift
println(Name: %s, name);
```

because `Name: %s` is not a valid Drift string literal.

Use:

```drift
println("Name: %s", name);
```

The same rule applies to every other format-string example in Drift.
