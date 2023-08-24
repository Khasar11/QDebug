
using System.Xml;

namespace QDebug.Shared.Configuration
{
    public class ConfigurationFile
    {
        private string Path { get; set; }

        public XmlDocument Document = new XmlDocument();

        public ConfigurationFile(string path)
        {
            this.Path = path;
            Document.Load(@$"{path}");
        }

        public string? Read(string path)
        {
            return Document.SelectSingleNode(path).Value;
        }

        public void Reload()
        {
            Document.Load(@$"{Path}");
        }
    }
}
