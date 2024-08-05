using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class MultiplyTests
{
	[Test]
	public void MultiplyNumbersReturnsSum()
	{
		var rule = new MultiplyRule(4, 5);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(20, expression.Compile()());
	}

	[Test]
	public void Multiply3NumbersReturnsSum()
	{
		var rule = new MultiplyRule(4, 5, 5);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(100, expression.Compile()());
	}

	[Test]
	public void MultiplySingleNumbersReturnsNumber()
	{
		var rule = new MultiplyRule(4);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(4, expression.Compile()());
	}
	
	[Test]
	public void MultiplyNumberStringReturnsSum()
	{
		var rule = new MultiplyRule(4, "5");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(20, expression.Compile()());
	}
}