using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class MaxTests
{
	[Test]
	public void SingleValueReturnsMax()
	{
		var rule = new MaxRule(3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(3m, expression.Compile()());
	}

	[Test]
	public void TwoValuesReturnsMax()
	{
		var rule = new MaxRule(3, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(3m, expression.Compile()());
	}
	
	[Test]
	public void ThreeValuesReturnsMax()
	{
		var rule = new MaxRule(3, 2, 4);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(4m, expression.Compile()());
	}
}