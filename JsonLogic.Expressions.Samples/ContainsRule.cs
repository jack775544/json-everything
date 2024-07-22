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

[Operator("contains")]
[JsonConverter(typeof(ContainsRuleJsonConverter))]
public class ContainsRule : Rule
{
	protected internal Rule Test { get; }
	protected internal Rule Value { get; }

	protected internal ContainsRule(Rule test, Rule value)
	{
		Test = test;
		Value = value;
	}

	public override JsonNode Apply(JsonNode? data, JsonNode? contextData = null) => throw new NotImplementedException("Contains rule not implemented for in memory evaluation");
}

public class ContainsRuleExpression : RuleExpression<ContainsRule>
{
	private static readonly MethodInfo _iLikeMethod = ((Func<DbFunctions, string, string, bool>)NpgsqlDbFunctionsExtensions.ILike).Method;
	private static readonly MethodInfo _stringConcat3Method = ((Func<string, string, string, string>)string.Concat).Method;

	/// <inheritdoc />
	public override Expression CreateExpression(ContainsRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var value = registry.CreateExpression(rule.Value, parameter, options with { WrapConstants = false });
		var test = registry.CreateExpression(rule.Test, parameter, options with { WrapConstants = false });
		var args = ExpressionTypeUtilities.Downcast(new[] { value, test }, typeof(string));

		return Expression.Call(
			_iLikeMethod,
			Expression.Constant(EF.Functions),
			args[0],
			Expression.Call(_stringConcat3Method, Expression.Constant("%"), args[1], Expression.Constant("%")));
	}
}

internal class ContainsRuleJsonConverter : WeaklyTypedJsonConverter<ContainsRule>
{
	public override ContainsRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = reader.TokenType == JsonTokenType.StartArray
			? options.ReadArray(ref reader, SampleJsonSerializerContext.Default.Rule)
			: [options.Read(ref reader, SampleJsonSerializerContext.Default.Rule)!];

		if (parameters == null || parameters.Length != 2)
			throw new JsonException("The contains rule needs an array of 2 parameters.");

		return new ContainsRule(parameters[0], parameters[1]);
	}

	public override void Write(Utf8JsonWriter writer, ContainsRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("contains");
		options.WriteList(writer, [value.Test, value.Value], SampleJsonSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}

public class ContainsRuleTests
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
		RuleRegistry.AddRule<ContainsRule>(SampleJsonSerializerContext.Default);
		var options = new CreateRegistryOptions();
		options.AddRule<ContainsRule>(new ContainsRuleExpression());
		options.WrapConstants = true;
		var registry = new RuleExpressionRegistry(options);

		var rule = JsonSerializer.Deserialize<Rule>("""{ "contains": ["WORLD", {"var": ["data"]}] }""")!;
		var expression = registry.CreateRuleExpression<TestData, bool>(rule);

		using var dbContext = new IttyBittyContext();
		var sql = dbContext.Data.Where(expression).ToQueryString();
		Console.WriteLine(sql);
	}
}