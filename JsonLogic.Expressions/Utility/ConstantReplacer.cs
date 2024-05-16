using System.Linq;
using System.Linq.Expressions;

namespace Json.Logic.Expressions.Utility;

internal class ConstantReplacer : ExpressionVisitor
{
	private class Box<T>(T field)
	{
		public T Field { get; set; } = field;
	}

	protected override Expression VisitConstant(ConstantExpression node)
	{
		var box = typeof(Box<>)
			.MakeGenericType(node.Type)
			.GetConstructors()
			.First()
			.Invoke([node.Value]);

		return Expression.PropertyOrField(Expression.Constant(box), nameof(Box<object>.Field));
	}

	public static Expression Replace(Expression expression)
	{
		return new ConstantReplacer().Visit(expression)!;
	}
}