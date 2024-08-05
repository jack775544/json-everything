using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class CatTests
{
	[Test]
	public void CatTwoStringsConcatsValues()
	{
		var rule = new CatRule("foo", "bar");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);

		Assert.AreEqual("foobar", expression.Compile()());
	}

	[Test]
	public void CatStringAndNullConcatsValues()
	{
		var rule = new CatRule("foo", LiteralRule.Null);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);

		Assert.AreEqual("foo", expression.Compile()());
	}

	[Test]
	public void CatStringAndNumberConcatsValues()
	{
		var rule = new CatRule("foo", 1);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);

		Assert.AreEqual("foo1", expression.Compile()());
	}

	[Test]
	public void CatStringAndBooleanConcatsValues()
	{
		var rule = new CatRule("foo", true);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);

		Assert.AreEqual("footrue", expression.Compile()());
	}

	[Test]
	public void CatStringAndArrayConcatsValues()
	{
		var array = new JsonArray(1, 2, 3);
		var rule = new CatRule("foo", array);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);

		Assert.AreEqual("foo1,2,3", expression.Compile()());
	}
}