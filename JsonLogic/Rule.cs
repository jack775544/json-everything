﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Logic.Rules;
using Json.More;

namespace Json.Logic;

/// <summary>
/// Provides a base class for rules.
/// </summary>
[JsonConverter(typeof(LogicComponentConverter))]
public abstract class Rule
{
	internal JsonNode? Source { get; set; }

	/// <summary>
	/// Applies the rule to the input data.
	/// </summary>
	/// <param name="data">The input data.</param>
	/// <param name="contextData">
	///     Optional secondary data.  Used by a few operators to pass a secondary
	///     data context to inner operators.
	/// </param>
	/// <returns>The result of the rule.</returns>
	public abstract JsonNode? Apply(JsonNode? data, JsonNode? contextData = null);

	/// <summary>
	/// Creates an expression predicate for the rule.
	/// </summary>
	/// <param name="parameter">An expression to be used as the JSON Logic data.</param>
	/// <param name="options">Options for creating the expression.</param>
	/// <returns>An expression body that represents the logic rule.</returns>
	public abstract Expression CreateExpression(Expression parameter, CreateExpressionOptions options);

	/// <summary>
	/// Casts a JSON value to a <see cref="LiteralRule"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator Rule(JsonNode? value) => new LiteralRule(value);
	/// <summary>
	/// Casts an `int` value to a <see cref="LiteralRule"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator Rule(int value) => new LiteralRule(value);
	/// <summary>
	/// Casts a `string` value to a <see cref="LiteralRule"/>.  Can also be used to create a `null` JSON literal.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator Rule(string? value) => value == null ? LiteralRule.Null : new LiteralRule(value);
	/// <summary>
	/// Casts a `bool` value to a <see cref="LiteralRule"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator Rule(bool value) => new LiteralRule(value);
	/// <summary>
	/// Casts a `long` value to a <see cref="LiteralRule"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator Rule(long value) => new LiteralRule(value);
	/// <summary>
	/// Casts a `decimal` value to a <see cref="LiteralRule"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator Rule(decimal value) => new LiteralRule(value);
	/// <summary>
	/// Casts a `float` value to a <see cref="LiteralRule"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator Rule(float value) => new LiteralRule(value);
	/// <summary>
	/// Casts a `double` value to a <see cref="LiteralRule"/>.
	/// </summary>
	/// <param name="value">The value.</param>
	public static implicit operator Rule(double value) => new LiteralRule(value);
}

/// <summary>
/// Options for the create expression logic method.
/// </summary>
public class CreateExpressionOptions
{
	/// <summary>
	/// If set to true then constant values will be wrapped in an object and the value will be fetched from this wrapper object.
	/// This causes EF Core to use SQL variables when generating SQL.
	/// </summary>
	public bool WrapConstants { get; set; }
	
	/// <summary>
	/// Culture info for parse operations.
	/// </summary>
	public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;
}

/// <summary>
/// Provides serialization for all <see cref="Rule"/> derivatives.
/// </summary>
public class LogicComponentConverter : JsonConverter<Rule>
{
	/// <summary>
	/// Indicates whether <see langword="null" /> should be passed to the converter
	/// on serialization, and whether <see cref="F:System.Text.Json.JsonTokenType.Null" />
	/// should be passed on deserialization.
	/// </summary>
	public override bool HandleNull => true;

	/// <summary>
	/// Gets or sets whether to save the source data for re-serialization; default is true.
	/// </summary>
	public bool SaveSource { get; set; } = true;

	/// <summary>Reads and converts the JSON to type <see cref="Rule"/>.</summary>
	/// <param name="reader">The reader.</param>
	/// <param name="typeToConvert">The type to convert.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	/// <returns>The converted value.</returns>
	public override Rule Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var node = options.Read(ref reader, JsonLogicSerializerContext.Default.JsonNode);
		Rule rule;

		if (node is JsonObject)
		{
			if (node is not JsonObject { Count: 1 } data)
			{
				rule = new LiteralRule(node);
			}
			else
			{
				var (op, args) = data.First();

				var ruleType = RuleRegistry.GetRule(op) ??
				               throw new JsonException($"Cannot identify rule for {op}");
				var typeInfo = RuleRegistry.GetTypeInfo(ruleType) ??
				               options.GetTypeInfo(ruleType) ??
				               throw new JsonException($"Cannot get JsonTypeInfo for rule type {ruleType}");

				rule = args is null
					? (Rule)JsonSerializer.Deserialize("[]", typeInfo)!
					: (Rule)args.Deserialize(typeInfo)!;
			}
		}
		else if (node is JsonArray)
		{
			var data = node.AsArray().Select(x => x.Deserialize(JsonLogicSerializerContext.Default.Rule)!).ToArray();
			rule = new RuleCollection(data);
		}
		else
			rule = new LiteralRule(node);

		if (SaveSource)
			rule.Source = node;
		return rule;
	}

	/// <summary>Writes a specified value as JSON.</summary>
	/// <param name="writer">The writer to write to.</param>
	/// <param name="value">The value to convert to JSON.</param>
	/// <param name="options">An object that specifies serialization options to use.</param>
	public override void Write(Utf8JsonWriter writer, Rule value, JsonSerializerOptions options)
	{
		if (value.Source != null)
		{
			options.Write(writer, value.Source, JsonLogicSerializerContext.Default.JsonNode);
			return;
		}

		var ruleType = value.GetType();
		var typeInfo = RuleRegistry.GetTypeInfo(ruleType)!;
		var converter = (IWeaklyTypedJsonConverter)typeInfo.Converter;
		converter.Write(writer, value, options, typeInfo);
	}
}
