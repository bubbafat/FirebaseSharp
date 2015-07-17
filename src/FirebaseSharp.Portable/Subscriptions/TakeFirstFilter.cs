using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable.Subscriptions
{
    class TakeFirstFilter : ISubscriptionFilter
    {
        private readonly int _count;
        public TakeFirstFilter(int count)
        {
            _count = count;
        }

        public IEnumerable<IDataSnapshot> Filter(IEnumerable<IDataSnapshot> snapshots)
        {
            return snapshots.Take(_count);
        }
    }
}
