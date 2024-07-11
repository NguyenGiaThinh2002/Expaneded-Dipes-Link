using Cloudtoid.Interprocess;
using SharedProgram.Shared;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace IPCSharedMemory
{
    public class IPCSharedHelper : IDisposable
    {
        readonly QueueFactory _Factory;
        readonly QueueOptions _Options;
        readonly string _QueueName;
        readonly long _Capacity;
        readonly IPublisher _Publisher;
        readonly ISubscriber _Subscriber;
        public int countRec = 0;
        public byte[]? ReceiveData;
        public ConcurrentQueue<byte[]> MessageQueue = new();
        CancellationTokenSource? _ctsThreadListenData;

        // 1024 * 1024 * 100 = 100MB 
        public IPCSharedHelper(int index, string queueName, long capacity = 1024 * 1024 * 100, bool isReceiver = false)
        {
            _QueueName = queueName + (index + 1);
            _Capacity = capacity;
            _Factory = new();
            _Options = new(queueName: _QueueName, capacity: _Capacity);
            _Publisher = _Factory.CreatePublisher(_Options);
            _Subscriber = _Factory.CreateSubscriber(_Options);

            if (isReceiver)
            {
                ListenReceiveData();
            }
        }

        public void SendData(byte[] data)
        {
            try
            {
                _Publisher.TryEnqueue(data);
            }
            catch (Exception)
            {
                Debug.WriteLine("Send data via IPC fail");
            }
        }


        public void ListenReceiveData()
        {
            _ctsThreadListenData = new CancellationTokenSource();
            var token = _ctsThreadListenData.Token;
           Task.Run(async () =>
            {
                try
                {
                    while (!token.IsCancellationRequested)
                    {
                        while(_Subscriber.TryDequeue(default, out ReadOnlyMemory<byte> message) && message.Length > 0)
                        {
                            // await Task.Run(()=>  MessageQueue.Enqueue((byte[])message.ToArray().Clone()));
                            MessageQueue.Enqueue((byte[])message.ToArray().Clone());
                            await Task.Delay(1);
                        }
                        await Task.Delay(1);  
                    }
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("Thread listening data is cancelled.");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error in IPC Listen Data: " + ex.Message);
                }
            });
        }


        public void Dispose()
        {
            _Publisher?.Dispose();
            _Subscriber?.Dispose();
            _ctsThreadListenData?.Cancel();
            _ctsThreadListenData?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
