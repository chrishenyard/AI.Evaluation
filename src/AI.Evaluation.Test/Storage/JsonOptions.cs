using System.Text.Json;
using System.Text.Json.Serialization;

namespace AI.Evaluation.Test.Storage;

public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default =
        new(JsonSerializerDefaults.General)
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            WriteIndented = true,
            AllowTrailingCommas = true,
            IgnoreReadOnlyProperties = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

    public static readonly JsonSerializerOptions Compact =
        new(Default)
        {
            WriteIndented = false,
        };
}

