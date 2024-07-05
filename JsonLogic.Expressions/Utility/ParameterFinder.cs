using System.Linq.Expressions;

namespace Json.Logic.Expressions.Utility;

internal class ParameterFinder : ExpressionVisitor
{
	private bool _foundParam;

	private ParameterFinder() { }

	protected override Expression VisitParameter(ParameterExpression node)
	{
		_foundParam = true;
		return base.VisitParameter(node);
	}

	public static bool ContainsParameter(Expression node)
	{
		var finder = new ParameterFinder();
		finder.Visit(node);
		return finder._foundParam;
	}
}