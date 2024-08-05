using System.Collections.Generic;
using System.Text.Json;
using Json.Logic.Rules;
using NUnit.Framework;
using static Json.Logic.JsonLogic;

namespace Json.Logic.Expressions.Tests;

public class GithubTests
{
	[Test]
	public void Pull303_CustomConverters()
	{
		var rule = JsonSerializer.Deserialize("{ \"+\" : [ 1, 2 ] }", TestDataSerializerContext.Default.Rule);

		Assert.IsInstanceOf<AddRule>(rule);
		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<decimal>(rule!);
		Assert.AreEqual(3, expression.Compile()());
	}

	public class Issue383RelationshipToProposer
	{
		public string? DataCode { get; set; }
		public string? DisplayText { get; set; }
	}

	public class Issue383Data
	{
		public bool HasAdditionalDrivers { get; set; }
		public List<object> AdditionalDrivers { get; set; } = [];
		public Issue383RelationshipToProposer? RelationshipToProposer;
	}

	[Test]
	public void Issue383_NoneUsesLocalValueForVarResolution()
	{
		var rule = None(
			Variable("additionalDrivers"),
			StrictEquals(
				Variable("relationshipToProposer.dataCode"),
				"J"
			)
		);

		var data = new Issue383Data
		{
			HasAdditionalDrivers = true,
			RelationshipToProposer = new Issue383RelationshipToProposer
			{
				DataCode = "J",
				DisplayText = null,
			}
		};

		var expression = RuleExpressionRegistry.Current.CreateRuleExpression<Issue383Data, bool>(rule);
		Assert.IsTrue(expression.Compile()(data));
	}
}