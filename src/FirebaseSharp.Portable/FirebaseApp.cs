using System;
using System.Collections.Generic;
using System.Threading;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json;

namespace FirebaseSharp.Portable
{
    public static class ServerValue
    {
        public static ServerTimestamp TIMESTAMP
        {
            get { return new ServerTimestamp(); }
        }

        public class ServerTimestamp
        {
            [JsonProperty(PropertyName = ".sv")] public string Timestamp = "timestamp";
        }
    }

    public sealed class FirebaseApp : IFirebaseApp
    {
        private readonly Uri _rootUri;
        private readonly SyncDatabase _cache;
        private readonly SubscriptionDatabase _subscriptions;
        private readonly SubscriptionProcessor _subProcessor;
        private readonly CancellationTokenSource _shutdownToken = new CancellationTokenSource();

        internal FirebaseApp(Uri rootUri, IFirebaseNetworkConnection connection)
        {
            _rootUri = rootUri;
            _cache = new SyncDatabase(this, connection);
            _cache.Changed += FireChangeEvents;
            _subscriptions = new SubscriptionDatabase(this, _cache);
            _subProcessor = new SubscriptionProcessor(_shutdownToken.Token);
            GoOnline();
        }

        public FirebaseApp(Uri root, string auth = null)
        {
            _rootUri = root;
            _cache = new SyncDatabase(this, new FirebaseNetworkConnection(root, auth));
            _cache.Changed += FireChangeEvents;
            _subscriptions = new SubscriptionDatabase(this, _cache);
            _subProcessor = new SubscriptionProcessor(_shutdownToken.Token);
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

            _subscriptions.ClearDone();
        }

        internal Uri RootUri
        {
            get { return _rootUri; }
        }

        internal void Set(FirebasePath path, string value, FirebaseStatusCallback callback)
        {
            _cache.Set(path, value, callback);
        }

        internal void Set(FirebasePath path, object value, FirebaseStatusCallback callback)
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

        internal string Push(FirebasePath path, object value, FirebaseStatusCallback callback)
        {
            return _cache.Push(path, value, callback);
        }

        internal Guid Subscribe(string eventName, FirebasePath path, SnapshotCallback callback, object context,
            IEnumerable<ISubscriptionFilter> filters)
        {
            return _subscriptions.Subscribe(path, eventName, callback, context, false, filters);
        }

        internal void Unsubscribe(Guid queryId)
        {
            _subscriptions.Unsubscribe(queryId);
        }

        internal Guid SubscribeOnce(string eventName, FirebasePath path, SnapshotCallback callback, object context,
            IEnumerable<ISubscriptionFilter> filters, FirebaseStatusCallback cancelledCallback)
        {
            return _subscriptions.Subscribe(path, eventName, callback, context, true, filters);
        }

        public void Dispose()
        {
            _shutdownToken.Cancel();
            using (_cache)
            {
            }
            using (_shutdownToken)
            {
            }
            using (_subProcessor)
            {
            }
        }

        internal void SetPriority(FirebasePath path, FirebasePriority priority, FirebaseStatusCallback callback)
        {
            _cache.SetPriority(path, priority, callback);
        }

        internal void SetWithPriority(FirebasePath path, string value, FirebasePriority priority,
            FirebaseStatusCallback callback)
        {
            _cache.SetWithPriority(path, value, priority, callback);
        }

        internal void Fire(SnapshotCallback callback, DataSnapshot snap, object context)
        {
            _subProcessor.Add(callback, snap, context);
        }
    }
}