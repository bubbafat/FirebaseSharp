using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    class OrderByPriorityFilter : ISubscriptionFilter
    {
        public JToken Apply(JToken filtered, IFilterContext context)
        {
            JObject result = new JObject();

            foreach (var ordered in filtered.Children().OrderBy(c => c.First, new FirebasePrioritySorter()))
            {
                result.Add(ordered);
            }

            return result;
        }
    }
}
