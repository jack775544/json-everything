using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="MoreThanEqualRule"/>
/// </summary>
public class MoreThanEqualRuleExpression : RuleExpression<MoreThanEqualRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(MoreThanEqualRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var a = registry.CreateExpression(rule.A, parameter, options);
		var b = registry.CreateExpression(rule.B, parameter, options);
		var args = new[] { a, b }.DowncastComparable();
		return Expression.GreaterThanOrEqual(args[0], args[1]);
	}
}
