using System;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Filters;
using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable
{
    internal sealed class Firebase : IFirebase
    {
        private readonly FirebaseApp _app;
        private readonly string _path;
        private readonly string _key;
        private readonly string _parent;

        internal Firebase(FirebaseApp app, string path)
        {
            _app = app;
            _path = path;
            _key = null;

            if (_path != null)
            {
                _key = _path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                _parent = BuildParentPath(_path);
            }
        }

        private string BuildParentPath(string path)
        {
            var parts = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length <= 1)
            {
                return "/";
            }

            StringBuilder sb = new StringBuilder();
            foreach (var part in parts.Take(parts.Length - 1))
            {
                sb.AppendFormat("/{0}", part.Trim());
            }

            return sb.ToString();
        }

        FirebaseQuery CreateQuery()
        {
            return new FirebaseQuery(_app, _path);
        }

        public IFirebaseReadonlyQuery On(string eventName, SnapshotCallback callback)
        {
            return CreateQuery().On(eventName, callback);
        }

        public IFirebaseReadonlyQuery On(string eventName, SnapshotCallback callback, object context)
        {
            return CreateQuery().On(eventName, callback, context);
        }

        public IFirebaseReadonlyQuery Once(string eventName, SnapshotCallback callback, FirebaseStatusCallback cancelledCallback = null)
        {
            return CreateQuery().Once(eventName, callback, cancelledCallback);
        }

        public IFirebaseReadonlyQuery Once(string eventName, SnapshotCallback callback, object context,
            FirebaseStatusCallback cancelledCallback = null)
        {
            return CreateQuery().Once(eventName, callback, context, cancelledCallback);
        }

        public IFilterableQueryExecutor OrderByChild(string child)
        {
            return CreateQuery().OrderByChild(child);
        }

        public IFilterableQueryExecutor OrderByKey()
        {
            return CreateQuery().OrderByKey();
        }

        public IFilterableQueryExecutor OrderByValue<T>()
        {

            return CreateQuery().OrderByValue<T>();
        }

        public IFilterableQueryExecutor OrderByPriority()
        {
            return CreateQuery().OrderByPriority();
        }

        public IFirebaseQueryExecutorAny StartAt(string startingValue)
        {
            return CreateQuery().StartAt(startingValue);
        }

        public IFirebaseQueryExecutorAny StartAt(long startingValue)
        {
            return CreateQuery().StartAt(startingValue);
        }

        public IOrderableQueryExecutor EndAt(string endingValue)
        {
            return CreateQuery().EndAt(endingValue);
        }

        public IOrderableQueryExecutor EndAt(long endingValue)
        {
            return CreateQuery().EndAt(endingValue);
        }

        public IOrderableQueryExecutor EqualTo(string value)
        {
            return CreateQuery().EqualTo(value);
        }

        public IOrderableQueryExecutor EqualTo(long value)
        {
            return CreateQuery().EqualTo(value);
        }

        public IOrderableQueryExecutor LimitToFirst(int limit)
        {
            return CreateQuery().LimitToFirst(limit);
        }

        public IOrderableQueryExecutor LimitToLast(int limit)
        {
            return CreateQuery().LimitToLast(limit);
        }

        public IFirebase Ref()
        {
            return new Firebase(_app, _path);
        }

        public IFirebase Child(string childPath)
        {
            childPath = childPath.Trim(new[] {'/', ' '});
            return _app.Child(String.Format("{0}/{1}", _path, childPath));
        }

        public IFirebase Parent()
        {
            return _app.Child(_parent);
        }

        public IFirebase Root()
        {
            return _app.Child("/");
        }

        public string Key
        {
            get { return _key; }
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

        public void SetWithPriority(string value, FirebasePriority priority, FirebaseStatusCallback callback = null)
        {
            throw new NotImplementedException();
        }

        public void SetPriority(FirebasePriority priority, FirebaseStatusCallback callback = null)
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
