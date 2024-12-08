﻿using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="SomeRule"/>
/// </summary>
public class SomeRuleExpression : RuleExpression<SomeRule>
{
	private static readonly MethodInfo _anyMethod = typeof(Enumerable)
		.GetMethods()
		.Where(x => x.Name == nameof(Enumerable.Any))
		.Single(x => x.GetParameters().Length == 2);

	/// <inheritdoc />
	public override Expression CreateExpression(SomeRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var input = registry.CreateExpressionInternal(rule.Input, parameter, options);

		if (!LogicTypeExtensions.TryGetGenericCollectionType(input.Type, out var type))
		{
			throw new JsonLogicException("Non collection passed when the expecting collection in some rule");
		}

		var param = Expression.Parameter(type, type.Name);

		var body = registry.CreateExpressionInternal(rule.Rule, param, options);
		var normalisedBody = ExpressionTypeUtilities.Downcast([body], typeof(bool)).First();
		var args = ExpressionTypeUtilities.Downcast([input], type);
		return Expression.Call(
			_anyMethod.MakeGenericMethod(type),
			args[0],
			Expression.Lambda(normalisedBody, param));
	}
}
