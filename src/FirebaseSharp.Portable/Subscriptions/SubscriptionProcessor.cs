using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable.Subscriptions
{
    internal class SubscriptionProcessor : IDisposable
    {
        private readonly BlockingQueue<QueueSubscriptionEvent> _queue;
        private CancellationToken _token;
        private Task _drainTask;
        private readonly object _startLock = new object();
        private bool _started;

        public SubscriptionProcessor(CancellationToken token)
        {
            _queue = new BlockingQueue<QueueSubscriptionEvent>();
            _token = token;
        }

        private void Processor()
        {
            while (true)
            {
                _token.ThrowIfCancellationRequested();

                var next = _queue.Dequeue(_token);

                try
                {
                    next.Execute();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("ERROR: exception throw during subscription callback");
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        private void EnsureStarted()
        {
            if (!_started)
            {
                lock (_startLock)
                {
                    if (!_started)
                    {
                        _drainTask = Task.Run(() => Processor(), _token);
                        _started = true;
                    }
                }
            }
        }

        public void Add(SnapshotCallback callback, DataSnapshot snap, object context)
        {
            EnsureStarted();
            _queue.Enqueue(_token, new QueueSubscriptionEvent(callback, snap, context));
        }

        public void Dispose()
        {
            using (_queue)
            {
            }
        }
    }
}