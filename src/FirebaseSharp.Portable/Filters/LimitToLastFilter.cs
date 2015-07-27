using System.Linq;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    internal class LimitToLastFilter : ISubscriptionFilter
    {
        private readonly int _limit;

        public LimitToLastFilter(int limit)
        {
            _limit = limit;
        }

        public JToken Apply(JToken filtered, IFilterContext context)
        {
            // if they have 6 and we want 2 then skip to index 4 (exclusive)
            int toIndex = filtered.Children().Count() - _limit;

            foreach (var child in filtered.Children().Take(toIndex).ToList())
            {
                child.Remove();
            }

            return filtered;
        }
    }
}