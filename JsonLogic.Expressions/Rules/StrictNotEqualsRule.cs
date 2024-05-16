using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="StrictNotEqualsRule"/>
/// </summary>
public class StrictNotEqualsRuleExpression : RuleExpression<StrictNotEqualsRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(StrictNotEqualsRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var args = new[]
		{
			registry.CreateExpressionInternal(rule.A, parameter, options),
			registry.CreateExpressionInternal(rule.B, parameter, options)
		}.Downcast();
		return Expression.NotEqual(args[0], args[1]);
	}
}
