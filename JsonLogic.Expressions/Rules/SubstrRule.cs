﻿using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="SubstrRule"/>
/// </summary>
public class SubstrRuleExpression : RuleExpression<SubstrRule>
{
	private static readonly MethodInfo _substringMethod = typeof(string).GetMethod("Substring", [typeof(int)])!;
	private static readonly MethodInfo _substring2Method = typeof(string).GetMethod("Substring", [typeof(int), typeof(int)])!;

	/// <inheritdoc />
	public override Expression CreateExpression(SubstrRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var str = ExpressionTypeUtilities.Downcast(new [] { registry.CreateExpressionInternal(rule.Input, parameter, options) }).First().Stringify();
		var start = ExpressionTypeUtilities.Downcast(new[] { registry.CreateExpressionInternal(rule.Start, parameter, options) }, typeof(int)).First();
		var count = rule.Count != null
			? ExpressionTypeUtilities.Downcast(new[] { registry.CreateExpressionInternal(rule.Count, parameter, options) }, typeof(int)).First()
			: null;
		return count == null
			? Expression.Call(str, _substringMethod, start)
			: Expression.Call(str, _substring2Method, start, count);
	}
}
