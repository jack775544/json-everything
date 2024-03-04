﻿using System;
using System.Collections.Generic;
using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class MergeTests
{
	[Test]
	public void MergedArraysAreMerged()
	{
		var rule = new MergeRule(
			JsonNode.Parse("[1, 2, 3]"),
			JsonNode.Parse("[4, 5, 6]"));

		var expression = ExpressionTestHelpers.CreateRuleExpression<IEnumerable<decimal>>(rule);
		Assert.That(expression.Compile()(null), Is.EquivalentTo(new decimal[] { 1, 2, 3, 4, 5, 6 }));
	}

	[Test]
	public void SingleArrayIsSame()
	{
		var rule = new MergeRule(JsonNode.Parse("[1, 2, 3]"));

		var expression = ExpressionTestHelpers.CreateRuleExpression<IEnumerable<decimal>>(rule);
		Assert.That(expression.Compile()(null), Is.EquivalentTo(new decimal[] { 1, 2, 3 }));
	}

	[Test]
	public void MergedMixedArraysAreMerged()
	{
		var rule = new MergeRule(
			JsonNode.Parse("[1, 2, 3]"),
			JsonNode.Parse("[4, 5, 6]"),
			7);

		var expression = ExpressionTestHelpers.CreateRuleExpression<IEnumerable<decimal>>(rule);
		Assert.That(expression.Compile()(null), Is.EquivalentTo(new decimal[] { 1, 2, 3, 4, 5, 6, 7 }));
	}
	
	[Test]
	public void SingleValueIsArray()
	{
		var rule = new MergeRule(7);

		var expression = ExpressionTestHelpers.CreateRuleExpression<IEnumerable<decimal>>(rule);
		Assert.That(expression.Compile()(null), Is.EquivalentTo(new decimal[] { 7 }));
	}

	[Test]
	public void MergedEmptyIsEmpty()
	{
		var rule = new MergeRule();

		var expression = ExpressionTestHelpers.CreateRuleExpression<IEnumerable<object>>(rule);
		Assert.That(expression.Compile()(null), Is.EquivalentTo(Array.Empty<object>()));
	}
}