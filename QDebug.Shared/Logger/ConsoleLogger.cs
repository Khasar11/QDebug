using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDebug.Shared.Logger
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message, string level)
        {
            ConsoleColor consoleColor;
            switch (level.ToLower())
            {
                case "info": consoleColor = ConsoleColor.Yellow; break;
                case "debug": consoleColor = ConsoleColor.Green; break;
                case "warning": consoleColor = ConsoleColor.DarkYellow; break;
                case "error": consoleColor = ConsoleColor.Red; break;
                default: consoleColor = ConsoleColor.White; break;
            }
            var logMessage = $"[{DateTime.Now}] [{level}] {message}";
            Console.ForegroundColor = consoleColor;
            Console.WriteLine(logMessage);
            Console.ResetColor();
        }
    }
}
