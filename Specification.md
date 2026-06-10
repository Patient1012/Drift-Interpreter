### Drift Language Specification

Version 0.1 Draft

## Overview

Drift is a statically and explicitly typed programming language designed for clarity, simplicity, and performance. It uses C-style syntax while maintaining straightforward language rules and predictable behavior.

Source files use the `.drf` extension.

# Design Principles

Drift is designed around the following principles:

- Strong static typing
- Explicit declarations
- Predictable behavior
- Minimal syntax
- C-style control flow
- Readable source code
- Zero implicit type conversions

---

# Basic Syntax

Statements are terminated with a semicolon.

```drift
i32 x = 5;
println(x);
```

Blocks are enclosed using braces.

```drift
if (x > 0)
{
    println("positive");
}
```

---

# Comments

Single-line comments:

```drift
// This is a comment
```

Multi-line comments:

```drift
/*
This is a
multi-line comment
*/
```

---

# Primitive Types

## Integer Types

| Type | Size                  |
| ---- | --------------------- |
| i8   | 8-bit signed integer  |
| i16  | 16-bit signed integer |
| i32  | 32-bit signed integer |
| i64  | 64-bit signed integer |

## Floating Point Types

| Type | Size                  |
| ---- | --------------------- |
| f16  | 16-bit floating point |
| f32  | 32-bit floating point |
| f64  | 64-bit floating point |

## Other Types

| Type | Description       |
| ---- | ----------------- |
| bool | Boolean value     |
| char | Unicode character |
| str  | UTF-8 string      |

---

# Variables

Variables are mutable.

```drift
var i32 score = 100;
```

Values may be reassigned.

```drift
score = 150;
```

---

# Constants

Constants are immutable after initialization.

```drift
const i32 maxPlayers = 32;
```

Attempting reassignment produces an error.

```drift
maxPlayers = 64; // Error
```

---

# Expressions

## Arithmetic

```drift
var i32 result = 5 + 3;
var i32 total = result - 2;
```

Supported operators:

| Operator | Meaning        |
| -------- | -------------- |
| +        | Addition       |
| -        | Subtraction    |
| *        | Multiplication |
| /        | Division       |
| %        | Modulus        |

---

# Comparison Operators

| Operator | Meaning               |
| -------- | --------------------- |
| ==       | Equal                 |
| !=       | Not Equal             |
| <        | Less Than             |
| >        | Greater Than          |
| <=       | Less Than or Equal    |
| >=       | Greater Than or Equal |

Example:

```drift
if (score >= 50)
{
    println("pass");
}
```

---

# Conditional Statements

## If

```drift
if (x > 10)
{
    println("large");
}
```

## Else If

Drift uses the `elif` keyword.

```drift
if (x < 5)
{
    println("small");
}
elif (x < 10)
{
    println("medium");
}
```

## Else

```drift
else
{
    println("large");
}
```

---

# Loops

## While Loop

```drift
var i32 x = 0;

while (x < 5)
{
    println(x);
    x = x + 1;
}
```

---

# Functions

Functions are declared using the following syntax:

```drift
i32 add(i32 a, i32 b)
{
    return a + b;
}
```

Function calls:

```drift
println(add(2, 3));
```

Functions may accept zero or more parameters.

```drift
void greet()
{
    println("Hello");
}
```

---

# Return Statements

Functions may return values.

```drift
return result;
```

Void functions do not return a value.

---

# Input and Output

## println

Prints a value followed by a newline.

```drift
println("Hello, World!");
println(score);
```

---

# Type System

Drift is strongly and explicitly typed.

Variables must declare a type.

```drift
i32 score = 100;
```

Implicit type conversions are not performed.

```drift
i32 x = 5;
f32 y = x; // Error
```

Explicit conversion syntax is planned for a future version.

---

# Scope

Variables exist only within the block in which they are declared.

```drift
if (true)
{
    i32 x = 5;
}

println(x); // Error
```

Scope rules are planned for implementation.

---
# Arrays

Arrays are declared this way:

`mutability_type primitive_type[x,y,z];`

or 

`mutability_type primitive_type[x,y,z] = {{},{}};`

Example:

`var i32[2,3] = {12,3},{6,3},{8,3}};`

---
# Error Handling

Current implementation uses runtime exceptions.

Future versions may introduce:

* Structured compiler diagnostics
* Source location reporting
* Recoverable errors
* User-defined exceptions

---

# Future Features

Planned features include:

* For loops
* Arrays
* Structs
* Enums
* Modules
* Packages
* Pattern matching
* Generics
* Interfaces
* Classes
* Tree-sitter support
* Zed extension
* Native compiler backend

---

# Example Program

```drift
const i32 passMark = 50;

i32 checkScore(i32 score)
{
    if (score >= passMark)
    {
        return 1;
    }
    else
    {
        return 0;
    }
}

println(checkScore(75));
```
