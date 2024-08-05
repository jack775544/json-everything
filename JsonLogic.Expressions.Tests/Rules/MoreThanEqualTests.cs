using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class MoreThanEqualTests
{
	[Test]
	public void MoreThanEqualTwoNumbersReturnsTrue()
	{
		var rule = new MoreThanEqualRule(2, 1);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()());
	}
	
	[Test]
	public void MoreThanEqualNumberStringReturnsTrue()
	{
		var rule = new MoreThanEqualRule(2, "1");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()());
	}

	[Test]
	public void EqualTwoNumbersReturnsTrue()
	{
		var rule = new MoreThanEqualRule(1, 1);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()());
	}

	[Test]
	public void MoreThanEqualTwoNumbersReturnsFalse()
	{
		var rule = new MoreThanEqualRule(2, 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()());
	}

	[Test]
	public void MoreThanEqualBooleanThrowsError()
	{
		var rule = new MoreThanEqualRule(false, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()());
	}

	[Test]
	public void MoreThanEqualNullCastsNullToZero()
	{
		var rule = new MoreThanEqualRule(LiteralRule.Null, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()());
	}
}