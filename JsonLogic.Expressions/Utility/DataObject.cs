using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json.Nodes;
using Json.Logic.Expressions.Rules;

namespace Json.Logic.Expressions.Utility;

internal class DataObject(object? constant, CreateExpressionOptions options)
{
	public object? Field { get; set; } = constant;

	public Expression? AsString() => Field switch
	{
		string field => ExpressionUtilities.CreateConstant(field, false, options),
		int field => ExpressionUtilities.CreateConstant(field.ToString(options.CultureInfo), false, options),
		long field => ExpressionUtilities.CreateConstant(field.ToString(options.CultureInfo), false, options),
		short field => ExpressionUtilities.CreateConstant(field.ToString(options.CultureInfo), false, options),
		byte field => ExpressionUtilities.CreateConstant(field.ToString(options.CultureInfo), false, options),
		double field => ExpressionUtilities.CreateConstant(field.ToString(options.CultureInfo), false, options),
		float field => ExpressionUtilities.CreateConstant(field.ToString(options.CultureInfo), false, options),
		decimal field => ExpressionUtilities.CreateConstant(field.ToString(options.CultureInfo), false, options),
		bool field => ExpressionUtilities.CreateConstant(field ? "true" : "false", false, options),
		JsonArray field => Expression.NewArrayInit(typeof(string), field.Select(x => LiteralRuleExpression.JsonNodeToExpression(x.Stringify(), null, options, false))),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(int).FullName}")
	};

	public Expression? AsBool() => Field switch
	{
		string field => ExpressionUtilities.CreateConstant(!string.IsNullOrEmpty(field), false, options),
		int field => ExpressionUtilities.CreateConstant(field != 0, false, options),
		long field => ExpressionUtilities.CreateConstant(field != 0, false, options),
		short field => ExpressionUtilities.CreateConstant(field != 0, false, options),
		byte field => ExpressionUtilities.CreateConstant(field != 0, false, options),
		double field => ExpressionUtilities.CreateConstant(field != 0, false, options),
		float field => ExpressionUtilities.CreateConstant(field != 0, false, options),
		decimal field => ExpressionUtilities.CreateConstant(field != 0, false, options),
		bool field => ExpressionUtilities.CreateConstant(field, false, options),
		JsonArray field => Expression.NewArrayInit(typeof(bool), field.Select(x => LiteralRuleExpression.JsonNodeToExpression(x.IsTruthy(), null, options, false))),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(int).FullName}")
	};

	public Expression? AsGuid()
	{
		if (Field is not string stringField)
		{
			return null;
		}

		return Guid.TryParse(stringField, out var guid)
			? ExpressionUtilities.CreateConstant(guid, false, options)
			: null;
	}

	public Expression? AsDateTime() => AsDateLike<DateTime>(DateTime.TryParse, DateTimeStyles.AdjustToUniversal);
	public Expression? AsDateOnly() => AsDateLike<DateOnly>(DateOnly.TryParse, DateTimeStyles.None);
	public Expression? AsTimeOnly() => AsDateLike<TimeOnly>(TimeOnly.TryParse, DateTimeStyles.None);

	public Expression? AsInt() => Field switch
	{
		string => AsNumber<string, int>(Field, Convert.ToInt32, int.TryParse),
		int => AsNumber<int, int>(Field, Convert.ToInt32, int.TryParse),
		long => AsNumber<long, int>(Field, Convert.ToInt32, int.TryParse),
		short => AsNumber<short, int>(Field, Convert.ToInt32, int.TryParse),
		byte => AsNumber<byte, int>(Field, Convert.ToInt32, int.TryParse),
		double => AsNumber<double, int>(Field, Convert.ToInt32, int.TryParse),
		float => AsNumber<float, int>(Field, Convert.ToInt32, int.TryParse),
		decimal => AsNumber<decimal, int>(Field, Convert.ToInt32, int.TryParse),
		bool field => ExpressionUtilities.CreateConstant(field ? 1 : 0, false, options),
		JsonArray field => AsArray(field, Convert.ToInt32, int.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(int).FullName}")
	};

	public Expression? AsLong() => Field switch
	{
		string => AsNumber<string, long>(Field, Convert.ToInt64, long.TryParse),
		int => AsNumber<int, long>(Field, Convert.ToInt64, long.TryParse),
		long => AsNumber<long, long>(Field, Convert.ToInt64, long.TryParse),
		short => AsNumber<short, long>(Field, Convert.ToInt64, long.TryParse),
		byte => AsNumber<byte, long>(Field, Convert.ToInt64, long.TryParse),
		double => AsNumber<double, long>(Field, Convert.ToInt64, long.TryParse),
		float => AsNumber<float, long>(Field, Convert.ToInt64, long.TryParse),
		decimal => AsNumber<decimal, long>(Field, Convert.ToInt64, long.TryParse),
		bool field => ExpressionUtilities.CreateConstant(field ? 1L : 0L, false, options),
		JsonArray field => AsArray(field, Convert.ToInt64, long.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(long).FullName}")
	};

	public Expression? AsShort() =>  Field switch
	{
		string => AsNumber<string, short>(Field, Convert.ToInt16, short.TryParse),
		int => AsNumber<int, short>(Field, Convert.ToInt16, short.TryParse),
		long => AsNumber<long, short>(Field, Convert.ToInt16, short.TryParse),
		short => AsNumber<short, short>(Field, Convert.ToInt16, short.TryParse),
		byte => AsNumber<byte, short>(Field, Convert.ToInt16, short.TryParse),
		double => AsNumber<double, short>(Field, Convert.ToInt16, short.TryParse),
		float => AsNumber<float, short>(Field, Convert.ToInt16, short.TryParse),
		decimal => AsNumber<decimal, short>(Field, Convert.ToInt16, short.TryParse),
		bool field => ExpressionUtilities.CreateConstant(field ? (short)1 : (short)0, false, options),
		JsonArray field => AsArray(field, Convert.ToInt16, short.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(short).FullName}")
	};

	public Expression? AsByte() =>  Field switch
	{
		string => AsNumber<string, byte>(Field, Convert.ToByte, byte.TryParse),
		int => AsNumber<int, byte>(Field, Convert.ToByte, byte.TryParse),
		long => AsNumber<long, byte>(Field, Convert.ToByte, byte.TryParse),
		short => AsNumber<short, byte>(Field, Convert.ToByte, byte.TryParse),
		byte => AsNumber<byte, byte>(Field, Convert.ToByte, byte.TryParse),
		double => AsNumber<double, byte>(Field, Convert.ToByte, byte.TryParse),
		float => AsNumber<float, byte>(Field, Convert.ToByte, byte.TryParse),
		decimal => AsNumber<decimal, byte>(Field, Convert.ToByte, byte.TryParse),
		bool field => ExpressionUtilities.CreateConstant(field ? (byte)1 : (byte)0, false, options),
		JsonArray field => AsArray(field, Convert.ToByte, byte.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(byte).FullName}")
	};
	public Expression? AsDouble() =>  Field switch
	{
		string => AsNumber<string, double>(Field, Convert.ToDouble, double.TryParse),
		int => AsNumber<int, double>(Field, Convert.ToDouble, double.TryParse),
		long => AsNumber<long, double>(Field, Convert.ToDouble, double.TryParse),
		short => AsNumber<short, double>(Field, Convert.ToDouble, double.TryParse),
		byte => AsNumber<byte, double>(Field, Convert.ToDouble, double.TryParse),
		double => AsNumber<double, double>(Field, Convert.ToDouble, double.TryParse),
		float => AsNumber<float, double>(Field, Convert.ToDouble, double.TryParse),
		decimal => AsNumber<decimal, double>(Field, Convert.ToDouble, double.TryParse),
		bool field => ExpressionUtilities.CreateConstant(field ? 1d : 0d, false, options),
		JsonArray field => AsArray(field, Convert.ToDouble, double.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(double).FullName}")
	};
	public Expression? AsFloat() =>  Field switch
	{
		string => AsNumber<string, float>(Field, Convert.ToSingle, float.TryParse),
		int => AsNumber<int, float>(Field, Convert.ToSingle, float.TryParse),
		long => AsNumber<long, float>(Field, Convert.ToSingle, float.TryParse),
		short => AsNumber<short, float>(Field, Convert.ToSingle, float.TryParse),
		byte => AsNumber<byte, float>(Field, Convert.ToSingle, float.TryParse),
		double => AsNumber<double, float>(Field, Convert.ToSingle, float.TryParse),
		float => AsNumber<float, float>(Field, Convert.ToSingle, float.TryParse),
		decimal => AsNumber<decimal, float>(Field, Convert.ToSingle, float.TryParse),
		bool field => ExpressionUtilities.CreateConstant(field ? 1f : 0f, false, options),
		JsonArray field => AsArray(field, Convert.ToSingle, float.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(float).FullName}")
	};
	public Expression? AsDecimal() =>  Field switch
	{
		string => AsNumber<string, decimal>(Field, Convert.ToDecimal, decimal.TryParse),
		int => AsNumber<int, decimal>(Field, Convert.ToDecimal, decimal.TryParse),
		long => AsNumber<long, decimal>(Field, Convert.ToDecimal, decimal.TryParse),
		short => AsNumber<short, decimal>(Field, Convert.ToDecimal, decimal.TryParse),
		byte => AsNumber<byte, decimal>(Field, Convert.ToDecimal, decimal.TryParse),
		double => AsNumber<double, decimal>(Field, Convert.ToDecimal, decimal.TryParse),
		float => AsNumber<float, decimal>(Field, Convert.ToDecimal, decimal.TryParse),
		decimal => AsNumber<decimal, decimal>(Field, Convert.ToDecimal, decimal.TryParse),
		bool field => ExpressionUtilities.CreateConstant(field ? 1m : 0m, false, options),
		JsonArray field => AsArray(field, Convert.ToDecimal, decimal.TryParse),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(decimal).FullName}")
	};

	private delegate bool NumberParser<T>(string str, NumberStyles numberStyles, IFormatProvider formatProvider, out T result);

	private NewArrayExpression AsArray<TTo>(JsonArray field, Func<decimal, TTo> converter, NumberParser<TTo> parser)
	{
		var defaultValue = typeof(TTo).IsValueType
			? Expression.Constant(Activator.CreateInstance(typeof(TTo)))
			: Expression.Constant(null);
		return Expression.NewArrayInit(
			typeof(TTo),
			field.Select(x => AsNumber(x.Numberify(), converter, parser) ?? defaultValue));
	}

	private Expression? AsNumber<TFrom, TTo>(object? field, Func<TFrom, TTo> converter, NumberParser<TTo> parser)
	{
		if (field is string stringField && parser(stringField, NumberStyles.Any, options.CultureInfo, out var value))
		{
			return ExpressionUtilities.CreateConstant(value, false, options);
		}

		if (field is TFrom numberField)
		{
			return ExpressionUtilities.CreateConstant(converter(numberField), false, options);
		}

		return null;
	}

	private delegate bool DateParser<T>(string str, IFormatProvider formatProvider, DateTimeStyles styles, out T result);

	private Expression? AsDateLike<T>(DateParser<T> parser, DateTimeStyles style)
	{
		if (Field is not string stringField)
		{
			return null;
		}

		return parser(stringField, options.CultureInfo, style, out var dateLike)
			? ExpressionUtilities.CreateConstant(dateLike, false, options)
			: null;
	}
}

internal class DataArray<T>(object? constant, CreateExpressionOptions options) : DataObject(constant, options);
