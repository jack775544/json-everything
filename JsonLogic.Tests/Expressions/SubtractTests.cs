using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class SubtractTests
{
	[Test]
	public void SubtractNumbersReturnsSum()
	{
		var rule = new SubtractRule(4, 5);
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(-1, expression.Compile()(null));
	}

	[Test]
	public void SubtractNumberAndStringReturnsSum()
	{
		var rule = new SubtractRule(4, "5");
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);
		Assert.AreEqual(-1, expression.Compile()(null));
	}
}