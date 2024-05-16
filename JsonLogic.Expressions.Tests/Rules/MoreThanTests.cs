using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class MoreThanTests
{
	[Test]
	public void MoreThanTwoNumbersReturnsTrue()
	{
		var rule = new MoreThanRule(2, 1);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void EqualTwoNumbersReturnsFalse()
	{
		var rule = new MoreThanRule(1, 1);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void MoreThanTwoNumbersReturnsFalse()
	{
		var rule = new MoreThanRule(1, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void MoreThanBooleanThrowsError()
	{
		var rule = new MoreThanRule(false, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void MoreThanNullCastsNullToZero()
	{
		var rule = new MoreThanRule(LiteralRule.Null, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}
}