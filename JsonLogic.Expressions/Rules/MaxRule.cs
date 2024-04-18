using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="MaxRule"/>
/// </summary>
public class MaxRuleExpression : RuleExpression<MaxRule>
{
	private static readonly MethodInfo _maxMethod = ((Func<decimal, decimal, decimal>)Math.Max).Method;

	/// <inheritdoc />
	public override Expression CreateExpression(MaxRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		return ExpressionExtensions.EvaluateItems(rule.Items, registry, parameter, options)
			.Downcast()
			.Select(x => x.Numberify(options))
			.Aggregate((a, c) => Expression.Call(_maxMethod, a, c));
	}
}
