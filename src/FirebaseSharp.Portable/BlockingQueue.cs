using System;
using System.Collections.Concurrent;
using System.Threading;

namespace FirebaseSharp.Portable
{
    internal class BlockingQueue<T> : IDisposable
    {
        private readonly BlockingCollection<T> _queue = new BlockingCollection<T>(new ConcurrentQueue<T>());

        public void Enqueue(CancellationToken cancel, T item)
        {
            int attempts = 0;
            while (attempts++ < 10)
            {
                if (_queue.TryAdd(item, -1, cancel))
                {
                    return;
                }
            }

            throw new Exception("Unable to enqueue the item");
        }

        public T Dequeue(CancellationToken cancel)
        {
            int attempts = 0;
            while (attempts++ < 10)
            {
                T item;

                if (_queue.TryTake(out item, -1, cancel))
                {
                    return item;
                }
            }

            throw new Exception("Unable to dequeue the item");
        }

        public void Dispose()
        {
            using (_queue)
            {
            }
        }
    }
}