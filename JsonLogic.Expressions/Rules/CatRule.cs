using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="CatRule"/>
/// </summary>
public class CatRuleExpression : RuleExpression<CatRule>
{
	private static readonly MethodInfo _stringConcat2Method = ((Func<string, string, string>)string.Concat).Method;
	private static readonly MethodInfo _stringConcat3Method = ((Func<string, string, string, string>)string.Concat).Method;
	private static readonly MethodInfo _stringConcat4Method = ((Func<string, string, string, string, string>)string.Concat).Method;

	/// <inheritdoc />
	public override Expression CreateExpression(CatRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var items = ExpressionTypeUtilities.Downcast(ExpressionUtilities.EvaluateItems(rule.Items, registry, parameter, options), typeof(string))
			.Select(ExpressionExtensions.Stringify)
			.ToList();

		return items.Count switch
		{
			0 => Expression.Constant(""),
			1 => items[0],
			2 => Expression.Call(_stringConcat2Method, items[0], items[1]),
			3 => Expression.Call(_stringConcat3Method, items[0], items[1], items[2]),
			4 => Expression.Call(_stringConcat4Method, items[0], items[1], items[2], items[3]),
			_ => items.Aggregate((a, c) => Expression.Call(_stringConcat2Method, a, c))
		};
	}
}
