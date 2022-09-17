using System;

namespace BLREdit.API.REST_API.Gitlab;

public sealed class GitlabEvidence
{
    public string sha { get; set; }
    public string filepath { get; set; }
    public DateTime collected_at { get; set; }
}
