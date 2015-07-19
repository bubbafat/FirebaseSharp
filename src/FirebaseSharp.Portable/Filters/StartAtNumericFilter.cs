using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    class StartAtNumericFilter : ISubscriptionFilter
    {
        private readonly long _startingValue;

        public StartAtNumericFilter(long startingValue)
        {
            _startingValue = startingValue;
        }

        public JToken Apply(JToken filtered)
        {
            JObject result = new JObject();

            foreach (var child in filtered.Children().SkipWhile(t => t.First.Value<long>() < _startingValue))
            {
                result.Add(child);
            }

            return result;
        }
    }
}
