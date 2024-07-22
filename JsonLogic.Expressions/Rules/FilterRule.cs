using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="FilterRule"/>
/// </summary>
public class FilterRuleExpression : RuleExpression<FilterRule>
{
	private static readonly MethodInfo _whereMethod = typeof(Enumerable)
		.GetMethods()
		.Where(x => x.Name == "Where")
		.Single(x => x.GetParameters().Last().ParameterType.GetGenericArguments().Length == 2);

	/// <inheritdoc />
	public override Expression CreateExpression(FilterRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var input = registry.CreateExpressionInternal(rule.Input, parameter, options);

		if (!LogicTypeExtensions.TryGetGenericCollectionType(input.Type, out var type))
		{
			throw new JsonLogicException("Non collection passed when the expecting collection in none rule");
		}

		var param = Expression.Parameter(type, type.Name);
		var body = registry.CreateExpressionInternal(rule.Rule, param, options);
		var args = ExpressionTypeUtilities.Downcast(new[] { input }, type);
		return Expression.Call(
			_whereMethod.MakeGenericMethod(type),
			args[0],
			Expression.Lambda(body, param));
	}
}
