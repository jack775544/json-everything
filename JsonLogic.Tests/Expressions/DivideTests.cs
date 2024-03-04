using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class DivideTests
{
	[Test]
	public void DivideNumbersReturnsSum()
	{
		var rule = new DivideRule(4, 5);
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(.8m, expression.Compile()(null));
	}
}