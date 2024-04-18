using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="LiteralRule"/>
/// </summary>
public class LiteralRuleExpression : RuleExpression<LiteralRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(LiteralRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		return rule.Value == null
			? Expression.Constant(null)
			: JsonNodeToExpression(rule.Value, parameter, options, true);
	}
	
	internal static Expression JsonNodeToExpression(JsonNode? node, Expression parameter, CreateExpressionOptions options, bool createBox)
	{
		if (node == null)
		{
			return ExpressionExtensions.CreateConstant<object?>(null, createBox, options);
		}

		switch (node.GetValueKind())
		{
			case JsonValueKind.Undefined:
				return ExpressionExtensions.CreateConstant<object?>(null, createBox, options);
			case JsonValueKind.Object:
				throw new NotImplementedException("Object values not yet implemented");
			case JsonValueKind.Array:
				var values = node.AsArray().Select(x => JsonNodeToExpression(x!, parameter, options, false)).ToList();

				if (values.Count == 0)
				{
					return Expression.NewArrayInit(typeof(object));
				}

				return Expression.NewArrayInit(values[0].Type, values);
			case JsonValueKind.String:
				return ExpressionExtensions.CreateConstant(node.GetValue<string>(), createBox, options);
			case JsonValueKind.Number:
				return ExpressionExtensions.CreateConstant(node.Numberify(), createBox, options);
			case JsonValueKind.True:
				return ExpressionExtensions.CreateConstant(true, createBox, options);
			case JsonValueKind.False:
				return ExpressionExtensions.CreateConstant(false, createBox, options);
			case JsonValueKind.Null:
				return ExpressionExtensions.CreateConstant<object?>(null, createBox, options);
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}
