using System;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class LogTests
{
	[Test]
	public void LogNotImplemented()
	{
		var rule = new LogRule("Nothing");
		Assert.Throws<NotImplementedException>(() => RuleExpressionRegistry.Current.CreateRuleExpression<object>(rule));
	}
}