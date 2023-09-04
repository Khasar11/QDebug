
using MongoDB.Driver.Core.Configuration;
using QDebug.Shared.Logger;
using QDebug.Shared.Other;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Workstation.ServiceModel.Ua;
using Workstation.ServiceModel.Ua.Channels;
using Utils = QDebug.Shared.Other.Utils;

namespace QDebug.Server.Connections
{
    public class OPCUAConnection
    {

        public string Url { get; private set; }

        private string UserName { get; set; }

        private string Password { get; set; }
        private bool UserAuth { get; set; }

        private Logger Logger;

        public ClientSessionChannel? Client { get; set; }

        public bool isConnected { get; set; } = false;

        public uint Handle { get; set; }

        // ip and port deconstructed from Url
        public readonly string Ip;
        public readonly int Port;


        public OPCUAConnection(string url, bool userAuth, string userName, string password, uint handle, Logger logger)
        {
            Url = url;
            if (!Url.EndsWith("/")) Url = Url + "/"; // incase / is missing at end (only needed for getting readonly port)
            UserAuth = userAuth;
            UserName = userName;
            Password = password;
            Logger = logger;
            Handle = handle;

            Regex reg = new Regex("(?<=//)(.*)(?=:)"); // get everything between // and :
            Ip = reg.Match(Url).Value;
            var postIp = Url.Substring(Url.NthIndexOf(':', 2) + 1);
            Port = int.Parse(postIp.Substring(0, postIp.IndexOf('/')));
            InitClient();
        }

        private void InitClient()
        {
            // describe this client application.
            var clientDescription = new ApplicationDescription
            {
                ApplicationName = "Workstation.UaClient.FeatureTests",
                ApplicationUri = $"urn:{System.Net.Dns.GetHostName()}:Workstation.UaClient.FeatureTests",
                ApplicationType = ApplicationType.Client
            };

            // create a 'ClientSessionChannel', a client-side channel that opens a 'session' with the server.
            Client = new ClientSessionChannel(
                clientDescription,
                null, // no x509 certificates
                new AnonymousIdentity(), // no user identity, handle later
                Url, // the public endpoint of a server at opcua.rocks.
                SecurityPolicyUris.None); // no encryption

            Client.Opening += c_Opening;
            Client.Closing += c_Closing;
            Client.Opened += c_Opened;
            Client.Closed += c_Closed;
            Client.Faulted += c_Faulted;
        }

        private void c_Opening(object sender, EventArgs e)
        {
            Logger.Info($"OPCUA {Url} AS {UserAuth} feedback: Connecting...");
        }

        private void c_Closing(object sender, EventArgs e)
        {
            Logger.Info($"OPCUA {Url} AS {UserAuth} feedback: Disconnecting...");
        }

        private void c_Opened(object sender, EventArgs e)
        {
            isConnected = true;
            Logger.Info($"OPCUA {Url} AS {UserAuth} feedback: Connection established");
        }

        private void c_Closed(object sender, EventArgs e)
        {
            isConnected = false;
            Logger.Info($"OPCUA {Url} AS {UserAuth} feedback: Disconnected");
        }

        private void c_Faulted(object sender, EventArgs e)
        {
            isConnected = false;
            Logger.Error($"OPCUA {Url} AS {UserAuth} feedback: Faulted");
        }

        public async Task ConnectAsync()
        {
            try
            {
                if (Client != null)
                    await Client.OpenAsync();
            }
            catch (NullReferenceException exception)
            {
                Logger.Error($"OPCUA {Url} AS {UserAuth} feedback: Connection failure {exception.Message}");
            }
            catch (Exception exception)
            {
                Logger.Error($"OPCUA {Url} AS {UserAuth} feedback: Connection failure {exception.Message}");
            } 
        }

        public static X509Certificate2 GenerateSelfSignedCertificate() // from stackoverflow
        {
            string secp256r1Oid = "1.2.840.10045.3.1.7";  //oid for prime256v1(7)  other identifier: secp256r1

            string subjectName = "Self-Signed-Cert-Example";

            var ecdsa = ECDsa.Create(ECCurve.CreateFromValue(secp256r1Oid));

            var certRequest = new CertificateRequest($"CN={subjectName}", ecdsa, HashAlgorithmName.SHA256);

            //add extensions to the request (just as an example)
            //add keyUsage
            certRequest.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, true));

            X509Certificate2 generatedCert = certRequest.CreateSelfSigned(DateTimeOffset.Now.AddDays(-1), DateTimeOffset.Now.AddYears(10)); // generate the cert and sign!

            X509Certificate2 pfxGeneratedCert = new X509Certificate2(generatedCert.Export(X509ContentType.Pfx)); //has to be turned into pfx or Windows at least throws a security credentials not found during sslStream.connectAsClient or HttpClient request...

            return pfxGeneratedCert;
        }
    }
}
