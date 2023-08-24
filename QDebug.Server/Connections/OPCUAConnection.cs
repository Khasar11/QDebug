using MongoDB.Driver.Core.Configuration;
using Opc.Ua;
using Siemens.UAClientHelper;

namespace QDebug.Server.Connections
{
    public class OPCUAConnection
    {

        public string Url { get; private set; }

        private string UserName { get; set; }

        private string Password { get; set; }
        private bool UserAuth { get; set; }

        private readonly UAClientHelperAPI UAClientHelperAPI;

        public OPCUAConnection(string Url, bool UserAuth, string UserName, string Password)
        {
            this.Url = Url;
            this.UserAuth = UserAuth;
            this.UserName = UserName;
            this.Password = Password;
            this.UAClientHelperAPI = new UAClientHelperAPI();
        }

        public async Task ConnectAsync()
        {
            await UAClientHelperAPI.Connect(new EndpointDescription(Url), UserAuth, UserName, Password);
            if (UAClientHelperAPI.Session.Connected)
            {
                Console.WriteLine($"OPCUA {UserName} AT {Url} feedback: Connection OK");
            } else Console.WriteLine($"OPCUA {UserName} AT {Url} feedback: Connection Failure");
        }
    }
}
