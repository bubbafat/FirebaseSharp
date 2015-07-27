using System;
using System.Diagnostics;
using FirebaseSharp.Portable;
using FirebaseSharp.Portable.Interfaces;

namespace PriorityQueue
{
    internal class QueueItem<T>
    {
        public ServerValue.ServerTimestamp Timestamp;
        public T Data;
    }

    internal sealed class JobQueue<T>
    {
        private readonly IFirebase _jobs;
        private readonly IFirebaseReadonlyQuery _query;

        public JobQueue(IFirebase jobs, Func<T, bool> callback)
        {
            // create our own copy and ignore filters
            _jobs = jobs.Child("/");

            _query = _jobs
                .On("child_changed", (snap, child, context) =>
                {
                    if (snap.Exists)
                    {
                        if (snap["Timestamp"][".sv"].Exists)
                        {
                            // local version
                            return;
                        }

                        T data = snap["Data"].Value<T>();

                        try
                        {
                            if (callback(data))
                            {
                                // remove from the queue
                                snap.Ref().Remove();
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("ERROR: {0}", ex);
                        }
                    }
                });
        }

        public void Enqueue(T job)
        {
            QueueItem<T> item = new QueueItem<T>
            {
                Data = job,
                Timestamp = ServerValue.TIMESTAMP,
            };

            IFirebase path = _jobs.Push();
            path.Set(item);
        }
    }
}