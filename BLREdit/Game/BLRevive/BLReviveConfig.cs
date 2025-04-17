using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLREdit.Game.BLRevive
{
    public class BLReviveConfig
    {
        public BLReviveConsoleConfig? Console { get; set; } = new();
        public BLReviveLoggerConfig? Logger { get; set; } = new();
        public Dictionary<string, object> Modules { get; set; } = [];
        public BLReviveServerConfig? Server { get; set; } = new();
    }

    public class BLReviveConsoleConfig
    {
        public List<string> CmdBlacklist { get; set; } = [];
        public List<string> CmdWhitelist { get; set; } = [];
        public bool Enable { get; set; }
    }

    public class BLReviveLoggerConfig
    {
        public string FilePath { get; set; } = "blrevive-{server}{timestamp}.log";
        public string Level { get; set; } = "info";
        public string Target { get; set; } = "file";
    }

    public class BLReviveServerConfig
    {
        public bool AuthenticateUsers { get; set; }
        public bool Enable { get; set; } = true;
    }
}
