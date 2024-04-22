using System.Linq;
using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="SubtractRule"/>
/// </summary>
public class SubtractRuleExpression : RuleExpression<SubtractRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(SubtractRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var items = ExpressionUtilities.EvaluateItems(rule.Items, registry, parameter, options)
			.Downcast()
			.Select(x => x.Numberify(0, options))
			.ToList();

		return items.Count switch
		{
			0 => ExpressionUtilities.CreateConstant(0, true, options),
			1 => Expression.NegateChecked(items[0].Numberify(options)),
			_ => items.Aggregate(Expression.SubtractChecked),
		};
	}
}
