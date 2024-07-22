using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;
using Json.More;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="MapRule"/>
/// </summary>
public class MapRuleExpression : RuleExpression<MapRule>
{
	private static readonly MethodInfo _selectMethod = typeof(Enumerable)
		.GetMethods()
		.Where(x => x.Name == "Select")
		.Single(x => x.GetParameters().Last().ParameterType.GetGenericArguments().Length == 2);

	/// <inheritdoc />
	public override Expression CreateExpression(MapRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var input = registry.CreateExpressionInternal(rule.Input, parameter, options);

		if (!LogicTypeExtensions.TryGetGenericCollectionType(input.Type, out var type))
		{
			throw new JsonLogicException("Non collection passed when the expecting collection in none rule");
		}

		var param = Expression.Parameter(type, type.Name);
		var body = registry.CreateExpressionInternal(rule.Rule, param, options);
		var args = ExpressionTypeUtilities.Downcast(new[] { input }, type);
		return Expression.Call(
			_selectMethod.MakeGenericMethod(type, body.Type),
			args[0],
			Expression.Lambda(body, param));
	}
}
