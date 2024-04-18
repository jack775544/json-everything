using Json.Logic.Expressions;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class MinTests
{
	[Test]
	public void SingleValueReturnsMin()
	{
		var rule = new MinRule(3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(3m, expression.Compile()(null));
	}

	[Test]
	public void TwoValuesReturnsMin()
	{
		var rule = new MinRule(3, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(2m, expression.Compile()(null));
	}
	
	[Test]
	public void ThreeValuesReturnsMin()
	{
		var rule = new MinRule(3, 2, 4);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(2m, expression.Compile()(null));
	}
}