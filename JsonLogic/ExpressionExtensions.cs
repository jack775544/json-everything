using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Json.Logic;

internal static class ExpressionExtensions
{
	private static readonly MethodInfo _convertStringToDecimalMethod = ((Func<string, decimal>)Convert.ToDecimal).Method;
	private static readonly MethodInfo _isNullOrEmptyMethod = ((Func<string, bool>)string.IsNullOrEmpty).Method;
	private static readonly MethodInfo _stringJoin = ((Func<string, IEnumerable<string>, string>)string.Join).Method;
	private static readonly MethodInfo _toStringMethod = typeof(object).GetMethod("ToString")!;
	private static readonly MethodInfo _selectMethod = typeof(Enumerable)
		.GetMethods()
		.Where(x => x.Name == "Select")
		.Single(x => x.GetParameters().Last().ParameterType.GetGenericArguments().Length == 2);

	///  <summary>
	///  Provides a loose-cast to a number.
	///  </summary>
	///  <param name="expression">The node.</param>
	///  <param name="nullCast">What to cast null to</param>
	///  <returns>
	/// 	A string representation of the value as follows:
	/// 
	///  - empty string returns 0
	///  - all other strings try to parse a number from the value
	///  - true returns 1
	///  - false returns 0
	/// 	- numbers are unchanged
	///  - null, objects, and arrays return null (not numberifiable)
	///  </returns>
	public static Expression Numberify(this Expression expression)
	{
		return expression.Numberify(Expression.Constant(null));
	}
	
	public static Expression Numberify(this Expression expression, Expression nullCast)
	{
		// This will be null
		if (expression.Type == typeof(object)) return nullCast;
		if (expression.Type == typeof(string)) return Expression.Call(_convertStringToDecimalMethod, expression);
		if (expression.Type == typeof(bool)) return Expression.Condition(expression, Expression.Constant(1M), Expression.Constant(0M));

		return expression;
	}

	/// <summary>
	/// Determines whether a value can be considered as `true`.
	/// </summary>
	/// <param name="expression">The element.</param>
	/// <returns>
	/// `true` if the value is:
	///
	/// - a non-empty array
	/// - a non-empty string
	/// - a non-zero number
	/// - true
	///
	///	`false` otherwise
	///
	/// </returns>
	public static Expression IsTruthy(this Expression expression)
	{
		if (expression.Type == typeof(object)) return Expression.Constant(false);
		if (expression.Type.IsArray)
			return Expression.Condition(
				Expression.GreaterThan(expression.GetCollectionLength(), Expression.Constant(0)),
				Expression.Constant(true),
				Expression.Constant(false));
		if (expression.Type == typeof(bool)) return expression;
		if (expression.Type == typeof(string))
			return Expression.Not(Expression.Call(_isNullOrEmptyMethod, expression));
		if (expression.Type == typeof(decimal)) return Expression.NotEqual(expression, Expression.Constant(0M));

		return Expression.Constant(false);
	}
	
	/// <summary>
	/// Provides a loose-cast to a string.
	/// </summary>
	/// <param name="expression">The node.</param>
	/// <returns>
	///	A string representation of the value as follows:
	///
	/// - arrays are the elements stringified and comma-delimited
	/// - null returns the empty string
	/// - objects return null (not stringifiable)
	///	- numbers and booleans return their JSON equivalents
	/// - strings are unchanged
	/// </returns>
	public static Expression Stringify(this Expression expression)
	{
		if (expression.Type == typeof(string)) return expression;
		
		if (expression.Type == typeof(object)) return Expression.Condition(
			Expression.Equal(expression, Expression.Constant(null)),
			Expression.Constant(string.Empty),
			Expression.Constant(null, typeof(string)));

		if (TryGetGenericListType(expression.Type, out var listType))
		{
			var callbackParam = Expression.Parameter(listType, "toStringParam");
			var toStringCallback = Expression.Lambda(Expression.Call(callbackParam, _toStringMethod), callbackParam);
			return Expression.Call(
				_stringJoin,
				Expression.Constant(","),
				Expression.Call(
					_selectMethod.MakeGenericMethod(listType, typeof(string)),
					expression,
					toStringCallback));
		}

		if (expression.Type == typeof(bool)) return Expression.Condition(
			Expression.Equal(expression, Expression.Constant(true)),
			Expression.Constant("true"),
			Expression.Constant("false"));

		var convertMethod = typeof(Convert)
			.GetMethods()
			.Where(x => x.Name == "ToString")
			.SingleOrDefault(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == expression.Type);

		if (convertMethod != null)
		{
			return Expression.Call(convertMethod, expression);
		}

		return Expression.Constant(null);
	}
	
	/// <summary>
	/// Gets the generic type from a list type.
	/// </summary>
	/// <param name="type">The type to get the generic list type from.</param>
	/// <param name="genericType">The retrieved generic type. Is null when return value is false.</param>
	/// <returns>true if type param is a list, false otherwise.</returns>
	public static bool TryGetGenericListType(Type type, [NotNullWhen(true)]out Type? genericType)
	{
		genericType = type
			.GetInterfaces()
			.Where(x => x.IsGenericType)
			.SingleOrDefault(x => x.GetGenericTypeDefinition() == typeof(IList<>))
			?.GetGenericArguments()
			.Single();

		return genericType != null;
	}
	
	public static bool TryGetGenericCollectionType(this Type type, [NotNullWhen(true)]out Type? genericType)
	{
		if (type.IsArray)
		{
			genericType = type.GetElementType()!;
			return true;
		}

		genericType = type
			.GetInterfaces()
			.Where(x => x.IsGenericType)
			.SingleOrDefault(x => x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			?.GetGenericArguments()
			.Single();

		return genericType != null;
	}

	public static bool IsCollectionType(this Type type)
	{
		return type.IsArray || TryGetGenericListType(type, out _);
	}

	public static Expression GetCollectionItem(this Expression expression, int index)
	{
		if (expression.Type.IsArray)
		{
			return Expression.ArrayIndex(expression, Expression.Constant(index));
		}

		if (TryGetGenericListType(expression.Type, out var collectionType))
		{
			var getItem = typeof(IList<>).MakeGenericType(collectionType).GetMethod("get_Item", [typeof(int)])!;
			return Expression.Call(
				expression,
				getItem,
				Expression.Constant(index));
		}

		throw new InvalidOperationException($"Invalid collection type {expression.Type.Name}");
	}
	
	public static Expression GetCollectionLength(this Expression expression)
	{
		if (expression.Type.IsArray)
		{
			return Expression.ArrayLength(expression);
		}

		if (TryGetGenericListType(expression.Type, out _))
		{
			// var countProperty = typeof(ICollection<>).GetProperty("Count");
			return Expression.PropertyOrField(expression, "Count");
		}

		throw new InvalidOperationException($"Invalid collection type {expression.Type.Name}");
	}

	public static IEnumerable<Expression> EvaluateItems(IEnumerable<Rule> items, Expression parameter, CreateExpressionOptions options)
	{
		return items.Select(x => x.CreateExpression(parameter, options));
	}

	public static Expression CreateConstant<T>(T constant, CreateExpressionOptions options)
	{
		if (!options.WrapConstants)
		{
			return Expression.Constant(constant);
		}

		var data = new DataObject<T>
		{
			Field = constant
		};

		return Expression.PropertyOrField(Expression.Constant(data), nameof(DataObject<object>.Field));

	}

	private class DataObject<T>
	{
		public T? Field { get; set; }
	}
}

internal record PropertyReplacerInfo(Type TypeToReplaceFor, MemberInfo PropertyToReplaceFor, Expression ExpressionToUse);

internal class PropertyReplacer : ExpressionVisitor
{
	private readonly ICollection<PropertyReplacerInfo> _replacerInfos;

	public PropertyReplacer(ICollection<PropertyReplacerInfo> replacerInfos)
	{
		_replacerInfos = replacerInfos;
	}

	protected override Expression VisitMember(MemberExpression node)
	{
		if (node.Expression == null)
		{
			return node;
		}

		var replacerInfo = _replacerInfos.FirstOrDefault(r => r.TypeToReplaceFor == node.Expression.Type && r.PropertyToReplaceFor == node.Member);
		if (replacerInfo == null)
		{
			return node;
		}

		return replacerInfo.ExpressionToUse;
	}

	public static Expression Replace(Expression expression, ICollection<PropertyReplacerInfo> replacerInfos)
	{
		return new PropertyReplacer(replacerInfos).Visit(expression);
	}
}