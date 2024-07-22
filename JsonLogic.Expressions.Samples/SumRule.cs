using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Logic.Expressions.Utility;
using Json.More;

namespace Json.Logic.Expressions.Logic;

[Operator("sum")]
[JsonConverter(typeof(SumRuleJsonConverter))]
public class SumRule : Rule
{
	protected internal Rule Value { get; }

	protected internal SumRule(Rule value)
	{
		Value = value;
	}

	public override JsonNode Apply(JsonNode? data, JsonNode? contextData = null) => throw new NotImplementedException("Contains rule not implemented for in memory evaluation");
}

public class SumRuleExpression : RuleExpression<SumRule>
{
	private static readonly MethodInfo[] _sumMethods = typeof(Enumerable)
		.GetMethods()
		.Where(x => x.Name == nameof(Enumerable.Sum) && x.GetParameters().Length == 1)
		.ToArray();

	/// <inheritdoc />
	public override Expression CreateExpression(SumRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var value = registry.CreateExpression(rule.Value, parameter, options with { WrapConstants = false });
		var arg = ExpressionTypeUtilities.DowncastNumber(new[] { value })[0];

		if (!LogicTypeExtensions.TryGetGenericCollectionType(arg.Type, out var collectionType))
		{
			throw new JsonLogicException("Sum method expects a collection type as a parameter");
		}

		var sumMethod = _sumMethods.SingleOrDefault(x => x.ReturnParameter.ParameterType == collectionType);

		if (sumMethod == null)
		{
			throw new JsonLogicException($"Called sum method on unsupported type {collectionType}");
		}

		return Expression.Call(
			sumMethod,
			arg);
	}
}

internal class SumRuleJsonConverter : WeaklyTypedJsonConverter<SumRule>
{
	public override SumRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = reader.TokenType == JsonTokenType.StartArray
			? options.ReadArray(ref reader, SampleJsonSerializerContext.Default.Rule)
			: [options.Read(ref reader, SampleJsonSerializerContext.Default.Rule)!];

		if (parameters == null || parameters.Length != 1)
			throw new JsonException("The contains rule needs an array.");

		return new SumRule(parameters[0]);
	}

	public override void Write(Utf8JsonWriter writer, SumRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("sum");
		options.WriteList(writer, [value.Value], SampleJsonSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}

public class SumRuleTests
{
	private record TestData(List<int> Items);

	[Test]
	public void SumIsCorrect()
	{
		RuleRegistry.AddRule<SumRule>(SampleJsonSerializerContext.Default);
		var options = new CreateRegistryOptions();
		options.AddRule<SumRule>(new SumRuleExpression());
		var registry = new RuleExpressionRegistry(options);

		var rule = JsonSerializer.Deserialize<Rule>("""{ "sum": [{"var": ["items"]}] }""")!;
		var expression = registry.CreateRuleExpression<TestData, int>(rule);

		var data = new TestData([1, 2, 3, 4, 5]);
		Assert.AreEqual(15, expression.Compile()(data));
	}
}