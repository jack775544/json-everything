using System.Globalization;

namespace Json.Logic.Expressions;

/// <summary>
/// Options for the create expression logic method.
/// </summary>
public record CreateExpressionOptions
{
	/// <summary>
	/// If set to true then constant values will be wrapped in an object and the value will be fetched from this wrapper object.
	/// This causes EF Core to use SQL variables when generating SQL.
	/// </summary>
	public bool WrapConstants { get; set; }

	/// <summary>
	/// Culture info for parse operations.
	/// </summary>
	public CultureInfo CultureInfo { get; set; } = CultureInfo.InvariantCulture;
}