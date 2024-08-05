using System.Linq;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class AndTests
{
	[TestCase(false, true, false)]
	[TestCase(false, false, false)]
	[TestCase(false, false, true)]
	[TestCase(true, true, true)]
	[TestCase(true, true, true, true)]
	[TestCase(false, true, true, false)]
	[TestCase(false, false)]
	[TestCase(true, true)]
	public void AndReturnsCorrectly(bool result, params bool[] conditions)
	{
		var conditionRules = conditions.Select(x => new LiteralRule(x) as Rule).ToArray();
		var rule = new AndRule(result, conditionRules);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.AreEqual(result, expression.Compile()());
	}
}