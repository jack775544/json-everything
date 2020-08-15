﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Pointer;

namespace Json.Schema
{
	[SchemaPriority(30)]
	[SchemaKeyword(Name)]
	[JsonConverter(typeof(UnevaluatedPropertiesKeywordJsonConverter))]
	public class UnevaluatedPropertiesKeyword : IJsonSchemaKeyword
	{
		internal const string Name = "unevaluatedProperties";

		public JsonSchema Schema { get; }

		static UnevaluatedPropertiesKeyword()
		{
			ValidationContext.RegisterConsolidationMethod(ConsolidateAnnotations);
		}
		public UnevaluatedPropertiesKeyword(JsonSchema value)
		{
			Schema = value;
		}

		public void Validate(ValidationContext context)
		{
			if (context.Instance.ValueKind != JsonValueKind.Object)
			{
				context.IsValid = true;
				return;
			}

			var overallResult = true;
			var annotation = context.TryGetAnnotation(PropertiesKeyword.Name);
			var evaluatedProperties = (annotation as List<string>)?.ToList() ?? new List<string>();
			annotation = context.TryGetAnnotation(PatternPropertiesKeyword.Name);
			evaluatedProperties.AddRange(annotation as List<string> ?? Enumerable.Empty<string>());
			annotation = context.TryGetAnnotation(AdditionalPropertiesKeyword.Name);
			evaluatedProperties.AddRange(annotation as List<string> ?? Enumerable.Empty<string>());
			var unevaluatedProperties = context.Instance.EnumerateObject().Where(p => !evaluatedProperties.Contains(p.Name)).ToList();
			evaluatedProperties.Clear();
			foreach (var property in unevaluatedProperties)
			{
				if (!context.Instance.TryGetProperty(property.Name, out var item)) continue;

				var subContext = ValidationContext.From(context,
					context.InstanceLocation.Combine(PointerSegment.Create($"{property.Name}")),
					item);
				Schema.ValidateSubschema(subContext);
				overallResult &= subContext.IsValid;
				context.NestedContexts.Add(subContext);
				if (subContext.IsValid)
					evaluatedProperties.Add(property.Name);
			}

			context.Annotations[Name] = evaluatedProperties;
			context.IsValid = overallResult;
		}

		private static void ConsolidateAnnotations(IEnumerable<ValidationContext> sourceContexts, ValidationContext destContext)
		{
			var allProperties = sourceContexts.Select(c => c.TryGetAnnotation(Name))
				.Where(a => a != null)
				.Cast<List<string>>()
				.SelectMany(a => a)
				.Distinct()
				.ToList();
			if (destContext.TryGetAnnotation(Name) is List<string> annotation)
				annotation.AddRange(allProperties);
			else
				destContext.Annotations[Name] = allProperties;
		}
	}

	public class UnevaluatedPropertiesKeywordJsonConverter : JsonConverter<UnevaluatedPropertiesKeyword>
	{
		public override UnevaluatedPropertiesKeyword Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
		{
			var schema = JsonSerializer.Deserialize<JsonSchema>(ref reader, options);

			return new UnevaluatedPropertiesKeyword(schema);
		}
		public override void Write(Utf8JsonWriter writer, UnevaluatedPropertiesKeyword value, JsonSerializerOptions options)
		{
			writer.WritePropertyName(UnevaluatedPropertiesKeyword.Name);
			JsonSerializer.Serialize(writer, value.Schema, options);
		}
	}
}