using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Json.Logic.Expressions.Utility;

internal static class ExpressionTypeUtilities
{
	private static readonly List<Type> _numberTypeHierarchy =
	[
		typeof(byte),
		typeof(short),
		typeof(long),
		typeof(int),
		typeof(float),
		typeof(double),
		typeof(decimal),
	];

	private static readonly List<Type> _comparableTypeHierarchy =
	[
		typeof(TimeOnly),
		typeof(DateOnly),
		typeof(DateTime),
		.._numberTypeHierarchy,
	];

	// List of types from the most general to the least general
	private static readonly List<Type> _typeHierarchy =
	[
		typeof(bool),
		.._comparableTypeHierarchy,
		typeof(Guid),
		typeof(string),
	];

	public static readonly Dictionary<Type, Func<DataObject, Expression?>> DataObjectConverters = new()
	{
		{ typeof(string), static data => data.AsString() },
		{ typeof(Guid), static data => data.AsGuid() },
		{ typeof(DateTime), static data => data.AsDateTime() },
		{ typeof(DateOnly), static data => data.AsDateOnly() },
		{ typeof(TimeOnly), static data => data.AsTimeOnly() },
		{ typeof(bool), static data => data.AsBool() },
		{ typeof(int), static data => data.AsInt() },
		{ typeof(long), static data => data.AsLong() },
		{ typeof(short), static data => data.AsShort() },
		{ typeof(byte), static data => data.AsByte() },
		{ typeof(double), static data => data.AsDouble() },
		{ typeof(float), static data => data.AsFloat() },
		{ typeof(decimal), static data => data.AsDecimal() },
		{ typeof(object), static data => Expression.Constant(data.Field) }
	};

	public static List<Expression> Downcast(this IEnumerable<Expression> expressions, Type? desiredType = null)
	{
		var expressionList = expressions.ToList();
		desiredType ??= GetDesiredType(expressionList, _typeHierarchy);
		var visitor = new DataObjectReplacer(desiredType);
		return expressionList.Select(x => visitor.Visit(x)).ToList();
	}

	public static List<Expression> DowncastNumber(this IEnumerable<Expression> expressions)
	{
		var expressionList = expressions.ToList();
		var types = GetDataObjectTypes(expressionList);
		var mostCommonType = types.Any(x => _numberTypeHierarchy.Contains(x))
			? types.MaxBy(x => _numberTypeHierarchy.IndexOf(x)) ?? _numberTypeHierarchy.Last()
			: _numberTypeHierarchy.Last();
		var visitor = new DataObjectReplacer(mostCommonType, new DataObject(0, new CreateExpressionOptions()));
		return expressionList.Select(x => visitor.Visit(x)).ToList();
	}

	public static List<Expression> DowncastComparable(this IEnumerable<Expression> expressions)
	{
		var expressionList = expressions.ToList();
		var mostCommonType = _comparableTypeHierarchy.Last();

		var desiredType = expressionList.Select(x => x.Type).FirstOrDefault(x => x != typeof(DataObject));
		if (desiredType != null && desiredType != typeof(object))
		{
			mostCommonType = desiredType;
		}
		else
		{
			var objects = GetDataObjects(expressionList);
			var done = false;
			foreach (var comparableType in _comparableTypeHierarchy)
			{
				foreach (var dataObject in objects)
				{
					// TODO: Refactor this to not use exceptions for general flow control, Make TryConvert method instead.
					try
					{
						DataObjectToExpression(dataObject, comparableType);
						mostCommonType = comparableType;
						done = true;
						break;
					}
					catch
					{
						// Can't convert
					}
				}

				if (done) break;
			}
		}

		var visitor = new DataObjectReplacer(mostCommonType, new DataObject(0, new CreateExpressionOptions()));
		return expressionList.Select(x => visitor.Visit(x)).ToList();
	}

	public static List<Type> GetDataObjectTypes(IEnumerable<Expression> expressions)
	{
		var inspector = new DataObjectTypeInspector();

		foreach (var expression in expressions)
		{
			inspector.Visit(expression);
		}

		return inspector.DiscoveredTypes.ToList();
	}

	public static List<DataObject> GetDataObjects(IEnumerable<Expression> expressions)
	{
		var inspector = new DataObjectTypeInspector();

		foreach (var expression in expressions)
		{
			inspector.Visit(expression);
		}

		return inspector.DiscoveredObjects.ToList();
	}

	private static Type GetDesiredType(List<Expression> expressions, List<Type> typeHierarchy)
	{
		// If something is explicitly declaring its type, then use it.
		var desiredType = expressions.Select(x => x.Type).FirstOrDefault(x => !typeof(DataObject).IsAssignableFrom(x));
		if (desiredType != null && desiredType != typeof(object))
		{
			return desiredType;
		}

		var discoveredTypes = GetDataObjectTypes(expressions);

		// If the type needs to be inferred, then convert to the most common type
		// The most common type will be the one that has the lowest value in the type hierarchy
		var mostCommonType = discoveredTypes.MaxBy(x => _typeHierarchy.IndexOf(x)) ?? typeof(string);
		return mostCommonType;
	}

	private class NullableBox<T>(T? field) where T : struct
	{
		public T? Field { get; set; } = field;
	}

	public static Expression DataObjectToExpression(DataObject dataObject, Type convertTo)
	{
		var isArray = false;
		if (convertTo != typeof(string) && convertTo.TryGetGenericCollectionType(out var collectionType))
		{
			isArray = true;
			convertTo = collectionType;
		}

		var isNullable = false;
		if (convertTo.IsGenericType && convertTo.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			convertTo = convertTo.GetGenericArguments()[0];
			isNullable = true;
		}

		if (convertTo.IsEnum && dataObject.Field != null)
		{
			string enumValue;

			if (dataObject.Field is string stringField)
			{
				enumValue = stringField;
			}
			else
			{
				enumValue = dataObject.Field.ToString()!;
			}

			return !isNullable
				? Expression.Constant(Enum.Parse(convertTo, enumValue))
				: Expression.PropertyOrField(Expression.Constant(typeof(NullableBox<>)
					.MakeGenericType(convertTo)
					.GetConstructors()
					.Single()
					// .Single(x => x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == convertTo)
					.Invoke([Enum.Parse(convertTo, enumValue)])), nameof(NullableBox<int>.Field));
		}

		if (convertTo != typeof(string) && typeof(IEnumerable).IsAssignableFrom(convertTo) && convertTo.TryGetGenericCollectionType(out var tempConvertTo))
		{
			convertTo = tempConvertTo;
		}

		var result = DataObjectConverters.TryGetValue(convertTo, out var converter)
			? converter(dataObject)
			: throw new InvalidOperationException($"Could not find converter to convert {dataObject.Field} to {convertTo.FullName}");

		if (result == null)
		{
			throw new InvalidOperationException($"Could not convert {dataObject.Field} to {convertTo.FullName}");
		}

		return isArray
			? result is NewArrayExpression
				? result
				: Expression.NewArrayInit(convertTo, result.Type.IsValueType ? Expression.Convert(result, typeof(object)) : result)
			: result;
	}
}