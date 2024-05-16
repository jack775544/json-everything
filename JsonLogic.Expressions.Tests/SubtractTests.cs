using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests;

public class SubtractTests
{
	[Test]
	public void SubtractNumbersReturnsSum()
	{
		var rule = new SubtractRule(4, 5);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(-1, expression.Compile()(null));
	}

	[Test]
	public void SubtractNumberAndStringReturnsSum()
	{
		var rule = new SubtractRule(4, "5");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(-1, expression.Compile()(null));
	}

	[Test]
	public void SubtractSingleNumber()
	{
		var rule = new SubtractRule(4);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(-4, expression.Compile()(null));
	}
	
	[Test]
	public void SubtractThreeNumbers()
	{
		var rule = new SubtractRule(4, 3, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(-1, expression.Compile()(null));
	}
}