using System;
using System.Linq.Expressions;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="MissingRule"/>
/// </summary>
public class MissingRuleExpression : RuleExpression<MissingRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(MissingRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		throw new NotImplementedException("Missing method not implemented");
	}
}
