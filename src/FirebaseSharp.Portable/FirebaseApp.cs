using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;

namespace FirebaseSharp.Portable
{
    public sealed class FirebaseApp : IFirebaseApp
    {
        private readonly Uri _rootUri;
        private readonly SyncDatabase _cache;
        private readonly SubscriptionDatabase _subscriptions;

        internal FirebaseApp(Uri rootUri, IFirebaseNetworkConnection connection)
        {
            _rootUri = rootUri;
            _cache = new SyncDatabase(this, connection);
            _cache.Changed += FireChangeEvents;
            _subscriptions = new SubscriptionDatabase(this, _cache);
            GoOnline();
        }

        public FirebaseApp(Uri root, string auth = null)
        {
            _rootUri = root;
            _cache = new SyncDatabase(this, new FirebaseNetworkConnection(root, auth));
            _cache.Changed += FireChangeEvents;
            _subscriptions = new SubscriptionDatabase(this, _cache);
            GoOnline();
        }

        public IFirebase Child(string path)
        {
            return Child(new FirebasePath(path));
        }

        internal Firebase Child(FirebasePath path)
        {
            return new Firebase(this, path);   
        }

        public void GoOnline()
        {
            _cache.GoOnline();
        }

        public void GoOffline()
        {
            _cache.GoOffline();
        }

        private void FireChangeEvents(object sender, JsonCacheUpdateEventArgs args)
        {
            foreach (var sub in _subscriptions.Subscriptions)
            {
                sub.Process(_cache);
            }
        }

        internal Uri RootUri
        {
            get
            {
                return _rootUri;
            }
        }

        internal void Set(FirebasePath path, string value, FirebaseStatusCallback callback)
        {
            _cache.Set(path, value, callback);
        }

        internal void Update(FirebasePath path, string value, FirebaseStatusCallback callback)
        {
            _cache.Update(path, value, callback);
        }

        internal string Push(FirebasePath path, string value, FirebaseStatusCallback callback)
        {
            return _cache.Push(path, value, callback);
        }

        internal Guid Subscribe(string eventName, FirebasePath path, SnapshotCallback callback, object context, IEnumerable<ISubscriptionFilter> filters)
        {
            return _subscriptions.Subscribe(path, eventName, callback, context, false, filters);
        }

        internal void Unsubscribe(Guid queryId)
        {
            _subscriptions.Unsubscribe(queryId);
        }

        internal Guid SubscribeOnce(string eventName, FirebasePath path, SnapshotCallback callback, object context, IEnumerable<ISubscriptionFilter> filters, FirebaseStatusCallback cancelledCallback)
        {
            return _subscriptions.Subscribe(path, eventName, callback, context, true, filters);
        }

        public void Dispose()
        {
            using (_cache) { }
        }
    }
}
