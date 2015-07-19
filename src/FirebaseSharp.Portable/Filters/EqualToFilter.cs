using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    class EqualToFilter<T> : ISubscriptionFilter
        where T: IEquatable<T>
    {
        private readonly T _value;

        public EqualToFilter(T value)
        {
            _value = value;
        }

        public JToken Apply(JToken filtered)
        {
            JObject result = new JObject();

            foreach (var child in filtered.Children().Where(t => t.First.Value<T>().Equals(_value)))
            {
                result.Add(child);
            }

            return result;
        }
    }
}
