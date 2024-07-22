using System.Linq;
using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="AddRule"/>
/// </summary>
public class AddRuleExpression : RuleExpression<AddRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(AddRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var items = ExpressionTypeUtilities.Downcast(ExpressionUtilities.EvaluateItems(rule.Items, registry, parameter, options))
			.Select(x => x.Numberify(0, options))
			.ToList();

		return items.Count switch
		{
			0 => ExpressionUtilities.CreateConstant(0, true, options),
			1 => items[0].Numberify(options),
			_ => items.Aggregate(Expression.AddChecked),
		};
	}
}
