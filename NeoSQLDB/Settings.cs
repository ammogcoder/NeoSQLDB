using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace NeoSQLDB
{
    internal class Settings
    {
        public DatabaseSettings DBMainNet { get; }
        public DatabaseSettings DBTestNet { get; }
        public NodeSettings NodesMainNet { get; }
        public NodeSettings NodesTestNet { get; }
        public NodeSettings NodesNEPMainNet { get; }
        public NodeSettings NodesNEPTestNet { get; }
        public byte AddressVersion { get; }

        public static Settings Default { get; }

        static Settings()
        {
            IConfigurationSection section = new ConfigurationBuilder().AddJsonFile("config.json").Build().GetSection("ApplicationConfiguration");
            Default = new Settings(section);
        }

        public Settings(IConfigurationSection section)
        {
            DBMainNet = new DatabaseSettings(section.GetSection("Databases:MainNet"));
            DBTestNet = new DatabaseSettings(section.GetSection("Databases:TestNet"));
            NodesMainNet = new NodeSettings(section.GetSection("Nodes:MainNet"));
            NodesTestNet = new NodeSettings(section.GetSection("Nodes:TestNet"));
            NodesNEPMainNet = new NodeSettings(section.GetSection("Nodes:NEPMainNet"));
            NodesNEPTestNet = new NodeSettings(section.GetSection("Nodes:NEPTestNet"));
            AddressVersion = byte.Parse(section.GetSection("AddressVersion").Value);
        }
    }
    internal class DatabaseSettings
    {
        public string Name { get; }
        public string Connection { get; }
        public bool Active { get; }
        public bool NepSyncActive { get; }
        public bool Debug { get; }

        public DatabaseSettings(IConfigurationSection section)
        {
            Name = section.GetSection("Name").Value;
            Connection = section.GetSection("Connection").Value;
            Active = bool.Parse(section.GetSection("Active").Value);
            NepSyncActive = bool.Parse(section.GetSection("NepSyncActive").Value);
            Debug = bool.Parse(section.GetSection("Debug").Value);
        }
    }
    internal class NodeSettings
    {
        public List<string> Nodes { get; }

        public NodeSettings(IConfigurationSection section)
        {
            Nodes = new List<string>();
            foreach (var s in section.GetChildren())
            {
                Nodes.Add(s.Value);
            }
        }
    }
}
