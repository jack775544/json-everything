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
using Microsoft.EntityFrameworkCore;

namespace Json.Logic.Expressions.Logic;

[Operator("^")]
[JsonConverter(typeof(PowerRuleJsonConverter))]
public class PowerRule : Rule
{
	protected internal Rule Base { get; }
	protected internal Rule Exponent { get; }

	protected internal PowerRule(Rule @base, Rule exponent)
	{
		Base = @base;
		Exponent = exponent;
	}

	public override JsonNode Apply(JsonNode? data, JsonNode? contextData = null) => throw new NotImplementedException("Contains rule not implemented for in memory evaluation");
}

public class PowerRuleExpression : RuleExpression<PowerRule>
{
	private static readonly MethodInfo _powMethod = ((Func<double, double, double>)Math.Pow).Method;

	/// <inheritdoc />
	public override Expression CreateExpression(PowerRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var @base = registry.CreateExpression(rule.Base, parameter, options with { WrapConstants = false });
		var exponent = registry.CreateExpression(rule.Exponent, parameter, options with { WrapConstants = false });
		var args = ExpressionTypeUtilities.DowncastNumber(new[] { @base, exponent }, typeof(double));

		args[0] = AsDouble(args[0]);
		args[1] = AsDouble(args[1]);

		return Expression.Call(_powMethod, args[0], args[1]);
	}

	private Expression AsDouble(Expression input) => input.Type != typeof(double)
		? Expression.Convert(input, typeof(double))
		: input;
}

internal class PowerRuleJsonConverter : WeaklyTypedJsonConverter<PowerRule>
{
	public override PowerRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = reader.TokenType == JsonTokenType.StartArray
			? options.ReadArray(ref reader, SampleJsonSerializerContext.Default.Rule)
			: [options.Read(ref reader, SampleJsonSerializerContext.Default.Rule)!];

		if (parameters == null || parameters.Length != 2)
			throw new JsonException("The power rule needs an array of length 2.");

		return new PowerRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, PowerRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("^");
		options.WriteList(writer, [value.Base, value.Exponent], SampleJsonSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}

public class PowerRuleTests
{
	private class TestData
	{
		public Guid Id { get; init; }
		public int IntValue { get; init; }
		public double DoubleValue { get; init; }
	}

	private class IttyBittyContext : DbContext
	{
		public DbSet<TestData> Data => Set<TestData>();

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseNpgsql();
		}
	}

	[TestCase(nameof(TestData.IntValue))]
	[TestCase(nameof(TestData.DoubleValue))]
	public void PowerIsCorrect(string field)
	{
		var expression = CreateExpression(field);

		var data = new TestData
		{
			DoubleValue = 4,
			IntValue = 4,
		};
		Console.WriteLine(expression.ToString());
		Assert.AreEqual(16, expression.Compile()(data));
	}

	[TestCase(nameof(TestData.IntValue))]
	[TestCase(nameof(TestData.DoubleValue))]
	public void CanCreateSql(string field)
	{
		var expression = CreateExpression(field);
		using var dbContext = new IttyBittyContext();
		Console.WriteLine(dbContext.Data.Select(expression).ToQueryString());
	}

	private Expression<Func<TestData, double>> CreateExpression(string field)
	{
		RuleRegistry.AddRule<PowerRule>(SampleJsonSerializerContext.Default);
		var options = new CreateRegistryOptions();
		options.AddRule<PowerRule>(new PowerRuleExpression());
		var registry = new RuleExpressionRegistry(options);

		var rule = JsonSerializer.Deserialize<Rule>($$"""{ "^": [{"var": ["{{field}}"]}, 2] }""")!;
		return registry.CreateRuleExpression<TestData, double>(rule);
	}
}