using System;
using System.Linq;
using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

namespace Json.Logic.Expressions.Rules;

/// <summary>
/// Handles creating expressions for the <see cref="VariableRule"/>
/// </summary>
public class VariableRuleExpression : RuleExpression<VariableRule>
{
	/// <inheritdoc />
	public override Expression CreateExpression(VariableRule rule, RuleExpressionRegistry registry, Expression parameter, CreateExpressionOptions options)
	{
		if (rule.Path == null)
		{
			return Expression.Constant(null);
		}

		var path = rule.Path.Apply().Stringify();

		if (path == null)
		{
			throw new InvalidOperationException("Variable path is null");
		}

		if (string.IsNullOrEmpty(path))
		{
			return parameter;
		} 

		var pathSegments = path.Split('.');

		var property = GetPropertyOrField(parameter, pathSegments);

		if (rule.DefaultValue != null)
		{
			if (
				property.Type.IsGenericType && property.Type.GetGenericTypeDefinition() == typeof(Nullable<>)
				|| !property.Type.IsValueType)
			{
				var defaultValue = registry.CreateExpression(rule.DefaultValue, parameter, options);
				var args = new[]
				{
					property,
					defaultValue
				}.Downcast();

				return Expression.Coalesce(args[0], args[1]);
			}
		}

		return property;
	}

	private static Expression GetPropertyOrField(Expression expression, string[] path)
	{
		return path.Aggregate(expression, GetPropertyOrArrayMember);
	}

	private static Expression GetPropertyOrArrayMember(Expression expression, string path)
	{
		if (expression.Type.IsCollectionType())
		{
			return int.TryParse(path, out var index)
				? expression.GetCollectionItem(index)
				: Expression.Constant(null);
		}

		try
		{
			return Expression.PropertyOrField(expression, path);
		}
		catch (ArgumentException)
		{
			return Expression.Constant(null);
		}
	}
}
