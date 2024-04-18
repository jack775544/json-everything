using System;
using System.Linq.Expressions;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="MissingSomeRule"/>
/// </summary>
public class MissingSomeRuleExpression : RuleExpression<MissingSomeRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(MissingSomeRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		throw new NotImplementedException("Missing method not implemented");
	}
}
