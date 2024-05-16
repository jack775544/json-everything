using System.Linq;
using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="BooleanCastRule"/>
/// </summary>
public class BooleanCastRuleExpression : RuleExpression<BooleanCastRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(BooleanCastRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var expression = registry.CreateExpressionInternal(rule.Value, parameter, options);
		return new [] { expression }.Downcast().First().IsTruthy();
	}
}
