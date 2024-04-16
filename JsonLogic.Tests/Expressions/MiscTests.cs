using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

internal class TestData
{
	public int IntField { get; set; }
	public double DoubleField { get; set; }
	public string? StringField { get; set; }
	public DateTime DateField { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Rule))]
internal partial class TestDataSerializerContext : JsonSerializerContext
{
}

public class MiscTests
{
	[Test]
	public void BigTestCase()
	{
		var logic = """
		            {
		              "and": [
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
		var rule = JsonSerializer.Deserialize<Rule>(logic, TestDataSerializerContext.Default.Rule);
		
		var expression = ExpressionTestHelpers.CreateRuleExpression<TestData, bool>(rule!);
		var func = expression.Compile();

		var data = new TestData[]
		{
			new TestData(),
			new TestData(),
		};

		Assert.DoesNotThrow(() => data.Where(x => func(x)).ToList());
	}
}