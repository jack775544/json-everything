using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class NotTests
{
	[Test]
	public void EmptyArrayIsTrue()
	{
		var rule = new NotRule(JsonNode.Parse("[]"));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void NonEmptyArrayIsFalse()
	{
		var rule = new NotRule(JsonNode.Parse("[1]"));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void EmptyStringIsTrue()
	{
		var rule = new NotRule("");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void NonEmptyStringIsFalse()
	{
		var rule = new NotRule("foo");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void ZeroIsTrue()
	{
		var rule = new NotRule(0);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void NonZeroIsFalse()
	{
		var rule = new NotRule(1);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void FalseIsTrue()
	{
		var rule = new NotRule(false);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void TrueIsFalse()
	{
		var rule = new NotRule(true);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void NullIsTrue()
	{
		var rule = new NotRule(JsonNode.Parse("null"));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
}