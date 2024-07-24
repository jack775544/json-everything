﻿using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="LessThanRule"/>
/// </summary>
public class LessThanRuleExpression : RuleExpression<LessThanRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(LessThanRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var a = registry.CreateExpressionInternal(rule.A, parameter, options);
		var b = registry.CreateExpressionInternal(rule.B, parameter, options);

		if (rule.C == null)
		{
			var args = ExpressionTypeUtilities.DowncastComparable(new[] { a, b });
			return Expression.LessThan(args[0], args[1]);
		}

		var c = registry.CreateExpressionInternal(rule.C, parameter, options);

		var argsWithC = ExpressionTypeUtilities.DowncastComparable(new[] { a, b, c });
		return Expression.AndAlso(
			Expression.LessThan(argsWithC[0], argsWithC[1]),
			Expression.LessThan(argsWithC[1], argsWithC[2]));
	}
}