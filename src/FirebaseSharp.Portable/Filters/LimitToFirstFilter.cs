using System.Linq;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    internal class LimitToFirstFilter : ISubscriptionFilter
    {
        private readonly int _limit;

        public LimitToFirstFilter(int limit)
        {
            _limit = limit;
        }

        public JToken Apply(JToken filtered, IFilterContext context)
        {
            foreach (var child in filtered.Children().Skip(_limit).ToList())
            {
                child.Remove();
            }

            return filtered;
        }
    }
}