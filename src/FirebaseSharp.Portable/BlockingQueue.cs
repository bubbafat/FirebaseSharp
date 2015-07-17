using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FirebaseSharp.Portable
{
    internal class BlockingQueue<T>
    {
        private readonly Queue<T> _queue = new Queue<T>();
        private readonly SemaphoreSlim _semaphone = new SemaphoreSlim(1, 1);
        private readonly ManualResetEvent _available = new ManualResetEvent(false);

        public void Enqueue(CancellationToken cancel, T item)
        {
            _semaphone.Wait(cancel);
            try
            {
                _queue.Enqueue(item);
                _available.Set();
            }
            finally
            {
                _semaphone.Release();
            }
        }

        public T Dequeue(CancellationToken cancel)
        {
            while (true)
            {
                _semaphone.Wait(cancel);
                try
                {
                    if (_queue.Any())
                    {
                        return _queue.Dequeue();
                    }

                    _available.Reset();
                }
                finally
                {
                    _semaphone.Release();
                }

                _available.WaitOne(TimeSpan.FromSeconds(1));
            }
        }
    }
}
