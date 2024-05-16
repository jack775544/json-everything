using System.Text.Json.Serialization;

namespace Json.Logic.Expressions.Tests;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Rule))]
internal partial class TestDataSerializerContext : JsonSerializerContext
{
}