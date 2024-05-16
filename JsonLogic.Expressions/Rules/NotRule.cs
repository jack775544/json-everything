using System.Linq;
using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="NotRule"/>
/// </summary>
public class NotRuleExpression : RuleExpression<NotRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(NotRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		return Expression.Not(new [] { registry.CreateExpressionInternal(rule.Value, parameter, options) }.Downcast().First().IsTruthy());
	}
}
