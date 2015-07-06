using System;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable
{
    internal sealed class Firebase : IFirebase
    {
        private readonly FirebaseApp _app;
        private readonly string _path;

        public Firebase(FirebaseApp app, string path)
        {
            _app = app;
            _path = path;
            AbsoluteUri = new Uri(_app.RootUri, path);
        }

        public void On(string eventName, SnapshotCallback callback)
        {
            throw new NotImplementedException();
        }

        public void On(string eventName, object context, SnapshotCallback callback)
        {
            throw new NotImplementedException();
        }

        public void Off(string eventName, SnapshotCallback callback)
        {
            throw new NotImplementedException();
        }

        public void Off(string eventName, object context, SnapshotCallback callback)
        {
            throw new NotImplementedException();
        }

        public void Once(string eventName, SnapshotCallback callback, FirebaseStatusCallback cancelledCallback = null)
        {
            throw new NotImplementedException();
        }

        public void Once(string eventName, object context, SnapshotCallback callback, FirebaseStatusCallback cancelledCallback = null)
        {
            throw new NotImplementedException();
        }

        public IFirebase OrderByChild(string key)
        {
            throw new NotImplementedException();
        }

        public IFirebase OrderByKey()
        {
            throw new NotImplementedException();
        }

        public IFirebase OrderByValue()
        {
            throw new NotImplementedException();
        }

        public IFirebase OrderByPriority()
        {
            throw new NotImplementedException();
        }

        public IFirebase StartAt(object startingValue)
        {
            throw new NotImplementedException();
        }

        public IFirebase EndAt(object startingValue)
        {
            throw new NotImplementedException();
        }

        public IFirebase equalTo(object value)
        {
            throw new NotImplementedException();
        }

        public IFirebase limitToFirst(int limit)
        {
            throw new NotImplementedException();
        }

        public IFirebase limitToLast(int limit)
        {
            throw new NotImplementedException();
        }

        public IFirebase Ref()
        {
            throw new NotImplementedException();
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

        public string Key { get; private set; }
        public Uri AbsoluteUri { get; private set; }
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

        public void Transaction(TransactionUpdate updateCallback, TransactionComplete completeCallback = null, bool applyLocally = true)
        {
            throw new NotImplementedException();
        }

        public void CreateUser(string email, string password, FirebaseStatusCallback callback = null)
        {
            throw new NotImplementedException();
        }

        public void ChangeEmail(string oldEmail, string newEmail, string password, FirebaseStatusCallback callback = null)
        {
            throw new NotImplementedException();
        }

        public void ChangePassword(string email, string oldPassword, string newPassword, FirebaseStatusCallback callback = null)
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
