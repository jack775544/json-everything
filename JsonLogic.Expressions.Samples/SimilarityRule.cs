using System;
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

[Operator("similarity")]
[JsonConverter(typeof(SimilarityRuleJsonConverter))]
public class SimilarityRule : Rule
{
	protected internal Rule Test { get; }
	protected internal Rule Value { get; }

	protected internal SimilarityRule(Rule test, Rule value)
	{
		Test = test;
		Value = value;
	}

	public override JsonNode Apply(JsonNode? data, JsonNode? contextData = null) => throw new NotImplementedException("Similarity rule not implemented for in memory evaluation");
}

public class SimilarityRuleExpression : RuleExpression<SimilarityRule>
{
	private static readonly MethodInfo _trigramSimilarityMethod = ((Func<DbFunctions, string, string, double>)NpgsqlTrigramsDbFunctionsExtensions.TrigramsSimilarityDistance).Method;

	/// <inheritdoc />
	public override Expression CreateExpression(SimilarityRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var value = registry.CreateExpression(rule.Value, parameter, options with { WrapConstants = false });
		var test = registry.CreateExpression(rule.Test, parameter, options with { WrapConstants = false });
		var args = ExpressionTypeUtilities.Downcast(new[] { value, test }, typeof(string));

		return Expression.Call(
			_trigramSimilarityMethod,
			Expression.Constant(EF.Functions),
			args[0],
			args[1]);
	}
}

internal class SimilarityRuleJsonConverter : WeaklyTypedJsonConverter<SimilarityRule>
{
	public override SimilarityRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = reader.TokenType == JsonTokenType.StartArray
			? options.ReadArray(ref reader, SampleJsonSerializerContext.Default.Rule)
			: [options.Read(ref reader, SampleJsonSerializerContext.Default.Rule)!];

		if (parameters == null || parameters.Length != 2)
			throw new JsonException("The contains rule needs an array of 2 parameters.");

		return new SimilarityRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, SimilarityRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("similarity");
		options.WriteList(writer, [value.Test, value.Value], SampleJsonSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}

public class SimilarityRuleTests
{
	private class TestData
	{
		public Guid Id { get; set; }
		public string? Data { get; set; }
	}

	private class IttyBittyContext : DbContext
	{
		public DbSet<TestData> Data => Set<TestData>();

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseNpgsql();
		}
	}

	[Test]
	public void CanCreateSql()
	{
		RuleRegistry.AddRule<SimilarityRule>(SampleJsonSerializerContext.Default);
		var options = new CreateRegistryOptions();
		options.AddRule<SimilarityRule>(new SimilarityRuleExpression());
		options.WrapConstants = true;
		var registry = new RuleExpressionRegistry(options);

		var rule = JsonSerializer.Deserialize<Rule>("""{ "similarity": ["WORLD", {"var": ["data"]}] }""")!;
		var parameter = Expression.Parameter(typeof(TestData), "arg");
		
		var array = Expression.NewArrayInit(
			typeof(object),
			[
				Expression.PropertyOrField(parameter, nameof(TestData.Data)),
				Expression.Convert(registry.CreateExpression(rule, parameter), typeof(object)),
			]);
		var expression = Expression.Lambda<Func<TestData, object[]>>(array, parameter);

		using var dbContext = new IttyBittyContext();
		var sql = dbContext.Data
			.OrderBy(Expression.Lambda<Func<TestData, object>>(array.Expressions[1], parameter))
			.Select(expression)
			.Take(10)
			.ToQueryString();
		Console.WriteLine(sql);
	}
}