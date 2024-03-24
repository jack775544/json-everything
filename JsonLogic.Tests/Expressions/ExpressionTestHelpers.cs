using System;
using System.Linq.Expressions;

namespace Json.Logic.Tests.Expressions;

public class ExpressionTestHelpers
{
	public static Expression<Func<object?, TReturn>> CreateRuleExpression<TReturn>(Rule rule, CreateExpressionOptions? options = null)
	{
		var parameter = Expression.Parameter(typeof(object), "arg");
		var expression = rule.CreateExpression(parameter, options ?? new CreateExpressionOptions());
		if (expression.Type == typeof(DataObject))
		{
			expression = Expression.PropertyOrField(expression, nameof(DataObject.Field));
		}
		return Expression.Lambda<Func<object?, TReturn>>(expression, parameter);
	}

	public static Expression<Func<TParam, TReturn>> CreateRuleExpression<TParam, TReturn>(Rule rule, CreateExpressionOptions? options = null)
	{
		var parameter = Expression.Parameter(typeof(TParam), "arg");
		var expression = rule.CreateExpression(parameter, options ?? new CreateExpressionOptions());
		if (expression.Type == typeof(DataObject))
		{
			expression = Expression.PropertyOrField(expression, nameof(DataObject.Field));
		}
		return Expression.Lambda<Func<TParam, TReturn>>(expression, parameter);
	}
}