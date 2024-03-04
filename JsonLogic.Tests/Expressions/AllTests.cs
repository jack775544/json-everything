using System.Text.Json.Nodes;
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

		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
		// JsonAssert.IsTrue(rule.Apply());
	}

	[Test]
	public void AllDoNotMatchCondition()
	{
		var rule = new AllRule(JsonNode.Parse("[1,-2,3]"),
			new MoreThanRule(new VariableRule(""), 0));

		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}
}