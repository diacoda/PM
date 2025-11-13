using System.Text.Json;
using System.Text.Json.Serialization;

namespace PM.API.HostedServices;
/// <summary>
/// JSON converter for DateOnly to ensure compatibility on .NET 6.
/// </summary>
public sealed class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    private const string Format = "yyyy-MM-dd";

    /// <summary>
    /// Reads and converts the JSON to DateOnly.
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="typeToConvert"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var s = reader.GetString();
        if (DateOnly.TryParseExact(s, Format, null, System.Globalization.DateTimeStyles.None, out var d))
            return d;

        // Fallback: try general parse
        return DateOnly.Parse(s!);
    }

    /// <summary>
    /// Writes the DateOnly value as JSON.
    /// </summary>
    /// <param name="writer"></param>
    /// <param name="value"></param>
    /// <param name="options"></param>
    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(Format));
    }
}