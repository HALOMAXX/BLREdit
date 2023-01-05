using System;
using System.Buffers.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using BLREdit.UI;
using BLREdit.Import;
using System.Collections.Generic;

namespace BLREdit;

public sealed class JsonDoubleConverter : JsonConverter<double>
{
    const bool skipInputValidation = true; // Set to true to prevent intermediate parsing.  Be careful to ensure your raw JSON is well-formed.

    public override void Write(Utf8JsonWriter writer, double value, JsonSerializerOptions options)
    {
        Span<byte> utf8bytes = stackalloc byte[33]; // JsonConstants.MaximumFormatDecimalLength + 2, https://github.com/dotnet/runtime/blob/v6.0.11/src/libraries/System.Text.Json/src/System/Text/Json/JsonConstants.cs#L85
        if (!(!double.IsInfinity(value) && !double.IsNaN(value)))
            // Utf8JsonWriter does not take into account JsonSerializerOptions.NumberHandling so we have to make a recursive call to serialize
            JsonSerializer.Serialize(writer, value, new JsonSerializerOptions { NumberHandling = options.NumberHandling });
        else if (Utf8Formatter.TryFormat(value, utf8bytes.Slice(0, utf8bytes.Length - 2), out var bytesWritten))
        {
            // Check to make sure the value was actually serialized as an integer and not, say, using scientific notation for large values.
            if (IsInteger(utf8bytes, bytesWritten))
            {
                utf8bytes[bytesWritten++] = (byte)'.';
                utf8bytes[bytesWritten++] = (byte)'0';
            }
            writer.WriteRawValue(utf8bytes.Slice(0, bytesWritten), skipInputValidation);
        }
        else // Buffer was too small?
            writer.WriteNumberValue(value);
    }

    static bool IsInteger(Span<byte> utf8bytes, int bytesWritten)
    {
        if (bytesWritten <= 0)
            return false;
        var start = utf8bytes[0] == '-' ? 1 : 0;
        for (var i = start; i < bytesWritten; i++)
            if (!(utf8bytes[i] >= '0' && utf8bytes[i] <= '9'))
                return false;
        return start < bytesWritten;
    }

    public override double Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        // TODO: Handle "NaN", "Infinity", "-Infinity"
        reader.GetDouble();
}

public sealed class JsonFloatConverter : JsonConverter<float>
{
    const bool skipInputValidation = true; // Set to true to prevent intermediate parsing.  Be careful to ensure your raw JSON is well-formed.

    public override void Write(Utf8JsonWriter writer, float value, JsonSerializerOptions options)
    {
        Span<byte> utf8bytes = stackalloc byte[33]; // JsonConstants.MaximumFormatDecimalLength + 2, https://github.com/dotnet/runtime/blob/v6.0.11/src/libraries/System.Text.Json/src/System/Text/Json/JsonConstants.cs#L85
        if (!(!float.IsInfinity(value) && !float.IsNaN(value)))
            // Utf8JsonWriter does not take into account JsonSerializerOptions.NumberHandling so we have to make a recursive call to serialize
            JsonSerializer.Serialize(writer, value, new JsonSerializerOptions { NumberHandling = options.NumberHandling });
        else if (Utf8Formatter.TryFormat(value, utf8bytes.Slice(0, utf8bytes.Length - 2), out var bytesWritten))
        {
            // Check to make sure the value was actually serialized as an integer and not, say, using scientific notation for large values.
            if (IsInteger(utf8bytes, bytesWritten))
            {
                utf8bytes[bytesWritten++] = (byte)'.';
                utf8bytes[bytesWritten++] = (byte)'0';
            }
            writer.WriteRawValue(utf8bytes.Slice(0, bytesWritten), skipInputValidation);
        }
        else // Buffer was too small?
            writer.WriteNumberValue(value);
    }

    static bool IsInteger(Span<byte> utf8bytes, int bytesWritten)
    {
        if (bytesWritten <= 0)
            return false;
        var start = utf8bytes[0] == '-' ? 1 : 0;
        for (var i = start; i < bytesWritten; i++)
            if (!(utf8bytes[i] >= '0' && utf8bytes[i] <= '9'))
                return false;
        return start < bytesWritten;
    }

    public override float Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
        // TODO: Handle "NaN", "Infinity", "-Infinity"
        (float)reader.GetDecimal();
}

public class JsonUIBoolConverter : JsonConverter<UIBool>
{
    public override UIBool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return new UIBool(reader.GetBoolean());
    }

    public override void Write(Utf8JsonWriter writer, UIBool value, JsonSerializerOptions options)
    {
        writer.WriteBooleanValue(value.Is);
    }
}

public sealed class JsonBLRItemConverter : JsonConverter<BLRItem>
{
    public override BLRItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var item = new BLRItem();
        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    string propertyName = reader.GetString();
                    if (reader.TokenType != JsonTokenType.Null)
                    {
                        switch (propertyName)
                        {
                            case nameof(BLRItem.Category):
                                reader.Read();
                                item.Category = reader.GetString();
                                break;
                            case nameof(BLRItem.CP):
                                reader.Read();
                                item.CP = reader.GetDouble();
                                break;
                            case nameof(BLRItem.DescriptorName):
                                reader.Read();
                                item.DescriptorName = reader.GetString();
                                break;
                            case nameof(BLRItem.Icon):
                                reader.Read();
                                item.Icon = reader.GetString();
                                break;
                            case nameof(BLRItem.LMID):
                                reader.Read();
                                item.LMID = reader.GetInt32();
                                break;
                            case nameof(BLRItem.Name):
                                reader.Read();
                                item.Name = reader.GetString();
                                break;
                            case nameof(BLRItem.NameID):
                                reader.Read();
                                item.NameID = reader.GetInt32();
                                break;
                            case nameof(BLRItem.PawnModifiers):
                                //reader.Read();
                                item.PawnModifiers = JsonSerializer.Deserialize<BLRPawnModifiers>(ref reader, options);
                                break;
                            case nameof(BLRItem.SupportedMods):
                                //reader.Read();
                                item.SupportedMods = JsonSerializer.Deserialize<List<string>>(ref reader, options);
                                break;
                            case nameof(BLRItem.Tooltip):
                                reader.Read();
                                item.Tooltip = reader.GetString();
                                break;
                            case nameof(BLRItem.UID):
                                reader.Read();
                                item.UID = reader.GetInt32();
                                break;
                            case nameof(BLRItem.ValidFor):
                                //reader.Read();
                                item.ValidFor = JsonSerializer.Deserialize<List<int>>(ref reader, options);
                                break;
                            case nameof(BLRItem.WeaponModifiers):
                                //reader.Read();
                                item.WeaponModifiers = JsonSerializer.Deserialize<BLRWeaponModifiers>(ref reader, options);
                                break;
                            case nameof(BLRItem.WeaponStats):
                                //reader.Read();
                                item.WeaponStats = JsonSerializer.Deserialize<BLRWeaponStats>(ref reader, options);
                                break;
                            case nameof(BLRItem.WikiStats):
                                //reader.Read();
                                item.WikiStats = JsonSerializer.Deserialize<BLRWikiStats>(ref reader, options);
                                break;
                        }
                    }
                    break;
                case JsonTokenType.EndObject: return item;
            }
        }
        return item;
    }

    public override void Write(Utf8JsonWriter writer, BLRItem value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value != null)
        {
            writer.WriteString(nameof(value.Category), value.Category);

            if (value.CP > 0)
            { writer.WriteNumber(nameof(value.CP), value.CP); }

            writer.WriteString(nameof(value.DescriptorName), value.DescriptorName);

            writer.WriteString(nameof(value.Icon), value.Icon);

            if (value.LMID >= 0)
            { writer.WriteNumber(nameof(value.LMID), value.LMID); }

            writer.WriteString(nameof(value.Name), value.Name);

            writer.WriteNumber(nameof(value.NameID), value.NameID);

            if (value.PawnModifiers != null)
            {
                writer.WritePropertyName(nameof(value.PawnModifiers));
                JsonSerializer.Serialize(writer, value.PawnModifiers, options);
            }

            if (value.SupportedMods != null && value.SupportedMods.Count > 0)
            {
                writer.WritePropertyName(nameof(value.SupportedMods));
                JsonSerializer.Serialize(writer, value.SupportedMods, options);
            }

            writer.WriteString(nameof(value.Tooltip), value.Tooltip);

            writer.WriteNumber(nameof(value.UID), value.UID);

            if (value.ValidFor != null && value.ValidFor.Count > 0)
            {
                writer.WritePropertyName(nameof(value.ValidFor));
                JsonSerializer.Serialize(writer, value.ValidFor, options);
            }

            if (value.WeaponModifiers != null)
            {
                writer.WritePropertyName(nameof(value.WeaponModifiers));
                JsonSerializer.Serialize(writer, value.WeaponModifiers, options);
            }

            if (value.WeaponStats != null)
            {
                writer.WritePropertyName(nameof(value.WeaponStats));
                JsonSerializer.Serialize(writer, value.WeaponStats, options);
            }

            if (value.WikiStats != null)
            {
                writer.WritePropertyName(nameof(value.WikiStats));
                JsonSerializer.Serialize(writer, value.WikiStats, options);
            }
        }
        writer.WriteEndObject();
    }
}
