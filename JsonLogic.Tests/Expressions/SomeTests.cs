using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class SomeTests
{
	[Test]
	public void SomeMatchCondition()
	{
		var rule = new SomeRule(JsonNode.Parse("[1,2,3]"),
			new StrictEqualsRule(new VariableRule(""), 2));
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void SomeDoNotMatchCondition()
	{
		var rule = new SomeRule(JsonNode.Parse("[1,2,3]"),
			new StrictEqualsRule(new VariableRule(""), 0));
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}
}