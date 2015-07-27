using System;
using System.Linq;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    internal class EqualToFilter<T> : ISubscriptionFilter
        where T : IEquatable<T>
    {
        private readonly T _value;

        public EqualToFilter(T value)
        {
            _value = value;
        }

        public JToken Apply(JToken filtered, IFilterContext context)
        {
            JObject result = new JObject();

            JObject obj = filtered as JObject;
            if (obj != null)
            {
                foreach (var ordered in filtered.Children().Cast<JProperty>().Where(c =>
                {
                    if (c.Value.Type == JTokenType.Object)
                    {
                        var test = c.Value[context.FilterColumn];
                        if (test != null)
                        {
                            if (test is JValue)
                            {
                                T val;
                                try
                                {
                                    val = test.Value<T>();
                                }
                                catch (Exception)
                                {
                                    return false;
                                }

                                return val.Equals(_value);
                            }

                            return false;
                        }
                    }

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