using System.Globalization;

public class Interpreter
{
    private readonly Dictionary<string, FunctionDeclaration> functions = new();
    private readonly RuntimeEnvironment globals = new();
    private RuntimeEnvironment environment;

    private FunctionDeclaration? entryPoint;
    private bool entryPointInvoked;

    public Interpreter()
    {
        environment = globals;
    }

    public void Run(List<Statement> statements)
    {
        functions.Clear();
        globals.Clear();
        environment = globals;
        entryPoint = null;
        entryPointInvoked = false;

        // Register top-level functions first so calls may appear before declarations.
        foreach (var statement in statements)
        {
            if (statement is not FunctionDeclaration function)
                continue;

            functions[function.Name] = function;

            if (function.Name.Equals("main", StringComparison.OrdinalIgnoreCase))
            {
                if (entryPoint != null)
                    throw new RuntimeException(
                        $"Multiple entry points found: '{entryPoint.Name}' and '{function.Name}'.",
                        function.Location);

                entryPoint = function;
            }
        }

        try
        {
            foreach (var statement in statements)
            {
                if (statement is FunctionDeclaration) continue;
                Execute(statement);
            }
        }
        catch (ReturnException returned)
        {
            throw new RuntimeException(
                "'return' can only be used inside a function.",
                returned.Location);
        }

        if (entryPoint != null && !entryPointInvoked)
        {
            if (entryPoint.Parameters.Count != 0)
                throw new RuntimeException(
                    $"Entry point '{entryPoint.Name}' must not declare parameters.",
                    entryPoint.Location);

            InvokeFunction(entryPoint, new List<object?>(), entryPoint.Location);
        }
    }

    private void ExecuteBlock(List<Statement> body)
    {
        var previous = environment;
        environment = new RuntimeEnvironment(previous);

        try
        {
            foreach (var statement in body) Execute(statement);
        }
        finally
        {
            environment = previous;
        }
    }

    private ValueType ParseValueType(string type, SourcePosition location)
    {
        return type switch
        {
            "i8" => ValueType.I8,
            "i16" => ValueType.I16,
            "i32" => ValueType.I32,
            "i64" => ValueType.I64,

            "ui8" => ValueType.UI8,
            "ui16" => ValueType.UI16,
            "ui32" => ValueType.UI32,
            "ui64" => ValueType.UI64,

            "f16" => ValueType.F16,
            "f32" => ValueType.F32,
            "f64" => ValueType.F64,

            "uf16" => ValueType.UF16,
            "uf32" => ValueType.UF32,
            "uf64" => ValueType.UF64,

            "str" => ValueType.String,
            "bool" => ValueType.Bool,
            "char" => ValueType.Char,
            "void" => ValueType.Null,
            _ => throw new RuntimeException($"Unknown type '{type}'.", location)
        };
    }

    private void Execute(Statement statement)
    {
        if (statement is PrintStatement print)
        {
            Print(print);
        }
        else if (statement is IfStatement ifStatement)
        {
            if (RequireBool(Evaluate(ifStatement.Condition), "if condition", ifStatement.Condition.Location))
            {
                ExecuteBlock(ifStatement.ThenBranch);
                return;
            }

            foreach (var elif in ifStatement.ElifBranches)
            {
                if (!RequireBool(Evaluate(elif.Condition), "elif condition", elif.Condition.Location))
                    continue;

                ExecuteBlock(elif.Body);
                return;
            }

            if (ifStatement.ElseBranch != null)
                ExecuteBlock(ifStatement.ElseBranch);
        }
        else if (statement is WhileStatement whileStatement)
        {
            while (RequireBool(
                       Evaluate(whileStatement.Condition),
                       "while condition",
                       whileStatement.Condition.Location))
            {
                ExecuteBlock(whileStatement.Body);
            }
        }
        else if (statement is ArrayDeclaration arrayDeclaration)
        {
            if (arrayDeclaration.Dimensions.Count != 2)
                throw new RuntimeException(
                    "Only two-dimensional arrays are supported.",
                    arrayDeclaration.Location);

            var array = new ArrayValue(arrayDeclaration.Dimensions[0], arrayDeclaration.Dimensions[1]);
            environment.Define(arrayDeclaration.Name, new Value(array, ValueType.Array, false));
        }
        else if (statement is VarDeclaration declaration)
        {
            var value = Evaluate(declaration.Value);
            var type = ParseValueType(declaration.Type, declaration.Location);
            var coerced = CoerceValue(type, value, declaration.Location);

            environment.Define(declaration.Name, new Value(coerced, type, declaration.IsConst));
        }
        else if (statement is ArrayAssignment arrayAssignment)
        {
            if (arrayAssignment.Target == null || arrayAssignment.Value == null)
                throw new RuntimeException("Invalid array assignment.", arrayAssignment.Location);

            var value = Lookup(arrayAssignment.Target.Name, arrayAssignment.Location);

            if (value.Type != ValueType.Array)
                throw new RuntimeException(
                    $"'{arrayAssignment.Target.Name}' is not an array.",
                    arrayAssignment.Location);

            var array = (ArrayValue)value.Data!;

            var x = Convert.ToInt32(
                Evaluate(arrayAssignment.Target.X),
                CultureInfo.InvariantCulture);
            var y = Convert.ToInt32(
                Evaluate(arrayAssignment.Target.Y),
                CultureInfo.InvariantCulture);

            array.Data[array.Index(x, y)] = Evaluate(arrayAssignment.Value)!;
        }
        else if (statement is CallStatement callStatement)
        {
            Evaluate(callStatement.Call);
        }
        else if (statement is FunctionDeclaration function)
        {
            functions[function.Name] = function;
        }
        else if (statement is AssignmentStatement assignment)
        {
            var variable = Lookup(assignment.Name, assignment.Location);

            if (variable.IsConst)
                throw new RuntimeException(
                    $"Cannot modify constant '{assignment.Name}'.",
                    assignment.Location);

            variable.Data = CoerceValue(
                variable.Type,
                Evaluate(assignment.Value),
                assignment.Location);
            variable.WasModified = true;
        }
        else if (statement is ReturnStatement returnStatement)
        {
            object? value = null;
            if (returnStatement.Value != null) value = Evaluate(returnStatement.Value);
            throw new ReturnException(value, returnStatement.Location);
        }
    }

    private void Print(PrintStatement print)
    {
        if (print.Arguments.Count == 0)
        {
            Console.WriteLine();
            return;
        }

        List<object?> values = new();
        foreach (var argument in print.Arguments)
            values.Add(Evaluate(argument));

        if (values[0] is string format)
        {
            var formatArguments = values.Skip(1).ToList();
            var formatLocation = print.Arguments[0].Location.IsKnown
                ? print.Arguments[0].Location
                : print.Location;

            Console.WriteLine(DriftFormatter.Format(format, formatArguments, formatLocation));
            return;
        }

        if (values.Count == 1)
        {
            Console.WriteLine(values[0]);
            return;
        }

        throw new RuntimeException(
            "println expects one value or a format string followed by arguments.",
            print.Location);
    }

    private Value Lookup(string name, SourcePosition location)
    {
        if (!environment.TryGet(name, out var value))
            throw new RuntimeException($"Undefined variable '{name}'.", location);

        return value;
    }

    private object? Evaluate(Expression expression)
    {
        if (expression is LiteralExpression literal)
        {
            return literal.Kind switch
            {
                LiteralKind.Integer => long.Parse(literal.Text, CultureInfo.InvariantCulture),
                LiteralKind.Float => double.Parse(literal.Text, CultureInfo.InvariantCulture),
                LiteralKind.String => literal.Text,
                LiteralKind.Boolean => bool.Parse(literal.Text),
                LiteralKind.Character => literal.Text[0],
                _ => throw new RuntimeException(
                    $"Unsupported literal '{literal.Text}'.",
                    literal.Location)
            };
        }

        if (expression is VariableExpression variable)
        {
            return Lookup(variable.Name, variable.Location).Data;
        }

        if (expression is UnaryExpression unary)
        {
            var operand = Evaluate(unary.Operand);

            if (unary.Operator == "-")
            {
                if (IsFloating(operand)) return -ToDouble(operand, unary.Location);
                if (IsIntegral(operand)) return -ToInt64(operand, unary.Location);

                throw new RuntimeException(
                    "Unary '-' requires a numeric operand.",
                    unary.Location);
            }

            if (unary.Operator == "!")
                return !RequireBool(operand, "unary '!' operand", unary.Location);

            throw new RuntimeException(
                $"Unknown unary operator '{unary.Operator}'.",
                unary.Location);
        }

        if (expression is ArrayAccess access)
        {
            var value = Lookup(access.Name, access.Location);

            if (value.Type != ValueType.Array)
                throw new RuntimeException(
                    $"'{access.Name}' is not an array.",
                    access.Location);

            var array = (ArrayValue)value.Data!;
            var x = Convert.ToInt32(Evaluate(access.X), CultureInfo.InvariantCulture);
            var y = Convert.ToInt32(Evaluate(access.Y), CultureInfo.InvariantCulture);

            return array.Data[array.Index(x, y)];
        }

        if (expression is BinaryExpression binary)
        {
            return EvaluateBinary(binary);
        }

        if (expression is CallExpression call)
        {
            return EvaluateCall(call);
        }

        throw new RuntimeException("Unknown expression type.", expression.Location);
    }

    private object? EvaluateBinary(BinaryExpression binary)
    {
        var left = Evaluate(binary.Left);
        var right = Evaluate(binary.Right);

        if (binary.Operator == "==" || binary.Operator == "!=")
        {
            bool equal = ValuesEqual(left, right);
            return binary.Operator == "==" ? equal : !equal;
        }

        if (binary.Operator == "+" && left is string leftString && right is string rightString)
            return leftString + rightString;

        if (!IsNumeric(left) || !IsNumeric(right))
            throw new RuntimeException(
                $"Operator '{binary.Operator}' requires numeric operands.",
                binary.Location);

        if (IsFloating(left) || IsFloating(right))
        {
            double leftNumber = ToDouble(left, binary.Location);
            double rightNumber = ToDouble(right, binary.Location);

            if ((binary.Operator == "/" || binary.Operator == "%") && rightNumber == 0)
                throw new RuntimeException("Division by zero.", binary.Location);

            return binary.Operator switch
            {
                "+" => leftNumber + rightNumber,
                "-" => leftNumber - rightNumber,
                "*" => leftNumber * rightNumber,
                "/" => leftNumber / rightNumber,
                "%" => leftNumber % rightNumber,
                "<" => leftNumber < rightNumber,
                ">" => leftNumber > rightNumber,
                "<=" => leftNumber <= rightNumber,
                ">=" => leftNumber >= rightNumber,
                _ => throw new RuntimeException(
                    $"Unknown operator '{binary.Operator}'.",
                    binary.Location)
            };
        }

        long leftInteger = ToInt64(left, binary.Location);
        long rightInteger = ToInt64(right, binary.Location);

        if ((binary.Operator == "/" || binary.Operator == "%") && rightInteger == 0)
            throw new RuntimeException("Division by zero.", binary.Location);

        return binary.Operator switch
        {
            "+" => leftInteger + rightInteger,
            "-" => leftInteger - rightInteger,
            "*" => leftInteger * rightInteger,
            "/" => leftInteger / rightInteger,
            "%" => leftInteger % rightInteger,
            "<" => leftInteger < rightInteger,
            ">" => leftInteger > rightInteger,
            "<=" => leftInteger <= rightInteger,
            ">=" => leftInteger >= rightInteger,
            _ => throw new RuntimeException(
                $"Unknown operator '{binary.Operator}'.",
                binary.Location)
        };
    }

    private object? EvaluateCall(CallExpression call)
    {
        if (!functions.TryGetValue(call.Name, out var function))
            throw new RuntimeException($"Unknown function '{call.Name}'.", call.Location);

        List<object?> arguments = new();
        foreach (var argument in call.Arguments)
            arguments.Add(Evaluate(argument));

        return InvokeFunction(function, arguments, call.Location);
    }

    private object? InvokeFunction(
        FunctionDeclaration function,
        List<object?> arguments,
        SourcePosition callLocation)
    {
        if (ReferenceEquals(function, entryPoint))
            entryPointInvoked = true;

        if (arguments.Count != function.Parameters.Count)
            throw new RuntimeException(
                $"Function '{function.Name}' expects {function.Parameters.Count} argument(s) but got {arguments.Count}.",
                callLocation);

        var previous = environment;
        environment = new RuntimeEnvironment(globals);

        try
        {
            for (int i = 0; i < function.Parameters.Count; i++)
            {
                var value = InferValue(arguments[i], callLocation);
                environment.Define(function.Parameters[i], value);
            }

            foreach (var statement in function.Body)
                Execute(statement);
        }
        catch (ReturnException returned)
        {
            if (function.ReturnType == "void")
            {
                if (returned.Value != null)
                    throw new RuntimeException(
                        $"Void function '{function.Name}' cannot return a value.",
                        returned.Location);

                return null;
            }

            if (returned.Value == null)
                throw new RuntimeException(
                    $"Function '{function.Name}' must return a value.",
                    returned.Location);

            return CoerceValue(
                ParseValueType(function.ReturnType, function.Location),
                returned.Value,
                callLocation);
        }
        finally
        {
            environment = previous;
        }

        if (function.ReturnType != "void")
            throw new RuntimeException(
                $"Function '{function.Name}' must return a value.",
                function.Location);

        return null;
    }

    private Value InferValue(object? value, SourcePosition location)
    {
        return new Value(value, InferValueType(value, location), false);
    }

    private ValueType InferValueType(object? value, SourcePosition location)
    {
        return value switch
        {
            sbyte => ValueType.I8,
            short => ValueType.I16,
            int => ValueType.I32,
            long => ValueType.I64,
            byte => ValueType.UI8,
            ushort => ValueType.UI16,
            uint => ValueType.UI32,
            ulong => ValueType.UI64,
            Half => ValueType.F16,
            float => ValueType.F32,
            double => ValueType.F64,
            string => ValueType.String,
            bool => ValueType.Bool,
            char => ValueType.Char,
            ArrayValue => ValueType.Array,
            null => ValueType.Null,
            _ => throw new RuntimeException(
                $"Unsupported runtime value '{value.GetType().Name}'.",
                location)
        };
    }

    private object? CoerceValue(ValueType type, object? value, SourcePosition location)
    {
        try
        {
            return type switch
            {
                ValueType.I8 => checked((sbyte)ToInt64(value, location)),
                ValueType.I16 => checked((short)ToInt64(value, location)),
                ValueType.I32 => checked((int)ToInt64(value, location)),
                ValueType.I64 => ToInt64(value, location),

                ValueType.UI8 => checked((byte)ToUInt64(value, location)),
                ValueType.UI16 => checked((ushort)ToUInt64(value, location)),
                ValueType.UI32 => checked((uint)ToUInt64(value, location)),
                ValueType.UI64 => ToUInt64(value, location),

                ValueType.F16 => (Half)ToDouble(value, location),
                ValueType.F32 => (float)ToDouble(value, location),
                ValueType.F64 => ToDouble(value, location),

                ValueType.UF16 => (Half)ToDouble(value, location),
                ValueType.UF32 => (float)ToDouble(value, location),
                ValueType.UF64 => ToDouble(value, location),

                ValueType.String => value as string
                    ?? throw new RuntimeException("Expected a string value.", location),
                ValueType.Bool => value is bool boolean
                    ? boolean
                    : throw new RuntimeException("Expected a boolean value.", location),
                ValueType.Char => value is char character
                    ? character
                    : throw new RuntimeException("Expected a character value.", location),
                ValueType.Array => value as ArrayValue
                    ?? throw new RuntimeException("Expected an array value.", location),
                ValueType.Null => value == null
                    ? null
                    : throw new RuntimeException("Expected a void value.", location),
                _ => throw new RuntimeException(
                    $"Cannot store a value of type '{type}'.",
                    location)
            };
        }
        catch (OverflowException)
        {
            throw new RuntimeException(
                $"Value '{value}' is out of range for type '{type}'.",
                location);
        }
        catch (FormatException)
        {
            throw new RuntimeException(
                $"Value '{value}' cannot be converted to type '{type}'.",
                location);
        }
    }

    private static bool RequireBool(object? value, string context, SourcePosition location)
    {
        if (value is bool boolean) return boolean;

        throw new RuntimeException($"{context} must be a boolean.", location);
    }

    private static bool ValuesEqual(object? left, object? right)
    {
        if (IsNumeric(left) && IsNumeric(right))
        {
            if (IsFloating(left) || IsFloating(right))
                return ToDouble(left) == ToDouble(right);

            return ToInt64(left) == ToInt64(right);
        }

        return Equals(left, right);
    }

    private static bool IsIntegral(object? value)
    {
        return value is sbyte or short or int or long or byte or ushort or uint or ulong;
    }

    private static bool IsFloating(object? value)
    {
        return value is Half or float or double;
    }

    private static bool IsNumeric(object? value)
    {
        return IsIntegral(value) || IsFloating(value);
    }

    private static long ToInt64(object? value, SourcePosition location = default)
    {
        if (!IsIntegral(value))
            throw new RuntimeException("Expected an integer value.", location);

        return Convert.ToInt64(value, CultureInfo.InvariantCulture);
    }

    private static ulong ToUInt64(object? value, SourcePosition location = default)
    {
        if (!IsIntegral(value))
            throw new RuntimeException("Expected an integer value.", location);

        return Convert.ToUInt64(value, CultureInfo.InvariantCulture);
    }

    private static double ToDouble(object? value, SourcePosition location = default)
    {
        if (!IsNumeric(value))
            throw new RuntimeException("Expected a numeric value.", location);

        return value switch
        {
            Half half => (double)half,
            float single => single,
            double number => number,
            _ => Convert.ToDouble(value, CultureInfo.InvariantCulture)
        };
    }
}
