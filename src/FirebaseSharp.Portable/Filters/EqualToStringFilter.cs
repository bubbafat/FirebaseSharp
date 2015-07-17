using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    class EqualToStringFilter : ISubscriptionFilter
    {
        private readonly string _value;

        public EqualToStringFilter(string value)
        {
            this._value = value;
        }

        public JToken Apply(JToken filtered)
        {
            throw new NotImplementedException();
        }
    }
}
