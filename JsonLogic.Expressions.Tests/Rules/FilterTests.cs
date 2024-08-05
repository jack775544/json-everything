using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class FilterTests
{
	[Test]
	public void FilterRuleReturnsCorrectValues()
	{
		var rule = new FilterRule(JsonNode.Parse("[1,2,3,4]"),
			new StrictEqualsRule(new VariableRule(""), 2));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<IEnumerable<decimal>>(rule);
		Assert.That(expression.Compile()(), Is.EquivalentTo(new []{ 2m }));
	}

	[Test]
	public void FilterRuleReturnsDoubles()
	{
		var rule = new FilterRule(JsonNode.Parse("[1,2,3,4]"),
			new StrictEqualsRule(
				new ModRule(new VariableRule(""), 2m),
				0m));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<IEnumerable<decimal>>(rule);
		Assert.That(expression.Compile()(), Is.EquivalentTo(new []{ 2m, 4m }));
	}
}