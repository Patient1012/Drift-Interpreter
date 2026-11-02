public class CallStatement  : Statement
{
    public CallExpression Call;

    public CallStatement(CallExpression call)
    {
        Call = call;
    }
}