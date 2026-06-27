public class Interpreter
{
    private Dictionary<string, FunctionDeclaration> functions = new();
    private Dictionary<string, Value> variables = new();

    public void Run(List<Statement> statements)
    {
        foreach (var stmt in statements) Execute(stmt);
        foreach (var pair in variables)
        {

            if (!pair.Value.IsConst && !pair.Value.WasModified)
                throw new Exception($"Warning: Variable '{pair.Key}' was never mutated.");
        }
    }

    private bool TypeMatches(ValueType expected, object? value)
    {
        return expected switch
        {
            ValueType.I8 => value is sbyte,
            ValueType.I16 => value is short,
            ValueType.I32 => value is int,
            ValueType.I64 => value is long,
            
            ValueType.UI8 => value is byte,
            ValueType.UI16 => value is ushort,
            ValueType.UI32 => value is uint,
            ValueType.UI64 => value is ulong,
            
            ValueType.F16 => value is Half,
            ValueType.F32 => value is float,
            ValueType.F64 => value is double,
            
            ValueType.UF16 => value is Half,
            ValueType.UF32 => value is float,
            ValueType.UF64 => value is double,
            
            ValueType.String => value is string,
            ValueType.Bool => value is bool,
            ValueType.Array => value is ArrayValue,
            ValueType.Null => value is null,
            _ => false
        };
    }

    private ValueType ParseValueType(string type)
    {
        return type switch
        {
            // "i8" or "i16" or "i32" or "i64" => ValueType.Int,
            "i8" => ValueType.I8,
            "i16" => ValueType.I16,
            "i32" => ValueType.I32,
            "i64" => ValueType.I64,
            
            "ui8" => ValueType.I8,
            "ui16" => ValueType.I16,
            "ui32" => ValueType.I32,
            "ui64" => ValueType.I64,
            
            "f16" => ValueType.F16,
            "f32" => ValueType.F32,
            "f64" => ValueType.F64,
            
            "uf16" => ValueType.F16,
            "uf32" => ValueType.F32,
            "uf64" => ValueType.F64,
            
            "str" => ValueType.String,
            "bool" => ValueType.Bool,
            "void" => ValueType.Null,
            _ => throw new Exception($"Unknown type '{type}'.")
        };
    }

    private void Execute(Statement statement)
    {
        if (statement is PrintStatement p)
        {
            Console.WriteLine(Evaluate(p.Value));
        }
        else if (statement is IfStatement i)
        {
            if ((bool)Evaluate(i.Condition))
            {
                foreach (var stmt in i.ThenBranch) Execute(stmt);
                return;
            }

            foreach (var elif in i.ElifBranches)
                if ((bool)Evaluate(elif.Condition))
                {
                    foreach (var stmt in elif.Body) Execute(stmt);
                    return;
                }

            if (i.ElseBranch != null)
                foreach (var stmt in i.ElseBranch)
                    Execute(stmt);
        }
        else if (statement is WhileStatement w)
        {
            while ((bool)Evaluate(w.Condition))
                foreach (var stmt in w.Body)
                    Execute(stmt);
        }

        if (statement is ArrayDeclaration arr)
        {
            var array = new ArrayValue(arr.Dimensions[0], arr.Dimensions[1]);

            variables[arr.Name] = new Value(
                array,
                ValueType.Array,
                false, // or arr.IsConst if your AST supports it
                false
            );
        }
        else if (statement is VarDeclaration v)
        {
            object? value = Evaluate(v.Value);
            ValueType type = ParseValueType(v.Type);

            if (!TypeMatches(type, value))
            {
                throw new Exception(
                    $"Cannot assign value of type '{value?.GetType().Name ?? "null"}' to {type}."
                );
            }

            variables[v.Name] = new Value(value, type, v.IsConst, false);
        }

        if (statement is ArrayAssignment a)
        {
            var value = variables[a.Target.Name];

            if (value.Type != ValueType.Array) throw new Exception($"{a.Target.Name} is not an array.");
            var array = (ArrayValue)value.Data;

            var x = Convert.ToInt32(Evaluate(a.Target.X));
            var y = Convert.ToInt32(Evaluate(a.Target.Y));

            array.Data[array.Index(x, y)] = Evaluate(a.Value);
        }
        else if (statement is CallStatement c)
        {
            Evaluate(c.Call);
        }
        else if (statement is FunctionDeclaration f)
        {
            functions[f.Name] = f;
        }
        else if (statement is AssignmentStatement assign)
        {
            if (!variables.TryGetValue(assign.Name, out var variable))
                throw new Exception($"Unknown variable '{assign.Name}'.");
            if (variable.IsConst) throw new Exception($"Cannot modify constant '{assign.Name}'.");
            variable.Data = Evaluate(assign.Value);
            variable.WasModified = true;
        }
        else if (statement is ReturnStatement r)
        {
            object? value = null;
            if (r.Value != null) value = Evaluate(r.Value);
            throw new ReturnException(value);
        }
    }

    private object? Evaluate(Expression expr)
    {
        if (expr is LiteralExpression l) return l.Value;
        if (expr is VariableExpression v) return variables[v.Name].Data;
        if (expr is ArrayAccess a)
        {
            var value = variables[a.Name];

            if (value.Type != ValueType.Array) throw new Exception($"{a.Name} is not an array.");
            var array = (ArrayValue)value.Data;

            var x = Convert.ToInt32(Evaluate(a.X));
            var y = Convert.ToInt32(Evaluate(a.Y));

            return array.Data[array.Index(x, y)];
        }

        if (expr is BinaryExpression b)
        {
            Console.WriteLine(b.Left.GetType().Name);
            Console.WriteLine(Evaluate(b.Left) ?? "NULL");

            // Problem here
            var left = (int)Evaluate(b.Left);
            var right = (int)Evaluate(b.Right);

            return b.Operator switch
            {
                "+" => left + right,
                "-" => left - right,
                "<" => left < right,
                ">" => left > right,
                "<=" => left <= right,
                ">=" => left >= right,
                "==" => left == right,
                "!=" => left != right,
                _ => throw new Exception($"Unknown operator {b.Operator}")
            };
        }

        if (expr is CallExpression call)
        {
            var function = functions[call.Name];
            try
            {
                foreach (var stmt in function.Body) Execute(stmt);
            }
            catch (ReturnException ret)
            {
                if (function.ReturnType == "void")
                {
                    if (ret.Value != null)
                        throw new Exception($"Void function '{function.Name}' cannot return a value.");
                    return null;
                }

                if (ret.Value == null) throw new Exception($"Function '{function.Name}' must return a value.");
                if (!TypeMatches(ParseValueType(function.ReturnType), ret.Value))
                {
                    throw new Exception($"Function '{function.Name}' returns '{ret.Value?.GetType().Name}' but expected '{function.ReturnType}'");
                }
                return ret.Value;
            }

            if (function.ReturnType != "void") throw new Exception($"Function '{function.Name}' must return a value.");
            return null;
        }
        throw new Exception("Unknown expression type");
    }
}