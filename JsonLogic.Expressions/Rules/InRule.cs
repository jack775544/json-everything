using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="InRule"/>
/// </summary>
public class InRuleExpression : RuleExpression<InRule>
{
	private static readonly MethodInfo _containsMethod = typeof(string)
		.GetMethods()
		.Where(x => x.Name == "Contains")
		.Single(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == typeof(string));

	/// <inheritdoc />
	public override Expression CreateExpression(InRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		return Expression.Call(
			new [] { registry.CreateExpressionInternal(rule.Value, parameter, options) }.Downcast(typeof(string)).First().Stringify(),
			_containsMethod,
			new [] { registry.CreateExpressionInternal(rule.Test, parameter, options) }.Downcast(typeof(string)).First().Stringify());
	}
}
