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
		var args = ExpressionTypeUtilities.Downcast(new[]
		{
			registry.CreateExpressionInternal(rule.A, parameter, options),
			registry.CreateExpressionInternal(rule.B, parameter, options),
		});
		return Expression.Divide(args[0], args[1]);
	}
}
