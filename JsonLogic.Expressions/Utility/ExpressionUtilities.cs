using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Nodes;

namespace Json.Logic.Expressions.Utility;

internal class ExpressionUtilities
{
	public static IEnumerable<Expression> EvaluateItems(IEnumerable<Rule> items, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		return items.Select(x => registry.CreateExpressionInternal(x, parameter, options));
	}

	public static Expression CreateConstant(JsonNode? constant, bool createBox, CreateExpressionOptions options)
	{
		if (createBox)
		{
			return Expression.Constant(new DataObject(constant, options));
		}

		return Expression.Constant(constant);
	}
}