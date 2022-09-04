using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.Game
{
    public class ProxyModule
    {
        public string AuthorName { get; set; } = "SuperEwald";
        public string ModuleName { get; set; } = "LoadoutManager";
        public string Version { get; set; } = string.Empty;
    }

    public class RepositoryProxyModule
    {
        public RepositoryProvider RepositoryProvider { get; set; } = RepositoryProvider.Gitlab;
        public string Owner { get; set; } = "blrevive";
        public string Repository { get; set; } = "modules/loadout-manager";
        public string ModuleName { get; set; } = "LoadoutManager";
    }

    public enum RepositoryProvider
    { 
        GitHub,
        Gitlab
    }
}
