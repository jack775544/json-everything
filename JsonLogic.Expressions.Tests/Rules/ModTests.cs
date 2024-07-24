using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class ModTests
{
	[Test]
	public void ModNumbersReturnsSum()
	{
		var rule = new ModRule(4, 5);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(4, expression.Compile()(null));
	}

	[Test]
	public void ModNumbersReturnsRemainder()
	{
		var rule = new ModRule(5, 4);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(1, expression.Compile()(null));
	}
}