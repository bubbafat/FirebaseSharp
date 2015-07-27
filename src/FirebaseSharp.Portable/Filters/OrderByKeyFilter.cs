using System.Linq;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    internal class OrderByKeyFilter : ISubscriptionFilter
    {
        public JToken Apply(JToken filtered, IFilterContext context)
        {
            JObject result = new JObject();

            foreach (var child in filtered.Children().OrderBy(t => t.Path, new FirebaseKeySorter()))
            {
                result.Add(child);
            }

            return result;
        }
    }
}