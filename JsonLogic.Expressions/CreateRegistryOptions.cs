using System;
using System.Collections.Generic;
using Json.Logic.Expressions.Rules;
using Json.Logic.Rules;

namespace Json.Logic.Expressions;

/// <summary>
/// Options for creating a <see cref="RuleExpressionRegistry"/>.
/// </summary>
public record CreateRegistryOptions : CreateExpressionOptions
{
	internal Dictionary<Type, IRuleExpression> Registry { get; } = new();

	/// <summary>
	/// Creates a new options object with all the default JsonLogic rules added.
	/// </summary>
	public CreateRegistryOptions()
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
	
	/// <summary>
	/// Adds a new rule that will be contained in the registry when it is created.
	/// </summary>
	/// <typeparam name="TRule">The type of the rule to add.</typeparam>
	/// <typeparam name="TRuleExpression">The type of the expression builder for the rule.</typeparam>
	public void AddRule<TRule, TRuleExpression>()
		where TRule : Rule
		where TRuleExpression : RuleExpression<TRule>, new()
	{
		AddRule(typeof(TRule), new TRuleExpression());
	}

	/// <summary>
	/// Adds a new rule that will be contained in the registry when it is created.
	/// </summary>
	/// <param name="ruleExpression">An instance of the new rule expression.</param>
	/// <typeparam name="TRule">The type of the rule to add.</typeparam>
	public void AddRule<TRule>(IRuleExpression ruleExpression) where TRule : Rule
	{
		AddRule(typeof(TRule), ruleExpression);
	}

	/// <summary>
	/// Adds a new rule that will be contained in the registry when it is created.
	/// </summary>
	/// <param name="ruleExpression">An instance of the new rule expression.</param>
	/// <param name="ruleType">The type of the rule to add.</param>
	public void AddRule(Type ruleType, IRuleExpression ruleExpression)
	{
		Registry[ruleType] = ruleExpression;
	}

	/// <summary>
	/// Removes a rule. This rule will not be able to have expressions contructed from it
	/// when the registry is created.
	/// </summary>
	/// <typeparam name="TRule">The type of rule to remove.</typeparam>
	public void RemoveRule<TRule>() where TRule : Rule
	{
		RemoveRule(typeof(TRule));
	}

	/// <summary>
	/// Removes a rule. This rule will not be able to have expressions contructed from it
	/// when the registry is created.
	/// </summary>
	/// <param name="ruleType">The type of rule to remove.</param>
	public void RemoveRule(Type ruleType)
	{
		Registry.Remove(ruleType);
	}

	/// <summary>
	/// Clears all rules. Rules must be manually added back to this object for the registry to do anything.
	/// </summary>
	public void ClearRules()
	{
		Registry.Clear();
	}
}