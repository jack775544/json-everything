﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Nodes;

namespace Json.Logic.Expressions.Utility;

/// <summary>
/// Helper methods for working with expressions.
/// </summary>
public static class ExpressionTypeUtilities
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
		typeof(DateTimeOffset),
		.._numberTypeHierarchy,
	];

	// List of types from the most general to the least general
	private static readonly List<Type> _typeHierarchy =
	[
		typeof(bool),
		.._comparableTypeHierarchy,
		typeof(TimeSpan),
		typeof(Guid),
		typeof(string),
	];

	private static readonly Dictionary<Type, Func<DataObject, bool, Expression?>> _dataObjectConverters = new()
	{
		{ typeof(string), static (data, _) => data.AsString() },
		{ typeof(Guid), static (data, nullable) => data.AsGuid(nullable) },
		{ typeof(DateTime), static (data, nullable) => data.AsDateTime(nullable) },
		{ typeof(DateOnly), static (data, nullable) => data.AsDateOnly(nullable) },
		{ typeof(TimeOnly), static (data, nullable) => data.AsTimeOnly(nullable) },
		{ typeof(DateTimeOffset), static (data, nullable) => data.AsDateTimeOffset(nullable) },
		{ typeof(TimeSpan), static (data, nullable) => data.AsTimeSpan(nullable) },
		{ typeof(bool), static (data, nullable) => data.AsBool(nullable) },
		{ typeof(int), static (data, nullable) => data.AsInt(nullable) },
		{ typeof(long), static (data, nullable) => data.AsLong(nullable) },
		{ typeof(short), static (data, nullable) => data.AsShort(nullable) },
		{ typeof(byte), static (data, nullable) => data.AsByte(nullable) },
		{ typeof(double), static (data, nullable) => data.AsDouble(nullable) },
		{ typeof(float), static (data, nullable) => data.AsFloat(nullable) },
		{ typeof(decimal), static (data, nullable) => data.AsDecimal(nullable) },
		{ typeof(object), static (data, _) => Expression.Constant(data.Field) }
	};

	/// <summary>
	/// Casts the provided collection of expressions to a common type. Any literals that are passed in will attempt to be parsed as the type of any non literal values.
	/// </summary>
	/// <param name="expressions">The list of expressions to cast.</param>
	/// <param name="desiredType">The desired type to cast things to. Note this is not a guarantee that the cast will succeed.</param>
	/// <returns>A list of expressions in the same order as the input, all converted to the same type.</returns>
	public static List<Expression> Downcast(IEnumerable<Expression> expressions, Type? desiredType = null)
	{
		var expressionList = expressions.ToList();
		desiredType ??= GetDesiredType(expressionList, _typeHierarchy);
		var visitor = new DataObjectReplacer(desiredType, false);
		return expressionList.Select(x => visitor.Visit(x)).ToList();
	}

	/// <summary>
	/// Casts the provided collection of expressions to a common comparable type. Any literals that are passed in will attempt to be parsed as the type of any non literal values.
	/// </summary>
	/// <param name="expressions">The list of expressions to cast.</param>
	/// <param name="desiredType">The desired numerical type.</param>
	/// <returns>A list of expressions in the same order as the input, all converted to the same type.</returns>
	public static List<Expression> DowncastNumber(IEnumerable<Expression> expressions, Type? desiredType = null)
	{
		var expressionList = expressions.ToList();

		desiredType ??= GetDesiredType(expressionList, _numberTypeHierarchy);

		var visitor = new DataObjectReplacer(desiredType, false, new DataObject(JsonNode.Parse("0"), new CreateExpressionOptions()));
		return expressionList.Select(x => visitor.Visit(x)).ToList();
	}

	/// <summary>
	/// Casts the provided collection of expressions to a common numerical type. Any literals that are passed in will attempt to be parsed as the type of any non literal values.
	/// </summary>
	/// <param name="expressions">The list of expressions to cast.</param>
	/// <returns>A list of expressions in the same order as the input, all converted to the same type.</returns>
	public static List<Expression> DowncastComparable(IEnumerable<Expression> expressions)
	{
		var expressionList = expressions.ToList();
		var mostCommonType = _comparableTypeHierarchy.Last();

		var desiredType = expressionList.Select(x => x.Type).FirstOrDefault(x => x != typeof(DataObject));
		var nullable = false;
		if (desiredType != null && desiredType != typeof(object))
		{
			mostCommonType = desiredType;
		}
		else
		{
			var results = GetDataObjects(expressionList);
			var objects = results.DiscoveredObjects;
			nullable = results.Nullable;
			var done = false;
			foreach (var comparableType in _comparableTypeHierarchy)
			{
				foreach (var dataObject in objects)
				{
					// TODO: Refactor this to not use exceptions for general flow control, Make TryConvert method instead.
					try
					{
						DataObjectToExpression(dataObject, nullable, comparableType);
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

		var visitor = new DataObjectReplacer(mostCommonType, nullable, new DataObject(0, new CreateExpressionOptions()));
		return expressionList.Select(x => visitor.Visit(x)).ToList();
	}

	private static List<Type> GetDataObjectTypes(IEnumerable<Expression> expressions)
	{
		var inspector = new DataObjectTypeInspector();

		foreach (var expression in expressions)
		{
			inspector.Visit(expression);
		}

		return inspector.DiscoveredTypes.ToList();
	}

	private static IDataObjectTypeInspectorResults GetDataObjects(IEnumerable<Expression> expressions)
	{
		var inspector = new DataObjectTypeInspector();

		foreach (var expression in expressions)
		{
			inspector.Visit(expression);
		}

		return inspector;
	}

	private static Type GetDesiredType(List<Expression> expressions, List<Type> typeHierarchy)
	{
		// Prefer parameter types
		var parameter = expressions.FirstOrDefault(ParameterFinder.ContainsParameter);

		if (parameter != null)
		{
			return parameter.Type;
		}

		// If something is explicitly declaring its type, then use it.
		var desiredType = expressions.Select(x => x.Type).FirstOrDefault(x => !typeof(DataObject).IsAssignableFrom(x));
		if (desiredType != null && desiredType != typeof(object))
		{
			return desiredType;
		}

		var discoveredTypes = GetDataObjectTypes(expressions);

		// If the type needs to be inferred, then convert to the most common type
		// The most common type will be the one that has the lowest value in the type hierarchy
		var mostCommonType = discoveredTypes.MaxBy(typeHierarchy.IndexOf) ?? typeof(string);
		return mostCommonType;
	}

	private class NullableBox<T>(T? field) where T : struct
	{
		public T? Field { get; set; } = field;
	}

	internal static Expression DataObjectToExpression(DataObject dataObject, bool nullable, Type convertTo)
	{
		var isArray = false;
		if (convertTo != typeof(string) && LogicTypeExtensions.TryGetGenericCollectionType(convertTo, out var collectionType))
		{
			isArray = true;
			convertTo = collectionType;
		}

		var isNullable = nullable;
		if (convertTo.IsGenericType && convertTo.GetGenericTypeDefinition() == typeof(Nullable<>))
		{
			convertTo = convertTo.GetGenericArguments()[0];
			isNullable = true;
		}

		if (convertTo.IsEnum)
		{
			string enumValue;

			switch (dataObject.Field)
			{
				case null:
					return CreateEnumExpression(true, convertTo, null);
				case JsonArray array:
				{
					var enumInitializers = array.Select(node => node == null
						? CreateEnumExpression(true, convertTo, null)
						: CreateEnumExpression(isNullable, convertTo, node.ToString()));

					var arrayType = isNullable
						? typeof(Nullable<>).MakeGenericType(convertTo)
						: convertTo;

					return Expression.NewArrayInit(arrayType, enumInitializers);
				}
				default:
					enumValue = dataObject.Field.ToString()!;
					break;
			}

			return CreateEnumExpression(isNullable, convertTo, enumValue);
		}

		if (convertTo != typeof(string) && typeof(IEnumerable).IsAssignableFrom(convertTo) && LogicTypeExtensions.TryGetGenericCollectionType(convertTo, out var tempConvertTo))
		{
			convertTo = tempConvertTo;
		}

		var result = _dataObjectConverters.TryGetValue(convertTo, out var converter)
			? converter(dataObject, isNullable)
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

	private static Expression CreateEnumExpression(bool isNullable, Type convertTo, string? enumValue) => !isNullable
		? Expression.Constant(Enum.Parse(convertTo, enumValue!))
		: Expression.PropertyOrField(Expression.Constant(typeof(NullableBox<>)
			.MakeGenericType(convertTo)
			.GetConstructors()
			.Single()
			.Invoke([enumValue == null ? null : Enum.Parse(convertTo, enumValue)])), nameof(NullableBox<int>.Field));
}