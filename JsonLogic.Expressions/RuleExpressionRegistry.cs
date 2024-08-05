using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Json.Logic.Expressions.Utility;

namespace Json.Logic.Expressions;

/// <summary>
/// A registry containing all JsonLogic rules and helpers for creating expressions.
/// The default registry can be accessed by the <see cref="Current"/> field, or a new rule registry can also be constructed.
/// </summary>
public class RuleExpressionRegistry
{
	/// <summary>
	/// The default rule registry.
	/// </summary>
	public static RuleExpressionRegistry Current { get; } = new();

	private readonly Dictionary<Type, IRuleExpression> _registry;
	private readonly CreateRegistryOptions _defaultOptions;

	/// <summary>
	/// Constructs a new rule registry with all built in rules added.
	/// </summary>
	public RuleExpressionRegistry(CreateRegistryOptions? options = null)
	{
		_defaultOptions = options ?? new CreateRegistryOptions();
		_registry = new Dictionary<Type, IRuleExpression>(_defaultOptions.Registry);
	}

	/// <summary>
	/// Creates an expression predicate for the provided rule.
	/// </summary>
	/// <param name="rule">The rule to create the expression for.</param>
	/// <param name="parameter">An expression to be used as the JSON Logic data.</param>
	/// <param name="options">Options for creating the expression.</param>
	/// <returns>An expression body that represents the logic rule.</returns>
	internal Expression CreateExpressionInternal(Rule rule, Expression parameter, CreateExpressionOptions options)
	{
		return _registry[rule.GetType()].CreateRuleExpression(rule, this, parameter, options);
	}
	
	/// <summary>
	/// Creates an expression predicate for the provided rule.
	/// </summary>
	/// <param name="rule">The rule to create the expression for.</param>
	/// <param name="parameter">An expression to be used as the JSON Logic data.</param>
	/// <param name="options">Options for creating the expression.</param>
	/// <returns>An expression body that represents the logic rule.</returns>
	public Expression CreateExpression(Rule rule, Expression parameter, CreateExpressionOptions? options = null)
	{
		options ??= _defaultOptions;
		var expression = CreateExpressionInternal(rule, parameter, options);
		if (typeof(DataObject).IsAssignableFrom(expression.Type))
		{
			expression = ExpressionTypeUtilities.Downcast([expression]).First();
		}
		return options.WrapConstants ? ConstantReplacer.Replace(expression) : expression;
	}

	/// <summary>
	/// Creates an expression lambda for the provided rule.
	/// </summary>
	/// <param name="rule">The rule to create the expression for.</param>
	/// <param name="options">Options for creating the expression.</param>
	/// <typeparam name="TReturn">The return type of the rule.</typeparam>
	/// <returns>A lambda expression that expresses the logic of the provided rule.</returns>
	public Expression<Func<object?, TReturn>> CreateRuleExpression<TReturn>(Rule rule, CreateExpressionOptions? options = null)
	{
		var parameter = Expression.Parameter(typeof(object), "arg");
		var expression = CreateExpression(rule, parameter, options ?? _defaultOptions);
		return Expression.Lambda<Func<object?, TReturn>>(expression, parameter);
	}

	/// <summary>
	/// Creates an expression lambda for the provided rule.
	/// </summary>
	/// <param name="rule">The rule to create the expression for.</param>
	/// <param name="options">Options for creating the expression.</param>
	/// <typeparam name="TParam">The input type for the rule to use.</typeparam>
	/// <typeparam name="TReturn">The return type of the rule.</typeparam>
	/// <returns>A lambda expression that expresses the logic of the provided rule.</returns>
	public Expression<Func<TParam, TReturn>> CreateRuleExpression<TParam, TReturn>(Rule rule, CreateExpressionOptions? options = null)
	{
		var parameter = Expression.Parameter(typeof(TParam), "arg");
		var expression = CreateExpression(rule, parameter, options ?? _defaultOptions);
		return Expression.Lambda<Func<TParam, TReturn>>(expression, parameter);
	}
}