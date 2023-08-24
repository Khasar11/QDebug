using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDebug.Shared.Logger
{
    public class Logger
    {
        private readonly ILogger _logger;

        public Logger(ILogger logger)
        {
            _logger = logger;
        }

        public void Debug(string message)
        {
            _logger.Log(message, "DEBUG");
        }

        public void Info(string message)
        {
            _logger.Log(message, "INFO");
        }

        public void Warning(string message)
        {
            _logger.Log(message, "WARNING");
        }

        public void Error(string message)
        {
            _logger.Log(message, "ERROR");
        }
    }
}
