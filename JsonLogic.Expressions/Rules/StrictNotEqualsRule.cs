using System.Linq.Expressions;
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
		return registry.CreateExpressionInternal(
			new LooseNotEqualsRule(rule.A, rule.B),
			parameter,
			options);
	}
}
