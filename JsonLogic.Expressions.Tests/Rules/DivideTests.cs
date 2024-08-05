using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class DivideTests
{
	[Test]
	public void DivideNumbersReturnsSum()
	{
		var rule = new DivideRule(4, 5);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(.8m, expression.Compile()());
	}
}