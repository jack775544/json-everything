using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="InRule"/>
/// </summary>
public class InRuleExpression : RuleExpression<InRule>
{
	private static readonly MethodInfo _stringContainsMethod = typeof(string)
		.GetMethods()
		.Where(x => x.Name == nameof(string.Contains))
		.Single(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(string));

	private static readonly MethodInfo _enumerableContainsMethod = typeof(Enumerable)
		.GetMethods()
		.Where(x => x.Name == nameof(Enumerable.Contains))
		.Single(x => x.GetParameters().Length == 2 
			&& x.IsGenericMethod
			&& x.GetParameters()[0].ParameterType.ContainsGenericParameters
			&& x.GetGenericArguments()[0] == x.GetParameters()[0].ParameterType.GetGenericArguments()[0]
			&& x.GetGenericArguments()[0] == x.GetParameters()[1].ParameterType);

	/// <inheritdoc />
	public override Expression CreateExpression(InRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		// Check if the value is a string
		var valueString = ExpressionTypeUtilities.Downcast(new [] { registry.CreateExpressionInternal(rule.Value, parameter, options) }, typeof(string)).First();

		// If passed a string then use the "".Contains method
		if (valueString.Type == typeof(string))
		{
			return Expression.Call(
				valueString,
				_stringContainsMethod,
				ExpressionTypeUtilities.Downcast(new [] { registry.CreateExpressionInternal(rule.Test, parameter, options) }, typeof(string)).First().Stringify());
		}

		var value = registry.CreateExpressionInternal(rule.Value, parameter, options);
		var test = registry.CreateExpressionInternal(rule.Test, parameter, options);

		if (value.Type == typeof(object))
		{
			return Expression.Constant(false);
		}

		if (!LogicTypeExtensions.TryGetGenericCollectionType(value.Type, out var collectionType))
		{
			throw new JsonLogicException($"Value parameter for In rule must be enumerable, found type {value.Type.FullName} instead");
		}

		var valueHasParam = ParameterFinder.ContainsParameter(value);
		var testHasParam = ParameterFinder.ContainsParameter(test);

		Type? desiredType;
		if (valueHasParam)
		{
			desiredType = collectionType;
		}
		else if (testHasParam)
		{
			desiredType = test.Type;
		}
		else
		{
			desiredType = null;
		}

		// Otherwise the value should be an array instead
		var args = ExpressionTypeUtilities.Downcast(new[] { value, test }, desiredType);

		if (!LogicTypeExtensions.TryGetGenericCollectionType(args[0].Type, out var convertedCollectionType))
		{
			// This should not really be possible, but check just in case.
			throw new JsonLogicException($"Converted value parameter for In rule must be enumerable, found type {args[0].Type.FullName} instead");
		}

		return Expression.Call(
			_enumerableContainsMethod.MakeGenericMethod(convertedCollectionType),
			args[0],
			args[1]);
	}
}
