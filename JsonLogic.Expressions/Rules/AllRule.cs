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
		var input = registry.CreateExpressionInternal(rule.Input, parameter, options);

		if (!LogicTypeExtensions.TryGetGenericCollectionType(input.Type, out var paramType))
		{
			throw new JsonLogicException("Non collection passed when the expecting collection in none rule");
		}

		var param = Expression.Parameter(paramType, paramType.Name);
		var body = registry.CreateExpressionInternal(rule.Rule, param, options);

		var args = ExpressionTypeUtilities.Downcast(new[] { input }, paramType);
		
		return Expression.Call(
			_allMethod.MakeGenericMethod(paramType),
			args[0],
			Expression.Lambda(body, param));
	}
}
