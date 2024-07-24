using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Json.Logic.Expressions.Rules;

namespace Json.Logic.Expressions.Utility;

/// <summary>
/// Extensions for <see cref="Type"/>.
/// </summary>
public static class LogicTypeExtensions
{
	/// <summary>
	/// Gets the generic type from a list type.
	/// </summary>
	/// <param name="type">The type to get the generic list type from.</param>
	/// <param name="genericType">The retrieved generic type. Is null when return value is false.</param>
	/// <returns>true if type param is a list, false otherwise.</returns>
	internal static bool TryGetGenericListType(this Type type, [NotNullWhen(true)]out Type? genericType)
	{
		genericType = type
			.GetInterfaces()
			.Where(x => x.IsGenericType)
			.SingleOrDefault(x => x.GetGenericTypeDefinition() == typeof(IList<>))
			?.GetGenericArguments()
			.Single();

		return genericType != null;
	}
	
	/// <summary>
	/// Tries getting the generic type parameter for the type passed in if it is a collection.
	/// </summary>
	/// <param name="type">The collection type to ge the generic for.</param>
	/// <param name="genericType">The output generic type.</param>
	/// <returns>True if the input type is a collection and a generic could be retrieved, false otherwise.</returns>
	public static bool TryGetGenericCollectionType(Type type, [NotNullWhen(true)]out Type? genericType)
	{
		if (type.IsArray)
		{
			genericType = type.GetElementType()!;
			return true;
		}

		if (type.IsGenericType)
		{
			if (type.GetGenericTypeDefinition() == typeof(DataArray<>))
			{
				var arrayGeneric = type.GenericTypeArguments[0];
				if (arrayGeneric == typeof(LiteralRuleExpression.UndefinedLiteralType))
				{
					genericType = typeof(object);
				}
				else if (arrayGeneric == typeof(LiteralRuleExpression.ObjectLiteralType))
				{
					genericType = typeof(object);
				}
				else if (arrayGeneric == typeof(LiteralRuleExpression.ArrayLiteralType))
				{
					genericType = typeof(object);
				}
				else if (arrayGeneric == typeof(LiteralRuleExpression.StringLiteralType))
				{
					genericType = typeof(string);
				}
				else if (arrayGeneric == typeof(LiteralRuleExpression.NumberLiteralType))
				{
					genericType = typeof(decimal);
				}
				else if (arrayGeneric == typeof(LiteralRuleExpression.BooleanLiteralType))
				{
					genericType = typeof(bool);
				}
				else
				{
					genericType = null;
					return false;
				}

				return true;
			}

			if (type.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			{
				genericType = type.GenericTypeArguments[0];
				return true;
			}
		}

		genericType = type
			.GetInterfaces()
			.Where(x => x.IsGenericType)
			.SingleOrDefault(x => x.GetGenericTypeDefinition() == typeof(IEnumerable<>))
			?.GetGenericArguments()
			.Single();

		return genericType != null;
	}
	
	internal static bool IsCollectionType(this Type type)
	{
		return type.IsArray || TryGetGenericListType(type, out _);
	}
}