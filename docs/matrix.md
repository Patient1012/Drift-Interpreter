# Matrix Support

**Current status:** Matrix operations are **not yet implemented** in the Drift interpreter. The lexer, parser, and runtime have no support for matrix literals, declaration, arithmetic, or indexing. The feature is on the roadmap but is outside the current MVP scope.

## Why it is not yet implemented
- The AST, parser expressions, and runtime values are designed primarily for scalar types (`i8‑i64`, `ui8‑ui64`, `f16‑f64`, `str`, `bool`, `char`).  
- Two‑dimensional array AST nodes (`ArrayDeclaration`, `ArrayAssignment`, `ArrayAccess`) exist but are *unreachable from source* – they can only be created programmatically (e.g., via the REPL or embedded C# host).  
- No syntax for matrix literals (e.g., `[[1,2],[3,4]]`) has been added to the lexer/parser.  
- Runtime does not contain matrix‑specific operations (multiply, transpose, determinant, etc.).

## How to add matrix support (incremental plan)

### 1. Lexer
- Recognise a new token kind for matrix literals, or reuse the existing `LeftSquareBrace` / `RightSquareBrace` tokens with a custom syntax.
- Extend `ReadIdentifier` / `ReadNumber` to consume matrix brackets, or add a new `ReadMatrix` method that parses `[[ … ]]` blocks.
- Keyword tokens are not needed; matrix syntax is purely structural.

### 2. Parser
- Add a new expression kind `MatrixLiteral` (or reuse `ArrayLiteral` if the language only supports 2‑D) that holds a nested list of inner expressions.
- Update `ParsePrimary` to dispatch to a `ParseMatrixLiteral` method when it encounters `[`.
- `ParseMatrixLiteral` reads rows separated by `;` and expressions within each row separated by `,`. Example:
  ```
  [[1, 2]; [3, 4]]
  ```
- Ensure the parser validates rectangular shape (optional, runtime can also check).

### 3. AST
- Add `MatrixLiteral` node (or extend `ArrayLiteral` with a `dimensions` flag). Store `List<Expression> Rows` where each row is a `List<Expression>`.
- Keep existing `ArrayAccess` for indexing into a matrix variable.

### 4. Runtime / Interpreter
- Add a new `MatrixValue` class (or extend `ArrayValue` to flag 2‑D) that stores a flat `double[]` or `int[]` with row‑major ordering.
- Implement operations:
  - **Get/Set** via `matrix[i, j]` syntax (already supported by `ArrayAccess` if the target is a matrix).
  - **Transpose**.
  - **Multiply** (matrix‑matrix or matrix‑scalar).
  - **Determinant** (2×2 only, or generic algorithm).
- Modify `CoerceValue` and `Evaluate` to route matrix expressions to the new runtime type.
- Throw structured `RuntimeException` with source location if dimensions mismatch.

### 5. Semantic analysis (optional)
- Add a pass that checks matrix literals have consistent row lengths and that indexed accesses stay within bounds.

### 6. Documentation & Examples
- Update `docs/Spec.md` with matrix literal syntax and supported operations.
- Add example programs in `examples/` that demonstrate matrix creation, transpose, and multiplication.

## Compiling Drift to ASM (e.g., x86‑64) and matrix support

If Drift were compiled to native assembly instead of being interpreted, the matrix feature would be implemented at a lower level:

### 1. Code generation pipeline
- Extend the **code generator** (`CodeGen` or similar) to emit assembly instructions for matrix operations.
- For each AST node, emit sequences that use SIMD registers (AVX/SSE) or general‑purpose registers with software loops.

### 2. Data layout in memory
- Decide a memory layout: row‑major contiguous block, or array of pointers to rows.
- Emits `alloc`/`malloc` for matrix data, alignment considerations for SIMD (16‑byte or 32‑byte).

### 3. Instruction selection
- **Matrix addition/subtraction**: loop over elements, emit `addps`/`subps` (SIMD) or plain `add`/`sub` in a nested loop.
- **Matrix multiplication**: generate three nested loops (i, k, j) and emit `mulpd`/`madd` or call a library routine (e.g., `cblas_dgemm`).
- **Transpose**: generate a loop that reads `matrix[i*stride+j]` and writes to `transpose[j*stride+i]`.
- **Determinant**: for 2×2, emit direct code `a*d - b*c`; for larger matrices, emit a recursive LU‑decomposition or call LAPACK.

### 4. Runtime support library
- Include a small linear‑algebra library (or link against BLAS/LAPACK) that the generated assembly can call.
- Provide wrapper functions (e.g., `matrix_mul`, `matrix_transpose`) with C‑calling convention.

### 5. Example assembly snippet (pseudo‑x86)
```asm
; 2x2 matrix multiplication C = A * B
; A, B, C are pointers to 4‑element float arrays (row‑major)
movaps  xmm0, [rdi]      ; load A[0]
mulps   xmm1, [rsi]      ; B[0]*B[1] etc (depends on ordering)
; ... loop body using SIMD
```

### 6. Challenges
- **Alignment & overflow**: matrix sizes may not be known at compile time; generate runtime size checks.
- **Garbage collection** (if the runtime is managed): ensure matrix memory is rooted correctly.
- **Interop**: if Drift later adds foreign function interface, matrix data could be passed to C BLAS routines.

## Next steps for this repository
1. **Short‑term:** Add matrix literal parsing and a basic `MatrixValue` runtime type; expose `[[1,2];[3,4]]` syntax.
2. **Medium‑term:** Implement transpose and multiply operations; add test cases.
3. **Long‑term:** If targeting ASM, integrate a code‑gen backend and optional BLAS linkage; update `docs/architecture.md` with code‑gen notes.

**References**
- `docs/Spec.md` – currently no matrix section; add after implementation.
- `architecture.md` – describe the interpreter pipeline; extend when ASM backend is pursued.
- `tests/DriftInterpreter.Tests/` – add matrix‑related unit tests once the feature is started.