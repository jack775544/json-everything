using System.Linq.Expressions;
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
		return registry.CreateExpressionInternal(
			new LooseEqualsRule(rule.A, rule.B),
			parameter,
			options);
	}
}
