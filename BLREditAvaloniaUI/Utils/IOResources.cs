using System.Text.Json;
using System;
using System.Text.Json.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace BLREdit;

public sealed partial class IOResources
{
    static IOResources()
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

        JsonHttpClient = new HttpClient() { Timeout = new TimeSpan(0, 0, 10) };
        TextHttpClient = new HttpClient() { Timeout = new TimeSpan(0, 0, 10) };

        JsonHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        TextHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));
        
        if (!JsonHttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd($"BLREdit-{App.CurrentVersion}")) { Debug.WriteLine($"Failed to add {HttpRequestHeader.UserAgent} to JsonHttpClient"); };
        if (!TextHttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd($"BLREdit-{App.CurrentVersion}")) { Debug.WriteLine($"Failed to add {HttpRequestHeader.UserAgent} to TextHttpClient"); };
    }

    #region Network
    public static HttpClient JsonHttpClient { get; } 
    public static HttpClient TextHttpClient { get; } 
    #endregion Network
    #region Serialization
    public static JsonSerializerOptions JSODeserialization { get; } = new JsonSerializerOptions() { IncludeFields = true, Converters = { new JsonStringEnumConverter() } };
    public static JsonSerializerOptions JSOSerialization { get; } = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull, WriteIndented = true, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };
    public static JsonSerializerOptions JSOSerializationCompact { get; } = new JsonSerializerOptions() { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault | JsonIgnoreCondition.WhenWritingNull, WriteIndented = false, IncludeFields = true, Converters = { new JsonStringEnumConverter() } };
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
            return RemoveWhiteSpacesFromJson().Replace(JsonSerializer.Serialize(obj, JSOSerializationCompact), "$1");
        }
        else
        {
            return JsonSerializer.Serialize(obj, JSOSerialization);
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
        try { return JsonSerializer.Deserialize<T>(json, JSODeserialization); } 
        catch (Exception error) 
        { Debug.WriteLine(error); }
        return default;
    }
    #endregion Serialization
    #region Hashing
    public static string GetFileHash(string path)
    {
        return GetHash(File.ReadAllBytes(path));
    }
    public static string GetHash(byte[] data)
    {
        return BitConverter.ToString(SHA256.HashData(data)).Replace("-", string.Empty).ToLower();
    }

    [GeneratedRegex("(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+")]
    private static partial Regex RemoveWhiteSpacesFromJson();
    #endregion Hashing
}
