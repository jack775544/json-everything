using System.Text.Json.Nodes;
using Json.Logic.Expressions;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class BooleanCastTests
{
	[Test]
	public void EmptyArrayIsFalse()
	{
		var rule = new BooleanCastRule(JsonNode.Parse("[]"));

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void NonEmptyArrayIsTrue()
	{
		var rule = new BooleanCastRule(JsonNode.Parse("[1]"));

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void EmptyStringIsFalse()
	{
		var rule = new BooleanCastRule("");

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void NonEmptyStringIsTrue()
	{
		var rule = new BooleanCastRule("foo");

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void ZeroIsFalse()
	{
		var rule = new BooleanCastRule(0);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void NonZeroIsTrue()
	{
		var rule = new BooleanCastRule(1);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void FalseIsFalse()
	{
		var rule = new BooleanCastRule(false);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void TrueIsTrue()
	{
		var rule = new BooleanCastRule(true);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void NullIsFalse()
	{
		var rule = new BooleanCastRule(JsonNode.Parse("null"));

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void EmptyObjectIsFalse()
	{
		var rule = new BooleanCastRule(JsonNode.Parse("{}"));

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void NonEmptyObjectIsTrue()
	{
		var rule = new BooleanCastRule(JsonNode.Parse("{\"foo\":1}"));

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
}