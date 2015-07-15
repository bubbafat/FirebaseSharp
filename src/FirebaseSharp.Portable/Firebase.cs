using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable
{
    internal sealed class Firebase : IFirebase
    {
        private readonly FirebaseApp _app;
        private readonly string _path;

        private Firebase(FirebaseApp app, string path)
        {
            _app = app;
            _path = path;
        }

        public void On(string eventName, SnapshotCallback callback)
        {
            On(eventName, callback, null);
        }

        public void On(string eventName, SnapshotCallback callback, object context)
        {
            _app.Subscribe(eventName, _path, callback, context);
        }

        public void Off(string eventName, SnapshotCallback callback)
        {
            Off(eventName, callback, null);
        }

        public void Off(string eventName, SnapshotCallback callback, object context)
        {
            _app.Unsubscribe(eventName, _path, callback, context);
        }

        public void Once(string eventName, SnapshotCallback callback, FirebaseStatusCallback cancelledCallback = null)
        {
            Once(eventName, callback, null, cancelledCallback);
        }

        public void Once(string eventName, SnapshotCallback callback, object context,
            FirebaseStatusCallback cancelledCallback = null)
        {
            _app.SubscribeOnce(eventName, _path, callback, context, cancelledCallback);
        }

        private Firebase Clone()
        {
            return (Firebase) Ref();
        }

        public IFirebase OrderByChild(string key)
        {
            return Clone().Decorate("orderBy", new QuotedParameter(key));
        }

        public IFirebase OrderByKey()
        {
            return Clone().Decorate("orderBy", new QuotedParameter("$key"));
        }

        public IFirebase OrderByValue()
        {
            return Clone().Decorate("orderBy", new QuotedParameter("$value"));
        }

        public IFirebase OrderByPriority()
        {
            return Clone().Decorate("orderBy", new QuotedParameter("$priority"));
        }

        public IFirebase StartAt(string startingValue)
        {
            return Clone().Decorate("startAt", new QuotedParameter(startingValue));
        }

        public IFirebase StartAt(long startingValue)
        {
            return Clone().Decorate("startAt", new IntegerParameter(startingValue));
        }

        public IFirebase EndAt(string endingValue)
        {
            return Clone().Decorate("endAt", new QuotedParameter(endingValue));
        }

        public IFirebase EndAt(long endingValue)
        {
            return Clone().Decorate("endAt", new IntegerParameter(endingValue));
        }

        public IFirebase EqualTo(string value)
        {
            return Clone().Decorate("equalTo", new QuotedParameter(value));
        }

        public IFirebase EqualTo(long value)
        {
            return Clone().Decorate("equalTo", new IntegerParameter(value));
        }

        public IFirebase LimitToFirst(int limit)
        {
            return Clone().Decorate("LimitToFirst", new IntegerParameter(limit));
        }

        public IFirebase LimitToLast(int limit)
        {
            return Clone().Decorate("LimitToLast", new IntegerParameter(limit));
        }

        public IFirebase Ref()
        {
            return new Firebase(_app, _path, _decorations);
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
