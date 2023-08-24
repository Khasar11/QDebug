using MongoDB.Driver.Linq;
using QDebug.Server.Connections;
using QDebug.Server.Connections.DB;
using QDebug.Shared.Configuration;
using QDebug.Shared.Logger;
using S7.Net;
using System.Xml;
using System.Xml.XPath;

namespace QDebug.Server.Configuration
{
    public partial class ServerConfiguration : ConfigurationFile
    {
        private Logger logger;

        public ServerConfiguration(string path, Logger logger) : base(path)
        {
            this.logger = logger;
        }

        const string VENDOR = "vendor";
        const string IP = "ip";
        const string CPUTYPE = "cpuType";
        const string SLOT = "slot";
        const string RACK = "rack";
        const string TYPE = "type";
        const string PORT = "port";
        const string DATABASE = "database";
        const string USER = "user";
        const string PASS = "pass";
        const string COMMENT = "#comment";

        public List<OPCUAConnection> DeserializeOPCConnections()
        {
            int index = 0;
            List<OPCUAConnection> connections = new List<OPCUAConnection>();
            XmlNodeList? nodes = base.Document.SelectNodes("/configuration/opcua/opcConnection");
            return connections;
        }
        public List<DBConnection> DeserializeDBConnections()
        {
            logger.Debug("Attempting to deserialize DB connections");
            int index = 0;
            List<DBConnection> connections = new List<DBConnection>();
            XmlNodeList? nodes = base.Document.SelectNodes("/configuration/dbs/db");
            try
            {
                if (nodes is null) throw new Exception("Xml nodes while deserializing PLC connections is empty");
                foreach (XmlNode node in nodes)
                {
                    Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                    foreach (XmlNode node2 in node.ChildNodes)
                        if (node2.Name != COMMENT && node2.InnerText != null) keyValuePairs.Add(node2.Name, node2.InnerText);
                    string[] keys = keyValuePairs.Keys.ToArray();
                    string[] required = { TYPE, IP, PORT, DATABASE };
                    if (keys != null && !keys.ContainsAll(required))
                        throw new WrongfulConfigException(required);
                    DBConnection? connection = null;
                    switch (keyValuePairs[TYPE].ToLower())
                    {
                        case "mongodb":
                            connection = new DBConnection(new MongoDBConnection(keyValuePairs[DATABASE], keyValuePairs[IP], Int16.Parse(keyValuePairs[PORT]), logger));
                            break;
                        default: 
                            throw new Exception("Wrongful DB type input");
                    }
                    if (keys != null && connection is not null) connections.Add(connection);
                    index++;
                }
            }
            catch (FormatException exception)
            {
                DeserializeError(exception.Message, index, "DB");
            }
            catch (OverflowException exception)
            {
                DeserializeError(exception.Message, index, "DB");
            }
            catch (ArgumentException exception)
            {
                DeserializeError(exception.Message, index, "DB");
            }
            catch (XPathException exception)
            {
                DeserializeError(exception.Message, index, "DB");
            } catch (WrongfulConfigException exception)
            {
                DeserializeError(exception.Message, index, "DB");
            }
            catch (Exception exception)
            {
                DeserializeError(exception.Message, index, "DB");
            }
            return connections;
        }
        public List<PLCConnection> DeserializePLCConnections()
        {
            logger.Debug("Attempting to deserialize PLC connections");
            int index = 0;
            List<PLCConnection> connections = new List<PLCConnection>();
            XmlNodeList? nodes = base.Document.SelectNodes("/configuration/plcs/plcConnection");
            try
            {
                if (nodes is null) throw new Exception("Xml nodes while deserializing PLC connections is empty");
                foreach (XmlNode node in nodes)
                {
                    Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();
                    foreach (XmlNode node2 in node.ChildNodes)
                        if (node2.Name != COMMENT && node2.InnerText != null) keyValuePairs.Add(node2.Name, node2.InnerText);
                    string[] keys = keyValuePairs.Keys.ToArray();
                    string[] required = { VENDOR, IP, CPUTYPE, SLOT, RACK };
                    if (keys != null && !keys.ContainsAll(required))
                        throw new WrongfulConfigException(required);
                    CpuType CpuType;
                    S7.Net.CpuType.TryParse<CpuType>(keyValuePairs[CPUTYPE], true, out CpuType);
                    PLCConnection connection = new(
                        keyValuePairs[VENDOR],
                        CpuType,
                        keyValuePairs[IP],
                        Convert.ToInt16(keyValuePairs[RACK]),
                        Convert.ToInt16(keyValuePairs[SLOT]),
                        logger
                    );
                    if (keys != null) connections.Add(connection);
                    index++;
                }
            }
            catch (FormatException exception)
            {
                DeserializeError(exception.Message, index, "PLC");
            }
            catch (OverflowException exception)
            {
                DeserializeError(exception.Message, index, "PLC");
            }
            catch (ArgumentException exception)
            {
                DeserializeError(exception.Message, index, "PLC");
            }
            catch (XPathException exception)
            {
                DeserializeError(exception.Message, index, "PLC");
            }
            catch (WrongfulConfigException exception)
            {
                DeserializeError(exception.Message, index, "PLC");
            }
            catch (Exception exception)
            {
                DeserializeError(exception.Message, index, "PLC");
            }
            return connections;
        }

        private void DeserializeError(string ExceptionMessage, int index, string type)
        {
            logger.Error($"[DESERIALIZE {type}] An error occured while deserializing index {index}, please check your configuration");
            logger.Error(ExceptionMessage);
        }
    }
}
