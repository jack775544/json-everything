using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests;

public class InTests
{
	[Test]
	public void InTwoStringsSecondContainsFirstReturnsTrue()
	{
		var rule = new InRule("foo", "food");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void InTwoStringsNoMatchReturnsFalse()
	{
		var rule = new InRule("foo", "bar");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void InStringContainsNumberReturnsTrue()
	{
		var rule = new InRule(4, "foo4bar");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void InStringContainsBooleanReturnsTrue()
	{
		var rule = new InRule(true, "footruebar");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void InStringContainsNullReturnsFalse()
	{
		var rule = new InRule(true, "foo");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void InArrayContainsFirstReturnsTrue()
	{
		var array = new JsonArray(1, 2, 3);
		var rule = new InRule(2, array);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void InArrayDoesNotContainFirstReturnsFalse()
	{
		var array = new JsonArray(1, 2, 3);
		var rule = new InRule(5, array);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void InNullThrowsError()
	{
		var rule = new InRule(1, LiteralRule.Null);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void InBooleanThrowsError()
	{
		var rule = new InRule(1, false);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void InNumberThrowsError()
	{
		var rule = new InRule(1, 4);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsFalse(expression.Compile()(null));
	}
}