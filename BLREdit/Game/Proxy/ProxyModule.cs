using BLREdit.API.REST_API.GitHub;
using BLREdit.API.REST_API.Gitlab;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.Game.Proxy
{
    public class ProxyModule
    {
        public static void Save() { IOResources.SerializeFile("ModuleCache.json", CachedModules); }
        public static ObservableCollection<ProxyModule> CachedModules { get; set; } = IOResources.DeserializeFile<ObservableCollection<ProxyModule>>("ModuleCache.json") ?? new();


        public string AuthorName { get; set; } = "SuperEwald";
        public string ModuleName { get; set; } = "LoadoutManager";
        public DateTime Published { get; set; } = DateTime.MinValue;
        public string ReleaseID { get; set; } = string.Empty;
        public bool Client { get; set; } = true;
        public bool Server { get; set; } = true;

        public ProxyModule() { }
        public ProxyModule(GitHubRelease release, string moduleName, bool client, bool server)
        {
            AuthorName = release.author.url;
            ModuleName = moduleName;
            Published = release.published_at;
            Client = client;
            Server = server;
        }

        public ProxyModule(GitlabRelease release, string moduleName, bool client, bool server)
        {
            AuthorName = release.author.web_url;
            ModuleName = moduleName;
            Published = release.released_at;
            Client = client;
            Server = server;
        }
    }
}
