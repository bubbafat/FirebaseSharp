using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable.Subscriptions
{
    interface ISubscriptionFilter
    {
        IEnumerable<IDataSnapshot> Filter(IEnumerable<IDataSnapshot> snapshots);
    }
}
