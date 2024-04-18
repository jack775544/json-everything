using System.Text.Json.Nodes;
using Json.Logic.Expressions;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class ReduceTests
{
	[Test]
	public void AddAggregateIsSum()
	{
		var rule = new ReduceRule(
			JsonNode.Parse("[1, 2, 3]"),
			new AddRule(new VariableRule("current"), new VariableRule("accumulator")),
			0m);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(6m, expression.Compile()(null));
	}

	[Test]
	public void StringAggregateIsConcat()
	{
		var rule = new ReduceRule(
			JsonNode.Parse("""
				["ab", "cd", "ef"]
				"""),
			new CatRule(new VariableRule("accumulator"), new VariableRule("current")),
			"");

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);
		Assert.AreEqual("abcdef", expression.Compile()(null));
	}
}