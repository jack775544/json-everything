using System;
using System.Globalization;
using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class StrictEqualsTests
{
	[Test]
	public void NotEqualReturnsFalse()
	{
		var rule = new StrictEqualsRule(1, 2);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void EqualsReturnsTrue()
	{
		var rule = new StrictEqualsRule(1, 1);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void DateTimeEqualsReturnsTrue()
	{
		var now = DateTime.UtcNow;
		var nowLimitedPrecision = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, now.Second);
		var rule = new StrictEqualsRule(nowLimitedPrecision.ToString("o"), new VariableRule(""));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<DateTime, bool>(rule);
		Assert.IsTrue(expression.Compile()(nowLimitedPrecision));
	}

	[Test]
	public void CulturedDateTimeEqualsReturnsTrue()
	{
		var culture = CultureInfo.GetCultureInfo("en-AU");
		var now = DateTime.UtcNow;
		// g format has no seconds component
		var nowLimitedPrecision = new DateTime(now.Year, now.Month, now.Day, now.Hour, now.Minute, 0);
		var rule = new StrictEqualsRule(nowLimitedPrecision.ToString("g", culture), new VariableRule(""));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<DateTime, bool>(rule, new CreateExpressionOptions
		{
			CultureInfo = culture,
		});
		Assert.IsTrue(expression.Compile()(nowLimitedPrecision));
	}

	public enum Testing
	{
		A,
		B,
	}

	private record EnumTest(Testing Field, Testing? NullableField);

	[TestCase(nameof(EnumTest.Field), null, "\"A\"", true)]
	[TestCase(nameof(EnumTest.Field), null, "\"B\"", false)]
	[TestCase(nameof(EnumTest.NullableField), Testing.B, "\"B\"", true)]
	[TestCase(nameof(EnumTest.NullableField), Testing.B, "\"A\"", false)]
	[TestCase(nameof(EnumTest.NullableField), Testing.B, "null", false)]
	[TestCase(nameof(EnumTest.NullableField), null, "\"A\"", false)]
	[TestCase(nameof(EnumTest.NullableField), null, "null", true)]
	public void EnumEquals(string compareField, Testing? nullableField, string comparator, bool expected)
	{
		var record = new EnumTest(Testing.A, nullableField);
		var rule = new StrictEqualsRule(
			new VariableRule(compareField),
			new LiteralRule(JsonNode.Parse(comparator)));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<EnumTest, bool>(rule);
		Assert.AreEqual(expected, expression.Compile()(record));
	}
}