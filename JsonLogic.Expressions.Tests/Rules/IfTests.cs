using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class IfTests
{
	[Test]
	public void IfStandardReturnsTrueResult()
	{
		var rule = new IfRule(true, 1, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(1, expression.Compile()(null));
	}

	[Test]
	public void IfStandardReturnsFalseResult()
	{
		var rule = new IfRule(false, 1, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(2, expression.Compile()(null));
	}

	[Test]
	public void IfStandardReturnsSecondTrueResult()
	{
		var rule = new IfRule(false, 1, true, 2, 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(2, expression.Compile()(null));
	}

	[Test]
	public void IfStandardReturnsSecondFalseResult()
	{
		var rule = new IfRule(false, 1, false, 2, 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(3, expression.Compile()(null));
	}
}