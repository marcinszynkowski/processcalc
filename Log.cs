using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;

namespace ProcessCalc
{
    class Log
    {
        private StreamWriter log;
        private static string logDirectory = ConfigurationManager.AppSettings["LogDirectory"];
        private static string logFile = ConfigurationManager.AppSettings["LogFile"];

        // method for creating a directory for a log if it doesn't exist
        private static void CreateDirectoryForLog()
        {
            if (!Directory.Exists(@logDirectory))
            {
                Directory.CreateDirectory(@logDirectory);
            }
        }

        // write errors to log
        public void ToLogAfterError(string error)
        {
            Log.CreateDirectoryForLog();
            using (log = new StreamWriter(@logDirectory + logFile, true))
            {
                log.WriteLine(DateTime.Now + ": Error: " + error);
            }
        }

        // write to log info about killed processes
        public void ToLogKilledProcessInfo(string id, string ProcessName, string owner)
        {
            Log.CreateDirectoryForLog();
            using (log = new StreamWriter(@logDirectory + logFile, true))
            {
                log.WriteLine(DateTime.Now + " : " + "Killed process " + ProcessName + " with ID " + id + "and owner " + owner);
            }
        }

        // write to log messages from the app.
        public void ToLogWriteSth (string message)
        {
            Log.CreateDirectoryForLog();
            using (log = new StreamWriter(@logDirectory + logFile, true))
            {
                log.WriteLine(DateTime.Now + " : " + message);
            }
        }
    }
}
