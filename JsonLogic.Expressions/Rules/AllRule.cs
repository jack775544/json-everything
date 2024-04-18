using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="AllRule"/>
/// </summary>
public class AllRuleExpression : RuleExpression<AllRule>
{
	private static readonly MethodInfo _allMethod = typeof(Enumerable).GetMethod("All")!;

	/// <inheritdoc />
	public override Expression CreateExpression(AllRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var input = registry.CreateExpression(rule.Input, parameter, options);

		if (!input.Type.TryGetGenericCollectionType(out var paramType))
		{
			throw new JsonLogicException("Non collection passed when the expecting collection in none rule");
		}

		var param = Expression.Parameter(paramType, paramType.Name);
		var body = registry.CreateExpression(rule.Rule, param, options);
		return Expression.Call(
			_allMethod.MakeGenericMethod(paramType),
			input,
			Expression.Lambda(body, param));
	}
}
