﻿using System.Text.Json;
using System;
using System.Text.Json.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Net.Http;
using System.Net;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Security;
using System.Globalization;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using FluentResults;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;
using BLREdit.Core.Models.BLR.Client;

namespace BLREdit.Core.Utils;

public sealed partial class IOResources
{

    static IOResources()
    {
        //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls | SecurityProtocolType.SystemDefault;

        JsonHttpClient = new HttpClient() { Timeout = new TimeSpan(0, 0, 10) };
        TextHttpClient = new HttpClient() { Timeout = new TimeSpan(0, 0, 10) };

        JsonHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        TextHttpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/html"));
        
        //if (!JsonHttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd($"BLREdit-{App.CurrentVersion}")) { Debug.WriteLine($"Failed to add {HttpRequestHeader.UserAgent} to JsonHttpClient"); };
        //if (!TextHttpClient.DefaultRequestHeaders.UserAgent.TryParseAdd($"BLREdit-{App.CurrentVersion}")) { Debug.WriteLine($"Failed to add {HttpRequestHeader.UserAgent} to TextHttpClient"); };
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
        else if (!info.Directory.Exists) { info.Directory.Create(); };
        if (info.Exists) { info.Delete(); }
        using var file = info.CreateText();
        file.Write(Serialize(obj, compact));
        file.Close();
    }

    public static void DeserializeDirectoryInto<T>(RangeObservableCollection<T> collection, DirectoryInfo info)
    {
        var result = DeserializeDirectory<T>(info);
        if (result is not null && result.Count > 0 && collection is not null)
        { collection.AddRange(result); }
        else
        { Debug.WriteLine($"failed to deserialize {nameof(T)}:{info.Name}"); }
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
        if (!(dirInfo?.Exists ?? false)) return new();
        List<T> all = new();
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
            catch (SecurityException error) { Debug.WriteLine($"{error.Message}\n{error.StackTrace}"); }
            catch (FileNotFoundException error) { Debug.WriteLine($"{error.Message}\n{error.StackTrace}"); }
            catch (UnauthorizedAccessException error) { Debug.WriteLine($"{error.Message}\n{error.StackTrace}"); }
            catch (DirectoryNotFoundException error) { Debug.WriteLine($"{error.Message}\n{error.StackTrace}"); }
            catch (OutOfMemoryException error) { Debug.WriteLine($"{error.Message}\n{error.StackTrace}"); }
            catch (IOException error) { Debug.WriteLine($"{error.Message}\n{error.StackTrace}"); }
            catch (ArgumentNullException error) { Debug.WriteLine($"{error.Message}\n{error.StackTrace}"); }
        }
        return all;
    }

    public static T? DeserializeFile<T>(string filePath)
    {
            var info = new FileInfo(filePath);
            if (!info.Exists) { return default; }
            using var file = info.OpenText();
            return Deserialize<T>(file.ReadToEnd());
    }

    public static T? Deserialize<T>(string json)
    {
        try { return JsonSerializer.Deserialize<T>(json, JSODeserialization); } 
        catch (ArgumentNullException error) { Debug.WriteLine($"{error.Message}\n{error.StackTrace}"); }
        catch (JsonException error) { Debug.WriteLine($"{error.Message}\n{error.StackTrace}"); }
        catch (NotSupportedException error) { Debug.WriteLine($"{error.Message}\n{error.StackTrace}"); }
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
        #pragma warning disable CA1308
        return BitConverter.ToString(SHA256.HashData(data)).Replace("-", string.Empty, StringComparison.InvariantCulture).ToLowerInvariant();
        #pragma warning restore CA1308
    }

    [GeneratedRegex("(\"(?:[^\"\\\\]|\\\\.)*\")|\\s+")]
    private static partial Regex RemoveWhiteSpacesFromJson();
    #endregion Hashing
}
