using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="RuleCollection"/>
/// </summary>
public class RuleCollectionExpression : RuleExpression<RuleCollection>
{
	/// <inheritdoc />
	public override Expression CreateExpression(RuleCollection rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		return registry.CreateExpressionInternal(
			new LiteralRule(rule.Source),
			parameter,
			options);
	}
}
