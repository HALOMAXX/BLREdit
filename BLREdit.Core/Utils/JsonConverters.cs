using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections;
using System.Text.Json.Nodes;
using BLREdit.Core.Models.BLR.Item;

namespace BLREdit.Core.Utils;

public abstract class JsonGenericConverter<T> : JsonConverter<T>
{
    static Dictionary<string, PropertyInfo> Properties { get; } = new();
    static Dictionary<string, FieldInfo> Fields { get; } = new();
    protected static T? Default { get; set; }

    static JsonGenericConverter()
    { 
        var type = typeof(T);
        var props = type.GetProperties();
        var field = type.GetFields();
        foreach (var info in props)
        {
            Properties.Add(info.Name, info);
        }
        foreach (var info in field)
        {
            Fields.Add(info.Name, info);
        }
        //Default = Activator.CreateInstance<T>();
    }

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        T item = Activator.CreateInstance<T>();

        while (reader.Read())
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.PropertyName:
                    var propertyName = reader.GetString();
                    if (propertyName is not null && reader.TokenType != JsonTokenType.Null)
                    {
                        if (Properties.TryGetValue(propertyName, out var prop))
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
                        if(Fields.TryGetValue(propertyName, out var field))
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

        foreach (var prop in Properties.Values)
        {
            if (prop.CanWrite && prop.CanRead && !Attribute.IsDefined(prop, typeof(JsonIgnoreAttribute)))
            {
                var val = prop.GetValue(value);
                if (val is null) continue;
                if (val.Equals(prop.GetValue(Default))) continue;
                WriteData(writer, prop.Name, val, options);
            }
        }

        foreach (var field in Fields.Values)
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

//public abstract class JsonLoadoutManagerIDGenericConverter<T> : JsonConverter<T>
//{
//    static Dictionary<string, PropertyInfo> Properties { get; } = new();
//    static Dictionary<string, FieldInfo> Fields { get; } = new();
//    static Dictionary<string, int> NameToCategoryID { get; } = new() { 
//        { "Reciever", 19 }, //19,20
//        { "Barrel", 10 },
//        { "Scope", 14 },
//        { "Grip", 11 },
//        { "Stock", 15 },
//        { "Ammo", 9 },
//        { "Muzzle", 13 },
//        { "Magazine", 12 },
//        { "Skin", 18 },
//        { "CamoIndex", 17 },
//        { "Hanger", 4 },
//        { "BodyCamo", 16 },
//        { "UpperBody", 6 },
//        { "LowerBody", 7 },
//        { "Helmet", 5 },
//        { "Badge", 2 },
//        { "Gear_R1", 0 },
//        { "Gear_R2", 0 },
//        { "Gear_L1", 0 },
//        { "Gear_L2", 0 },
//        { "Tactical", 8 },
//        { "Avatar", 1 }
//    };
//    protected static T? Default { get; set; }

//    static JsonLoadoutManagerIDGenericConverter()
//    {
//        var type = typeof(T);
//        var props = type.GetProperties();
//        var field = type.GetFields();
//        foreach (var info in props)
//        {
//            Properties.Add(info.Name, info);
//        }
//        foreach (var info in field)
//        { 
//            Fields.Add(info.Name, info);
//        }
//        //Default = Activator.CreateInstance<T>();
//    }

//    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        T item = Activator.CreateInstance<T>();

//        while (reader.Read())
//        {
//            switch (reader.TokenType)
//            {
//                case JsonTokenType.PropertyName:
//                    var propertyName = reader.GetString();
//                    if (propertyName is not null && reader.TokenType != JsonTokenType.Null)
//                    {
//                        if (Properties.TryGetValue(propertyName, out var prop))
//                        {
//                            if (prop.CanWrite && prop.CanRead && !Attribute.IsDefined(prop, typeof(JsonIgnoreAttribute)))
//                            {
//                                switch (prop.PropertyType.Name)
//                                {
//                                    case "BLRItem":
//                                        reader.Read();
//                                        //TODO Change how gameVersion get selected
//                                        prop.SetValue(item, BLRItemList.ItemLists["v302"].GetItemByLoadoutManagerIDAndCategoryID(NameToCategoryID[propertyName],reader.GetInt32()));
//                                        break;
//                                    default:
//                                        prop.SetValue(item, JsonSerializer.Deserialize(ref reader, prop.PropertyType, options));
//                                        break;
//                                }
//                            }
//                        }

//                        if (Fields.TryGetValue(propertyName, out var field))
//                        {
//                            if (field.IsPublic && !Attribute.IsDefined(field, typeof(JsonIgnoreAttribute)))
//                            {
//                                switch (field.FieldType.Name)
//                                {
//                                    case "BLRItem":
//                                        reader.Read();
//                                        //TODO Change how gameVersion get selected
//                                        field.SetValue(item, BLRItemList.ItemLists["v302"].GetItemByLoadoutManagerIDAndCategoryID(NameToCategoryID[propertyName], reader.GetInt32()));
//                                        break;
//                                    default:
//                                        field.SetValue(item, JsonSerializer.Deserialize(ref reader, field.FieldType, options));
//                                        break;
//                                }
//                            }
//                        }
//                    }
//                    break;
//                case JsonTokenType.EndObject: return item;
//            }
//        }
//        return item;
//    }

//    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
//    {
//        if (value is null || value.Equals(Default) || writer is null) return;
//        writer.WriteStartObject();

//        foreach (var prop in Properties.Values)
//        {
//            if (prop.CanWrite && prop.CanRead && !Attribute.IsDefined(prop, typeof(JsonIgnoreAttribute)))
//            {
//                var val = prop.GetValue(value);
//                if (val is null) continue;
//                if (val.Equals(prop.GetValue(Default))) continue;
//                WriteData(writer, prop.Name, val, options);
//            }
//        }

//        foreach (var field in Fields.Values)
//        {
//            if (field.IsPublic && !Attribute.IsDefined(field, typeof(JsonIgnoreAttribute)))
//            {
//                var val = field.GetValue(value);
//                if (val is null) continue;
//                if (val.Equals(field.GetValue(Default))) continue;
//                WriteData(writer, field.Name, val, options);
//            }
//        }
//        writer.WriteEndObject();
//    }

//    private static void WriteData(Utf8JsonWriter writer, string propertyName, object value, JsonSerializerOptions options)
//    {
//        switch (value)
//        {
//            case BLRItem item:
//                writer.WriteNumber(propertyName, item.LoadoutManagerID);
//                break;
//            case IList list:
//                if (list.Count > 0)
//                {
//                    writer.WritePropertyName(propertyName);
//                    JsonSerializer.Serialize(writer, value, options);
//                }
//                break;
//            default:
//                writer.WritePropertyName(propertyName);
//                JsonSerializer.Serialize(writer, value, options);
//                break;
//        }
//    }
//}

//public abstract class JsonUnlockIDGenericConverter<T> : JsonConverter<T>
//{
//    static Dictionary<string, PropertyInfo> Properties { get; } = new();
//    static Dictionary<string, FieldInfo> Fields { get; } = new();
//    protected static T? Default { get; set; }

//    static JsonUnlockIDGenericConverter()
//    {
//        var type = typeof(T);
//        var props = type.GetProperties();
//        var field = type.GetFields();
//        foreach (var info in props)
//        {
//            Properties.Add(info.Name, info);
//        }
//        foreach (var info in field)
//        {
//            Fields.Add(info.Name, info);
//        }
//        //Default = Activator.CreateInstance<T>();
//    }

//    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
//    {
//        T item = Activator.CreateInstance<T>();

//        while (reader.Read())
//        {
//            switch (reader.TokenType)
//            {
//                case JsonTokenType.PropertyName:
//                    var propertyName = reader.GetString();
//                    if (propertyName is not null && reader.TokenType != JsonTokenType.Null)
//                    {
//                        if (Properties.TryGetValue(propertyName, out var prop))
//                        {
//                            if (prop.CanWrite && prop.CanRead && !Attribute.IsDefined(prop, typeof(JsonIgnoreAttribute)))
//                            {
//                                switch (prop.PropertyType.Name)
//                                {
//                                    case "BLRItem":
//                                        reader.Read();
//                                        //TODO Change how gameVersion gets selected
//                                        prop.SetValue(item, BLRItemList.ItemLists["v302"].GetItemByUnlockID(reader.GetInt32()));
//                                        break;
//                                    default:
//                                        prop.SetValue(item, JsonSerializer.Deserialize(ref reader, prop.PropertyType, options));
//                                        break;
//                                }
//                            }
//                        }

//                        if (Fields.TryGetValue(propertyName, out var field))
//                        {
//                            if (field.IsPublic && !Attribute.IsDefined(field, typeof(JsonIgnoreAttribute)))
//                            {
//                                switch (field.FieldType.Name)
//                                {
//                                    case "BLRItem":
//                                        reader.Read();
//                                        //TODO Change how gameVersion get selected
//                                        field.SetValue(item, BLRItemList.ItemLists["v302"].GetItemByUnlockID(reader.GetInt32()));
//                                        break;
//                                    default:
//                                        field.SetValue(item, JsonSerializer.Deserialize(ref reader, field.FieldType, options));
//                                        break;
//                                }
//                            }
//                        }
//                    }
//                    break;
//                case JsonTokenType.EndObject: return item;
//            }
//        }
//        return item;
//    }

//    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
//    {
//        if (value is null || value.Equals(Default) || writer is null) return;
//        writer.WriteStartObject();

//        foreach (var prop in Properties.Values)
//        {
//            if (prop.CanWrite && prop.CanRead && !Attribute.IsDefined(prop, typeof(JsonIgnoreAttribute)))
//            {
//                var val = prop.GetValue(value);
//                if (val is null) continue;
//                if (val.Equals(prop.GetValue(Default))) continue;
//                WriteData(writer, prop.Name, val, options);
//            }
//        }

//        foreach (var field in Fields.Values)
//        {
//            if (field.IsPublic && !Attribute.IsDefined(field, typeof(JsonIgnoreAttribute)))
//            {
//                var val = field.GetValue(value);
//                if (val is null) continue;
//                if (val.Equals(field.GetValue(Default))) continue;
//                WriteData(writer, field.Name, val, options);
//            }
//        }
//        writer.WriteEndObject();
//    }

//    private static void WriteData(Utf8JsonWriter writer, string propertyName, object value, JsonSerializerOptions options)
//    {
//        switch (value)
//        {
//            case BLRItem item:
//                writer.WriteNumber(propertyName, item.UnlockID);
//                break;
//            case IList list:
//                if (list.Count > 0)
//                {
//                    writer.WritePropertyName(propertyName);
//                    JsonSerializer.Serialize(writer, value, options);
//                }
//                break;
//            default:
//                writer.WritePropertyName(propertyName);
//                JsonSerializer.Serialize(writer, value, options);
//                break;
//        }
//    }
//}