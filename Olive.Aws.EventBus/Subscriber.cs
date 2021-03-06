﻿using Amazon.SQS.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Olive.Aws
{
    class Subscriber<TMessage> where TMessage : IEventBusMessage
    {
        public Func<TMessage, Task> Handler { get; }
        ReceiveMessageRequest Request;
        DeleteMessageRequest Receipt;
        EventBusQueue Queue;
        Thread PollingThread;

        public Subscriber(EventBusQueue queue, Func<TMessage, Task> handler)
        {
            Handler = handler;
            Queue = queue;
        }

        public void Start()
        {
            Request = new ReceiveMessageRequest
            {
                QueueUrl = Queue.QueueUrl,
                WaitTimeSeconds = 10
            };

            Receipt = new DeleteMessageRequest { QueueUrl = Queue.QueueUrl };

            (PollingThread = new Thread(KeepPolling)).Start();
        }

        async Task<List<KeyValuePair<TMessage, Message>>> FetchEvents()
        {
            var response = await Fetch();
            var result = new List<KeyValuePair<TMessage, Message>>();

            foreach (var item in response.Messages)
            {
                try
                {
                    var @event = JsonConvert.DeserializeObject<TMessage>(item.Body);
                    result.Add(new KeyValuePair<TMessage, Message>(@event, item));
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to deserialize event message to " +
                        typeof(TMessage).FullName + ":\r\n" + item.Body, ex);
                }
            }

            return result;
        }

        async Task<ReceiveMessageResponse> Fetch()
        {
            try
            {
                return await Queue.Client.ReceiveMessageAsync(Request);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to fetch from Queue " + Queue.QueueUrl, ex);
            }
        }

        async void KeepPolling()
        {
            while (true)
            {
                foreach (var item in await FetchEvents())
                {
                    try
                    {
                        await Handler(item.Key);

                        Receipt.ReceiptHandle = item.Value.ReceiptHandle;
                        await Queue.Client.DeleteMessageAsync(Receipt);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Failed to run queue event handler " +
                            Handler.Method.DeclaringType.FullName + "." +
                            Handler.Method.GetDisplayName(), ex);
                    }
                }
            }
        }
    }
}