using Amazon.Runtime.Internal.Endpoints.StandardLibrary;
using QDebug.Shared.Logger;
using S7.Net;

namespace QDebug.Server.Connections
{
    public class PLCConnection
    {
        public string Vendor { get; private set; }
        public CpuType CpuType { get; private set; }
        public string IP { get; private set; }
        public short Rack { get; private set; }
        public short Slot { get; private set; }

        public Plc? Plc = null;

        private Logger logger;

        public PLCConnection(string Vendor, CpuType CpuType, string IP, short Rack, short Slot, Logger logger)
        {
            this.Vendor = Vendor;
            this.CpuType = CpuType;
            this.IP = IP;
            this.Rack = Rack;
            this.Slot = Slot;
            this.logger = logger;
        }

        const string help = $"Check your PLC's connection settings, for Siemens check your security level and connection mechanisms";

        public async Task ConnectAsync()
        {
            if (Vendor.ToLower() == "siemens")
            {
                try
                {
                    Plc = new Plc(CpuType, IP, Rack, Slot);

                    logger.Info($"PLC {CpuType} AT {IP} feedback: Connecting...");
                    await Plc.OpenAsync();
                    if (Plc.IsConnected)
                    {
                        /* insert mongodb caching here */
                        logger.Info($"PLC {CpuType} AT {IP} feedback: Connection OK");
                        Plc.Close();
                    }
                    else
                    {
                        logger.Warning($"PLC {CpuType} AT {IP} feedback: Connection Failure");
                        logger.Debug(help);
                    }
                } catch(Exception exception) { 
                    logger.Error($"PLC {CpuType} AT {IP} feedback: Connection Failure with exception {Environment.NewLine}{exception.Message}");
                    logger.Debug(help);
                }
            }
        }
    }
}
