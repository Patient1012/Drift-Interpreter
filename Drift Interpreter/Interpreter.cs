public class Interpreter
{
    private Dictionary<string, Variable> variables = new();

    public void Run(List<Statement> statements)
    {
        foreach (var stmt in statements)
        {
            Execute(stmt);
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

        throw new Exception("Unknown expression type");
    }
}