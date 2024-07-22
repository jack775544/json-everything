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

[Operator("count")]
[JsonConverter(typeof(CountRuleJsonConverter))]
public class CountRule : Rule
{
	protected internal Rule Item { get; }

	protected internal CountRule(Rule item)
	{
		Item = item;
	}

	public override JsonNode Apply(JsonNode? data, JsonNode? contextData = null) => throw new NotImplementedException("Count rule not implemented for in memory evaluation");
}

public class CountRuleExpression : RuleExpression<CountRule>
{
	private static readonly MethodInfo _countMethod = typeof(Enumerable)
		.GetMethods()
		.Where(x => x.Name == nameof(Enumerable.Count))
		.Single(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType.IsGenericType && x.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEnumerable<>));

	/// <inheritdoc />
	public override Expression CreateExpression(CountRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var param = registry.CreateExpression(rule.Item, parameter, options with { WrapConstants = false });
		var args = ExpressionTypeUtilities.Downcast([param]);
		param = args[0];

		if (!LogicTypeExtensions.TryGetGenericCollectionType(param.Type, out var generic))
		{
			throw new JsonLogicException("Count rule must be passed a generic collection");
		}

		return Expression.Call(_countMethod.MakeGenericMethod(generic), args[0]);
	}
}

internal class CountRuleJsonConverter : WeaklyTypedJsonConverter<CountRule>
{
	public override CountRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = reader.TokenType == JsonTokenType.StartArray
			? options.ReadArray(ref reader, SampleJsonSerializerContext.Default.Rule)
			: [options.Read(ref reader, SampleJsonSerializerContext.Default.Rule)!];

		if (parameters == null || parameters.Length == 0)
			throw new JsonException("The count rule needs an array of parameters.");

		return new CountRule(parameters[0]);
	}

	public override void Write(Utf8JsonWriter writer, CountRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("count");
		options.WriteList(writer, [value.Item], SampleJsonSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}

public class CountRuleTests
{
	private record TestData(List<int> Items);

	[Test]
	public void CountIsCorrect()
	{
		RuleRegistry.AddRule<CountRule>(SampleJsonSerializerContext.Default);
		var options = new CreateRegistryOptions();
		options.AddRule<CountRule>(new CountRuleExpression());
		var registry = new RuleExpressionRegistry(options);

		var rule = JsonSerializer.Deserialize<Rule>("""{ "count": [{"var": ["items"]}] }""")!;
		var expression = registry.CreateRuleExpression<TestData, int>(rule);

		var data = new TestData([1, 2, 3, 4, 5]);
		Assert.AreEqual(
			data.Items.Count,
			expression.Compile()(data));
	}
}