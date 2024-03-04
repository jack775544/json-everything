using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class MapTests
{
	[Test]
	public void MapRuleReturnsCorrectValues()
	{
		var rule = new MapRule(JsonNode.Parse("[1,2,3]"),
			new StrictEqualsRule(new VariableRule(""), 2));
		var expression = ExpressionTestHelpers.CreateRuleExpression<IEnumerable<bool>>(rule);
		Assert.That(expression.Compile()(null), Is.EquivalentTo(new []{ false, true, false }));
	}

	[Test]
	public void MapRuleReturnsDoubles()
	{
		var rule = new MapRule(JsonNode.Parse("[1,2,3]"),
			new MultiplyRule(new VariableRule(""), 2M));
		var expression = ExpressionTestHelpers.CreateRuleExpression<IEnumerable<decimal>>(rule);
		Assert.That(expression.Compile()(null), Is.EquivalentTo(new []{ 2M, 4M, 6M }));
	}
}