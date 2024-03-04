﻿using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class FilterTests
{
	[Test]
	public void MapRuleReturnsCorrectValues()
	{
		var rule = new FilterRule(JsonNode.Parse("[1,2,3,4]"),
			new StrictEqualsRule(new VariableRule(""), 2));
		var expression = ExpressionTestHelpers.CreateRuleExpression<IEnumerable<decimal>>(rule);
		Assert.That(expression.Compile()(null), Is.EquivalentTo(new []{ 2m }));
	}

	[Test]
	public void MapRuleReturnsDoubles()
	{
		var rule = new FilterRule(JsonNode.Parse("[1,2,3,4]"),
			new StrictEqualsRule(
				new ModRule(new VariableRule(""), 2m),
				0m));
		var expression = ExpressionTestHelpers.CreateRuleExpression<IEnumerable<decimal>>(rule);
		Assert.That(expression.Compile()(null), Is.EquivalentTo(new []{ 2m, 4m }));
	}
}