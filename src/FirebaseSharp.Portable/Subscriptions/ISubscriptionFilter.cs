using FirebaseSharp.Portable.Filters;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Subscriptions
{
    internal interface ISubscriptionFilter
    {
        JToken Apply(JToken filtered, IFilterContext context);
    }
}