using System;
using System.Collections.Generic;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class VariableTests
{
	private record VariableTestData(decimal Foo, decimal Bar);

	private record NestedTestData(string Baz, VariableTestData Inner);

	[Test]
	public void VariableWithValidPathAndNoDefaultFetchesData()
	{
		var rule = new VariableRule("foo");
		var data = new VariableTestData(5, 10);
		var expression = ExpressionTestHelpers.CreateRuleExpression<VariableTestData, decimal>(rule);

		Assert.AreEqual(5M, expression.Compile()(data));
	}

	[Test]
	public void VariableWithInvalidPathAndNoDefaultThrowsError()
	{
		var rule = new VariableRule("Baz");
		var data = new VariableTestData(5, 10);
		var expression = ExpressionTestHelpers.CreateRuleExpression<VariableTestData, object>(rule);

		Assert.IsNull(expression.Compile()(data));
	}

	[Test]
	public void VariableWithValidPathAndDefaultFetchesData()
	{
		var rule = new VariableRule("foo", 11);
		var data = new VariableTestData(5, 10);
		var expression = ExpressionTestHelpers.CreateRuleExpression<VariableTestData, decimal>(rule);

		Assert.AreEqual(5M, expression.Compile()(data));
	}

	[Test]
	public void VariableWithInvalidPathAndDefaultReturnsDefault()
	{
		var rule = new VariableRule("baz", 11);
		var data = new VariableTestData(5, 10);
		var expression = ExpressionTestHelpers.CreateRuleExpression<VariableTestData, decimal>(rule);

		Assert.AreEqual(11M, expression.Compile()(data));
	}

	[Test]
	public void VariableWithEmptyPathReturnsEntireData()
	{
		var rule = new VariableRule("");
		var data = new VariableTestData(5, 10);
		var expression = ExpressionTestHelpers.CreateRuleExpression<VariableTestData, VariableTestData>(rule);
		var result = expression.Compile()(data);

		Assert.AreEqual(data.Foo, result.Foo);
		Assert.AreEqual(data.Bar, result.Bar);
	}

	[Test]
	public void VariableWithValidPathAndArrayTypeFetchesData()
	{
		var rule = new VariableRule("1");
		var data = new decimal[] { 0, 1, 2 };
		var expression = ExpressionTestHelpers.CreateRuleExpression<decimal[], decimal>(rule);

		Assert.AreEqual(1M, expression.Compile()(data));
	}

	[Test]
	public void VariableWithValidPathAndListTypeFetchesData()
	{
		var rule = new VariableRule("1");
		var data = new List<decimal> { 0, 1, 2 };
		var expression = ExpressionTestHelpers.CreateRuleExpression<List<decimal>, decimal>(rule);

		Assert.AreEqual(1M, expression.Compile()(data));
	}

	[Test]
	public void VariableWithValidNestedPathAndNoDefaultFetchesData()
	{
		var rule = new VariableRule("inner.foo");
		var data = new NestedTestData("hello world", new VariableTestData(5, 10));
		var expression = ExpressionTestHelpers.CreateRuleExpression<NestedTestData, decimal>(rule);

		Assert.AreEqual(5M, expression.Compile()(data));
	}

	[Test]
	public void VariableWithInvalidNestedPathAndDefaultReturnsDefault()
	{
		var rule = new VariableRule("inner.baz", 11M);
		var data = new NestedTestData("hello world", new VariableTestData(5, 10));
		var expression = ExpressionTestHelpers.CreateRuleExpression<NestedTestData, decimal>(rule);

		Assert.AreEqual(11M, expression.Compile()(data));
	}

	[Test]
	public void VariableWithNestedEmptyPathReturnsNestedData()
	{
		var rule = new VariableRule("inner");
		var data = new NestedTestData("hello world", new VariableTestData(5, 10));
		var expression = ExpressionTestHelpers.CreateRuleExpression<NestedTestData, VariableTestData>(rule);
		var result = expression.Compile()(data);

		Assert.AreEqual(data.Inner.Foo, result.Foo);
		Assert.AreEqual(data.Inner.Bar, result.Bar);
	}
}