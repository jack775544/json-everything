using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class LessThanTests
{
	[Test]
	public void LessThanTwoNumbersReturnsTrue()
	{
		var rule = new LessThanRule(1, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void EqualTwoNumbersReturnsFalse()
	{
		var rule = new LessThanRule(1, 1);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void LessThanTwoNumbersReturnsFalse()
	{
		var rule = new LessThanRule(3, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void LessThanBooleanReturnsTrue()
	{
		var rule = new LessThanRule(false, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void LessThanNullCastsNullToZero()
	{
		var rule = new LessThanRule(LiteralRule.Null, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
	
	[Test]
	public void LessThanNumberAndStringNumberReturnsTrue()
	{
		var rule = new LessThanRule(1, "2");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
	
	[Test]
	public void LessThanStringNumberAndNumberReturnsTrue()
	{
		var rule = new LessThanRule("1", 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
	
	[Test]
	public void LessThanTwoStringNumbersReturnsTrue()
	{
		var rule = new LessThanRule("1", "2");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void BetweenValueInRangeReturnsTrue()
	{
		var rule = new LessThanRule(1, 2, 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void BetweenValueAtLowEndReturnsFalse()
	{
		var rule = new LessThanRule(1, 1, 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void BetweenValueUnderLowEndReturnsFalse()
	{
		var rule = new LessThanRule(1, 0, 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void BetweenValueAtHighEndReturnsFalse()
	{
		var rule = new LessThanRule(1, 3, 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void BetweenValueOverHighEndReturnsFalse()
	{
		var rule = new LessThanRule(1, 4, 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void BetweenLowEndNotNumberReturnsFalse()
	{
		var rule = new LessThanRule(false, 4, 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void BetweenValueNotNumberReturnsFalse()
	{
		var rule = new LessThanRule(1, false, 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void BetweenHighEndNotNumberReturnsFalse()
	{
		var rule = new LessThanRule(1, 2, false);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}
	
	[Test]
	public void BetweenStringNumbersInRangeReturnsTrue()
	{
		var rule = new LessThanRule("1", "2", "3");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
	
	[Test]
	public void BetweenLowEndStringNumberReturnsTrue()
	{
		var rule = new LessThanRule("1", 2, 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
	
	[Test]
	public void BetweenValueStringNumberReturnsTrue()
	{
		var rule = new LessThanRule(1, "2", 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
	
	[Test]
	public void BetweenHighEndStringNumberReturnsTrue()
	{
		var rule = new LessThanRule(1, 2, "3");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
}