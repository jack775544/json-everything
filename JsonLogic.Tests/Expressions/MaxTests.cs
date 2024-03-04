using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class MaxTests
{
	[Test]
	public void SingleValueReturnsMax()
	{
		var rule = new MaxRule(3);
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(3m, expression.Compile()(null));
	}

	[Test]
	public void TwoValuesReturnsMax()
	{
		var rule = new MaxRule(3, 2);
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(3m, expression.Compile()(null));
	}
	
	[Test]
	public void ThreeValuesReturnsMax()
	{
		var rule = new MaxRule(3, 2, 4);
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(4m, expression.Compile()(null));
	}
}