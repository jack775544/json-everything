using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;

namespace Json.Logic.Rules;

/// <summary>
/// Provides a stand-in "rule" for literal values.
/// </summary>
/// <remarks>This is not exactly part of the specification, but it helps things in this library.</remarks>
[Operator("")]
[JsonConverter(typeof(LiteralRuleJsonConverter))]
public class LiteralRule : Rule
{
	internal JsonNode? Value { get; }

	internal static readonly LiteralRule Null = new(null);

	internal LiteralRule(JsonNode? value)
	{
		Value = value?.DeepClone();
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
		return Value;
	}

	public override Expression CreateExpression(Expression parameter, CreateExpressionOptions options)
	{
		if (Value == null)
		{
			return Expression.Constant(null);
		}

		return JsonNodeToExpression(Value, parameter, options);
	}

	internal static Expression JsonNodeToExpression(JsonNode? node, Expression parameter, CreateExpressionOptions options)
	{
		if (node == null)
		{
			return ExpressionExtensions.CreateConstant<object?>(null, options);
		}

		switch (node.GetValueKind())
		{
			case JsonValueKind.Undefined:
				return ExpressionExtensions.CreateConstant<object?>(null, options);
			case JsonValueKind.Object:
				throw new NotImplementedException("Object values not yet implemented");
			case JsonValueKind.Array:
				var values = node.AsArray().Select(x => JsonNodeToExpression(x!, parameter, options)).ToList();

				if (values.Count == 0)
				{
					return Expression.NewArrayInit(typeof(object));
				}

				return Expression.NewArrayInit(values[0].Type, values);
			case JsonValueKind.String:
				return ExpressionExtensions.CreateConstant(node.GetValue<string>(), options);
			case JsonValueKind.Number:
				return ExpressionExtensions.CreateConstant(node.Numberify(), options);
			case JsonValueKind.True:
				return ExpressionExtensions.CreateConstant(true, options);
			case JsonValueKind.False:
				return ExpressionExtensions.CreateConstant(false, options);
			case JsonValueKind.Null:
				return ExpressionExtensions.CreateConstant<object?>(null, options);
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}

internal class LiteralRuleJsonConverter : WeaklyTypedJsonConverter<LiteralRule>
{
	public override LiteralRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		// this is handled by Rule
		throw new NotImplementedException();
	}

	public override void Write(Utf8JsonWriter writer, LiteralRule value, JsonSerializerOptions options)
	{
		if (value.Value is null) 
			writer.WriteNullValue();
		else
			JsonSerializer.Serialize(writer, value.Value, JsonLogicSerializerContext.Default.JsonNode!);
	}
}