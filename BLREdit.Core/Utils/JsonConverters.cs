using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections;

namespace BLREdit.Core.Utils;

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
                    var propertyName = reader.GetString();
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
        if (value is null || value.Equals(Default) || writer is null) return;
        writer.WriteStartObject();

        foreach (var prop in Properties)
        {
            if (prop.CanWrite && prop.CanRead && !Attribute.IsDefined(prop, typeof(JsonIgnoreAttribute)))
            {
                var val = prop.GetValue(value);
                if (val is null) continue;
                if (val.Equals(prop.GetValue(Default))) continue;
                WriteData(writer, prop.Name, val, options);
            }
        }

        foreach (var field in Fields)
        {
            if (field.IsPublic && !Attribute.IsDefined(field, typeof(JsonIgnoreAttribute)))
            {
                var val = field.GetValue(value); 
                if (val is null) continue;
                if (val.Equals(field.GetValue(Default))) continue;
                WriteData(writer, field.Name, val, options);
            }
        }
        writer.WriteEndObject();
    }

    private static void WriteData(Utf8JsonWriter writer, string propertyName, object value, JsonSerializerOptions options)
    {
        switch (value)
        {
            case decimal decimalNumber:
                writer.WriteNumber(propertyName, decimalNumber);
                break;
            case double doublePrecision:
                writer.WriteNumber(propertyName, doublePrecision);
                break;
            case float floating:
                writer.WriteNumber(propertyName, floating);
                break;
            case int integer:
                writer.WriteNumber(propertyName, integer);
                break;
            case long longNumber:
                writer.WriteNumber(propertyName, longNumber);
                break;
            case uint unsignedInteger:
                writer.WriteNumber(propertyName, unsignedInteger);
                break;
            case ulong unsignedLong:
                writer.WriteNumber(propertyName, unsignedLong);
                break;
            case string text:
                if(!string.IsNullOrEmpty(text)) writer.WriteString(propertyName, text);
                break;
            case IList list:
                if (list.Count > 0)
                {
                    writer.WritePropertyName(propertyName);
                    JsonSerializer.Serialize(writer, value, options);
                }
                break;
            default:
                writer.WritePropertyName(propertyName);
                JsonSerializer.Serialize(writer, value, options);
                break;
        }
    }
}