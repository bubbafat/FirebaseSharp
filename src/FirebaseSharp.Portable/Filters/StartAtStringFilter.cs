﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;

namespace FirebaseSharp.Portable.Filters
{
    class StartAtStringFilter : ISubscriptionFilter
    {
        private readonly string _startingValue;

        public StartAtStringFilter(string startingValue)
        {
            _startingValue = startingValue;
        }

        public IEnumerable<IDataSnapshot> Filter(IEnumerable<IDataSnapshot> snapshots)
        {
            throw new NotImplementedException();
        }
    }
}
