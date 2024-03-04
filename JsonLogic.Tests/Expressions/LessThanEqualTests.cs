using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class LessThanEqualTests
{
	[Test]
	public void LessThanTwoNumbersReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, 2);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void EqualTwoNumbersReturnsTrue()
	{
		var rule = new LessThanEqualRule(2, 2);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void LessThanEqualTwoNumbersReturnsFalse()
	{
		var rule = new LessThanEqualRule(3, 2);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void LessThanEqualNullCastsNullToZero()
	{
		var rule = new LessThanEqualRule(LiteralRule.Null, 2);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
	
	[Test]
	public void LessThanEqualNumberAndStringNumberReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, "2");
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
	
	[Test]
	public void LessThanEqualStringNumberAndNumberReturnsTrue()
	{
		var rule = new LessThanEqualRule("1", 1);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
	
	[Test]
	public void LessThanEqualTwoStringNumbersReturnsTrue()
	{
		var rule = new LessThanEqualRule("1", "2");
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
	
	[Test]
	public void BetweenValueInRangeReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, 2, 3);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void BetweenValueAtLowEndReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, 1, 3);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void BetweenValueUnderLowEndReturnsFalse()
	{
		var rule = new LessThanEqualRule(1, 0, 3);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void BetweenValueAtHighEndReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, 3, 3);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void BetweenValueOverHighEndReturnsFalse()
	{
		var rule = new LessThanEqualRule(1, 4, 3);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void BetweenLowEndNotNumberReturnsFalse()
	{
		var rule = new LessThanEqualRule(false, 4, 3);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void BetweenValueNotNumberReturnsFalse()
	{
		var rule = new LessThanEqualRule(1, false, 3);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void BetweenHighEndNotNumberReturnsFalse()
	{
		var rule = new LessThanEqualRule(1, 2, false);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}
	
	[Test]
	public void BetweenLowEndStringNumberReturnsTrue()
	{
		var rule = new LessThanEqualRule("1", 1, 3);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
	
	[Test]
	public void BetweenValueStringNumberReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, "2", 3);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
	
	[Test]
	public void BetweenHighEndStringNumberReturnsTrue()
	{
		var rule = new LessThanEqualRule(1, 2, "3");
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}
}