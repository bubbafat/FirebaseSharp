using System;
using System.Collections.Generic;
using System.Linq;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;

namespace FirebaseSharp.Portable
{
    internal class SubscriptionDatabase
    {
        private readonly SyncDatabase _syncDb;
        private readonly List<Subscription> _subscriptions = new List<Subscription>();
        private readonly object _lock = new object();
        private readonly FirebaseApp _app;

        public SubscriptionDatabase(FirebaseApp app, SyncDatabase syncDb)
        {
            _app = app;
            _syncDb = syncDb;
        }

        public Guid Subscribe(FirebasePath path, string eventName, SnapshotCallback callback, object context, bool once,
            IEnumerable<ISubscriptionFilter> filters)
        {
            var sub = new Subscription(_app, filters)
            {
                Event = eventName,
                Callback = callback,
                Context = context,
                Once = once,
                Path = path,
            };

            if (!once)
            {
                lock (_lock)
                {
                    _subscriptions.Add(sub);
                }
            }

            _syncDb.ExecuteInitial(sub);

            return sub.SubscriptionId;
        }

        internal IEnumerable<Subscription> Subscriptions
        {
            get
            {
                lock (_lock)
                {
                    return _subscriptions.ToList();
                }
            }
        }

        public void Unsubscribe(Guid subscriptionId)
        {
            lock (_lock)
            {
                _subscriptions.RemoveAll(q => q.SubscriptionId == subscriptionId);
            }
        }

        internal void ClearDone()
        {
            lock (_lock)
            {
                _subscriptions.RemoveAll(q => q.Callback == null);
            }
        }
    }
}