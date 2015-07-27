using System;
using FirebaseSharp.Portable.Filters;
using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable
{
    internal sealed class Firebase : IFirebase
    {
        private readonly FirebaseApp _app;
        private readonly FirebasePath _path;

        internal Firebase(FirebaseApp app, FirebasePath path)
        {
            _app = app;
            _path = path;
        }

        private FirebaseQuery CreateQuery()
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

        public IFirebaseReadonlyQuery Once(string eventName, SnapshotCallback callback,
            FirebaseStatusCallback cancelledCallback = null)
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

        public IFilterableQueryExecutor OrderByValue()
        {
            return CreateQuery().OrderByValue();
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

        public IFirebaseQueryExecutorAny StartAtKey(string startingValue)
        {
            return CreateQuery().StartAtKey(startingValue);
        }

        public IFirebaseQueryExecutorAny EndAtKey(string endingValue)
        {
            return CreateQuery().EndAtKey(endingValue);
        }

        public IFirebaseQueryExecutorAny EqualTo(string value)
        {
            return CreateQuery().EqualTo(value);
        }

        public IFirebaseQueryExecutorAny EqualTo(long value)
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
            return _app.Child(_path.Child(childPath));
        }

        public IFirebase Parent()
        {
            if (_path.IsRoot)
            {
                return null;
            }

            return _app.Child(_path.Parent());
        }

        public IFirebase Root()
        {
            return _app.Child(new FirebasePath());
        }

        public string Key
        {
            get { return _path.Key; }
        }

        public Uri AbsoluteUri
        {
            get { return new Uri(_app.RootUri, _path.RelativeUri); }
        }

        public void Set(string value, FirebaseStatusCallback callback = null)
        {
            _app.Set(_path, value, callback);
        }

        public void Set(object value, FirebaseStatusCallback callback = null)
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

        public IFirebase Push(object value, FirebaseStatusCallback callback = null)
        {
            return Child(_app.Push(_path, value, callback));
        }

        public void SetWithPriority(string value, string priority, FirebaseStatusCallback callback = null)
        {
            _app.SetWithPriority(_path, value, new FirebasePriority(priority), callback);
        }

        public void SetWithPriority(string value, float priority, FirebaseStatusCallback callback = null)
        {
            _app.SetWithPriority(_path, value, new FirebasePriority(priority), callback);
        }

        public void SetPriority(string priority, FirebaseStatusCallback callback = null)
        {
            _app.SetPriority(_path, new FirebasePriority(priority), callback);
        }

        public void SetPriority(float priority, FirebaseStatusCallback callback = null)
        {
            _app.SetPriority(_path, new FirebasePriority(priority), callback);
        }

        public IFirebaseApp GetApp()
        {
            return _app;
        }

        public override string ToString()
        {
            return AbsoluteUri.ToString();
        }
    }
}