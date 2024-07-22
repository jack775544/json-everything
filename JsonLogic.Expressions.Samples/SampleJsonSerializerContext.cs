using System.Text.Json.Serialization;

namespace Json.Logic.Expressions.Logic;

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(Rule))]
[JsonSerializable(typeof(CountRule))]
[JsonSerializable(typeof(ContainsRule))]
[JsonSerializable(typeof(SumRule))]
[JsonSerializable(typeof(AvgRule))]
[JsonSerializable(typeof(PowerRule))]
[JsonSerializable(typeof(SimilarityRule))]
[JsonSerializable(typeof(LeastRule))]
[JsonSerializable(typeof(GreatestRule))]
internal partial class SampleJsonSerializerContext : JsonSerializerContext;
