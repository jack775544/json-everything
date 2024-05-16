using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Json.Logic.Expressions.Utility;

internal class ExpressionUtilities
{
	public static IEnumerable<Expression> EvaluateItems(IEnumerable<Rule> items, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		return items.Select(x => registry.CreateExpressionInternal(x, parameter, options));
	}

	public static Expression CreateConstant<T>(T constant, bool createBox, CreateExpressionOptions options)
	{
		if (createBox && constant is string or decimal or bool)
		{
			return Expression.Constant(new DataObject(constant, options));
		}

		return Expression.Constant(constant);
	}
}