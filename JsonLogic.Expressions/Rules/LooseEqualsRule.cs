using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="LooseEqualsRule"/>
/// </summary>
public class LooseEqualsRuleExpression : RuleExpression<LooseEqualsRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(LooseEqualsRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var args = new[]
		{
			registry.CreateExpression(rule.A, parameter, options),
			registry.CreateExpression(rule.B, parameter, options),
		}.Downcast();
		return Expression.Equal(args[0], args[1]);
	}
}
