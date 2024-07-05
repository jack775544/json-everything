using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="ReduceRule"/>
/// </summary>
public class ReduceRuleExpression : RuleExpression<ReduceRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(ReduceRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var input = registry.CreateExpressionInternal(rule.Input, parameter, options);
		var initial = new [] { registry.CreateExpressionInternal(rule.Initial, parameter, options) }.Downcast().First();

		if (!input.Type.TryGetGenericCollectionType(out var collectionType))
		{
			throw new JsonLogicException("Non collection passed when the expecting collection in reduce rule");
		}

		// The algorithm here is a little complex. The JsonLogic function is expecting a parameter object in the shape of
		// { current, accumulator }, however the C# aggregate method passes 2 args to represent those instead.
		// So we call provided rule expression with a class of that shape. Then we use an expression visitor to replace
		// instances where that class was accessed with the parameter expressions needed for C# aggregate instead.
		var stateType = typeof(ReduceState<,>).MakeGenericType(collectionType, initial.Type);
		var param = Expression.Parameter(stateType, "reduceState");
		var accumulatorParam = Expression.Parameter(initial.Type, "accumulator");
		var currentParam = Expression.Parameter(collectionType, "current");

		var body = PropertyReplacer.Replace(
			registry.CreateExpressionInternal(rule.Rule, param, options),
			new List<PropertyReplacerInfo>
			{
				new(stateType, stateType.GetMember(nameof(ReduceState<object, object>.Accumulator)).First(), accumulatorParam),
				new(stateType, stateType.GetMember(nameof(ReduceState<object, object>.Current)).First(), currentParam)
			});

		var args = new[] { input }.Downcast(collectionType);

		return Expression.Call(
			_aggregateMethod.MakeGenericMethod(collectionType, initial.Type),
			args[0],
			initial,
			Expression.Lambda(body, accumulatorParam, currentParam));
	}
	
	private static readonly MethodInfo _aggregateMethod = typeof(Enumerable)
		.GetMethods()
		.Where(x => x.Name == nameof(Enumerable.Aggregate))
		.Single(x => x.GetParameters().Length == 3);

	private class ReduceState<TSource, TAggregate>
	{
		public TSource? Current { get; set; }
		public TAggregate? Accumulator { get; set; }
	}
}
