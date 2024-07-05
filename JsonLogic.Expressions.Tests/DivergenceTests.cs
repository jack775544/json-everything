using System;
using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests;

/// <summary>
/// These tests are for displaying logic functions that have differences between regular and expression execution.
/// </summary>
public class DivergenceTests
{
	/// <summary>
	/// This test throws since object literals are not implemented.
	/// </summary>
	[Test]
	public void EmptyObjectIsFalse()
	{
		var rule = new BooleanCastRule(JsonNode.Parse("{}"));

		Assert.Throws<NotImplementedException>(() => RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule));
	}

	/// <summary>
	/// This test throws since object literals are not implemented.
	/// </summary>
	[Test]
	public void NonEmptyObjectIsTrue()
	{
		var rule = new BooleanCastRule(JsonNode.Parse("{\"foo\":1}"));

		Assert.Throws<NotImplementedException>(() => RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule));
	}

	/// <summary>
	/// This test throws since array literals must always contain elements of the same type.
	/// </summary>
	[Test]
	public void CatStringAndNestedArrayConcatsValues()
	{
		var array = new JsonArray(1, 2, 3);
		var nestedArray = new JsonArray(1, array, 3);
		var rule = new CatRule("foo", nestedArray);
		Assert.Throws<JsonLogicException>(() => RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule));
	}

	/// <summary>
	/// This test throws since object literals are not implemented.
	/// </summary>
	[Test]
	public void CatStringAndObjectConcatsValues()
	{
		var rule = new CatRule("foo", JsonNode.Parse("{}"));
		Assert.Throws<NotImplementedException>(() => RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule));
	}

	/// <summary>
	/// This test throws since object literals are not implemented.
	/// </summary>
	[Test]
	public void InObjectThrowsError()
	{
		var rule = new InRule(1, JsonNode.Parse("{}"));
		Assert.Throws<NotImplementedException>(() => RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule));
	}

	/// <summary>
	/// This test throws since object literals are not implemented.
	/// </summary>
	[Test]
	public void InStringContainsObjectThrowsError()
	{
		var rule = new InRule(JsonNode.Parse("{}"), "foo");
		Assert.Throws<NotImplementedException>(() => RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule));
	}

	/// <summary>
	/// This test throws since object literals are not implemented.
	/// </summary>
	[Test]
	public void EmptyObjectIsTrue()
	{
		var rule = new NotRule(JsonNode.Parse("{}"));
		Assert.Throws<NotImplementedException>(() => RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule));
	}

	/// <summary>
	/// This test throws since object literals are not implemented.
	/// </summary>
	[Test]
	public void NonEmptyObjectIsFalse()
	{
		var rule = new NotRule(JsonNode.Parse("{\"foo\":5}"));
		Assert.Throws<NotImplementedException>(() => RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule));
	}

	/// <summary>
	/// This test contains an inverted assertion (IsTrue instead of IsFalse) since only loose equals is implemented.
	/// </summary>
	[Test]
	public void LooseEqualsReturnsFalse()
	{
		var rule = new StrictEqualsRule(1, "1");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	/// <summary>
	/// This test contains an inverted assertion (IsFalse instead of IsTrue) since only loose equals is implemented.
	/// </summary>
	[Test]
	public void StrictNotEqualsReturnsTrue()
	{
		var rule = new StrictNotEqualsRule(1, "1");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	/// <summary>
	/// This test throws since the start value of substr is greater than the length of the string.
	/// This is since the JsonLogic substr rule is less strict than the .NET one.
	/// </summary>
	[Test]
	public void SubstrStartBeyondLengthNoCount()
	{
		var rule = new SubstrRule("foobar", 10);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);
		Assert.Throws<ArgumentOutOfRangeException>(() => expression.Compile()(null));
	}

	/// <summary>
	/// This test throws since the start value of substr is less than 0.
	/// This is since the JsonLogic substr rule is less strict than the .NET one.
	/// </summary>
	[Test]
	public void SubstrNegativeStartNoCount()
	{
		var rule = new SubstrRule("foobar", -2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);
		Assert.Throws<ArgumentOutOfRangeException>(() => expression.Compile()(null));
	}

	/// <summary>
	/// This test throws since the start value of substr is less than 0.
	/// This is since the JsonLogic substr rule is less strict than the .NET one.
	/// </summary>
	[Test]
	public void SubstrNegativeStartBeyondLengthNoCount()
	{
		var rule = new SubstrRule("foobar", -10);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);
		Assert.Throws<ArgumentOutOfRangeException>(() => expression.Compile()(null));
	}

	/// <summary>
	/// This test throws since the length of the substring goes out of bounds of the string.
	/// This is since the JsonLogic substr rule is less strict than the .NET one.
	/// </summary>
	[Test]
	public void SubstrStartCountBeyondLength()
	{
		var rule = new SubstrRule("foobar", 3, 5);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);
		Assert.Throws<ArgumentOutOfRangeException>(() => expression.Compile()(null));
	}

	/// <summary>
	/// This test throws since negative lengths for the substring function are not supported.
	/// This is since the JsonLogic substr rule is less strict than the .NET one.
	/// </summary>
	[Test]
	public void SubstrStartNegativeCount()
	{
		var rule = new SubstrRule("foobar", 2, -1);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);
		Assert.Throws<ArgumentOutOfRangeException>(() => expression.Compile()(null));
	}

	/// <summary>
	/// This test throws since negative lengths for the substring function are not supported.
	/// This is since the JsonLogic substr rule is less strict than the .NET one.
	/// </summary>
	[Test]
	public void SubstrStartNegativeCountBeyondLength()
	{
		var rule = new SubstrRule("foobar", 2, -10);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);
		Assert.Throws<ArgumentOutOfRangeException>(() => expression.Compile()(null));
	}
}