using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable
{
    internal class Subscription
    {
        private JToken _lastRead = null;

        private readonly object _lock = new object();
        public string Event { get; internal set; }
        public object Context { get; internal set; }
        public SnapshotCallback Callback { get; internal set; }
        public bool Once { get; internal set; }
        public string Path { get; internal set; }

        public bool Matches(DataSnapshot snapshot)
        {
            JToken last = _lastRead;
            JToken snap = snapshot.Token;

            if (snap != null)
            {
                _lastRead = snap.DeepClone();
            }
            else
            {
                _lastRead = null;
            }

            if (last == null && snap == null)
            {
                return true;
            }

            if (last == null || snap == null)
            {
                return false;
            }

            return JToken.DeepEquals(last, snap);
        }

        public void Fire(IDataSnapshot snapshot)
        {
            SnapshotCallback callback;
            
            lock (_lock)
            {
                callback = Callback;
                if (Once)
                {
                    Callback = null;
                }
            }

            if (callback != null)
            {
                callback(snapshot, null, Context);
            }
        }
    }

    internal class SubscriptionDatabase
    {
        private readonly List<Subscription> _subscriptions = new List<Subscription>();
        private readonly object _lock = new object();

        public void Subscribe(string path, string eventName, SnapshotCallback callback, object context, bool once)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Cannot subscribe to an empty path");
            }

            lock (_lock)
            {
                _subscriptions.Add(new Subscription
                {
                    Event = eventName,
                    Callback = callback,
                    Context = context,
                    Once = once,
                    Path = path,
                });
            }
        }

        internal IEnumerable<Subscription> Changed(DataSnapshot snapshot)
        {
            lock (_lock)
            {
                return _subscriptions.Where(s => !s.Matches(snapshot));
            }
        }

        public void Unsubscribe(string path, string eventName, SnapshotCallback callback, object context)
        {
            lock (_lock)
            {
                var toRemove = _subscriptions.Where(s => s.Event == eventName &&
                                                         s.Callback == callback &&
                                                         s.Context == context).ToList();
                foreach (var sub in toRemove)
                {
                    _subscriptions.Remove(sub);
                }
            }
        }
    }
}
