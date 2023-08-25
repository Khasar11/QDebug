
namespace QDebug.Server.Commands
{
    internal class MongoCommand
    {
        private Startup _application;
        public MongoCommand(Startup application, string[] args)
        {
            _application = application;
            args = args.Skip(1).ToArray(); // remove first entry 
        }
    }
}
