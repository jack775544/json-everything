using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests;

public class CatTests
{
	[Test]
	public void CatTwoStringsConcatsValues()
	{
		var rule = new CatRule("foo", "bar");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);

		Assert.AreEqual("foobar", expression.Compile()(null));
	}

	[Test]
	public void CatStringAndNullConcatsValues()
	{
		var rule = new CatRule("foo", LiteralRule.Null);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);

		Assert.AreEqual("foo", expression.Compile()(null));
	}

	[Test]
	public void CatStringAndNumberConcatsValues()
	{
		var rule = new CatRule("foo", 1);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);

		Assert.AreEqual("foo1", expression.Compile()(null));
	}

	[Test]
	public void CatStringAndBooleanConcatsValues()
	{
		var rule = new CatRule("foo", true);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);

		Assert.AreEqual("footrue", expression.Compile()(null));
	}

	[Test]
	public void CatStringAndArrayConcatsValues()
	{
		var array = new JsonArray(1, 2, 3);
		var rule = new CatRule("foo", array);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);

		Assert.AreEqual("foo1,2,3", expression.Compile()(null));
	}
}