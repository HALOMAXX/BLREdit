﻿using BLREdit.API.REST_API.GitHub;
using BLREdit.Game.Proxy;

using System;
using System.IO;
using System.IO.Packaging;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace BLREdit.API.REST_API.Gitlab;

public static class GitlabClient
{
    public static readonly RESTAPIClient Client = new(RepositoryProvider.Gitlab, "https://gitlab.com/api/v4/");

    public static async Task<GitlabRelease?> GetLatestRelease(string owner, string repo)
    {
        return await Client.GetLatestRelease<GitlabRelease>(owner, repo);
    }

    public static async Task<GitlabRelease[]?> GetReleases(string owner, string repo, int amount = 10, int page = 1)
    {
        return await Client.GetReleases<GitlabRelease>(owner, repo, amount, page);
    }

    public static async Task<GitlabFile?> GetFile(string owner, string repo, string branch, string file)
    {
        return await Client.GetFile<GitlabFile>(owner, repo, branch, file);
    }

    public static (bool, string) DownloadFileFromRelease(GitlabRelease? release, string destFile, string file, string fileExt = ".dll")
    {
        if (release is null || release.Assets is null) return (false, string.Empty);
        string downloadLink = string.Empty;
        foreach (var asset in release.Assets.Links)
        {
            if (asset.Name is not null && asset.Name.StartsWith(file) && asset.Name.EndsWith(fileExt))
            {
                downloadLink = asset.URL ?? string.Empty;
                break;
            }
        }

        if (string.IsNullOrEmpty(downloadLink))
        {
            LoggingSystem.Log($"No file found in Asset links gonna go down the deep end!");
            Regex regex = new($@"(\/uploads\/\w+\/{file}{fileExt})");
            if (regex.Match(release.Description) is Match match)
            {
                LoggingSystem.Log($"Found {match.Captures.Count} matches");
                if (match.Captures.Count > 0)
                {
                    foreach (var capture in match.Captures)
                    {
                        LoggingSystem.Log($"\t{capture}");
                        if (string.IsNullOrEmpty(downloadLink))
                        {
                            downloadLink = $"https://gitlab.com/{release.Owner}/{release.Repository}{capture}";
                        }
                    }
                }
            }
        }

        Directory.CreateDirectory(RESTAPIClient.DOWNLOAD);
        string downloadTarget = $"{RESTAPIClient.DOWNLOAD}{destFile}";
        if (File.Exists(downloadTarget)) File.Delete(downloadTarget);

        return (WebResources.DownloadFile(downloadLink, downloadTarget), downloadTarget);
    }

    public static async Task<GitlabPackage[]?> GetGenericPackages(string owner, string repository, string packageName)
    {
        string api = $"projects/{owner.Replace("/", "%2F")}%2F{repository.Replace("/", "%2F")}/packages?package_name={packageName}&order_by=version&sort=desc";

        var result = await Client.TryGetAPI<GitlabPackage[]>(api);
        if (result.Item1)
        {
            if (result.Item2 is GitlabPackage[] glPack)
            {
                foreach (var pack in glPack)
                {
                    pack.Owner = owner;
                    pack.Repository = repository;
                }
            }
            return result.Item2;
        }
        return default;
    }

    public static async Task<GitlabPackageFile[]?> GetPackageFiles(GitlabPackage package)
    {
        string api = $"projects/{package.Owner.Replace("/", "%2F")}%2F{package.Repository.Replace("/", "%2F")}/packages/{package.ID}/package_files";
        var result = await Client.TryGetAPI<GitlabPackageFile[]>(api);
        if (result.Item1)
        {
            return result.Item2;
        }
        return default;
    }

    public static async Task<GitlabPackageFile?> GetLatestPackageFile(GitlabPackage package, string fileName)
    {
        var result = await GetPackageFiles(package);
        if (result is null) return default;
        GitlabPackageFile? f = default;
        foreach (var file in result)
        {
            if (file.FileName == fileName)
            {
                if (f is null || f.CreatedAt < file.CreatedAt)
                {
                    f = file;
                }
            }
        }
        return f;
    }

    public static (bool, string, DateTime) DownloadPackage(GitlabPackage package, string destFile, string file, string fileExt = ".dll")
    {
        string api = $"projects/{package.Owner.Replace("/", "%2F")}%2F{package.Repository.Replace("/", "%2F")}/packages/generic/{package.Name}/{package.Version}/{file}{fileExt}";
        var task = Task.Run(() => Client.TryGetBytes(api));
        task.Wait();
        var result = task.Result;

        if (result.Item1)
        {
            Directory.CreateDirectory(RESTAPIClient.DOWNLOAD);
            string downloadTarget = $"{RESTAPIClient.DOWNLOAD}{destFile}";
            if (File.Exists(downloadTarget)) File.Delete(downloadTarget);
            File.WriteAllBytes(downloadTarget, result.Item2.Item1);
            return (true, downloadTarget, result.Item2.Item2);
        }
        return (false, "", DateTime.MinValue);
    }



}
