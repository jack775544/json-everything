using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="MoreThanRule"/>
/// </summary>
public class MoreThanRuleExpression : RuleExpression<MoreThanRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(MoreThanRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var a = registry.CreateExpressionInternal(rule.A, parameter, options);
		var b = registry.CreateExpressionInternal(rule.B, parameter, options);
		var args = new[] { a, b }.DowncastComparable();
		return Expression.GreaterThan(args[0], args[1]);
	}
}
