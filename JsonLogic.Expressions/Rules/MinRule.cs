using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="MinRule"/>
/// </summary>
public class MinRuleExpression : RuleExpression<MinRule>
{
	private static readonly MethodInfo _minMethod = ((Func<decimal, decimal, decimal>)Math.Min).Method;

	/// <inheritdoc />
	public override Expression CreateExpression(MinRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		return ExpressionExtensions.EvaluateItems(rule.Items, registry, parameter, options)
			.DowncastNumber()
			.Select(x => x.Numberify(options))
			.Aggregate((a, c) => Expression.Call(_minMethod, a, c));
	}
}
