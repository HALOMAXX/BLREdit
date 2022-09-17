using System;
using System.Text.Json.Serialization;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabFile
{
    public string file_name { get; set; }
    public string file_path { get; set; }
    public int size { get; set; }
    public string encoding { get; set; }
    public string content { get; set; }
    public string content_sha256 { get; set; }
    public string _ref { get; set; }
    public string blob_id { get; set; }
    public string commit_id { get; set; }
    public string last_commit_id { get; set; }
    public bool execute_filemode { get; set; }

    private string _decoded_content;
    [JsonIgnore]
    public string decoded_content
    {
        get
        {
            _decoded_content ??= System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(content));
            return _decoded_content;
        }
    }
}
