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
		var a = registry.CreateExpressionInternal(rule.A, parameter, options);
		var b = registry.CreateExpressionInternal(rule.B, parameter, options);
		var args = ExpressionTypeUtilities.DowncastComparable(new[] { a, b });
		return Expression.GreaterThanOrEqual(args[0], args[1]);
	}
}
