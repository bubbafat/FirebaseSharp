using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    class OrderByValueFilter<T> : ISubscriptionFilter
    {
        public JToken Apply(JToken filtered)
        {
            JObject result = new JObject();
            foreach (var child in filtered.Children().OrderBy(t => t.First.Value<T>()))
            {
                result.Add(child);
            }

            return result;
        }
    }
}
