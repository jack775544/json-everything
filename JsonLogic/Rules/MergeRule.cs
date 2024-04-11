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
/// Handles the `merge` operation.
/// </summary>
[Operator("merge")]
[JsonConverter(typeof(MergeRuleJsonConverter))]
public class MergeRule : Rule
{
	/// <summary>
	/// A sequence of arrays to merge into a single array.
	/// </summary>
	protected internal List<Rule> Items { get; }

	/// <summary>
	/// Creates a new instance of <see cref="MergeRule"/> when 'merge' operator is detected within json logic.
	/// </summary>
	/// <param name="items">A sequence of arrays to merge into a single array.</param>
	protected internal MergeRule(params Rule[] items)
	{
		Items = [.. items];
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
		var items = Items.Select(i => i.Apply(data, contextData)).SelectMany(e => e.Flatten());

		return items.ToJsonArray();
	}

	public override Expression CreateExpression(Expression parameter, CreateExpressionOptions options)
	{
		var items = ExpressionExtensions.EvaluateItems(Items, parameter, options).Downcast().ToList();

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
	
	private static readonly MethodInfo _selectManyMethod = typeof(Enumerable)
		.GetMethods()
		.Where(x => x.Name == nameof(Enumerable.SelectMany))
		.Single(x => x.GetParameters().Last().ParameterType.GetGenericArguments().Length == 2);
}

internal class MergeRuleJsonConverter : WeaklyTypedJsonConverter<MergeRule>
{
	public override MergeRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = reader.TokenType == JsonTokenType.StartArray
			? options.ReadArray(ref reader, JsonLogicSerializerContext.Default.Rule)
			: new[] { options.Read(ref reader, JsonLogicSerializerContext.Default.Rule)! };

		if (parameters == null) return new MergeRule();

		return new MergeRule(parameters);
	}

	public override void Write(Utf8JsonWriter writer, MergeRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("merge");
		options.WriteList(writer, value.Items, JsonLogicSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}
