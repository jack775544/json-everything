using System.Linq;
using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="MultiplyRule"/>
/// </summary>
public class MultiplyRuleExpression : RuleExpression<MultiplyRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(MultiplyRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var items = ExpressionExtensions.EvaluateItems(rule.Items, registry, parameter, options)
			.DowncastNumber()
			.Select(x => x.Numberify(options))
			.ToList();
		return items.Count == 1 ? items[0] : items.Aggregate(Expression.MultiplyChecked);
	}
}
