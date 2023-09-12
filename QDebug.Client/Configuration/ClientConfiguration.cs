
using QDebug.Shared.Configuration;
using System.Xml;
using System.Xml.XPath;

namespace QDebug.Client.Configuration
{
    public partial class ClientConfiguration : ConfigurationFile
    {
        private Startup _application;

        public ClientConfiguration(string path, Startup application) : base(path)
        {
            _application = application;
        }

        public string GetConfigObject(string path)
        {
            var single = Document.SelectSingleNode(path);
            if (single == null) return "";
            return single.InnerText;
        }
    }
}
