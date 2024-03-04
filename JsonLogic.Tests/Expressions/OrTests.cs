using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class OrTests
{
	[TestCase(true, true, false)]
	[TestCase(false, false, false)]
	[TestCase(true, false, true)]
	[TestCase(true, true, true)]
	[TestCase(true, true, true, true)]
	[TestCase(true, true, true, false)]
	[TestCase(false, false, false, false)]
	[TestCase(false, false)]
	[TestCase(true, true)]
	public void OrReturnsCorrectly(bool result, params bool[] conditions)
	{
		var conditionRules = conditions.Select(x => new LiteralRule(x) as Rule).ToArray();
		var rule = new OrRule(result, conditionRules);
		var expression = ExpressionTestHelpers.CreateRuleExpression<bool>(rule);
		Assert.AreEqual(result, expression.Compile()(null));
	}
}