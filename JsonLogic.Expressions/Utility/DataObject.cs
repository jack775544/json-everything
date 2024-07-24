using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Nodes;

namespace Json.Logic.Expressions.Utility;

internal class DataObject(JsonNode? constant, CreateExpressionOptions options)
{
	public JsonNode? Field { get; init; } = constant;

	public Expression AsString() => Field switch
	{
		null => Expression.Constant(null, typeof(string)),
		JsonArray field => Expression.NewArrayInit(typeof(string), field.Select(x => new DataObject(x, options).AsString())),
		_ => Expression.Constant(Field.Stringify()),
	};

	public Expression AsGuid(bool nullable) => AsStruct(nullable, node => Guid.Parse(node.Stringify()!));
	public Expression AsBool(bool nullable) => AsStruct(nullable, node => node.IsTruthy());
	public Expression AsDateTime(bool nullable) => AsStruct(nullable, node => DateTime.Parse(node.Stringify()!, options.CultureInfo, DateTimeStyles.AdjustToUniversal));
	public Expression AsDateOnly(bool nullable) => AsStruct(nullable, node => DateOnly.Parse(node.Stringify()!, options.CultureInfo, DateTimeStyles.None));
	public Expression AsTimeOnly(bool nullable) => AsStruct(nullable, node => TimeOnly.Parse(node.Stringify()!, options.CultureInfo, DateTimeStyles.None));
	public Expression AsDateTimeOffset(bool nullable) => AsStruct(nullable, node => DateTimeOffset.Parse(node.Stringify()!, options.CultureInfo, DateTimeStyles.AdjustToUniversal));
	public Expression AsTimeSpan(bool nullable) => AsStruct(nullable, node => TimeSpan.Parse(node.Stringify()!, options.CultureInfo));
	public Expression AsInt(bool nullable) => AsStruct(nullable, node => Convert.ToInt32(node.Numberify()));
	public Expression AsLong(bool nullable) => AsStruct(nullable, node => Convert.ToInt64(node.Numberify()));
	public Expression AsShort(bool nullable) => AsStruct(nullable, node => Convert.ToInt16(node.Numberify()));
	public Expression AsByte(bool nullable) => AsStruct(nullable, node => Convert.ToByte(node.Numberify()));
	public Expression AsDouble(bool nullable) => AsStruct(nullable, node => Convert.ToDouble(node.Numberify()));
	public Expression AsFloat(bool nullable) => AsStruct(nullable, node => Convert.ToSingle(node.Numberify()));
	public Expression AsDecimal(bool nullable) => AsStruct(nullable, node => node.Numberify()!.Value);

	private Expression AsStruct<TStruct>(bool nullable, Func<JsonNode, TStruct> parser) where TStruct : struct => Field switch
	{
		null => nullable ? Expression.Constant(null, typeof(TStruct?)) : throw new JsonLogicException($"Null found when expecting non nullable {typeof(TStruct).Name}"),
		JsonArray field => Expression.NewArrayInit(
			nullable ? typeof(TStruct?) : typeof(TStruct),
			field.Select(x => new DataObject(x, options).AsStruct(nullable, parser))),
		_ => Expression.Constant(
			parser(Field),
			nullable ? typeof(TStruct?) : typeof(TStruct)),
	};
}

internal class DataArray<T>(JsonNode? constant, CreateExpressionOptions options) : DataObject(constant, options);
