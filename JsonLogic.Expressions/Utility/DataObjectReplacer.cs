using System;
using System.Linq.Expressions;

namespace Json.Logic.Expressions.Utility;

internal class DataObjectReplacer(Type? desiredType, bool nullable, DataObject? nullCast = null) : ExpressionVisitor
{
	protected override Expression VisitConstant(ConstantExpression node)
	{
		if (node.Value == null)
		{
			if (node.Type == typeof(object) && desiredType != null && desiredType.IsGenericType && desiredType.GetGenericTypeDefinition() == typeof(Nullable<>))
			{
				node = Expression.Constant(nullCast, desiredType);
			}
		}

		// All data objects will be found in constants
		if (!typeof(DataObject).IsAssignableFrom(node.Type))
		{
			return node;
		}

		if (node.Value == null)
		{
			return Expression.Constant(null);
		}

		var dataObject = (DataObject)node.Value;

		return desiredType != null
			? ExpressionTypeUtilities.DataObjectToExpression(dataObject, nullable, desiredType)
			: ExpressionTypeUtilities.DataObjectToExpression(dataObject, nullable, dataObject.Field?.GetType() ?? typeof(object));
	}

	protected override Expression VisitConditional(ConditionalExpression node)
	{
		var test = Visit(node.Test);
		var ifTrue = Visit(node.IfTrue);
		var ifFalse = Visit(node.IfFalse);

		// Can suppress null warning since we know that our visitor will not give back nulls
		return Expression.Condition(test, ifTrue, ifFalse);
	}
}