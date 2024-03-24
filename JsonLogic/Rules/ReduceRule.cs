using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `reduce` operation.
/// </summary>
[Operator("reduce")]
[JsonConverter(typeof(ReduceRuleJsonConverter))]
public class ReduceRule : Rule
{
	/// <summary>
	/// A sequence of values to reduce.
	/// </summary>
	protected internal Rule Input { get; }
	/// <summary>
	/// The reduction to perform.
	/// </summary>
	protected internal Rule Rule { get; }
	/// <summary>
	/// The initial value to start the reduction. ie; the seed.
	/// </summary>
	protected internal Rule Initial { get; }

	/// <summary>
	/// Creates a new instance of <see cref="ReduceRule"/> when 'reduce' operator is detected within json logic.
	/// </summary>
	/// <param name="input">A sequence of values to reduce.</param>
	/// <param name="rule">The reduction to perform.</param>
	/// <param name="initial">The initial value to start the reduction. ie; the seed.</param>
	protected internal ReduceRule(Rule input, Rule rule, Rule initial)
	{
		Input = input;
		Rule = rule;
		Initial = initial;
	}

	/// <summary>
	/// Applies the rule to the input data.
	/// </summary>
	/// <param name="data">The input data.</param>
	/// <param name="contextData">
	///     Optional secondary data.  Used by a few operators to pass a secondary
	///     data context to inner operators.
	/// </param>
	/// <returns>The result of the rule.</returns>
	public override JsonNode? Apply(JsonNode? data, JsonNode? contextData = null)
	{
		var input = Input.Apply(data, contextData);
		var accumulator = Initial.Apply(data, contextData);

		if (input is not JsonArray arr) return accumulator;

		foreach (var element in arr)
		{
			var intermediary = new JsonObject
			{
				["current"] = element?.DeepClone(),
				["accumulator"] = accumulator?.DeepClone()
			};

			accumulator = Rule.Apply(data, intermediary);

			if (accumulator == null) break;
		}

		return accumulator;
	}

	public override Expression CreateExpression(Expression parameter, CreateExpressionOptions options)
	{
		var input = Input.CreateExpression(parameter, options);
		var initial = new [] { Initial.CreateExpression(parameter, options) }.Downcast().First();

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

		var rule = PropertyReplacer.Replace(
			Rule.CreateExpression(param, options),
			new List<PropertyReplacerInfo>
			{
				new(stateType, stateType.GetMember(nameof(ReduceState<object, object>.Accumulator)).First(), accumulatorParam),
				new(stateType, stateType.GetMember(nameof(ReduceState<object, object>.Current)).First(), currentParam)
			});

		return Expression.Call(
			_aggregateMethod.MakeGenericMethod(collectionType, initial.Type),
			input,
			initial,
			Expression.Lambda(rule, accumulatorParam, currentParam));
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

internal class ReduceRuleJsonConverter : WeaklyTypedJsonConverter<ReduceRule>
{
	public override ReduceRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters is not { Length: 3 })
			throw new JsonException("The reduce rule needs an array with 3 parameters.");

		return new ReduceRule(parameters[0], parameters[1], parameters[2]);
	}

	public override void Write(Utf8JsonWriter writer, ReduceRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("reduce");
		writer.WriteStartArray();
		options.Write(writer, value.Input, JsonLogicSerializerContext.Default.Rule);
		options.Write(writer, value.Rule, JsonLogicSerializerContext.Default.Rule);
		options.Write(writer, value.Initial, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndArray();
		writer.WriteEndObject();
	}
}
