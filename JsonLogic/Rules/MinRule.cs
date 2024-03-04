﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

// ReSharper disable PossibleMultipleEnumeration

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `min` operation.
/// </summary>
[Operator("min")]
[JsonConverter(typeof(MinRuleJsonConverter))]
public class MinRule : Rule
{
	/// <summary>
	/// The sequence of numbers to query for min.
	/// </summary>
	protected internal List<Rule> Items { get; }

	/// <summary>
	/// Creates a new instance of <see cref="MinRule"/> when 'min' operator is detected within json logic.
	/// </summary>
	/// <param name="a">The first numeric value in a sequence of numbers.</param>
	/// <param name="more">A sequence of numbers.</param>
	protected internal MinRule(Rule a, params Rule[] more)
	{
		Items = [a, .. more];
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
		var items = Items.Select(i => i.Apply(data, contextData)).Select(e => new { Type = e.JsonType(), Value = e.Numberify() }).ToList();
		var nulls = items.Where(i => i.Value == null);
		if (nulls.Any()) return null;

		return items.Min(i => i.Value!.Value);
	}

	public override Expression CreateExpression(Expression parameter)
	{
		return EvaluateItems(Items, parameter)
			.Select(ExpressionExtensions.Numberify)
			.Aggregate((a, c) => Expression.Call(_minMethod, a, c));
	}

	private static readonly MethodInfo _minMethod = ((Func<decimal, decimal, decimal>)Math.Min).Method;

}

internal class MinRuleJsonConverter : WeaklyTypedJsonConverter<MinRule>
{
	public override MinRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule);

		if (parameters == null || parameters.Length == 0)
			throw new JsonException("The min rule needs an array of parameters.");

		return new MinRule(parameters[0], parameters.Skip(1).ToArray());
	}

	public override void Write(Utf8JsonWriter writer, MinRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("min");
		options.WriteList(writer, value.Items, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}
