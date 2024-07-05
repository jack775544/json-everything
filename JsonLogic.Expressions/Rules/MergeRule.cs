using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="MergeRule"/>
/// </summary>
public class MergeRuleExpression : RuleExpression<MergeRule>
{
	private static readonly MethodInfo _selectManyMethod = typeof(Enumerable)
		.GetMethods()
		.Where(x => x.Name == nameof(Enumerable.SelectMany))
		.Single(x => x.GetParameters().Last().ParameterType.GetGenericArguments().Length == 2);

	/// <inheritdoc />
	public override Expression CreateExpression(MergeRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var items = ExpressionUtilities.EvaluateItems(rule.Items, registry, parameter, options).Downcast().ToList();

		var arrayItems = new List<Expression>(items.Count);
		Type? genericType = null;
		foreach (var item in items)
		{
			if (item.Type.TryGetGenericCollectionType(out var type))
			{
				genericType ??= type;

				if (genericType != type)
				{
					throw new InvalidOperationException("Mismatched array types");
				}

				arrayItems.Add(item);
			}
			else
			{
				genericType ??= item.Type;

				if (genericType != item.Type)
				{
					throw new InvalidOperationException("Mismatched array types");
				}

				arrayItems.Add(Expression.NewArrayInit(item.Type, item));
				
			}
		}

		if (genericType == null)
		{
			return Expression.NewArrayInit(typeof(object));
		}

		var genericEnumerable = typeof(IEnumerable<>).MakeGenericType(genericType);
		var param = Expression.Parameter(genericEnumerable, genericType.Name);

		return Expression.Call(
			_selectManyMethod.MakeGenericMethod(genericEnumerable, genericType),
			Expression.NewArrayInit(genericEnumerable, arrayItems),
			Expression.Lambda(param, param));
	}
}
