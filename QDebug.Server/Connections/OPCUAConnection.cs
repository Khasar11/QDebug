
using OPCUaClient;
using QDebug.Shared.Logger;
using QDebug.Shared.Other;

namespace QDebug.Server.Connections
{
    public class OPCUAConnection
    {

        public string Url { get; private set; }

        private string UserName { get; set; }

        private string Password { get; set; }
        private bool UserAuth { get; set; }

        private Logger Logger;

        public UaClient? Client { get; set; }


        public OPCUAConnection(string url, bool userAuth, string userName, string password, Logger logger)
        {
            Url = url;
            UserAuth = userAuth;
            UserName = userName;
            Password = password;
            Logger = logger;
        }

        public async Task ConnectAsync()
        {
            Logger.Info($"OPCUA {Url} AS {UserAuth} feedback: Connecting...");
            try
            {
                Client = new(Utils.GetLocalIPAddress(), Url, false, true, UserName, Password);
                await Client.ConnectAsync(5, true);
                if (Client.IsConnected) Logger.Info($"OPCUA {Url} AS {UserAuth} feedback: Connected");
                else throw new Exception($"Failure even after connecting async");
            } catch (Exception exception)
            {
                Logger.Error($"OPCUA {Url} AS {UserAuth} feedback: Connection failure {exception.Message}");
            }
        }
    }
}
