using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabFile
{
    [JsonPropertyName("file_name")]
    public string FileName { get; set; }
    [JsonPropertyName("file_path")]
    public string FilePath { get; set; }
    [JsonPropertyName("size")]
    public int Size { get; set; }
    [JsonPropertyName("encoding")]
    public string Encoding { get; set; }
    [JsonPropertyName("content")]
    public string Content { get; set; }
    [JsonPropertyName("content_sha256")]
    public string ContentSHA256 { get; set; }
    [JsonPropertyName("_ref")]
    public string Ref { get; set; }
    [JsonPropertyName("blob_id")]
    public string BlobID { get; set; }
    [JsonPropertyName("commit_id")]
    public string CommitID { get; set; }
    [JsonPropertyName("last_commit_id")]
    public string LastCommitID { get; set; }
    [JsonPropertyName("execute_filemode")]
    public bool ExecuteFilemode { get; set; }

    private string? decodedContent;
    [JsonIgnore]
    public string DecodedContent
    {
        get
        {
            decodedContent ??= System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(Content));
            return decodedContent;
        }
    }

    [JsonConstructor]
    public GitlabFile(string fileName, string filePath, int size, string encoding, string content, string contentSHA256, string @ref, string blobID, string commitID, string lastCommitID, bool executeFilemode)
    {
        FileName = fileName;
        FilePath = filePath;
        Size = size;
        Encoding = encoding;
        Content = content;
        ContentSHA256 = contentSHA256;
        Ref = @ref;
        BlobID = blobID;
        CommitID = commitID;
        LastCommitID = lastCommitID;
        ExecuteFilemode = executeFilemode;
    }
}
