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
		return ExpressionExtensions.EvaluateItems(rule.Items, registry, parameter, options)
			.Downcast()
			.Aggregate((Expression)Expression.Constant(false), Expression.OrElse);
	}
}
