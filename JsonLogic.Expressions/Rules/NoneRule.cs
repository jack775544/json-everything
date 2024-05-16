using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="NoneRule"/>
/// </summary>
public class NoneRuleExpression : RuleExpression<NoneRule>
{
	private static readonly MethodInfo _anyMethod = typeof(Enumerable)
		.GetMethods()
		.Where(x => x.Name == "Any")
		.Single(x => x.GetParameters().Length == 2);

	/// <inheritdoc />
	public override Expression CreateExpression(NoneRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		var input = registry.CreateExpressionInternal(rule.Input, parameter, options);

		if (!input.Type.TryGetGenericCollectionType(out var type))
		{
			throw new JsonLogicException("Non collection passed when the expecting collection in none rule");
		}

		var param = Expression.Parameter(type, type.Name);
		var body = registry.CreateExpressionInternal(rule.Rule, param, options);
		return Expression.Not(Expression.Call(
			_anyMethod.MakeGenericMethod(type),
			input,
			Expression.Lambda(body, param)));
	}
}
