using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Logic.Expressions.Rules;

namespace Json.Logic.Expressions.Utility;

internal class DataObject(object? constant, CreateExpressionOptions options)
{
	public object? Field { get; set; } = constant;

	public Expression? AsString() => Field switch
	{
		string field => Expression.Constant(field, typeof(string)),
		int field => Expression.Constant(field.ToString(options.CultureInfo), typeof(string)),
		long field => Expression.Constant(field.ToString(options.CultureInfo), typeof(string)),
		short field => Expression.Constant(field.ToString(options.CultureInfo), typeof(string)),
		byte field => Expression.Constant(field.ToString(options.CultureInfo), typeof(string)),
		double field => Expression.Constant(field.ToString(options.CultureInfo), typeof(string)),
		float field => Expression.Constant(field.ToString(options.CultureInfo), typeof(string)),
		decimal field => Expression.Constant(field.ToString(options.CultureInfo), typeof(string)),
		bool field => Expression.Constant(field ? "true" : "false", typeof(string)),
		JsonArray field => Expression.NewArrayInit(typeof(string), field.Select(x => LiteralRuleExpression.JsonNodeToExpression(x.Stringify(), null, options, false))),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(int).FullName}")
	};

	public Expression? AsBool(bool nullable) => Field switch
	{
		string field => CreateValueConstant(!string.IsNullOrEmpty(field), nullable),
		int field => CreateValueConstant(field != 0, nullable),
		long field => CreateValueConstant(field != 0, nullable),
		short field => CreateValueConstant(field != 0, nullable),
		byte field => CreateValueConstant(field != 0, nullable),
		double field => CreateValueConstant(field != 0, nullable),
		float field => CreateValueConstant(field != 0, nullable),
		decimal field => CreateValueConstant(field != 0, nullable),
		bool field => CreateValueConstant(field, nullable),
		JsonArray field => Expression.NewArrayInit(
			nullable ? typeof(bool?) : typeof(bool),
			field.Select(x =>
			{
				var type = nullable ? typeof(bool?) : typeof(bool);

				if (x != null && x.GetValueKind() != JsonValueKind.Null)
				{
					return Expression.Constant(x.IsTruthy(), type);
				}

				if (!nullable)
				{
					throw new Exception("Null found when trying to construct non null boolean array");
				}

				return Expression.Constant(null, type);
			})),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(int).FullName}")
	};

	public Expression? AsGuid(bool nullable)
	{
		if (Field is string stringField)
		{
			return Guid.TryParse(stringField, out var guid)
				// ? ExpressionUtilities.CreateConstant(guid, false, options)
				? CreateValueConstant(guid, nullable)
				: null;
		}

		if (Field is JsonArray arrayField)
		{
			if (nullable)
			{
				return Expression.NewArrayInit(typeof(Guid?), arrayField
					.Select(x => x.Stringify())
					.Select(x => string.IsNullOrWhiteSpace(x) ? null : Guid.Parse(x) as Guid?)
					.Select(x => Expression.Constant(x, typeof(Guid?))));
			}

			return Expression.NewArrayInit(typeof(Guid), arrayField
				.Select(x => x.Stringify())
				.Select(Guid.Parse!)
				.Select(x => Expression.Constant(x)));
		}

		return null;
	}

	public Expression? AsDateTime(bool nullable) => AsDateLike<DateTime>(DateTime.TryParse, DateTimeStyles.AdjustToUniversal, nullable);
	public Expression? AsDateOnly(bool nullable) => AsDateLike<DateOnly>(DateOnly.TryParse, DateTimeStyles.None, nullable);
	public Expression? AsTimeOnly(bool nullable) => AsDateLike<TimeOnly>(TimeOnly.TryParse, DateTimeStyles.None, nullable);
	public Expression? AsDateTimeOffset(bool nullable) => AsDateLike<DateTimeOffset>(DateTimeOffset.TryParse, DateTimeStyles.AdjustToUniversal, nullable);
	public Expression? AsTimeSpan(bool nullable) => AsDateLike<TimeSpan>(
		(string str, IFormatProvider provider, DateTimeStyles _, out TimeSpan result) => TimeSpan.TryParse(str, provider, out result),
		DateTimeStyles.None,
		nullable);

	public Expression? AsInt(bool nullable) => Field switch
	{
		string => AsNumber<string, int>(Field, Convert.ToInt32, int.TryParse, nullable),
		int => AsNumber<int, int>(Field, Convert.ToInt32, int.TryParse, nullable),
		long => AsNumber<long, int>(Field, Convert.ToInt32, int.TryParse, nullable),
		short => AsNumber<short, int>(Field, Convert.ToInt32, int.TryParse, nullable),
		byte => AsNumber<byte, int>(Field, Convert.ToInt32, int.TryParse, nullable),
		double => AsNumber<double, int>(Field, Convert.ToInt32, int.TryParse, nullable),
		float => AsNumber<float, int>(Field, Convert.ToInt32, int.TryParse, nullable),
		decimal => AsNumber<decimal, int>(Field, Convert.ToInt32, int.TryParse, nullable),
		bool field => CreateValueConstant(field ? 1 : 0, false),
		JsonArray field => AsNumberArray(field, Convert.ToInt32, int.TryParse, nullable),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(int).FullName}")
	};

	public Expression? AsLong(bool nullable) => Field switch
	{
		string => AsNumber<string, long>(Field, Convert.ToInt64, long.TryParse, nullable),
		int => AsNumber<int, long>(Field, Convert.ToInt64, long.TryParse, nullable),
		long => AsNumber<long, long>(Field, Convert.ToInt64, long.TryParse, nullable),
		short => AsNumber<short, long>(Field, Convert.ToInt64, long.TryParse, nullable),
		byte => AsNumber<byte, long>(Field, Convert.ToInt64, long.TryParse, nullable),
		double => AsNumber<double, long>(Field, Convert.ToInt64, long.TryParse, nullable),
		float => AsNumber<float, long>(Field, Convert.ToInt64, long.TryParse, nullable),
		decimal => AsNumber<decimal, long>(Field, Convert.ToInt64, long.TryParse, nullable),
		bool field => CreateValueConstant(field ? 1L : 0L, false),
		JsonArray field => AsNumberArray(field, Convert.ToInt64, long.TryParse, nullable),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(long).FullName}")
	};

	public Expression? AsShort(bool nullable) => Field switch
	{
		string => AsNumber<string, short>(Field, Convert.ToInt16, short.TryParse, nullable),
		int => AsNumber<int, short>(Field, Convert.ToInt16, short.TryParse, nullable),
		long => AsNumber<long, short>(Field, Convert.ToInt16, short.TryParse, nullable),
		short => AsNumber<short, short>(Field, Convert.ToInt16, short.TryParse, nullable),
		byte => AsNumber<byte, short>(Field, Convert.ToInt16, short.TryParse, nullable),
		double => AsNumber<double, short>(Field, Convert.ToInt16, short.TryParse, nullable),
		float => AsNumber<float, short>(Field, Convert.ToInt16, short.TryParse, nullable),
		decimal => AsNumber<decimal, short>(Field, Convert.ToInt16, short.TryParse, nullable),
		bool field => CreateValueConstant(field ? (short)1 : (short)0, false),
		JsonArray field => AsNumberArray(field, Convert.ToInt16, short.TryParse, nullable),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(short).FullName}")
	};

	public Expression? AsByte(bool nullable) => Field switch
	{
		string => AsNumber<string, byte>(Field, Convert.ToByte, byte.TryParse, nullable),
		int => AsNumber<int, byte>(Field, Convert.ToByte, byte.TryParse, nullable),
		long => AsNumber<long, byte>(Field, Convert.ToByte, byte.TryParse, nullable),
		short => AsNumber<short, byte>(Field, Convert.ToByte, byte.TryParse, nullable),
		byte => AsNumber<byte, byte>(Field, Convert.ToByte, byte.TryParse, nullable),
		double => AsNumber<double, byte>(Field, Convert.ToByte, byte.TryParse, nullable),
		float => AsNumber<float, byte>(Field, Convert.ToByte, byte.TryParse, nullable),
		decimal => AsNumber<decimal, byte>(Field, Convert.ToByte, byte.TryParse, nullable),
		bool field => CreateValueConstant(field ? (byte)1 : (byte)0, false),
		JsonArray field => AsNumberArray(field, Convert.ToByte, byte.TryParse, nullable),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(byte).FullName}")
	};
	public Expression? AsDouble(bool nullable) => Field switch
	{
		string => AsNumber<string, double>(Field, Convert.ToDouble, double.TryParse, nullable),
		int => AsNumber<int, double>(Field, Convert.ToDouble, double.TryParse, nullable),
		long => AsNumber<long, double>(Field, Convert.ToDouble, double.TryParse, nullable),
		short => AsNumber<short, double>(Field, Convert.ToDouble, double.TryParse, nullable),
		byte => AsNumber<byte, double>(Field, Convert.ToDouble, double.TryParse, nullable),
		double => AsNumber<double, double>(Field, Convert.ToDouble, double.TryParse, nullable),
		float => AsNumber<float, double>(Field, Convert.ToDouble, double.TryParse, nullable),
		decimal => AsNumber<decimal, double>(Field, Convert.ToDouble, double.TryParse, nullable),
		bool field => CreateValueConstant(field ? 1d : 0d, false),
		JsonArray field => AsNumberArray(field, Convert.ToDouble, double.TryParse, nullable),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(double).FullName}")
	};
	public Expression? AsFloat(bool nullable) => Field switch
	{
		string => AsNumber<string, float>(Field, Convert.ToSingle, float.TryParse, nullable),
		int => AsNumber<int, float>(Field, Convert.ToSingle, float.TryParse, nullable),
		long => AsNumber<long, float>(Field, Convert.ToSingle, float.TryParse, nullable),
		short => AsNumber<short, float>(Field, Convert.ToSingle, float.TryParse, nullable),
		byte => AsNumber<byte, float>(Field, Convert.ToSingle, float.TryParse, nullable),
		double => AsNumber<double, float>(Field, Convert.ToSingle, float.TryParse, nullable),
		float => AsNumber<float, float>(Field, Convert.ToSingle, float.TryParse, nullable),
		decimal => AsNumber<decimal, float>(Field, Convert.ToSingle, float.TryParse, nullable),
		bool field => CreateValueConstant(field ? 1f : 0f, false),
		JsonArray field => AsNumberArray(field, Convert.ToSingle, float.TryParse, nullable),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(float).FullName}")
	};
	public Expression? AsDecimal(bool nullable) => Field switch
	{
		string => AsNumber<string, decimal>(Field, Convert.ToDecimal, decimal.TryParse, nullable),
		int => AsNumber<int, decimal>(Field, Convert.ToDecimal, decimal.TryParse, nullable),
		long => AsNumber<long, decimal>(Field, Convert.ToDecimal, decimal.TryParse, nullable),
		short => AsNumber<short, decimal>(Field, Convert.ToDecimal, decimal.TryParse, nullable),
		byte => AsNumber<byte, decimal>(Field, Convert.ToDecimal, decimal.TryParse, nullable),
		double => AsNumber<double, decimal>(Field, Convert.ToDecimal, decimal.TryParse, nullable),
		float => AsNumber<float, decimal>(Field, Convert.ToDecimal, decimal.TryParse, nullable),
		decimal => AsNumber<decimal, decimal>(Field, Convert.ToDecimal, decimal.TryParse, nullable),
		bool field => CreateValueConstant(field ? 1m : 0m, false),
		JsonArray field => AsNumberArray(field, Convert.ToDecimal, decimal.TryParse, nullable),
		_ => throw new InvalidOperationException($"Could not convert {Field?.GetType().FullName} to {typeof(decimal).FullName}")
	};

	private delegate bool NumberParser<T>(string str, NumberStyles numberStyles, IFormatProvider formatProvider, out T result);

	private NewArrayExpression AsNumberArray<TTo>(JsonArray field, Func<decimal, TTo> converter, NumberParser<TTo> parser, bool nullable)
		where TTo : struct
	{
		var defaultValue = typeof(TTo).IsValueType
			? Expression.Constant(Activator.CreateInstance(typeof(TTo)))
			: Expression.Constant(null);
		return Expression.NewArrayInit(
			nullable ? typeof(TTo?) : typeof(TTo),
			field.Select(x =>
			{
				var value = x.Numberify();
				return AsNumber(value, converter, parser, nullable) ?? defaultValue;
			}));
	}

	private Expression? AsNumber<TFrom, TTo>(object? field, Func<TFrom, TTo> converter, NumberParser<TTo> parser, bool nullable)
		where TTo : struct
	{
		if (field is string stringField && parser(stringField, NumberStyles.Any, options.CultureInfo, out var value))
		{
			return CreateValueConstant(value, nullable);
		}

		if (field is TFrom numberField)
		{
			return CreateValueConstant(converter(numberField), nullable);
		}

		return null;
	}

	private delegate bool DateParser<T>(string str, IFormatProvider formatProvider, DateTimeStyles styles, out T result);

	private Expression? AsDateLike<T>(DateParser<T> parser, DateTimeStyles style, bool nullable)
		where T : struct
	{
		if (Field is string stringField)
		{
			return parser(stringField, options.CultureInfo, style, out var dateLike)
				? CreateValueConstant(dateLike, nullable)
				: null;
		}

		if (Field is JsonArray arrayField)
		{
			if (nullable)
			{
				return Expression.NewArrayInit(typeof(T?), arrayField
					.Select(x => x.Stringify())
					.Select(x =>
					{
						if (string.IsNullOrWhiteSpace(x))
						{
							return null;
						}

						if (parser(x, options.CultureInfo, style, out var dateLike))
						{
							return dateLike as T?;
						}

						return null;
					})
					.Select(x => Expression.Constant(x, typeof(T?))));
			}

			return Expression.NewArrayInit(typeof(T), arrayField
				.Select(x => x.Stringify())
				.Select(x =>
				{
					if (!parser(x!, options.CultureInfo, style, out var dateLike))
					{
						throw new JsonLogicException($"Invalid date time '{x}' when parsing literal");
					}

					return dateLike;
				})
				.Select(x => Expression.Constant(x)));
		}

		return null;

	}

	private static Expression CreateValueConstant<T>(T value, bool nullable) where T : struct
	{
		return Expression.Constant(value, nullable ? typeof(T?) : typeof(T));
	}
}

internal class DataArray<T>(object? constant, CreateExpressionOptions options) : DataObject(constant, options);
