public class Interpreter
{
    private Dictionary<string, Variable> variables = new();
    private Dictionary<string, FunctionDeclaration> functions = new();

    public void Run(List<Statement> statements)
    {
        foreach (var stmt in statements)
        {
            Execute(stmt);
        }
        foreach (var pair in variables)
        {
            if (!pair.Value.IsConst && !pair.Value.WasModified)
            {
                throw new Exception($"Warning: Variable '{pair.Key}' was never modified.");
            }
        }
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
            {
                if ((bool)Evaluate(elif.Condition))
                {
                    foreach (var stmt in elif.Body)
                        Execute(stmt);
                    return;
                }
            }

            if (i.ElseBranch != null)
            {
                foreach (var stmt in i.ElseBranch)
                    Execute(stmt);
            }
        }
        else if (statement is WhileStatement w)
        {
            while ((bool)Evaluate(w.Condition))
            {
                foreach (var stmt in w.Body)
                    Execute(stmt);
            }
        }
        else if (statement is VarDeclaration v)
        {
            variables[v.Name] = new Variable(Evaluate(v.Value), v.IsConst);

        }
        else if (statement is FunctionDeclaration f)
        {
            functions[f.Name] = f;
        }
        else if (statement is ReturnStatement r)
        {
            throw new ReturnException(Evaluate(r.Value));
        }
    }

    private object Evaluate(Expression expr)
    {
        if (expr is LiteralExpression l)
            return l.Value;

        if (expr is VariableExpression v)
            return variables[v.Name].Value;

        if (expr is BinaryExpression b)
        {
            int left = (int)Evaluate(b.Left);
            int right = (int)Evaluate(b.Right);

            return b.Operator switch
            {
                "+" => left + right,
                "-" => left - right,
                "<"  => left < right,
                ">"  => left > right,
                "<=" => left <= right,
                ">=" => left >= right,
                "==" => left == right,
                "!=" => left != right,
                _ => throw new Exception($"Unknown operator {b.Operator}")
            };
        }
        if (expr is CallExpression c)
        {
            FunctionDeclaration function = functions[c.Name];

            var oldVariables =
                new Dictionary<string, Variable>(variables);

            for (int i = 0; i < function.Parameters.Count; i++)
            {
                variables[function.Parameters[i]] =
                    new Variable(Evaluate(c.Arguments[i]), false);
            }
            try
            {
                foreach (var stmt in function.Body)
                {
                    Execute(stmt);
                }
            }
            catch (ReturnException ret)
            {
                variables = oldVariables;
                return ret.Value;
            }

            variables = oldVariables;

            return null;
        }

        throw new Exception("Unknown expression type");
    }
}