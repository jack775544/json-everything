using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class StrictEqualsTests
{
	[Test]
	public void NotEqualReturnsFalse()
	{
		var rule = new StrictEqualsRule(1, 2);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void EqualsReturnsTrue()
	{
		var rule = new StrictEqualsRule(1, 1);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void LooseEqualsReturnsFalse()
	{
		var rule = new StrictEqualsRule(1, "1");
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}
}