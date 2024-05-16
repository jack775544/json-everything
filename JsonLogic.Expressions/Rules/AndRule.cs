using System.Linq;
using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="AndRule"/>
/// </summary>
public class AndRuleExpression : RuleExpression<AndRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(AndRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var expressions = ExpressionUtilities.EvaluateItems(rule.Items, registry, parameter, options).Downcast(typeof(bool));
		return expressions.Count switch
		{
			0 => ExpressionUtilities.CreateConstant(true, false, options),
			1 => expressions[0],
			_ => expressions.Aggregate(Expression.AndAlso)
		};
	}
}
