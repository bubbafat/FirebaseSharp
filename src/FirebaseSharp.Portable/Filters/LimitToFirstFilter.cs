using System;
using System.Collections.Generic;
using System.Linq;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;

namespace FirebaseSharp.Portable.Filters
{
    class LimitToFirstFilter : ISubscriptionFilter
    {
        private readonly int _limit;

        public LimitToFirstFilter(int limit)
        {
            _limit = limit;
        }

        public IEnumerable<IDataSnapshot> Filter(IEnumerable<IDataSnapshot> snapshots)
        {
            return snapshots.Take(_limit);
        }
    }
}
