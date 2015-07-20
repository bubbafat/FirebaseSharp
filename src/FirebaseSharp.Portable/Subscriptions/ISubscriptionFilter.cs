using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Filters;
using FirebaseSharp.Portable.Interfaces;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Subscriptions
{
    interface ISubscriptionFilter
    {
        JToken Apply(JToken filtered, IFilterContext context);
    }
}
