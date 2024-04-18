using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Json.Logic.Expressions.Rules;
using Json.Logic.Expressions.Utility;
using Json.Logic.Rules;

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

	private readonly Dictionary<Type, IRuleExpression> _registry = new();

	/// <summary>
	/// Constructs a new rule registry with all built in rules added.
	/// </summary>
	public RuleExpressionRegistry()
	{
		AddRule<AddRule, AddRuleExpression>();
		AddRule<AllRule, AllRuleExpression>();
		AddRule<AndRule, AndRuleExpression>();
		AddRule<BooleanCastRule, BooleanCastRuleExpression>();
		AddRule<CatRule, CatRuleExpression>();
		AddRule<DivideRule, DivideRuleExpression>();
		AddRule<FilterRule, FilterRuleExpression>();
		AddRule<IfRule, IfRuleExpression>();
		AddRule<InRule, InRuleExpression>();
		AddRule<LessThanEqualRule, LessThanEqualRuleExpression>();
		AddRule<LessThanRule, LessThanRuleExpression>();
		AddRule<LiteralRule, LiteralRuleExpression>();
		AddRule<LogRule, LogRuleExpression>();
		AddRule<LooseEqualsRule, LooseEqualsRuleExpression>();
		AddRule<LooseNotEqualsRule, LooseNotEqualsRuleExpression>();
		AddRule<MapRule, MapRuleExpression>();
		AddRule<MaxRule, MaxRuleExpression>();
		AddRule<MergeRule, MergeRuleExpression>();
		AddRule<MinRule, MinRuleExpression>();
		AddRule<MissingRule, MissingRuleExpression>();
		AddRule<MissingSomeRule, MissingSomeRuleExpression>();
		AddRule<ModRule, ModRuleExpression>();
		AddRule<MoreThanEqualRule, MoreThanEqualRuleExpression>();
		AddRule<MoreThanRule, MoreThanRuleExpression>();
		AddRule<MultiplyRule, MultiplyRuleExpression>();
		AddRule<NoneRule, NoneRuleExpression>();
		AddRule<NotRule, NotRuleExpression>();
		AddRule<OrRule, OrRuleExpression>();
		AddRule<ReduceRule, ReduceRuleExpression>();
		AddRule<RuleCollection, RuleCollectionExpression>();
		AddRule<SomeRule, SomeRuleExpression>();
		AddRule<StrictEqualsRule, StrictEqualsRuleExpression>();
		AddRule<StrictNotEqualsRule, StrictNotEqualsRuleExpression>();
		AddRule<SubstrRule, SubstrRuleExpression>();
		AddRule<SubtractRule, SubtractRuleExpression>();
		AddRule<VariableRule, VariableRuleExpression>();
	}

	public void AddRule<TRule, TRuleExpression>()
		where TRule : Rule
		where TRuleExpression : RuleExpression<TRule>, new()
	{
		AddRule(typeof(TRule), new TRuleExpression());
	}

	public void AddRule<TRule>(RuleExpression<TRule> ruleExpression) where TRule : Rule
	{
		AddRule(typeof(TRule), ruleExpression);
	}

	public void AddRule<TRule>(IRuleExpression ruleExpression) where TRule : Rule
	{
		AddRule(typeof(TRule), ruleExpression);
	}

	public void AddRule(Type ruleType, IRuleExpression ruleExpression)
	{
		_registry[ruleType] = ruleExpression;
	}

	public void RemoveRule<TRule>() where TRule : Rule
	{
		RemoveRule(typeof(TRule));
	}

	public void RemoveRule(Type ruleType)
	{
		_registry.Remove(ruleType);
	}

	/// <summary>
	/// Creates an expression predicate for the provided rule.
	/// </summary>
	/// <param name="rule">The rule to create the expression for.</param>
	/// <param name="parameter">An expression to be used as the JSON Logic data.</param>
	/// <param name="options">Options for creating the expression.</param>
	/// <returns>An expression body that represents the logic rule.</returns>
	public Expression CreateExpression(Rule rule, Expression parameter, CreateExpressionOptions options)
	{
		return CreateExpression(rule, rule.GetType(), parameter, options);
	}

	/// <summary>
	/// Creates an expression predicate for the provided rule.
	/// </summary>
	/// <param name="rule">The rule to create the expression for.</param>
	/// <param name="ruleType">The type of the rule.</param>
	/// <param name="parameter">An expression to be used as the JSON Logic data.</param>
	/// <param name="options">Options for creating the expression.</param>
	/// <returns>An expression body that represents the logic rule.</returns>
	public Expression CreateExpression(Rule rule, Type ruleType, Expression parameter, CreateExpressionOptions options)
	{
		return _registry[ruleType].CreateRuleExpression(rule, this, parameter, options);
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
		var expression = CreateExpression(rule, parameter, options ?? new CreateExpressionOptions());
		if (expression.Type == typeof(DataObject))
		{
			expression = Expression.PropertyOrField(expression, nameof(DataObject.Field));
		}
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
		var expression = CreateExpression(rule, parameter, options ?? new CreateExpressionOptions());
		if (expression.Type == typeof(DataObject))
		{
			expression = Expression.PropertyOrField(expression, nameof(DataObject.Field));
		}
		return Expression.Lambda<Func<TParam, TReturn>>(expression, parameter);
	}
}