using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="StrictEqualsRule"/>
/// </summary>
public class StrictEqualsRuleExpression : RuleExpression<StrictEqualsRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(StrictEqualsRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var args = new[]
		{
			registry.CreateExpressionInternal(rule.A, parameter, options),
			registry.CreateExpressionInternal(rule.B, parameter, options)
		}.Downcast();
		return Expression.Equal(args[0], args[1]);
	}
}
