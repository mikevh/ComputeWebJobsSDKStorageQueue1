//----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
//
// Copyright (c) Microsoft Corporation. All rights reserved.
//
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
//----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
//----------------------------------------------------------------------------------

using Microsoft.Azure.WebJobs;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ComputeWebJobsSDKStorageQueue1
{
    //******************************************************************************************************
    // This will show you how to perform common scenarios using the Microsoft Azure Queue storage service using 
    // the Microsoft Azure WebJobs SDK. The scenarios covered include triggering a function when a new message comes
    // on a queue, sending a message on a queue.   
    // 
    // In this sample, the Program class starts the JobHost and creates the demo data. The Functions class
    // contains methods that will be invoked when messages are placed on the queues, based on the attributes in 
    // the method headers.
    //
    // To learn more about Microsoft Azure WebJobs SDK, please see http://go.microsoft.com/fwlink/?LinkID=320976
    //
    // TODO: Open app.config and paste your Storage connection string into the AzureWebJobsDashboard and
    //      AzureWebJobsStorage connection string settings.
    //*****************************************************************************************************

    class Program
    {


        static void Main()
        {
            Trace.Write("Hello from job 1 - " + ApplicationVersionHelper.BuildString);

            if (!VerifyConfiguration())
            {
                Console.ReadLine();
                return;
            }

            CreateDemoData();

            JobHost host = new JobHost();
            host.RunAndBlock();
        }

        private static bool VerifyConfiguration()
        {
            string webJobsDashboard = ConfigurationManager.ConnectionStrings["AzureWebJobsDashboard"].ConnectionString;
            string webJobsStorage = ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString;

            bool configOK = true;
            if (string.IsNullOrWhiteSpace(webJobsDashboard) || string.IsNullOrWhiteSpace(webJobsStorage))
            {
                configOK = false;
                Console.WriteLine("Please add the Azure Storage account credentials in App.config");

            }
            return configOK;
        }

        private static void CreateDemoData()
        {
            Console.WriteLine("Creating Demo data");
            Console.WriteLine("Functions will store logs in the 'azure-webjobs-hosts' container in the specified Azure storage account. The functions take in a TextWriter parameter for logging.");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(ConfigurationManager.ConnectionStrings["AzureWebJobsStorage"].ConnectionString);

            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("initialorder");
            CloudQueue queue2 = queueClient.GetQueueReference("initialorderproperty");
            queue.CreateIfNotExists();
            queue2.CreateIfNotExists();

            Order person = new Order()
            {
                Name = ConfigurationManager.AppSettings["MyTestValue"],
                OrderId = "42"
            };

            queue.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(person)));
            queue2.AddMessage(new CloudQueueMessage(JsonConvert.SerializeObject(person)));
        }
    }

    public static class ApplicationVersionHelper
    {
        private static string _machineNameHash;
        public static string MachineNameHash {
            get {
                if (null == _machineNameHash) {
                    _machineNameHash = Environment.MachineName;
                }
                return _machineNameHash;
            }
        }

        private static string _buildString;
        public static string BuildString {
            get {
                if (null == _buildString) {
                    try {
                        _buildString = String.Format("{0} {1}", CurrentFileVersion, RetrieveLinkerTimestamp().ToString());
                    }
                    catch (Exception e) {
                        _buildString = e.Message;
                    }
                }
                return _buildString;
            }
        }

        private static Version CurrentFileVersion { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        // http://stackoverflow.com/questions/1600962/displaying-the-build-date
        private static DateTime RetrieveLinkerTimestamp() {
            string filePath = Assembly.GetCallingAssembly().Location;
            const int c_PeHeaderOffset = 60;
            const int c_LinkerTimestampOffset = 8;
            byte[] b = new byte[2048];
            Stream s = null;
            try {
                s = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                s.Read(b, 0, 2048);
            }
            finally {
                if (s != null) {
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
