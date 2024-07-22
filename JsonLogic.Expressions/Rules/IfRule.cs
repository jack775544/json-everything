using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="IfRule"/>
/// </summary>
public class IfRuleExpression : RuleExpression<IfRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(IfRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var components = ExpressionUtilities.EvaluateItems(rule.Components, registry, parameter, options).ToList();
		return ExpressionTypeUtilities.Downcast(new [] { CreateRuleRecursive(components) }).First();
	}
	
	private static Expression CreateRuleRecursive(List<Expression> components)
	{
		if (components.Count <= 3)
		{
			return CreateRuleExpression(components);
		}

		return Expression.Condition(
			ExpressionTypeUtilities.Downcast(new [] { components[0] }, typeof(bool)).First(),
			components[1],
			CreateRuleRecursive(components.Skip(2).ToList()));
	}

	private static Expression CreateRuleExpression(List<Expression> components) => components.Count switch
	{
		0 => Expression.Constant(null),
		1 => ExpressionTypeUtilities.Downcast(new [] { components[0] }, typeof(bool)).First(),
		2 => Expression.Condition(ExpressionTypeUtilities.Downcast(new [] { components[0] }, typeof(bool)).First(), components[1], Expression.Constant(null)),
		3 => Expression.Condition(ExpressionTypeUtilities.Downcast(new [] { components[0] }, typeof(bool)).First(), components[1], components[2]),
		_ => throw new InvalidOperationException("Invalid number of arguments for if statement, expected 0, 1, 2 or 3 parameters")
	};
}

