using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable
{
    public class FirebaseApp : IFirebaseApp
    {
        private AuthenticationState _authState;
        private readonly Uri _rootUri;
        private readonly SyncDatabase _cache;
        private readonly SubscriptionDatabase _subscriptions;

        public FirebaseApp(Uri root)
        {
            _rootUri = root;
            _cache = new SyncDatabase(new FirebaseNetworkConnection(root));
            _subscriptions = new SubscriptionDatabase();
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
            throw new NotImplementedException();
        }

        public void GoOffline()
        {
            throw new NotImplementedException();
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

        internal void Subscribe(string eventName, string path, SnapshotCallback callback, object context)
        {
            _subscriptions.Subscribe(path, eventName, callback, context, false);
        }

        internal void Unsubscribe(string eventName, string path, SnapshotCallback callback, object context)
        {
            _subscriptions.Unsubscribe(path, eventName, callback, context);
        }

        internal void SubscribeOnce(string eventName, string path, SnapshotCallback callback, object context, FirebaseStatusCallback cancelledCallback)
        {
            _subscriptions.Subscribe(path, eventName, callback, context, true);
        }
    }
}
