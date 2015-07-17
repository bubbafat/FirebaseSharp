using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirebaseSharp.Portable
{
    class DeltaCache
    {
        private readonly SyncDatabase _syncDb;
        private readonly SubscriptionDatabase _subscriptions;

        public DeltaCache(SyncDatabase syncDb, SubscriptionDatabase subscriptions)
        {
            _syncDb = syncDb;
            _subscriptions = subscriptions;

            _syncDb.Changed += FireChangeEvents;
        }

        private void FireChangeEvents(object sender, JsonCacheUpdateEventArgs args)
        {
            foreach (var sub in _subscriptions.Changed(_syncDb))
            {
                sub.Fire();
            }
        }
    }
}
