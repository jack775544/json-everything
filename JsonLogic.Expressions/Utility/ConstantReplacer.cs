using System;
using System.Linq;
using System.Linq.Expressions;

namespace Json.Logic.Expressions.Utility;

internal class ConstantReplacer(Func<Expression, Expression>? replacer = null) : ExpressionVisitor
{
	private class Box<T>(T field)
	{
		public T Field { get; set; } = field;
	}

	protected override Expression VisitConstant(ConstantExpression node)
	{
		if (replacer != null)
		{
			return replacer(node);
		}

		var box = typeof(Box<>)
			.MakeGenericType(node.Type)
			.GetConstructors()
			.First()
			.Invoke([node.Value]);

		return Expression.PropertyOrField(Expression.Constant(box), nameof(Box<object>.Field));
	}

	public static Expression Replace(Expression expression, Func<Expression, Expression>? replacer = null)
	{
		return new ConstantReplacer(replacer).Visit(expression)!;
	}
}