using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class BooleanCastTests
{
	[Test]
	public void EmptyArrayIsFalse()
	{
		var rule = new BooleanCastRule(JsonNode.Parse("[]"));

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()());
	}

	[Test]
	public void NonEmptyArrayIsTrue()
	{
		var rule = new BooleanCastRule(JsonNode.Parse("[1]"));

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()());
	}

	[Test]
	public void EmptyStringIsFalse()
	{
		var rule = new BooleanCastRule("");

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()());
	}

	[Test]
	public void NonEmptyStringIsTrue()
	{
		var rule = new BooleanCastRule("foo");

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()());
	}

	[Test]
	public void ZeroIsFalse()
	{
		var rule = new BooleanCastRule(0);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()());
	}

	[Test]
	public void NonZeroIsTrue()
	{
		var rule = new BooleanCastRule(1);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()());
	}

	[Test]
	public void FalseIsFalse()
	{
		var rule = new BooleanCastRule(false);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()());
	}

	[Test]
	public void TrueIsTrue()
	{
		var rule = new BooleanCastRule(true);

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()());
	}

	[Test]
	public void NullIsFalse()
	{
		var rule = new BooleanCastRule(JsonNode.Parse("null"));

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()());
	}
	
	[TestCase("0", false)]
	[TestCase("1", true)]
	[TestCase("-1", true)]
	[TestCase("[]", false)]
	[TestCase("[1,2]", true)]
	[TestCase("\"\"", false)]
	[TestCase("\"anything\"", true)]
	[TestCase("null", false)]
	public void Truthiness(string text, bool expected)
	{
		var rule = new BooleanCastRule(JsonNode.Parse(text));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.AreEqual(expected, expression.Compile()());
	}
}