using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Json.Logic.Expressions.Utility;

internal interface IDataObjectTypeInspectorResults
{
	List<Type> DiscoveredTypes { get; }
	List<DataObject> DiscoveredObjects { get; }
	bool Nullable { get; }
}

internal class DataObjectTypeInspector : ExpressionVisitor, IDataObjectTypeInspectorResults
{
	public List<Type> DiscoveredTypes { get; } = [];
	public List<DataObject> DiscoveredObjects { get; } = [];
	public bool Nullable { get; private set; }

	protected override Expression VisitConstant(ConstantExpression node)
	{
		if (!typeof(DataObject).IsAssignableFrom(node.Type) || node.Value == null)
		{
			Nullable = true;
			return node;
		}

		if (node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(DataArray<>))
		{
			if (LogicTypeExtensions.TryGetGenericCollectionType(node.Type, out var genericType))
			{
				DiscoveredTypes.Add(genericType);
				return node;
			}
		}

		var dataObject = (DataObject)node.Value;
		if (dataObject.Field is JsonNode field)
		{
			DiscoveredObjects.Add(dataObject);
			DiscoveredTypes.Add(KindToType(field));
		}
		else
		{
			Nullable = true;
		}

		return node;
	}

	private static Type KindToType(JsonNode node)
	{
		switch (node.GetValueKind())
		{
			case JsonValueKind.Undefined:
			case JsonValueKind.Object:
				return typeof(object);
			case JsonValueKind.Array:
				var array = node.AsArray();

				if (array.Count == 0)
				{
					return typeof(object);
				}

				var firstNode = array.FirstOrDefault(x => x != null);
				return firstNode == null ? typeof(object) : KindToType(firstNode);
			case JsonValueKind.String:
				return typeof(string);
			case JsonValueKind.Number:
				return typeof(decimal);
			case JsonValueKind.True:
			case JsonValueKind.False:
				return typeof(bool);
			case JsonValueKind.Null:
				return typeof(object);
			default:
				throw new ArgumentOutOfRangeException();
		}
	}
}