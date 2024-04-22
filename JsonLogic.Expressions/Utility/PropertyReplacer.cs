using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Json.Logic.Expressions.Utility;

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
		// Can suppress null warning since we know that our visitor will not give back nulls
		return new PropertyReplacer(replacerInfos).Visit(expression)!;
	}
}