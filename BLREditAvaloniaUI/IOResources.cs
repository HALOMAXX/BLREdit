using System.Text.Json;
using System;
using System.Text.Json.Serialization;
using System.IO;
using System.Diagnostics.CodeAnalysis;

namespace BLREdit;

public sealed class IOResources
{
    public static JsonSerializerOptions JSOFields { get; } = new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };
    public static JsonSerializerOptions JSOCompacted { get; } = new JsonSerializerOptions() { WriteIndented = false, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };

    public static void SerializeFile<T>(string filePath, T obj, bool compact = false)
    { 
        using var file = File.CreateText(filePath);
        file.Write(Serialize(obj, compact));
        file.Close();
    }

    public static string Serialize<T>(T obj, bool compact = false)
    {
        if (compact)
        {
            return JsonSerializer.Serialize<T>(obj, JSOCompacted);
        }
        else
        {
            return JsonSerializer.Serialize<T>(obj, JSOFields);
        }
    }

    public static T? DeserializeFile<T>(string filePath)
    {
        using var file = File.OpenText(filePath);
        return Deserialize<T>(file.ReadToEnd());
    }

    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, JSOFields);
    }

    public static string CreateFileHash(string path)
    {
        using var stream = File.OpenRead(path);
        using var crypto = System.Security.Cryptography.SHA256.Create();
        return BitConverter.ToString(crypto.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
    }
}
