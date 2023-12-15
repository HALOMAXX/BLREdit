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
        public Dictionary<string, object> Modules { get; set; } = new();
        public BLReviveServerConfig? Server { get; set; } = new();
    }

    public class BLReviveConsoleConfig
    {
        public List<string> CmdBlacklist { get; set; } = new();
        public List<string> CmdWhitelist { get; set; } = new();
        public bool Enable { get; set; } = false;
    }

    public class BLReviveLoggerConfig
    {
        public string FilePath { get; set; } = "blrevive-{server}{timestamp}.log";
        public string Level { get; set; } = "trace";
        public string Target { get; set; } = "file";
    }

    public class BLReviveServerConfig
    {
        public bool AuthenticateUsers { get; set; } = true;
        public bool Enable { get; set; } = true;
    }
}
