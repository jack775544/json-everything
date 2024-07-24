using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Nodes;

namespace Json.Logic.Expressions.Utility;

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
	///  <param name="options">The create options</param>
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
	public static Expression Numberify(this Expression expression, CreateExpressionOptions options)
	{
		return expression.Numberify(Expression.Constant(new DataObject(null, options)), options);
	}

	public static Expression Numberify(this Expression expression, object nullCast, CreateExpressionOptions options)
	{
		return expression.Numberify(Expression.Constant(new DataObject(JsonNode.Parse($"{nullCast}"), options)), options);
	}

	public static Expression Numberify(this Expression expression, Expression nullCast, CreateExpressionOptions options)
	{
		// This will be null
		if (expression.Type == typeof(DataObject)) return ExpressionTypeUtilities.Downcast(new [] { expression }, typeof(decimal)).First();
		if (expression.Type == typeof(object)) return nullCast;
		if (expression.Type == typeof(string)) return Expression.Call(_convertStringToDecimalMethod, expression);
		if (expression.Type == typeof(bool)) return Expression.Condition(
			expression,
			Expression.Constant(new DataObject(JsonNode.Parse("1"), options)),
			Expression.Constant(new DataObject(JsonNode.Parse("0"), options)));

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
		if (expression.Type == typeof(int)) return Expression.NotEqual(expression, Expression.Constant(0));
		if (expression.Type == typeof(long)) return Expression.NotEqual(expression, Expression.Constant(0L));
		if (expression.Type == typeof(short)) return Expression.NotEqual(expression, Expression.Constant((short)0));
		if (expression.Type == typeof(byte)) return Expression.NotEqual(expression, Expression.Constant((byte)0));
		if (expression.Type == typeof(double)) return Expression.NotEqual(expression, Expression.Constant(0d));
		if (expression.Type == typeof(float)) return Expression.NotEqual(expression, Expression.Constant(0f));
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
		
		if (expression.Type == typeof(object))
		{
			return Expression.Condition(
				Expression.Equal(expression, Expression.Constant(null)),
				Expression.Constant(string.Empty),
				Expression.Constant(null, typeof(string)));
		}

		if (expression.Type.TryGetGenericListType(out var listType))
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

	public static Expression GetCollectionItem(this Expression expression, int index)
	{
		if (expression.Type.IsArray)
		{
			return Expression.ArrayIndex(expression, Expression.Constant(index));
		}

		if (expression.Type.TryGetGenericListType(out var collectionType))
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

		if (expression.Type.TryGetGenericListType(out _))
		{
			return Expression.PropertyOrField(expression, "Count");
		}

		throw new InvalidOperationException($"Invalid collection type {expression.Type.Name}");
	}
}
