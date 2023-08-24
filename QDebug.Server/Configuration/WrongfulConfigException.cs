using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDebug.Server.Configuration
{
    [Serializable]
    public class WrongfulConfigException : Exception
    {
        public WrongfulConfigException() : base() { }
        public WrongfulConfigException(string[] missingArguments) : base($"Wrongful input in config, missing one or more arguments: {missingArguments}")
        {
        }
        public WrongfulConfigException(string message, Exception inner) : base(message, inner) { }

    }
}
