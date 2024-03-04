using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class SubstrTests
{
	[Test]
	public void SubstrStartNoCount()
	{
		var rule = new SubstrRule("foobar", 3);
		var expression = ExpressionTestHelpers.CreateRuleExpression<string>(rule);
		Assert.AreEqual("bar", expression.Compile()(null));
	}

	[Test]
	public void SubstrStartBeyondLengthNoCount()
	{
		var rule = new SubstrRule("foobar", 10);
		var expression = ExpressionTestHelpers.CreateRuleExpression<string>(rule);
		Assert.AreEqual(string.Empty, expression.Compile()(null));
	}

	[Test]
	public void SubstrNegativeStartNoCount()
	{
		var rule = new SubstrRule("foobar", -2);
		var expression = ExpressionTestHelpers.CreateRuleExpression<string>(rule);
		Assert.AreEqual("ar", expression.Compile()(null));
	}

	[Test]
	public void SubstrNegativeStartBeyondLengthNoCount()
	{
		var rule = new SubstrRule("foobar", -10);
		var expression = ExpressionTestHelpers.CreateRuleExpression<string>(rule);
		Assert.AreEqual("foobar", expression.Compile()(null));
	}

	[Test]
	public void SubstrStartCount()
	{
		var rule = new SubstrRule("foobar", 3, 2);
		var expression = ExpressionTestHelpers.CreateRuleExpression<string>(rule);
		Assert.AreEqual("ba", expression.Compile()(null));
	}

	[Test]
	public void SubstrStartCountBeyondLength()
	{
		var rule = new SubstrRule("foobar", 3, 5);
		var expression = ExpressionTestHelpers.CreateRuleExpression<string>(rule);
		Assert.AreEqual("bar", expression.Compile()(null));
	}

	[Test]
	public void SubstrStartNegativeCount()
	{
		var rule = new SubstrRule("foobar", 2, -1);
		var expression = ExpressionTestHelpers.CreateRuleExpression<string>(rule);
		Assert.AreEqual("oba", expression.Compile()(null));
	}

	[Test]
	public void SubstrStartNegativeCountBeyondLength()
	{
		var rule = new SubstrRule("foobar", 2, -10);
		var expression = ExpressionTestHelpers.CreateRuleExpression<string>(rule);
		Assert.AreEqual(string.Empty, expression.Compile()(null));
	}
}