using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="DivideRule"/>
/// </summary>
public class DivideRuleExpression : RuleExpression<DivideRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(DivideRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var args = new[]
		{
			registry.CreateExpression(rule.A, parameter, options),
			registry.CreateExpression(rule.B, parameter, options),
		}.Downcast();
		return Expression.Divide(args[0], args[1]);
	}
}
