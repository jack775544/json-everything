using System;
using System.Globalization;
using System.Linq.Expressions;

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
		bool field => ExpressionUtilities.CreateConstant(field ? 1 : 0, false, options),
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
		bool field => ExpressionUtilities.CreateConstant(field ? 1L : 0L, false, options),
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
		bool field => ExpressionUtilities.CreateConstant(field ? (short)1 : (short)0, false, options),
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
		bool field => ExpressionUtilities.CreateConstant(field ? (byte)1 : (byte)0, false, options),
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
		bool field => ExpressionUtilities.CreateConstant(field ? 1d : 0d, false, options),
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
		bool field => ExpressionUtilities.CreateConstant(field ? 1f : 0f, false, options),
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
		bool field => ExpressionUtilities.CreateConstant(field ? 1m : 0m, false, options),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(decimal).FullName}")
	};

	private delegate bool NumberParser<T>(string str, NumberStyles numberStyles, IFormatProvider formatProvider, out T result);

	private Expression? AsNumber<TFrom, TTo>(Func<TFrom, TTo> converter, NumberParser<TTo> parser)
	{
		if (Field is string stringField && parser(stringField, NumberStyles.Any, options.CultureInfo, out var value))
		{
			return ExpressionUtilities.CreateConstant(value, false, options);
		}

		if (Field is TFrom numberField)
		{
			return ExpressionUtilities.CreateConstant(converter(numberField), false, options);
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
			? ExpressionUtilities.CreateConstant(dateOnly, false, options)
			: null;
	}
}