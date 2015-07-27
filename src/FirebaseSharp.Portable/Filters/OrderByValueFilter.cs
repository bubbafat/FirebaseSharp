using System.Linq;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    internal class OrderByValueFilter : ISubscriptionFilter
    {
        public JToken Apply(JToken filtered, IFilterContext context)
        {
            JObject result = new JObject();

            JObject obj = filtered as JObject;
            if (obj != null)
            {
                foreach (var ordered in filtered.Children().Cast<JProperty>().OrderBy(c =>
                {
                    if (c.Value is JValue)
                    {
                        return c.Value;
                    }

                    return (JObject) null;
                }, new FirebaseValueSorter()))
                {
                    result.Add(ordered);
                }
            }

            return result;
        }
    }
}