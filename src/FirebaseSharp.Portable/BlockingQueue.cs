using System.Collections.Generic;
using System.Threading;

namespace FirebaseSharp.Portable
{
    internal class BlockingQueue<T>
    {
        private readonly LinkedList<T> _queue = new LinkedList<T>();

        public void Enqueue(T item)
        {
            lock (_queue)
            {
                _queue.AddFirst(item);
                Monitor.Pulse(_queue);
            }
        }

        public void Reque(T item)
        {
            lock (_queue)
            {
                _queue.AddLast(item);
                Monitor.Pulse(_queue);
            }
        }

        public T Dequeue()
        {
            lock (_queue)
            {
                while (_queue.Count == 0)
                    Monitor.Wait(_queue);
                var toReturn = _queue.Last.Value;
                _queue.RemoveLast();

                return toReturn;
            }
        }
    }
}
