using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="InRule"/>
/// </summary>
public class LessThanEqualRuleExpression : RuleExpression<LessThanEqualRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(LessThanEqualRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var a = registry.CreateExpressionInternal(rule.A, parameter, options);
		var b = registry.CreateExpressionInternal(rule.B, parameter, options);

		if (rule.C == null)
		{
			var args = new[] { a, b }.DowncastComparable();
			return Expression.LessThanOrEqual(args[0], args[1]);
		}

		var c = registry.CreateExpressionInternal(rule.C, parameter, options);

		var argsWithC = new[] { a, b, c }.DowncastComparable();
		return Expression.AndAlso(
			Expression.LessThanOrEqual(argsWithC[0], argsWithC[1]),
			Expression.LessThanOrEqual(argsWithC[1], argsWithC[2]));
	}
}