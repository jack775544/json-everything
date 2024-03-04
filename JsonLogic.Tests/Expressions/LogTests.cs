using System;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class LogTests
{
	[Test]
	public void LogNotImplemented()
	{
		var rule = new LogRule("Nothing");
		Assert.Throws<NotImplementedException>(() => ExpressionTestHelpers.CreateRuleExpression<object>(rule));
	}
}