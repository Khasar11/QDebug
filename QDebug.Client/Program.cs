using QDebug.Client;

namespace QDebugClient
{
    class QDebug
    {
        static void Main(string[] args)
        {
            Startup Startup = new Startup();
            Startup.FireStartup();
        }
    }
}