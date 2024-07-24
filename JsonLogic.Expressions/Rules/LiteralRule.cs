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
		return JsonNodeToExpression(rule.Value, parameter, options, true);
	}

	internal static Expression JsonNodeToExpression(JsonNode? node, Expression? parameter, CreateExpressionOptions options, bool createBox)
	{
		if (node == null)
		{
			return Expression.Constant(new DataObject(JsonNode.Parse("null"), options));
		}

		switch (node.GetValueKind())
		{
			case JsonValueKind.Undefined:
				return Expression.Constant(new DataObject(node, options));
			case JsonValueKind.Object:
				throw new NotImplementedException("Object values not yet implemented");
			case JsonValueKind.Array:
				var valueArray = node.AsArray();

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

				return Expression.Constant(typeof(DataArray<>)
					.MakeGenericType(kind switch
					{
						LiteralType.Undefined => typeof(UndefinedLiteralType),
						LiteralType.Object => typeof(ObjectLiteralType),
						LiteralType.Array => typeof(ArrayLiteralType),
						LiteralType.String => typeof(StringLiteralType),
						LiteralType.Number => typeof(NumberLiteralType),
						LiteralType.Boolean => typeof(BooleanLiteralType),
						_ => throw new ArgumentOutOfRangeException()
					})
					.GetConstructors()
					.Single()
					.Invoke([valueArray, options]));

			case JsonValueKind.String:
			case JsonValueKind.Number:
			case JsonValueKind.True:
			case JsonValueKind.False:
			case JsonValueKind.Null:
				return Expression.Constant(new DataObject(node, options));
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

	internal class UndefinedLiteralType;
	internal class ObjectLiteralType;
	internal class ArrayLiteralType;
	internal class StringLiteralType;
	internal class NumberLiteralType;
	internal class BooleanLiteralType;

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
