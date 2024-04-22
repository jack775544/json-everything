using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Json.Logic.Expressions.Utility;

internal class DataObjectTypeInspector : ExpressionVisitor
{
	public List<Type> DiscoveredTypes { get; } = [];
	public List<DataObject> DiscoveredObjects { get; } = [];

	protected override Expression VisitConstant(ConstantExpression node)
	{
		if (node.Type != typeof(DataObject) || node.Value == null)
		{
			return node;
		}

		var dataObject = ((DataObject)node.Value);
		var field = dataObject.Field;
		if (field != null)
		{
			DiscoveredObjects.Add(dataObject);
			DiscoveredTypes.Add(field.GetType());
		}

		return node;
	}
}