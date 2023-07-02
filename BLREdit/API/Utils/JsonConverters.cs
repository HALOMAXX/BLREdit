using System;
using System.Buffers.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using BLREdit.UI;
using BLREdit.Import;
using System.Collections.Generic;
using System.CodeDom;
using PeNet.Header.Net.MetaDataTables;
using System.Reflection;
using System.Linq;

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

public class JsonGenericConverter<T> : JsonConverter<T>
{
    static PropertyInfo[] Properties { get; } = typeof(T).GetProperties();
    static FieldInfo[] Fields { get; } = typeof(T).GetFields();
    static T Default { get; } = Activator.CreateInstance<T>();


    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var item = Activator.CreateInstance<T>();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    string propertyName = reader.GetString();
                    if (reader.TokenType != JsonTokenType.Null)
                    {
                        foreach (var prop in Properties)
                        {
                            if (prop.Name == propertyName)
                            {
                                if (prop.CanWrite && prop.CanRead && !Attribute.IsDefined(prop, typeof(JsonIgnoreAttribute)))
                                {
                                    switch (prop.PropertyType.Name)
                                    {
                                        case "double":
                                            reader.Read();
                                            prop.SetValue(item, reader.GetDouble());
                                            break;
                                        case "float":
                                            reader.Read();
                                            prop.SetValue(item, (float)reader.GetDouble());
                                            break;
                                        case "string":
                                            reader.Read();
                                            prop.SetValue(item, reader.GetString());
                                            break;
                                        default:
                                            prop.SetValue(item, JsonSerializer.Deserialize(ref reader, prop.PropertyType, options));
                                            break;
                                    }
                                }
                            }
                        }

                        foreach (var field in Fields)
                        {
                            if (field.Name == propertyName)
                            {
                                if (field.IsPublic && !Attribute.IsDefined(field, typeof(JsonIgnoreAttribute)))
                                {
                                    switch (field.FieldType.Name)
                                    {
                                        case "double":
                                            reader.Read();
                                            field.SetValue(item, reader.GetDouble());
                                            break;
                                        case "float":
                                            reader.Read();
                                            field.SetValue(item, (float)reader.GetDouble());
                                            break;
                                        case "string":
                                            reader.Read();
                                            field.SetValue(item, reader.GetString());
                                            break;
                                        default:
                                            field.SetValue(item, JsonSerializer.Deserialize(ref reader, field.FieldType, options));
                                            break;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case JsonTokenType.EndObject: return item;
            }
        }
        return item;
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        if (value != null)
        {
            foreach (var prop in Properties)
            {
                if (prop.CanWrite && prop.CanRead && !Attribute.IsDefined(prop, typeof(JsonIgnoreAttribute)))
                {
                    switch (prop.PropertyType.Name)
                    {
                        case "decimal":
                        case "double":
                        case "float":
                        case "int":
                        case "long":
                        case "uint":
                        case "ulong":
                            var num = prop.GetValue(value);
                            if (num != null && !num.Equals(prop.GetValue(Default)))
                            {
                                writer.WriteNumber(prop.Name, (decimal)num);
                            }
                            break;
                        case "string":
                            if (prop.GetValue(value) is string str && !string.IsNullOrEmpty(str) && str.Equals(prop.GetValue(Default)))
                            {
                                writer.WriteString(prop.Name, str);
                            }
                            break;
                        default:
                            var val = prop.GetValue(value);
                            if (val != null && !val.Equals(prop.GetValue(Default)))
                            {
                                writer.WritePropertyName(prop.Name);
                                JsonSerializer.Serialize(writer, val, options);
                            }
                            break;
                    }
                }
            }

            foreach (var field in Fields)
            {
                if (field.IsPublic && !Attribute.IsDefined(field, typeof(JsonIgnoreAttribute)))
                {
                    switch (field.FieldType.Name)
                    {
                        case "decimal":
                        case "double":
                        case "float":
                        case "int":
                        case "long":
                        case "uint":
                        case "ulong":
                            var num = field.GetValue(value);
                            if (num != null && !num.Equals(field.GetValue(Default)))
                            {
                                writer.WriteNumber(field.Name, (decimal)num);
                            }
                            break;
                        case "string":
                            if (field.GetValue(value) is string str && !string.IsNullOrEmpty(str) && str.Equals(field.GetValue(Default)))
                            {
                                writer.WriteString(field.Name, str);
                            }
                            break;
                        default:
                            var val = field.GetValue(value);
                            if (val != null && !val.Equals(field.GetValue(Default)))
                            {
                                writer.WritePropertyName(field.Name);
                                JsonSerializer.Serialize(writer, val, options);
                            }
                            break;
                    }
                }
            }
        }
        writer.WriteEndObject();
    }
}

public sealed class JsonBLRItemConverter : JsonGenericConverter<BLRItem>
{}

public sealed class JsonBLRPawnModifiersConverter : JsonGenericConverter<BLRPawnModifiers>
{}

public sealed class JsonBLRWeaponModifiersConverter : JsonGenericConverter<BLRWeaponModifiers>
{}

public sealed class JsonBLRWeaponStatsConverter : JsonGenericConverter<BLRWeaponStats>
{}

public sealed class JsonBLRWikiStatsConverter : JsonGenericConverter<BLRWikiStats>
{}