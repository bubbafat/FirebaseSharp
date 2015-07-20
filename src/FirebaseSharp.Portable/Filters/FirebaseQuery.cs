using System;
using System.Collections.Generic;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;

namespace FirebaseSharp.Portable.Filters
{
    class FirebaseQuery : IFirebaseQueryExecutorAny, IFirebaseReadonlyQuery
    {
        private readonly FirebaseApp _app;
        private Guid _queryId;
        private readonly string _path;
        private readonly List<ISubscriptionFilter> _filters;

        public FirebaseQuery(FirebaseApp app, string path)
        {
            _app = app;
            _path = path;
            _filters = new List<ISubscriptionFilter>();
        }

        public IFirebaseReadonlyQuery On(string eventName, SnapshotCallback callback)
        {
            return On(eventName, callback, null);
        }

        public IFirebaseReadonlyQuery On(string eventName, SnapshotCallback callback, object context)
        {
            _queryId = _app.Subscribe(eventName, _path, callback, context, _filters);
            return this;
        }

        public void Off()
        {
            _app.Unsubscribe(_queryId);
        }

        public IFirebaseReadonlyQuery Once(string eventName, SnapshotCallback callback, FirebaseStatusCallback cancelledCallback = null)
        {
            return Once(eventName, callback, null, cancelledCallback);
        }

        public IFirebaseReadonlyQuery Once(string eventName, SnapshotCallback callback, object context,
            FirebaseStatusCallback cancelledCallback = null)
        {
            _queryId = _app.SubscribeOnce(eventName, _path, callback, context, _filters, cancelledCallback);
            return this;
        }

        public IFilterableQueryExecutor OrderByChild(string child)
        {
            _filters.Add(new OrderByChildFilter(child));
            return this;
        }

        public IFilterableQueryExecutor OrderByKey()
        {
            _filters.Add(new OrderByKeyFilter());
            return this;
        }

        public IFilterableQueryExecutor OrderByValue<T>()
        {
            _filters.Add(new OrderByValueFilter<T>());
            return this;
        }

        public IFilterableQueryExecutor OrderByPriority()
        {
            _filters.Add(new OrderByPriorityFilter());
            return this;
        }

        public IFirebaseQueryExecutorAny StartAt(string startingValue)
        {
            _filters.Add(new StartAtStringFilter(startingValue));
            return this;
        }

        public IFirebaseQueryExecutorAny StartAt(long startingValue)
        {
            _filters.Add(new StartAtNumericFilter(startingValue));
            return this;
        }

        public IOrderableQueryExecutor EndAt(string endingValue)
        {
            _filters.Add(new EndAtStringFilter(endingValue));
            return this;
        }

        public IOrderableQueryExecutor EndAt(long endingValue)
        {
            _filters.Add(new EndAtNumericFilter(endingValue));
            return this;
        }

        public IOrderableQueryExecutor EqualTo(string value)
        {
            _filters.Add(new EqualToFilter<string>(value));
            return this;
        }

        public IOrderableQueryExecutor EqualTo(long value)
        {
            _filters.Add(new EqualToFilter<long>(value));
            return this;
        }

        public IOrderableQueryExecutor LimitToFirst(int limit)
        {
            _filters.Add(new LimitToFirstFilter(limit));
            return this;
        }

        public IOrderableQueryExecutor LimitToLast(int limit)
        {
            _filters.Add(new LimitToLastFilter(limit));
            return this;
        }

        public IFirebase Ref()
        {
            return new Firebase(_app, _path);
        }
    }
}
