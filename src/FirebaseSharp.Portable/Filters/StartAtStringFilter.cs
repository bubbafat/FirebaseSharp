using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    class StartAtStringFilter : ISubscriptionFilter
    {
        private readonly string _startingValue;

        public StartAtStringFilter(string startingValue)
        {
            _startingValue = startingValue;
        }

        public JToken Apply(JToken filtered, IFilterContext context)
        {
            JObject result = new JObject();

            foreach (var child in filtered.Children().SkipWhile(t => String.Compare(t.First[context.FilterColumn].Value<string>(), _startingValue, StringComparison.Ordinal) < 0))
            {
                result.Add(child);
            }

            return result;
        }
    }
}
