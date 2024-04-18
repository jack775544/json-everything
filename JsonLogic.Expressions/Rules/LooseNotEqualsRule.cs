﻿using System.Linq.Expressions;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="LooseNotEqualsRule"/>
/// </summary>
public class LooseNotEqualsRuleExpression : RuleExpression<LooseNotEqualsRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(LooseNotEqualsRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		return Expression.NotEqual(registry.CreateExpression(rule.A, parameter, options), registry.CreateExpression(rule.B, parameter, options));
	}
}
