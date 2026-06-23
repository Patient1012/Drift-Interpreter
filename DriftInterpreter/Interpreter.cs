public class Interpreter
{
  
    private Dictionary<string, FunctionDeclaration> functions = new();
    Dictionary<string, Value> variables = new();

    public void Run(List<Statement> statements)
    {
        foreach (var stmt in statements) Execute(stmt);
        foreach (var pair in variables)
            if (!pair.Value.IsConst && !pair.Value.WasModified)
                throw new Exception($"Warning: Variable '{pair.Key}' was never modified.");
    }

    private bool TypeMatches(string expectedType, object value)
    {
        return expectedType switch
        {
            "i8" or "i16" or "i32" or "i64" => value is int,
            "f16" or "f32" or "f64" => value is float or double,
            "str" => value is string,
            "bool" => value is bool,
            "void" => value == null,
            _ => false
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
                foreach (var stmt in i.ThenBranch)
                    Execute(stmt);
                return;
            }

            foreach (var elif in i.ElifBranches)
                if ((bool)Evaluate(elif.Condition))
                {
                    foreach (var stmt in elif.Body)
                        Execute(stmt);
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
            variables[arr.Name] = new ArrayValue(arr.Width, arr.Height);
        }
        else if (statement is VarDeclaration v)
        {
            variables[v.Name] = new Value(Type=ValueType.Array,v.Type, Evaluate(v.Value), v.IsConst);
        }
        if (statement is ArrayAssignment a)
        {
            var array = (ArrayValue)variables[a.Target.Name];

            int x = Convert.ToInt32(Evaluate(a.Target.X));
            int y = Convert.ToInt32(Evaluate(a.Target.Y));

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
        else if (statement is ReturnStatement r)
        {
            object? value = null;

            if (r.Value != null) value = Evaluate(r.Value);

            throw new ReturnException(value);
        }
    }

    private object Evaluate(Expression expr)
    {
        if (expr is LiteralExpression l)
            return l.Value;

        if (expr is VariableExpression v)
            return variables[v.Name].Value;
        
        if (expr is ArrayAccess a)
        {
            var array = (ArrayValue)variables[a.Name];

            int x = Convert.ToInt32(Evaluate(a.X));
            int y = Convert.ToInt32(Evaluate(a.Y));

            return array.Data[array.Index(x, y)];
        }

        if (expr is BinaryExpression b)
        {
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
                if (!TypeMatches(function.ReturnType, ret.Value))
                    throw new Exception(
                        $"Function '{function.Name}' returns '{ret.Value?.GetType().Name}' but expected '{function.ReturnType}'");

                return ret.Value;
            }

            if (function.ReturnType != "void") throw new Exception($"Function '{function.Name}' must return a value.");

            return null;
        }

        throw new Exception("Unknown expression type");
    }
}