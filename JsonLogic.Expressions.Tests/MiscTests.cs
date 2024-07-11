using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests;

internal class TestData
{
	public int IntField { get; set; }
	public double DoubleField { get; set; }
	public string? StringField { get; set; }
	public DateTime DateField { get; set; }
	public DateTime UtcDateField { get; set; }
	public DateOnly DateOnlyField { get; set; }
}

public class MiscTests
{
	[Test]
	public void BigTestCase()
	{
		// language=JSON
		var logic = """
		            {
		              "and": [
		                true,
		                { "==": [{ "var" : ["IntField"] }, 1] },
		                { "==": [{ "var" : ["DoubleField"] }, 1] },
		                { "==": [{ "var" : ["StringField"] }, "testing"] },
		                {
		                  "or": [
		                    { "==": [{ "var" : ["DateField"] }, "2020-01-01T00:00:00"] },
		                    { "==": [{ "var" : ["DateField"] }, "2020-01-01T00:00:00Z"] },
		                    { "<": [{ "var" : ["DateField"] }, "2020-01-01T00:00:00"] }
		                  ]
		                }
		              ]
		            }
		            """;
		var rule = JsonSerializer.Deserialize(logic, TestDataSerializerContext.Default.Rule);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<TestData, bool>(rule!, new CreateExpressionOptions
		{
			WrapConstants = true,
		});
		var func = expression.Compile();

		var data = new TestData[]
		{
			new TestData(),
			new TestData(),
		};

		Assert.DoesNotThrow(() => data.Where(x => func(x)).ToList());
	}

	[Test]
	public void TestMultiTypeEquals()
	{
		// language=JSON
		var logic = """
		            {
		            	"and": [
		            		{ "==": ["1", {"var": ["StringField"]}] },
		            		{ "==": ["1", [1, 2, 3]] },
		            		{ "==": ["1", null] },
		            		{ "==": ["1", true] },
		            		{ "==": [[1, 2, 3], {"var": ["StringField"]}] }
		            	]
		            }
		            """;
		var rule = JsonSerializer.Deserialize(logic, TestDataSerializerContext.Default.Rule);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<TestData, bool>(rule!);
		var func = expression.Compile();

		var data = new TestData[]
		{
			new TestData(),
			new TestData(),
		};

		Assert.IsEmpty(data.Where(x => func(x)).ToList());
	}

	[TestCase]
	public void WrapConstantsWorks()
	{
		// language=JSON
		var logic = """
		            { "==": [1, true] }
		            """;

		var rule = JsonSerializer.Deserialize(logic, TestDataSerializerContext.Default.Rule);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule!, new CreateExpressionOptions
		{
			WrapConstants = true,
		});
		var func = expression.Compile();

		Assert.AreEqual(true, func(null));
	}

	[TestCase]
	public void DateFilterWorks()
	{
		// language=JSON
		var logic = """
		            {
		                "and": [
		                    {
		                        "<=": ["2020-01-01T04:24:41.835Z", { "var": ["DateField"] }]
		                    },
		                    {
		                        ">=": ["2021-02-25T06:32:19.123Z", { "var": ["DateField"] }]
		                    }
		                ]
		            }
		            """;

		var rule = JsonSerializer.Deserialize(logic, TestDataSerializerContext.Default.Rule);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<TestData, bool>(rule!, new CreateExpressionOptions
		{
			WrapConstants = false,
		});
		var func = expression.Compile();

		var data = new TestData[]
		{
			new TestData { DateField = new DateTime(2020, 4, 4) },
			new TestData { DateField = new DateTime(2021, 4, 4) },
			new TestData { DateField = new DateTime(2020, 5, 4) },
		};

		var results = data.Where(func).ToList();
		Assert.AreEqual(2, results.Count);
	}
	
	[TestCase]
	public void DateOnlyFilterWorks()
	{
		// language=JSON
		var logic = """
		            {
		                "and": [
		                    {
		                        "<=": ["2020-01-01", { "var": ["DateOnlyField"] }]
		                    },
		                    {
		                        ">=": ["2021-02-25", { "var": ["DateOnlyField"] }]
		                    }
		                ]
		            }
		            """;

		var rule = JsonSerializer.Deserialize(logic, TestDataSerializerContext.Default.Rule);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<TestData, bool>(rule!, new CreateExpressionOptions
		{
			WrapConstants = false,
		});
		var func = expression.Compile();

		var data = new TestData[]
		{
			new TestData { DateOnlyField = new(2020, 4, 4) },
			new TestData { DateOnlyField = new(2021, 4, 4) },
			new TestData { DateOnlyField = new(2020, 5, 4) },
		};

		var results = data.Where(func).ToList();
		Assert.AreEqual(2, results.Count);
	}
	
	[TestCase("2020-01-01T00:00:00Z", DateTimeKind.Utc)]
	[TestCase("2020-01-01T00:00:00", DateTimeKind.Unspecified)]
	public void UtcConversionWorks(string input, DateTimeKind kind)
	{
		// language=JSON
		var logic = $$"""
		            {
		                "<=": ["{{input}}", { "var": ["UtcDateField"] }]
		            }
		            """;

		var rule = JsonSerializer.Deserialize(logic, TestDataSerializerContext.Default.Rule);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<TestData, bool>(rule!, new CreateExpressionOptions
		{
			WrapConstants = false,
		});
		// .ConfigureProperty<TestData, DateTime>(x => x.UtcDateField, x => x.ParseStyle(DateTimeStyles.AdjustToUniversal)));

		// Yikes, don't really want to write a visitor just for this test though
		var dateTime = (DateTime)((ConstantExpression)((BinaryExpression)expression.Body).Left).Value!;
		Assert.AreEqual(kind, dateTime.Kind);

		// var data = new TestData[]
		// {
		// 	new TestData(),
		// };
		//
		// var results = data.Where(func).ToList();
		// Assert.AreEqual(2, results.Count);
	}

	private record InIdTestData(Guid? Id);

	[Test]
	public void InId()
	{
		// language=JSON
		var logic = """
			{
			    "in": [
			        {"var": ["Id"]},
			        [
			            "000dc3ee-fa94-4b79-8d88-054262f71cf2",
			            "00e6cf79-1055-446a-a116-09cfc001ec3a"
			        ]
			    ]
			}
			""";

		var rule = JsonSerializer.Deserialize(logic, TestDataSerializerContext.Default.Rule)!;
		
		// Make sure the rule works with regular JSON logic
		var a = rule.Apply(new JsonObject(new Dictionary<string, JsonNode?> { ["Id"] = "000dc3ee-fa94-4b79-8d88-054262f71cf2" }))!;
		var b = rule.Apply(new JsonObject(new Dictionary<string, JsonNode?> { ["Id"] = "000dc3ee-fa94-4b79-8d88-054262f71cf3" }))!;
		Assert.IsTrue(a.GetValue<bool>());
		Assert.IsFalse(b.GetValue<bool>());

		// Now try expressions
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<InIdTestData, bool>(rule!, new CreateExpressionOptions
		{
			WrapConstants = false,
		});
		var func = expression.Compile();

		Assert.IsTrue(func(new InIdTestData(Guid.Parse("000dc3ee-fa94-4b79-8d88-054262f71cf2"))));
		Assert.IsFalse(func(new InIdTestData(Guid.Parse("000dc3ee-fa94-4b79-8d88-054262f71cf3"))));
	}
}