﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;

namespace FirebaseSharp.Portable.Filters
{
    class StartAtNumericFilter : ISubscriptionFilter
    {
        private readonly long _startingValue;

        public StartAtNumericFilter(long startingValue)
        {
            _startingValue = startingValue;
        }

        public IEnumerable<IDataSnapshot> Filter(IEnumerable<IDataSnapshot> snapshots)
        {
            throw new NotImplementedException();
        }
    }
}
