﻿using System;
using System.Linq.Expressions;
using System.Text.Json.Nodes;
using Json.Logic.Rules;
using NUnit.Framework;

namespace Json.Logic.Tests.Expressions;

public class ExtensionTests
{
	[TestCase("0", false)]
	[TestCase("1", true)]
	[TestCase("-1", true)]
	[TestCase("[]", false)]
	[TestCase("[1,2]", true)]
	[TestCase("\"\"", false)]
	[TestCase("\"anything\"", true)]
	[TestCase("null", false)]
	public void Truthiness(string text, bool expected)
	{
		var json = JsonNode.Parse(text);
		var expression = LiteralRule.JsonNodeToExpression(json, Expression.Constant(null), new CreateExpressionOptions());
		var truthyFunc = Expression.Lambda<Func<bool>>(expression.IsTruthy());
		Console.WriteLine(truthyFunc);

		Assert.AreEqual(expected, truthyFunc.Compile()());
	}
}