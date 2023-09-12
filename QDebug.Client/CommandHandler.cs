
using QDebug.Client;

namespace QDebug.Client
{
    public class CommandHandler
    {

        private Startup _application;
        public CommandHandler(Startup application)
        {
            _application = application;
        }
        public void EvaluateString(string input, ref bool stopEvaluating)
        {
            if (input == "stop") stopEvaluating = false;
            string[] args = input.Split(" ");
            string command = args[0];
            args = args.Skip(1).ToArray(); // remove first entry 
            switch (command)
            {
                
                default:
                    _application.Logger.Debug("Nonexistant command");
                    break;
            }
        }
    }
}
