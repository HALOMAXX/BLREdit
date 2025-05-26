using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace BLREdit.Game.BLRevive
{
    public sealed class BLReviveConfig
    {
        public BLReviveConsoleConfig? Console { get; set; } = new();
        public BLReviveLoggerConfig? Logger { get; set; } = new();
        public Dictionary<string, JsonObject?> Modules { get; set; } = [];
        public BLReviveServerConfig? Server { get; set; } = new();
        public BLReviveConfig Copy()
        {
            var modules = new KeyValuePair<string, JsonObject?>[this.Modules.Count];
            this.Modules.ToList().CopyTo(modules);
            var copy = new BLReviveConfig
            {
                Console = this.Console?.Copy() ?? new(),
                Logger = this.Logger?.Copy() ?? new(),
                Modules = modules.ToDictionary(x => x.Key, x => x.Value),
                Server = this.Server?.Copy() ?? new(),
            };
            return copy;
        }
    }

    public sealed class BLReviveConsoleConfig
    {
        public List<string> CmdBlacklist { get; set; } = [];
        public List<string> CmdWhitelist { get; set; } = [];
        public bool Enable { get; set; }
        public BLReviveConsoleConfig Copy()
        {
            var blacklist = new string[this.CmdBlacklist.Count];
            var whitelist = new string[this.CmdWhitelist.Count];
            this.CmdBlacklist.CopyTo(blacklist);
            this.CmdWhitelist.CopyTo(whitelist);
            var copy = new BLReviveConsoleConfig
            {
                CmdBlacklist = [.. blacklist],
                CmdWhitelist = [.. whitelist],
                Enable = this.Enable
            };
            return copy;
        }
    }

    public sealed class BLReviveLoggerConfig
    {
        public string FilePath { get; set; } = "blrevive-{server}{timestamp}.log";
        public string Level { get; set; } = "info";
        public string Target { get; set; } = "file";
        public BLReviveLoggerConfig Copy()
        {
            var copy = new BLReviveLoggerConfig
            {
                FilePath = this.FilePath,
                Level = this.Level,
                Target = this.Target
            };
            return copy;
        }
    }

    public sealed class BLReviveServerConfig
    {
        public bool AuthenticateUsers { get; set; }
        public bool Enable { get; set; } = true;
        public BLReviveServerConfig Copy() {
            var copy = new BLReviveServerConfig
            {
                AuthenticateUsers = this.AuthenticateUsers,
                Enable = this.Enable
            };
            return copy;
        }
    }
}
