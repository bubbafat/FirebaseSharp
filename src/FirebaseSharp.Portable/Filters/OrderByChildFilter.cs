﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    class OrderByChildFilter : ISubscriptionFilter
    {
        private readonly string _child;

        public OrderByChildFilter(string child)
        {
            _child = child;
        }

        public JToken Apply(JToken filtered, IFilterContext context)
        {
            context.FilterColumn = _child;

            JObject result = new JObject();

            JObject obj = filtered as JObject;
            if (obj != null)
            {
                foreach (var ordered in filtered.Children().Cast<JProperty>().OrderBy(c =>
                {
                    if (c.Value.Type == JTokenType.Object)
                    {
                        return ((JObject)c.Value)[_child];
                    }

                    return (JObject)null;
                }, new FirebaseValueSorter()))
                {
                    result.Add(ordered);
                }

            }

            return result;

        }
    }
}
