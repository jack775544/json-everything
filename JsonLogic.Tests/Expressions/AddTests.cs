using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class AddTests
{
	[Test]
	public void AddNumbersReturnsSum()
	{
		var rule = new AddRule(4, 5);
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(9, expression.Compile()(null));
	}

	[Test]
	public void AddSingleNumberDoesNothing()
	{
		var rule = new AddRule(3.14);
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(3.14, expression.Compile()(null));
	}

	[Test]
	public void AddSingleStringWithNumberCasts()
	{
		var rule = new AddRule("3.14");
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(3.14, expression.Compile()(null));
	}

	[Test]
	public void AddSingleTrueThrowsError()
	{
		var rule = new AddRule(true);
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(1, expression.Compile()(null));
	}

	[Test]
	public void AddSingleFalseThrowsError()
	{
		var rule = new AddRule(false);
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(0, expression.Compile()(null));
	}

	[Test]
	public void AddSingleNullReturns0()
	{
		var rule = new AddRule(LiteralRule.Null);
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(0, expression.Compile()(null));
	}
}