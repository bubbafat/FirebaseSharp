using System;
using System.Collections.Generic;
using System.Linq;
using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable.Subscriptions
{
    internal class TakeLastFilter : ISubscriptionFilter
    {
        private readonly int _count;

        public TakeLastFilter(int count)
        {
            _count = count;
        }

        public IEnumerable<IDataSnapshot> Filter(IEnumerable<IDataSnapshot> snapshots)
        {
            return snapshots.Skip(Math.Max(0, snapshots.Count() - _count));
        }
    }
}
