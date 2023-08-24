using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QDebug.Shared.Logger
{
    public class FileLogger : ILogger
    {
        private readonly string _filePath;

        public FileLogger(string filePath)
        {
            _filePath = filePath;
        }

        public void Log(string message, string level)
        {
            var logMessage = $"[{DateTime.Now}] [{level}] {message}";

            using (var writer = File.AppendText(_filePath))
            {
                writer.WriteLine(logMessage);
            }
        }
    }
}
