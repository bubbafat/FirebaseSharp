using System;
using System.Linq;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    class EndAtKeyFilter : ISubscriptionFilter
    {
        private readonly string _endingKey;
        public EndAtKeyFilter(string endingKey)
        {
            _endingKey = endingKey;
        }
        public JToken Apply(JToken filtered, IFilterContext context)
        {
            JObject result = new JObject();

            JObject obj = filtered as JObject;
            if (obj != null)
            {
                foreach (var ordered in filtered.Children().Cast<JProperty>().TakeWhile(c =>
                {
                    if (c.Value == null || c.Value.Type == JTokenType.Null)
                    {
                        return true;
                    }

                    return String.Compare(c.Path, _endingKey, StringComparison.Ordinal) <= 0;
                }))
                {
                    result.Add(ordered);
                }
            }

            return result;
        }
    }
}
