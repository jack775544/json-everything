using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
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

	public static Expression Numberify<T>(this Expression expression, T nullCast, CreateExpressionOptions options)
	{
		return expression.Numberify(CreateConstant(nullCast, true, options), options);
	}

	public static Expression Numberify(this Expression expression, Expression nullCast, CreateExpressionOptions options)
	{
		// This will be null
		if (expression.Type == typeof(DataObject)) return expression;
		if (expression.Type == typeof(object)) return nullCast;
		if (expression.Type == typeof(string)) return Expression.Call(_convertStringToDecimalMethod, expression);
		if (expression.Type == typeof(bool)) return Expression.Condition(
			expression,
			Expression.Constant(new DataObject(1, options)),
			Expression.Constant(new DataObject(0, options)));

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
		
		if (expression.Type == typeof(object))
		{
			return Expression.Condition(
				Expression.Equal(expression, Expression.Constant(null)),
				Expression.Constant(string.Empty),
				Expression.Constant(null, typeof(string)));
		}

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
			return Expression.PropertyOrField(expression, "Count");
		}

		throw new InvalidOperationException($"Invalid collection type {expression.Type.Name}");
	}

	public static IEnumerable<Expression> EvaluateItems(IEnumerable<Rule> items, Expression parameter, CreateExpressionOptions options)
	{
		return items.Select(x => x.CreateExpression(parameter, options));
	}

	public static Expression CreateConstant<T>(T constant, bool createBox, CreateExpressionOptions options)
	{
		if (createBox && constant is string or decimal)
		{
			return Expression.Constant(new DataObject(constant, options));
		}

		if (!options.WrapConstants)
		{
			return Expression.Constant(constant);
		}

		var data = new DataObject(constant, options);

		return Expression.PropertyOrField(Expression.Constant(data), nameof(DataObject.Field));
	}

	public static List<Expression> Downcast(this IEnumerable<Expression> expressions, Type? desiredType = null)
	{
		var expressionList = expressions.ToList();
		desiredType ??= GetDesiredType(expressionList);
		var visitor = new DataObjectReplacer(desiredType);
		return expressionList.Select(x => visitor.Visit(x)).ToList();
	}

	private static Type GetDesiredType(List<Expression> expressions)
	{
		// If something is explicitly declaring its type, then use it.
		var desiredType = expressions.Select(x => x.Type).FirstOrDefault(x => x != typeof(DataObject));
		if (desiredType != null)
		{
			return desiredType;
		}

		var inspector = new DataObjectTypeInspector();
		foreach (var expression in expressions)
		{
			inspector.Visit(expression);
		}

		// If the type needs to be inferred, then convert to the most common type
		// The most common type will be the one that has the lowest value in the type hierarchy
		var mostCommonType = inspector.DiscoveredTypes.MinByCustom(x => _typeHierarchy.IndexOf(x)) ?? typeof(string);
		return mostCommonType;
	}

	// List of types from the most general to the least general
	private static readonly List<Type> _typeHierarchy = [
		typeof(string),
		typeof(Guid),
		typeof(DateTime),
		typeof(DateOnly),
		typeof(TimeOnly),
		typeof(decimal),
		typeof(double),
		typeof(float),
		typeof(int),
		typeof(long),
		typeof(short),
		typeof(byte),
	];

	internal static TSource? MinByCustom<TSource, TKey>(this IEnumerable<TSource> self, Func<TSource, TKey> keySelector)
	{
		var minKey = default(TKey);
		var minItem = default(TSource);
		var first = true;
		var comparer = Comparer<TKey>.Default;

		foreach (var item in self)
		{
			var key = keySelector(item);

			if (first)
			{
				first = false;
				minKey = key;
				minItem = item;
			}

			if (comparer.Compare(key, minKey) < 0)
			{
				minKey = key;
				minItem = item;
			}
		}

		return minItem;
	}

	public static Expression DataObjectToExpression(DataObject dataObject, Type convertTo)
	{
		var result = DataObjectConverters.TryGetValue(convertTo, out var converter)
			? converter(dataObject)
			: throw new InvalidOperationException($"Could not find converter to convert {dataObject.Field} to {convertTo.FullName}");

		if (result == null)
		{
			throw new InvalidOperationException($"Could not convert {dataObject.Field} to {convertTo.FullName}");
		}

		return result;
	}

	public static readonly Dictionary<Type, Func<DataObject, Expression?>> DataObjectConverters = new()
	{
		{ typeof(string), static data => data.AsString() },
		{ typeof(Guid), static data => data.AsGuid() },
		{ typeof(DateTime), static data => data.AsDateTime() },
		{ typeof(DateOnly), static data => data.AsDateOnly() },
		{ typeof(TimeOnly), static data => data.AsTimeOnly() },
		{ typeof(int), static data => data.AsInt() },
		{ typeof(long), static data => data.AsLong() },
		{ typeof(short), static data => data.AsShort() },
		{ typeof(byte), static data => data.AsByte() },
		{ typeof(double), static data => data.AsDouble() },
		{ typeof(float), static data => data.AsFloat() },
		{ typeof(decimal), static data => data.AsDecimal() },
	};
}

internal class DataObject(object? constant, CreateExpressionOptions options)
{
	public object? Field { get; set; } = constant;

	public Expression? AsString()
	{
		if (Field is string stringField)
		{
			return ExpressionExtensions.CreateConstant(stringField, false, options);
		}

		if (Field is decimal numberField)
		{
			return ExpressionExtensions.CreateConstant(numberField.ToString(options.CultureInfo), false, options);
		}

		return null;
	}

	public Expression? AsGuid()
	{
		if (Field is not string stringField)
		{
			return null;
		}

		return Guid.TryParse(stringField, out var guid)
			? ExpressionExtensions.CreateConstant(guid, false, options)
			: null;
	}

	public Expression? AsDateTime() => AsDateLike<DateTime>(DateTime.TryParse);
	public Expression? AsDateOnly() => AsDateLike<DateOnly>(DateOnly.TryParse);
	public Expression? AsTimeOnly() => AsDateLike<TimeOnly>(TimeOnly.TryParse);

	public Expression? AsInt() => Field switch
	{
		string => AsNumber<string, int>(Convert.ToInt32, int.TryParse),
		int => AsNumber<int, int>(Convert.ToInt32, int.TryParse),
		long => AsNumber<long, int>(Convert.ToInt32, int.TryParse),
		short => AsNumber<short, int>(Convert.ToInt32, int.TryParse),
		byte => AsNumber<byte, int>(Convert.ToInt32, int.TryParse),
		double => AsNumber<double, int>(Convert.ToInt32, int.TryParse),
		float => AsNumber<float, int>(Convert.ToInt32, int.TryParse),
		decimal => AsNumber<decimal, int>(Convert.ToInt32, int.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(int).FullName}")
	};

	public Expression? AsLong() => Field switch
	{
		string => AsNumber<string, long>(Convert.ToInt64, long.TryParse),
		int => AsNumber<int, long>(Convert.ToInt64, long.TryParse),
		long => AsNumber<long, long>(Convert.ToInt64, long.TryParse),
		short => AsNumber<short, long>(Convert.ToInt64, long.TryParse),
		byte => AsNumber<byte, long>(Convert.ToInt64, long.TryParse),
		double => AsNumber<double, long>(Convert.ToInt64, long.TryParse),
		float => AsNumber<float, long>(Convert.ToInt64, long.TryParse),
		decimal => AsNumber<decimal, long>(Convert.ToInt64, long.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(long).FullName}")
	};

	public Expression? AsShort() =>  Field switch
	{
		string => AsNumber<string, short>(Convert.ToInt16, short.TryParse),
		int => AsNumber<int, short>(Convert.ToInt16, short.TryParse),
		long => AsNumber<long, short>(Convert.ToInt16, short.TryParse),
		short => AsNumber<short, short>(Convert.ToInt16, short.TryParse),
		byte => AsNumber<byte, short>(Convert.ToInt16, short.TryParse),
		double => AsNumber<double, short>(Convert.ToInt16, short.TryParse),
		float => AsNumber<float, short>(Convert.ToInt16, short.TryParse),
		decimal => AsNumber<decimal, short>(Convert.ToInt16, short.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(short).FullName}")
	};

	public Expression? AsByte() =>  Field switch
	{
		string => AsNumber<string, byte>(Convert.ToByte, byte.TryParse),
		int => AsNumber<int, byte>(Convert.ToByte, byte.TryParse),
		long => AsNumber<long, byte>(Convert.ToByte, byte.TryParse),
		short => AsNumber<short, byte>(Convert.ToByte, byte.TryParse),
		byte => AsNumber<byte, byte>(Convert.ToByte, byte.TryParse),
		double => AsNumber<double, byte>(Convert.ToByte, byte.TryParse),
		float => AsNumber<float, byte>(Convert.ToByte, byte.TryParse),
		decimal => AsNumber<decimal, byte>(Convert.ToByte, byte.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(byte).FullName}")
	};
	public Expression? AsDouble() =>  Field switch
	{
		string => AsNumber<string, double>(Convert.ToDouble, double.TryParse),
		int => AsNumber<int, double>(Convert.ToDouble, double.TryParse),
		long => AsNumber<long, double>(Convert.ToDouble, double.TryParse),
		short => AsNumber<short, double>(Convert.ToDouble, double.TryParse),
		byte => AsNumber<byte, double>(Convert.ToDouble, double.TryParse),
		double => AsNumber<double, double>(Convert.ToDouble, double.TryParse),
		float => AsNumber<float, double>(Convert.ToDouble, double.TryParse),
		decimal => AsNumber<decimal, double>(Convert.ToDouble, double.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(double).FullName}")
	};
	public Expression? AsFloat() =>  Field switch
	{
		string => AsNumber<string, float>(Convert.ToSingle, float.TryParse),
		int => AsNumber<int, float>(Convert.ToSingle, float.TryParse),
		long => AsNumber<long, float>(Convert.ToSingle, float.TryParse),
		short => AsNumber<short, float>(Convert.ToSingle, float.TryParse),
		byte => AsNumber<byte, float>(Convert.ToSingle, float.TryParse),
		double => AsNumber<double, float>(Convert.ToSingle, float.TryParse),
		float => AsNumber<float, float>(Convert.ToSingle, float.TryParse),
		decimal => AsNumber<decimal, float>(Convert.ToSingle, float.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(float).FullName}")
	};
	public Expression? AsDecimal() =>  Field switch
	{
		string => AsNumber<string, decimal>(Convert.ToDecimal, decimal.TryParse),
		int => AsNumber<int, decimal>(Convert.ToDecimal, decimal.TryParse),
		long => AsNumber<long, decimal>(Convert.ToDecimal, decimal.TryParse),
		short => AsNumber<short, decimal>(Convert.ToDecimal, decimal.TryParse),
		byte => AsNumber<byte, decimal>(Convert.ToDecimal, decimal.TryParse),
		double => AsNumber<double, decimal>(Convert.ToDecimal, decimal.TryParse),
		float => AsNumber<float, decimal>(Convert.ToDecimal, decimal.TryParse),
		decimal => AsNumber<decimal, decimal>(Convert.ToDecimal, decimal.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(decimal).FullName}")
	};

	private delegate bool NumberParser<T>(string str, NumberStyles numberStyles, IFormatProvider formatProvider, out T result);

	private Expression? AsNumber<TFrom, TTo>(Func<TFrom, TTo> converter, NumberParser<TTo> parser)
	{
		if (Field is string stringField && parser(stringField, NumberStyles.Any, options.CultureInfo, out var value))
		{
			return ExpressionExtensions.CreateConstant(value, false, options);
		}

		if (Field is TFrom numberField)
		{
			return ExpressionExtensions.CreateConstant(converter(numberField), false, options);
		}

		return null;
	}

	private delegate bool DateParser<T>(string str, IFormatProvider formatProvider, DateTimeStyles styles, out T result);

	private Expression? AsDateLike<T>(DateParser<T> parser)
	{
		if (Field is not string stringField)
		{
			return null;
		}

		return parser(stringField, options.CultureInfo, DateTimeStyles.None, out var dateOnly)
			? ExpressionExtensions.CreateConstant(dateOnly, false, options)
			: null;
	}
}

internal class DataObjectReplacer(Type? desiredType) : ExpressionVisitor
{
	protected override Expression VisitConstant(ConstantExpression node)
	{
		// All data objects will be found in constants
		if (node.Type != typeof(DataObject))
		{
			return node;
		}

		if (node.Value == null)
		{
			return Expression.Constant(null);
		}

		var dataObject = (DataObject)node.Value;

		return desiredType != null
			? ExpressionExtensions.DataObjectToExpression(dataObject, desiredType)
			: ExpressionExtensions.DataObjectToExpression(dataObject, dataObject.Field?.GetType() ?? typeof(object));
	}

	protected override Expression VisitConditional(ConditionalExpression node)
	{
		var test = Visit(node.Test);
		var ifTrue = Visit(node.IfTrue);
		var ifFalse = Visit(node.IfFalse);

		return Expression.Condition(test, ifTrue, ifFalse);
	}
}

internal class DataObjectTypeInspector : ExpressionVisitor
{
	public List<Type> DiscoveredTypes { get; } = [];

	protected override Expression VisitConstant(ConstantExpression node)
	{
		if (node.Type == typeof(DataObject) && node.Value != null)
		{
			var field = ((DataObject)node.Value).Field;
			if (field != null)
			{
				DiscoveredTypes.Add(field.GetType());
			}
		}

		return node;
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