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

[Operator("least")]
[JsonConverter(typeof(LeastRuleJsonConverter))]
public class LeastRule : Rule
{
	protected internal Rule Item { get; }

	protected internal LeastRule(Rule item)
	{
		Item = item;
	}

	public override JsonNode Apply(JsonNode? data, JsonNode? contextData = null) => throw new NotImplementedException("Least rule not implemented for in memory evaluation");
}

public class LeastRuleExpression : RuleExpression<LeastRule>
{
	private static readonly MethodInfo _minMethod = typeof(Enumerable)
		.GetMethods()
		.Single(x => x.Name == nameof(Enumerable.Min) && x.IsGenericMethod && x.GetParameters().Length == 1);

	/// <inheritdoc />
	public override Expression CreateExpression(LeastRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var param = registry.CreateExpression(rule.Item, parameter, options with { WrapConstants = false });
		var args = ExpressionTypeUtilities.Downcast([param]);
		param = args[0];

		if (!LogicTypeExtensions.TryGetGenericCollectionType(param.Type, out var generic))
		{
			throw new JsonLogicException("Least rule must be passed a generic collection");
		}

		return Expression.Call(_minMethod.MakeGenericMethod(generic), args[0]);
	}
}

internal class LeastRuleJsonConverter : WeaklyTypedJsonConverter<LeastRule>
{
	public override LeastRule? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
	{
		var parameters = reader.TokenType == JsonTokenType.StartArray
			? options.ReadArray(ref reader, SampleJsonSerializerContext.Default.Rule)
			: [options.Read(ref reader, SampleJsonSerializerContext.Default.Rule)!];

		if (parameters == null || parameters.Length == 0)
			throw new JsonException("The min rule needs an array of parameters.");

		return new LeastRule(parameters[0]);
	}

	public override void Write(Utf8JsonWriter writer, LeastRule value, JsonSerializerOptions options)
	{
		writer.WriteStartObject();
		writer.WritePropertyName("least");
		options.WriteList(writer, [value.Item], SampleJsonSerializerContext.Default.Rule);
		writer.WriteEndObject();
	}
}

public class LeastRuleTests
{
	private record TestData(List<int> Items);
	
	private class TestTable
	{
		public Guid Id { get; set; }
		public ICollection<TestTable2> Relations { get; set; } = [];
	}
	
	private class TestTable2
	{
		public Guid Id { get; set; }
		public int Data { get; set; }
	}

	private class IttyBittyContext : DbContext
	{
		public DbSet<TestTable> TestTable => Set<TestTable>();
		public DbSet<TestTable2> TestTable2 => Set<TestTable2>();

		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder.UseNpgsql();
		}
	}

	[Test]
	public void MinIsCorrect()
	{
		var registry = RegisterMinExpression();
		var rule = JsonSerializer.Deserialize<Rule>("""{ "least": [{"var": ["items"]}] }""")!;
		var expression = registry.CreateRuleExpression<TestData, int>(rule);

		var data = new TestData([1, 2, 3, 4, 5]);
		Assert.AreEqual(
			data.Items.Min(),
			expression.Compile()(data));
	}

	[Test]
	public void CanCreateSql()
	{
		var registry = RegisterMinExpression();
		using var dbContext = new IttyBittyContext();
		var rule = JsonSerializer.Deserialize<Rule>($$"""
			{
				"==": [
					1,
					{
						"least": [
							{
								"map": [
									{ "var": ["{{nameof(TestTable.Relations)}}"] },
									{ "var": ["{{nameof(TestTable2.Data)}}"] }
								]
							}
						]
					}
				]
			}
			""")!;
		var expression = registry.CreateRuleExpression<TestTable, bool>(rule);
		var sql = dbContext.TestTable.Where(expression).ToQueryString();
		Console.WriteLine(sql);
	}

	private RuleExpressionRegistry RegisterMinExpression()
	{
		RuleRegistry.AddRule<LeastRule>(SampleJsonSerializerContext.Default);
		var options = new CreateRegistryOptions();
		options.AddRule<LeastRule>(new LeastRuleExpression());
		return new RuleExpressionRegistry(options);
	}
}