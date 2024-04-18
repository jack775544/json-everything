using System.Text.Json.Nodes;
using Json.Logic.Expressions;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class AllTests
{
	[Test]
	public void AllMatchCondition()
	{
		var rule = new AllRule(JsonNode.Parse("[1,2,3]"),
			new MoreThanRule(new VariableRule(""), 0));

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void AllDoNotMatchCondition()
	{
		int[] data = [1, -2, 3];
		var rule = new AllRule(new VariableRule(""),
			new MoreThanRule(new VariableRule(""), 0));

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<int[], bool>(rule);
		Assert.IsFalse(expression.Compile()(data));
	}
}