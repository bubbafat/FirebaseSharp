using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    class EndAtNumericFilter : ISubscriptionFilter
    {
        private readonly long _endingValue;

        public EndAtNumericFilter(long endingValue)
        {
            _endingValue = endingValue;
        }

        public JToken Apply(JToken filtered, IFilterContext context)
        {
            JObject result = new JObject();

            foreach (var child in filtered.Children().TakeWhile(t => t[context.FilterColumn].Value<long>() <= _endingValue))
            {
                result.Add(child);
            }

            return result;
        }
    }
}
