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

	internal static Expression JsonNodeToExpression(JsonNode? node, Expression? parameter, CreateExpressionOptions options, bool createBox)
	{
		if (node == null)
		{
			return ExpressionUtilities.CreateConstant<object?>(null, createBox, options);
		}

		switch (node.GetValueKind())
		{
			case JsonValueKind.Undefined:
				return ExpressionUtilities.CreateConstant<object?>(null, createBox, options);
			case JsonValueKind.Object:
				throw new NotImplementedException("Object values not yet implemented");
			case JsonValueKind.Array:
				var valueArray = node.AsArray();

				if (valueArray.Count == 0)
				{
					return Expression.NewArrayInit(typeof(object));
				}

				var kind = LiteralType.Undefined;
				foreach (var value in valueArray)
				{
					if (value != null)
					{
						var valueKind = GetLiteralType(value);

						if (kind != LiteralType.Undefined && kind != valueKind)
						{
							throw new JsonLogicException("Only homogenous arrays are allowed in logic literals");
						}

						kind = valueKind;
					}
				}

				if (createBox)
				{
					return kind switch
					{
						LiteralType.Undefined => Expression.Constant(new DataArray<object>(node.AsArray(), options)),
						LiteralType.Object => throw new NotImplementedException("Object value literals not yet implemented"),
						LiteralType.Array => throw new NotImplementedException("Nested array literals not yet implemented"),
						LiteralType.String => Expression.Constant(new DataArray<string>(node.AsArray(), options)),
						LiteralType.Number => Expression.Constant(new DataArray<decimal>(node.AsArray(), options)),
						LiteralType.Boolean => Expression.Constant(new DataArray<bool>(node.AsArray(), options)),
						_ => throw new ArgumentOutOfRangeException()
					};
				}

				var values = valueArray
					.Select(x => JsonNodeToExpression(x!, parameter, options, false))
					.ToList();
				return Expression.NewArrayInit(values[0].Type, values);
			case JsonValueKind.String:
				return ExpressionUtilities.CreateConstant(node.GetValue<string>(), createBox, options);
			case JsonValueKind.Number:
				return ExpressionUtilities.CreateConstant(node.Numberify(), createBox, options);
			case JsonValueKind.True:
				return ExpressionUtilities.CreateConstant(true, createBox, options);
			case JsonValueKind.False:
				return ExpressionUtilities.CreateConstant(false, createBox, options);
			case JsonValueKind.Null:
				return ExpressionUtilities.CreateConstant<object?>(null, createBox, options);
			default:
				throw new ArgumentOutOfRangeException();
		}
	}

	private enum LiteralType
	{
		Undefined,
		Object,
		Array,
		String,
		Number,
		Boolean,
	}

	private static LiteralType GetLiteralType(JsonNode? node)
	{
		if (node == null)
		{
			return LiteralType.Undefined;
		}

		return node.GetValueKind() switch
		{
			JsonValueKind.Undefined => LiteralType.Undefined,
			JsonValueKind.Object => LiteralType.Object,
			JsonValueKind.Array => LiteralType.Array,
			JsonValueKind.String => LiteralType.String,
			JsonValueKind.Number => LiteralType.Number,
			JsonValueKind.True => LiteralType.Boolean,
			JsonValueKind.False => LiteralType.Boolean,
			JsonValueKind.Null => LiteralType.Undefined,
			_ => throw new ArgumentOutOfRangeException()
		};
	}
}
