using System;
using System.Collections.Generic;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;

namespace FirebaseSharp.Portable
{
    public sealed class FirebaseApp : IFirebaseApp
    {
        private AuthenticationState _authState;
        private readonly Uri _rootUri;
        private readonly SyncDatabase _cache;
        private readonly SubscriptionDatabase _subscriptions;

        internal FirebaseApp(Uri rootUri, IFirebaseNetworkConnection connection)
        {
            _rootUri = rootUri;
            _cache = new SyncDatabase(connection);
            _subscriptions = new SubscriptionDatabase(_cache);
            _cache.Changed += FireChangeEvents;
            GoOnline();
        }

        public FirebaseApp(Uri root)
        {
            _rootUri = root;
            _cache = new SyncDatabase(new FirebaseNetworkConnection(root));
            _subscriptions = new SubscriptionDatabase(_cache);
            _cache.Changed += FireChangeEvents;
            GoOnline();
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

        internal void Set(string path, string value, FirebaseStatusCallback callback)
        {
            _cache.Set(path, value, callback);
        }

        internal void Update(string path, string value, FirebaseStatusCallback callback)
        {
            _cache.Update(path, value, callback);
        }

        internal string Push(string path, string value, FirebaseStatusCallback callback)
        {
            return _cache.Push(path, value, callback);
        }

        public IFirebase Child(string path)
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

        public void AuthWithCustomToken(string authToken)
        {
            throw new NotImplementedException();
        }

        public void AuthAnonymously()
        {
            throw new NotImplementedException();
        }

        public void AuthWithPassword(string email, string password)
        {
            throw new NotImplementedException();
        }

        public void AuthWithOAuthToken(string provider, string credentials)
        {
            throw new NotImplementedException();
        }

        public AuthenticationState GetAuth()
        {
            throw new NotImplementedException();
        }

        public void Unauth()
        {
            throw new NotImplementedException();
        }

        public event AuthChangedEvent AuthChanged;

        internal void Subscribe(string eventName, string path, SnapshotCallback callback, object context, IEnumerable<ISubscriptionFilter> filters)
        {
            _subscriptions.Subscribe(path, eventName, callback, context, false, filters);
        }

        internal void Unsubscribe(string eventName, string path, SnapshotCallback callback, object context)
        {
            _subscriptions.Unsubscribe(path, eventName, callback, context);
        }

        internal void SubscribeOnce(string eventName, string path, SnapshotCallback callback, object context, IEnumerable<ISubscriptionFilter> filters, FirebaseStatusCallback cancelledCallback)
        {
            _subscriptions.Subscribe(path, eventName, callback, context, true, filters);
        }

        public void Dispose()
        {
            using (_cache) { }
        }
    }
}
