using System;
using System.Linq;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    internal class EndAtStringFilter : ISubscriptionFilter
    {
        private readonly string _endingValue;

        public EndAtStringFilter(string endingValue)
        {
            _endingValue = endingValue;
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

                    if (c.Value.Type == JTokenType.Object)
                    {
                        var test = ((JObject) c.Value)[context.FilterColumn];
                        if (test is JProperty)
                        {
                            test = ((JProperty) test).Value;
                        }

                        if (test != null && test.Type != JTokenType.Null)
                        {
                            if (test.Type == JTokenType.String)
                            {
                                return String.Compare(test.Value<string>(),
                                    _endingValue,
                                    StringComparison.Ordinal) <= 0;
                            }

                            // non-nulls aren't skipped
                            return false;
                        }

                        // skip missing/null
                        return true;
                    }

                    // there was something - but not the right type
                    return false;
                }))
                {
                    result.Add(ordered);
                }
            }

            return result;
        }
    }
}