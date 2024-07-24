using System;
using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Expressions.Tests.Rules;

public class InTests
{
	[Test]
	public void InTwoStringsSecondContainsFirstReturnsTrue()
	{
		var rule = new InRule("foo", "food");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);
		
		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void InTwoStringsNoMatchReturnsFalse()
	{
		var rule = new InRule("foo", "bar");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void InStringContainsNumberReturnsTrue()
	{
		var rule = new InRule(4, "foo4bar");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void InStringContainsBooleanReturnsTrue()
	{
		var rule = new InRule(true, "footruebar");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void InStringContainsNullReturnsFalse()
	{
		var rule = new InRule(true, "foo");
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void InArrayContainsFirstReturnsTrue()
	{
		var array = new JsonArray(1, 2, 3);
		var rule = new InRule(2, array);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsTrue(expression.Compile()(null));
	}

	[Test]
	public void InArrayDoesNotContainFirstReturnsFalse()
	{
		var array = new JsonArray(1, 2, 3);
		var rule = new InRule(5, array);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void InNullThrowsError()
	{
		var rule = new InRule(1, LiteralRule.Null);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.Throws<NullReferenceException>(() => expression.Compile()(null));
	}

	[Test]
	public void InBooleanThrowsError()
	{
		var rule = new InRule(1, false);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsFalse(expression.Compile()(null));
	}

	[Test]
	public void InNumberThrowsError()
	{
		var rule = new InRule(1, 4);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<bool>(rule);

		Assert.IsFalse(expression.Compile()(null));
	}

	private record InTestData<T>(T Data);

	[Test]
	public void GuidArrayContainsString() => InTestLogic(Guid.NewGuid(), Guid.NewGuid(), guid => guid.ToString());
	[Test]
	public void NullableGuidArrayContainsString() => InTestLogic<Guid?>(Guid.NewGuid(), Guid.NewGuid(), guid => guid?.ToString());
	[Test]
	public void DateTimeArrayContainsString() => InTestLogic(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1), dt => dt.ToString("O"));
	[Test]
	public void NullableDateTimeArrayContainsString() => InTestLogic<DateTime?>(DateTime.UtcNow.Date, DateTime.UtcNow.Date.AddDays(1), dt => dt?.ToString("O"));
	[Test]
	public void NullableIntArrayContainsString() => InTestLogic<int?>(1, 2, i => i?.ToString());
	[Test]
	public void NullableBoolArrayContainsString() => InTestLogic<bool?>(true, null, b => b);

	private void InTestLogic<T>(T value1, T value2, Func<T, JsonNode?> transformer)
	{
		var rule = new InRule(
			new VariableRule(nameof(InTestData<T>.Data)),
			new JsonArray(transformer(value1), transformer(value2)));
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<InTestData<T>, bool>(rule);

		Assert.IsTrue(expression.Compile()(new InTestData<T>(value1)));
		Assert.IsTrue(expression.Compile()(new InTestData<T>(value2)));
	}
}