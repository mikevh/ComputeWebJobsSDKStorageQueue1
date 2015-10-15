using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace SecondJob
{
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    class Program
    {
        // Please set the following connection strings in app.config for this WebJob to run:
        // AzureWebJobsDashboard and AzureWebJobsStorage
        static void Main()
        {
            Console.WriteLine("job #2! - " + ApplicationVersionHelper.BuildString);
            var host = new JobHost();
            // The following code ensures that the WebJob will be running continuously
            //host.RunAndBlock();
        }
    }
    public static class ApplicationVersionHelper
    {
        private static string _machineNameHash;
        public static string MachineNameHash
        {
            get
            {
                if (null == _machineNameHash)
                {
                    _machineNameHash = Environment.MachineName;
                }
                return _machineNameHash;
            }
        }

        private static string _buildString;
        public static string BuildString
        {
            get
            {
                if (null == _buildString)
                {
                    try
                    {
                        _buildString = String.Format("{0} {1}", CurrentFileVersion, RetrieveLinkerTimestamp().ToString());
                    }
                    catch (Exception e)
                    {
                        _buildString = e.Message;
                    }
                }
                return _buildString;
            }
        }

        private static Version CurrentFileVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        // http://stackoverflow.com/questions/1600962/displaying-the-build-date
        private static DateTime RetrieveLinkerTimestamp()
        {
            string filePath = Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            Stream s = null;
            try
            {
                s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }

            int i = BitConverter.ToInt32(b, c_PeHeaderOffset);
            int secondsSince1970 = BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
            DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
            dt = dt.AddSeconds(secondsSince1970);
            dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
            return dt;
        }
    }
}
