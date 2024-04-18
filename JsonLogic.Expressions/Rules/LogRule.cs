using System;
using System.Linq.Expressions;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="LogRule"/>
/// </summary>
public class LogRuleExpression : RuleExpression<LogRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(LogRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		throw new NotImplementedException("Log method not implemented");
	}
}
