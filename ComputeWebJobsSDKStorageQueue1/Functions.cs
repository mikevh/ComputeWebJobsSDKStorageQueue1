﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace ComputeWebJobsSDKStorageQueue1
{
    public class Order
    {
        public string Name { get; set; }

        public string OrderId { get; set; }
    }

    public class Functions
    {
        public static void MultipleOutput([QueueTrigger("initialorder")] Order order, [Blob("orders/{OrderId}")] out string orderBlob, [Queue("orders")] out string orders)
        {
            orderBlob = order.OrderId;
            orders = order.OrderId;
        }

        /// <summary>
        /// Reads a message from the "orders" queue and writes a blob in the "orders" container
        /// </summary>
        public static void QueueToBlob([QueueTrigger("orders")] string orders, IBinder binder)
        {
            TextWriter writer = binder.Bind<TextWriter>(new BlobAttribute("orders/" + orders));
            writer.Write("Completed");
        }

        /// <summary>
        /// Shows binding parameters to properties of queue messages
        /// 
        /// The "Name" parameter will get the value of the "Name" property in the Order object
        /// The "DequeueCount" parameter has a special name and its value is retrieved from the actual CloudQueueMessage object
        /// </summary>
        public static void PropertyBinding([QueueTrigger("initialorderproperty")] Order initialorder, string Name, int dequeueCount, TextWriter log)
        {
            log.WriteLine("New order from: {0}", Name);
            log.WriteLine("Message dequeued {0} times", dequeueCount);
        }

        /// <summary>
        /// This function will always fail. It is used to demonstrate the poison queue messasge handling.
        /// After a binding or a function fails 5 times, the trigger message is moved into a poison queue
        /// </summary>
        public static void FailAlways([QueueTrigger("badqueue")] string message, int dequeueCount, TextWriter log)
        {
            log.WriteLine("When we reach 5 retries, the message will be moved into the badqueue-poison queue");
            log.WriteLine("Current dequeue count: " + dequeueCount);

            throw new InvalidOperationException("Simulated failure");
        }

        /// <summary>
        /// This function will be invoked when a message ends up in the poison queue
        /// </summary>
        public static void BindToPoisonQueue([QueueTrigger("badqueue-poison")] string message, TextWriter log)
        {
            log.Write("This message couldn't be processed by the original function: " + message);
        }
    }
}
