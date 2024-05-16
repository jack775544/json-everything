using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class SubstrTests
{
	[Test]
	public void SubstrStartNoCount()
	{
		var rule = new SubstrRule("foobar", 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);
		Assert.AreEqual("bar", expression.Compile()(null));
	}

	[Test]
	public void SubstrStartCount()
	{
		var rule = new SubstrRule("foobar", 3, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<string>(rule);
		Assert.AreEqual("ba", expression.Compile()(null));
	}
}