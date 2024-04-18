using System.Collections.Generic;
using Json.Logic.Expressions;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class AddTests
{
	[Test]
	public void AddNumbersReturnsSum()
	{
		var rule = new AddRule(4, 5);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(9, expression.Compile()(null));
	}

	[Test]
	public void AddNumberAndStringReturnsSum()
	{
		var data = new KeyValuePair<int, int>(4, 2);
		var rule = new AddRule(new VariableRule("Key"), "5");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<KeyValuePair<int, int>, int>(rule);

		Assert.AreEqual(9, expression.Compile()(data));
	}

	[Test]
	public void AddSingleNumberDoesNothing()
	{
		var rule = new AddRule(3.14);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(3.14, expression.Compile()(null));
	}

	[Test]
	public void AddSingleStringWithNumberCasts()
	{
		var rule = new AddRule("3.14");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(3.14, expression.Compile()(null));
	}

	[Test]
	public void AddSingleTrueThrowsError()
	{
		var rule = new AddRule(true);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<object>(rule);

		Assert.AreEqual(1, expression.Compile()(null));
	}

	[Test]
	public void AddSingleFalseThrowsError()
	{
		var rule = new AddRule(false);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<object>(rule);

		Assert.AreEqual(0, expression.Compile()(null));
	}

	[Test]
	public void AddSingleNullReturns0()
	{
		var rule = new AddRule(LiteralRule.Null);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<int>(rule);

		Assert.AreEqual(0, expression.Compile()(null));
	}
}