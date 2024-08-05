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

		Assert.AreEqual(1, expression.Compile()());
	}

	[Test]
	public void IfStandardReturnsFalseResult()
	{
		var rule = new IfRule(false, 1, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(2, expression.Compile()());
	}

	[Test]
	public void IfStandardReturnsSecondTrueResult()
	{
		var rule = new IfRule(false, 1, true, 2, 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(2, expression.Compile()());
	}

	[Test]
	public void IfStandardReturnsSecondFalseResult()
	{
		var rule = new IfRule(false, 1, false, 2, 3);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule);

		Assert.AreEqual(3, expression.Compile()());
	}

	private record IfVarTestData(string Name);
	
	[Test]
	public void IfVarReturnsTrue()
	{
		var rule = new IfRule(
			new LooseEqualsRule(new VariableRule(nameof(IfVarTestData.Name)), "heyo"),
			new VariableRule(nameof(IfVarTestData.Name)),
			"ayo");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<IfVarTestData, string>(rule);

		Assert.AreEqual("heyo", expression.Compile()(new IfVarTestData("heyo")));
	}
}