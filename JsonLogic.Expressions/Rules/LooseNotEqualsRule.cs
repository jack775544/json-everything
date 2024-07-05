using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="LooseNotEqualsRule"/>
/// </summary>
public class LooseNotEqualsRuleExpression : RuleExpression<LooseNotEqualsRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(LooseNotEqualsRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var args = new[]
		{
			registry.CreateExpressionInternal(rule.A, parameter, options),
			registry.CreateExpressionInternal(rule.B, parameter, options)
		}.Downcast();

		try
		{
			return Expression.NotEqual(args[0], args[1]);
		}
		catch
		{
			return Expression.Constant(true);
		}
	}
}
