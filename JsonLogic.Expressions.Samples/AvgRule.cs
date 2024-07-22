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

[Operator("avg")]
[JsonConverter(typeof(AvgRuleJsonConverter))]
public class AvgRule : Rule
{
	protected internal Rule Value { get; }

	protected internal AvgRule(Rule value)
	{
		Value = value;
	}

	public override JsonNode Apply(JsonNode? data, JsonNode? contextData = null) => throw new NotImplementedException("Contains rule not implemented for in memory evaluation");
}

public class AvgRuleExpression : RuleExpression<AvgRule>
{
	private static readonly MethodInfo[] _averageMethods = typeof(Enumerable)
		.GetMethods()
		.Where(x => x.Name == nameof(Enumerable.Average) && x.GetParameters().Length == 1)
		.ToArray();

	/// <inheritdoc />
	public override Expression CreateExpression(AvgRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var value = registry.CreateExpression(rule.Value, parameter, options with { WrapConstants = false });
		var arg = ExpressionTypeUtilities.DowncastNumber(new[] { value })[0];

		if (!LogicTypeExtensions.TryGetGenericCollectionType(arg.Type, out var collectionType))
		{
			throw new JsonLogicException("Sum method expects a collection type as a parameter");
		}

		var avgMethod = _averageMethods.SingleOrDefault(x =>
		{
			var param = x.GetParameters()[0];
			var generic = param.ParameterType.GenericTypeArguments[0];
			return generic == collectionType;
		});

		if (avgMethod == null)
		{
			throw new JsonLogicException($"Called sum method on unsupported type {collectionType}");
		}

		return Expression.Call(
			avgMethod,
			arg);
	}
}

internal class AvgRuleJsonConverter : WeaklyTypedJsonConverter<AvgRule>
{
	public override AvgRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = reader.TokenType == JsonTokenType.StartArray
			? options.ReadArray(ref reader, SampleJsonSerializerContext.Default.Rule)
			: [options.Read(ref reader, SampleJsonSerializerContext.Default.Rule)!];

		if (parameters == null || parameters.Length != 1)
			throw new JsonException("The contains rule needs an array.");

		return new AvgRule(parameters[0]);
	}

	public override void Write(Utf8JsonWriter writer, AvgRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("avg");
		options.WriteList(writer, [value.Value], SampleJsonSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}

public class AvgRuleTests
{
	private record TestData(List<int> Items);

	[Test]
	public void AvgIsCorrect()
	{
		RuleRegistry.AddRule<AvgRule>(SampleJsonSerializerContext.Default);
		var options = new CreateRegistryOptions();
		options.AddRule<AvgRule>(new AvgRuleExpression());
		var registry = new RuleExpressionRegistry(options);

		var rule = JsonSerializer.Deserialize<Rule>("""{ "avg": [{"var": ["items"]}] }""")!;
		var expression = registry.CreateRuleExpression<TestData, double>(rule);

		var data = new TestData([1, 2, 3, 4, 5]);
		Assert.AreEqual(3, expression.Compile()(data));
	}
}