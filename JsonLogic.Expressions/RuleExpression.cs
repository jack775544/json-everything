using System.Linq.Expressions;

namespace Json.Logic.Expressions;

/// <summary>
/// 
/// </summary>
/// <typeparam name="TRule"></typeparam>
public interface IRuleExpression
{
	/// <summary>
	/// Creates an expression predicate for the rule.
	/// </summary>
	/// <param name="rule">The rule to create the expression for.</param>
	/// <param name="registry">The registry used to create this rule.</param>
	/// <param name="parameter">An expression to be used as the JSON Logic data.</param>
	/// <param name="options">Options for creating the expression.</param>
	/// <returns>An expression body that represents the logic rule.</returns>
	Expression CreateRuleExpression(Rule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options);
}

/// <summary>
/// 
/// </summary>
/// <typeparam name="TRule"></typeparam>
public abstract class RuleExpression<TRule> : IRuleExpression
	where TRule : Rule
{
	/// <summary>
	/// Creates an expression predicate for the rule.
	/// </summary>
	/// <param name="rule">The rule to create the expression for.</param>
	/// <param name="registry">The registry used to create this rule.</param>
	/// <param name="parameter">An expression to be used as the JSON Logic data.</param>
	/// <param name="options">Options for creating the expression.</param>
	/// <returns>An expression body that represents the logic rule.</returns>
	public abstract Expression CreateExpression(TRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options);

	/// <inheritdoc />
	public Expression CreateRuleExpression(Rule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		return CreateExpression((TRule)rule, registry, parameter, options);
	}
}