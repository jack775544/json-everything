﻿using System;
using System.Linq.Expressions;

namespace Json.Logic.Expressions.Utility;

internal class DataObjectReplacer(Type? desiredType, DataObject? nullCast = null) : ExpressionVisitor
{
	protected override Expression VisitConstant(ConstantExpression node)
	{
		if (node.Value == null)
		{
			node = Expression.Constant(nullCast);
		}

		// All data objects will be found in constants
		if (node.Type != typeof(DataObject))
		{
			return node;
		}

		if (node.Value == null)
		{
			return Expression.Constant(null);
		}

		var dataObject = (DataObject)node.Value;

		return desiredType != null
			? ExpressionTypeUtilities.DataObjectToExpression(dataObject, desiredType)
			: ExpressionTypeUtilities.DataObjectToExpression(dataObject, dataObject.Field?.GetType() ?? typeof(object));
	}

	protected override Expression VisitConditional(ConditionalExpression node)
	{
		var test = Visit(node.Test);
		var ifTrue = Visit(node.IfTrue);
		var ifFalse = Visit(node.IfFalse);

		// Can suppress null warning since we know that our visitor will not give back nulls
		return Expression.Condition(test!, ifTrue!, ifFalse!);
	}
}