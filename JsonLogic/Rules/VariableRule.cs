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
using Json.Pointer;

namespace Json.Logic.Rules;

/// <summary>
/// Handles the `var` operation.
/// </summary>
[Operator("var")]
[JsonConverter(typeof(VariableRuleJsonConverter))]
public class VariableRule : Rule
{
	internal Rule? Path { get; }
	internal Rule? DefaultValue { get; }

	internal VariableRule()
	{
	}
	internal VariableRule(Rule path)
	{
		Path = path;
	}
	internal VariableRule(Rule path, Rule defaultValue)
	{
		Path = path;
		DefaultValue = defaultValue;
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
		if (Path == null) return data;

		var path = Path.Apply(data, contextData);
		var pathString = path.Stringify()!;
		if (pathString == string.Empty) return contextData ?? data;

		var pointer = JsonPointer.Parse(pathString == string.Empty ? "" : $"/{pathString.Replace('.', '/')}");
		if (pointer.TryEvaluate(contextData ?? data, out var pathEval) ||
			pointer.TryEvaluate(data, out pathEval))
			return pathEval;

		return DefaultValue?.Apply(data, contextData) ?? null;
	}

	public override Expression CreateExpression(Expression parameter, CreateExpressionOptions options)
	{
		if (Path == null)
		{
			return Expression.Constant(null);
		}

		var path = Path.Apply().Stringify();

		if (path == null)
		{
			throw new InvalidOperationException("Variable path is null");
		}

		if (string.IsNullOrEmpty(path))
		{
			return parameter;
		} 

		var pathSegments = path.Split('.');

		var property = GetPropertyOrField(parameter, pathSegments);

		if (DefaultValue != null)
		{
			if (
				property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(Nullable<>)
				|| !property.Type.IsValueType)
			{
				var defaultValue = DefaultValue.CreateExpression(parameter, options);
				var args = new[]
				{
					property,
					defaultValue
				}.Downcast();

				return Expression.Coalesce(args[0], args[1]);
			}
		}

		return property;

		
		
		// return Expression.Coalesce(
		// 	Expression.Convert(
		// 		property,
		// 		defaultValue.Type.IsValueType
		// 			? typeof(Nullable<>).MakeGenericType(defaultValue.Type)
		// 			: defaultValue.Type),
		// 	defaultValue);
	}

	[RequiresUnreferencedCode("Calls System.Linq.Expressions.Expression.PropertyOrField(Expression, String)")]
	private static Expression GetPropertyOrField(Expression expression, string[] path)
	{
		return path.Aggregate(expression, GetPropertyOrArrayMember);
	}

	[RequiresUnreferencedCode("Calls System.Linq.Expressions.Expression.PropertyOrField(Expression, String)")]
	private static Expression GetPropertyOrArrayMember(Expression expression, string path)
	{
		if (expression.Type.IsCollectionType())
		{
			return int.TryParse(path, out var index)
				? expression.GetCollectionItem(index)
				: Expression.Constant(null);
		}

		try
		{
			return Expression.PropertyOrField(expression, path);
		}
		catch (ArgumentException)
		{
			return Expression.Constant(null);
		}
	}
}

internal class VariableRuleJsonConverter : WeaklyTypedJsonConverter<VariableRule>
{
	public override VariableRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = reader.TokenType == JsonTokenType.StartArray
			? options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule)
			: new[] { options.Read(ref reader, JsonLogicSerializerContext.Default.Rule)! };

		if (parameters is not ({ Length: 0 } or { Length: 1 } or { Length: 2 }))
			throw new JsonException("The var rule needs an array with 0, 1, or 2 parameters.");

		return parameters.Length switch
		{
			0 => new VariableRule(),
			1 => new VariableRule(parameters[0]),
			_ => new VariableRule(parameters[0], parameters[1])
		};
	}

	public override void Write(Utf8JsonWriter writer, VariableRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("var");
		if (value.DefaultValue != null)
		{
			writer.WriteStartArray();
			options.Write(writer, value.Path, JsonLogicSerializerContext.Default.Rule!);
			options.Write(writer, value.DefaultValue, JsonLogicSerializerContext.Default.Rule);
			writer.WriteEndArray();
		}
		else
			options.Write(writer, value.Path, JsonLogicSerializerContext.Default.Rule!);

		writer.WriteEndObject();
	}
}
