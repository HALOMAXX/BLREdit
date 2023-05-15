using System.Text.Json;
using System;
using System.Text.Json.Serialization;
using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using BLREdit.Models;
using System.Collections.ObjectModel;

namespace BLREdit;

public sealed class IOResources
{
    public static JsonSerializerOptions JSOFields { get; } = new JsonSerializerOptions() { WriteIndented = true, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };
    public static JsonSerializerOptions JSOCompacted { get; } = new JsonSerializerOptions() { WriteIndented = false, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };

    public static void SerializeFile<T>(string filePath, T obj, bool compact = false)
    {
        var info = new FileInfo(filePath);
        if (info.Directory is null) { Directory.CreateDirectory(info.FullName.AsSpan(0, info.FullName.Length - info.Name.Length).ToString()); }
        else if (!info.Directory.Exists) { info.Directory?.Create(); };
        if (info.Exists) { info.Delete(); }
        using var file = info.CreateText();
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

    public static List<T> DeserializeDirectory<T>(DirectoryInfo dirInfo)
    {
        List<T> all = new();
        if (dirInfo.Exists)
        {
            var files = dirInfo.GetFiles("*.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                try
                {
                    using var fileStream = file.OpenText();
                    var data = fileStream.ReadToEnd();
                    fileStream.Close();
                    if (Deserialize<T[]>(data) is T[] gameModes)
                    {
                        all.AddRange(gameModes);
                    }
                    else if (Deserialize<T>(data) is T gameMode)
                    {
                        all.Add(gameMode);
                    }
                }
                catch { }
            }
        }
        return all;
    }

    public static T? DeserializeFile<T>(string filePath)
    {
        var info = new FileInfo(filePath);
        if (info.Exists)
        {
            using var file = info.OpenText();
            return Deserialize<T>(file.ReadToEnd());
        }
        return default;
    }

    public static T? Deserialize<T>(string json)
    {
        try { return JsonSerializer.Deserialize<T>(json, JSOFields); } catch { }
        return default;
    }

    public static string CreateFileHash(string path)
    {
        using var stream = File.OpenRead(path);
        using var crypto = System.Security.Cryptography.SHA256.Create();
        return BitConverter.ToString(crypto.ComputeHash(stream)).Replace("-", string.Empty).ToLower();
    }
}
