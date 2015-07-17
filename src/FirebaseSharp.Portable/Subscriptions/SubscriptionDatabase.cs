using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;

namespace FirebaseSharp.Portable
{
    internal class SubscriptionDatabase
    {
        private readonly List<Subscription> _subscriptions = new List<Subscription>();
        private readonly object _lock = new object();

        public void Subscribe(string path, string eventName, SnapshotCallback callback, object context, bool once, IEnumerable<ISubscriptionFilter> filters)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Cannot subscribe to an empty path");
            }

            lock (_lock)
            {
                _subscriptions.Add(new Subscription(filters)
                {
                    Event = eventName,
                    Callback = callback,
                    Context = context,
                    Once = once,
                    Path = path,
                });
            }
        }

        internal IEnumerable<Subscription> Subscriptions
        {
            get
            {
                return _subscriptions;                
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
