using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class MultiplyTests
{
	[Test]
	public void MultiplyNumbersReturnsSum()
	{
		var rule = new MultiplyRule(4, 5);
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(20, expression.Compile()(null));
	}

	[Test]
	public void Multiply3NumbersReturnsSum()
	{
		var rule = new MultiplyRule(4, 5, 5);
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(100, expression.Compile()(null));
	}

	[Test]
	public void MultiplySingleNumbersReturnsNumber()
	{
		var rule = new MultiplyRule(4);
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(4, expression.Compile()(null));
	}
	
	[Test]
	public void MultiplyNumberStringReturnsSum()
	{
		var rule = new MultiplyRule(4, "5");
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(20, expression.Compile()(null));
	}
}