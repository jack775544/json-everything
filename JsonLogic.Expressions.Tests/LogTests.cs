using System;
using Json.Logic.Expressions;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class LogTests
{
	[Test]
	public void LogNotImplemented()
	{
		var rule = new LogRule("Nothing");
		Assert.Throws<NotImplementedException>(() => RuleExpressionRegistry.Current.CreateRuleExpression<object>(rule));
	}
}