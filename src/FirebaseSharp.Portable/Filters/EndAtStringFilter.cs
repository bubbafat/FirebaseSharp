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

        public JToken Apply(JToken filtered, IFilterContext context)
        {
            JObject result = new JObject();

            foreach (var child in filtered.Children().TakeWhile(t => String.Compare(t[context.FilterColumn].Value<string>(), _endingValue, StringComparison.Ordinal) <= 0))
            {
                result.Add(child);
            }

            return result;
        }
    }
}
