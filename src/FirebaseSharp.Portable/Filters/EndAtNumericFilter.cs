using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;

namespace FirebaseSharp.Portable.Filters
{
    class EndAtNumericFilter : ISubscriptionFilter
    {
        private readonly long _endingValue;

        public EndAtNumericFilter(long endingValue)
        {
            _endingValue = endingValue;
        }

        public IEnumerable<IDataSnapshot> Filter(IEnumerable<IDataSnapshot> snapshots)
        {
            throw new NotImplementedException();
        }
    }
}
