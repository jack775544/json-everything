using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests;

public class StrictNotEqualsTests
{
	[Test]
	public void NotEqualReturnsTrue()
	{
		var rule = new StrictNotEqualsRule(1, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void EqualsReturnsFalse()
	{
		var rule = new StrictNotEqualsRule(1, 1);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}
}