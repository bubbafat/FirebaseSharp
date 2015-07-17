using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    class EndAtStringFilter : ISubscriptionFilter
    {
        private readonly string _endingValue;

        public EndAtStringFilter(string endingValue)
        {
            _endingValue = endingValue;
        }

        public JToken Apply(JToken filtered)
        {
            throw new NotImplementedException();
        }
    }
}
