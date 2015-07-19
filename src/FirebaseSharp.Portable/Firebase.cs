using System;
using System.Collections.Generic;
using FirebaseSharp.Portable.Filters;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;

namespace FirebaseSharp.Portable
{
    internal sealed class Firebase : IFirebase
    {
        private readonly FirebaseApp _app;
        private readonly string _path;
        private readonly List<ISubscriptionFilter> _filters;

        internal Firebase(FirebaseApp app, string path, List<ISubscriptionFilter> filters = null)
        {
            _app = app;
            _path = path;

            _filters = filters == null 
                ? new List<ISubscriptionFilter>() 
                : new List<ISubscriptionFilter>(filters);
        }

        public IFirebase On(string eventName, SnapshotCallback callback)
        {
            return On(eventName, callback, null);
        }

        public IFirebase On(string eventName, SnapshotCallback callback, object context)
        {
            _app.Subscribe(eventName, _path, callback, context, _filters);
            return this;
        }

        public void Off(string eventName, SnapshotCallback callback)
        {
            Off(eventName, callback, null);
        }

        public void Off(string eventName, SnapshotCallback callback, object context)
        {
            _app.Unsubscribe(eventName, _path, callback, context);
        }

        public IFirebase Once(string eventName, SnapshotCallback callback, FirebaseStatusCallback cancelledCallback = null)
        {
            return Once(eventName, callback, null, cancelledCallback);
        }

        public IFirebase Once(string eventName, SnapshotCallback callback, object context,
            FirebaseStatusCallback cancelledCallback = null)
        {
            _app.SubscribeOnce(eventName, _path, callback, context, _filters, cancelledCallback);
            return this;
        }

        public IFirebase OrderByChild(string child)
        {
            _filters.Add(new OrderByChildFilter(child));
            return this;
        }

        public IFirebase OrderByKey()
        {
            _filters.Add(new OrderByKeyFilter());
            return this;
        }

        public IFirebase OrderByValue<T>()
        {
            _filters.Add(new OrderByValueFilter<T>());
            return this;
        }

        public IFirebase OrderByPriority()
        {
            _filters.Add(new OrderByPriorityFilter());
            return this;
        }

        public IFirebase StartAt(string startingValue)
        {
            _filters.Add(new StartAtStringFilter(startingValue));
            return this;
        }

        public IFirebase StartAt(long startingValue)
        {
            _filters.Add(new StartAtNumericFilter(startingValue));
            return this;
        }

        public IFirebase EndAt(string endingValue)
        {
            _filters.Add(new EndAtStringFilter(endingValue));
            return this;
        }

        public IFirebase EndAt(long endingValue)
        {
            _filters.Add(new EndAtNumericFilter(endingValue));
            return this;
        }

        public IFirebase EqualTo(string value)
        {
            _filters.Add(new EqualToFilter<string>(value));
            return this;
        }

        public IFirebase EqualTo(long value)
        {
            _filters.Add(new EqualToFilter<long>(value));
            return this;
        }

        public IFirebase LimitToFirst(int limit)
        {
            _filters.Add(new LimitToFirstFilter(limit));
            return this;
        }

        public IFirebase LimitToLast(int limit)
        {
            _filters.Add(new LimitToLastFilter(limit));
            return this;
        }

        public IFirebase Ref()
        {
            return new Firebase(_app, _path, _filters);
        }

        public IFirebase Child(string childPath)
        {
            throw new NotImplementedException();
        }

        public IFirebase Parent()
        {
            throw new NotImplementedException();
        }

        public IFirebase Root()
        {
            throw new NotImplementedException();
        }

        public string Key
        {
            get { throw new NotImplementedException(); }
        }

        public Uri AbsoluteUri
        {
            get { return new Uri(_app.RootUri, _path); }

        }

        public void Set(string value, FirebaseStatusCallback callback = null)
        {
            _app.Set(_path, value, callback);
        }

        public void Update(string value, FirebaseStatusCallback callback = null)
        {
            _app.Update(_path, value, callback);
        }

        public void Remove(FirebaseStatusCallback callback = null)
        {
            _app.Set(_path, null, callback);
        }

        public IFirebase Push(string value, FirebaseStatusCallback callback = null)
        {
            return Child(_app.Push(_path, value, callback));
        }

        public void SetWithPriority(string value, IFirebasePriority priority, FirebaseStatusCallback callback = null)
        {
            throw new NotImplementedException();
        }

        public void SetPriority(IFirebasePriority priority, FirebaseStatusCallback callback = null)
        {
            throw new NotImplementedException();
        }

        public void Transaction(TransactionUpdate updateCallback, TransactionComplete completeCallback = null,
            bool applyLocally = true)
        {
            throw new NotImplementedException();
        }

        public void CreateUser(string email, string password, FirebaseStatusCallback callback = null)
        {
            throw new NotImplementedException();
        }

        public void ChangeEmail(string oldEmail, string newEmail, string password,
            FirebaseStatusCallback callback = null)
        {
            throw new NotImplementedException();
        }

        public void ChangePassword(string email, string oldPassword, string newPassword,
            FirebaseStatusCallback callback = null)
        {
            throw new NotImplementedException();
        }

        public void RemoveUser(string email, string password, FirebaseStatusCallback callback = null)
        {
            throw new NotImplementedException();
        }

        public void ResetPassword(string email, FirebaseStatusCallback callback = null)
        {
            throw new NotImplementedException();
        }

        public IFirebaseApp GetApp()
        {
            return _app;
        }
    }
}
