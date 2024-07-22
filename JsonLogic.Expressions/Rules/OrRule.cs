using System.Linq;
using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="OrRule"/>
/// </summary>
public class OrRuleExpression : RuleExpression<OrRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(OrRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var args = ExpressionTypeUtilities.Downcast(ExpressionUtilities.EvaluateItems(rule.Items, registry, parameter, options));
		return args.Count switch
		{
			0 => Expression.Constant(false),
			1 => args[0],
			_ => args.Aggregate(Expression.OrElse),
		};
	}
}
