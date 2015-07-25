using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using FirebaseSharp.Portable.Interfaces;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    class FirebasePrioritySorter : IComparer<JObject>
    {
        readonly Lazy<FirebaseKeySorter> _keySort = new Lazy<FirebaseKeySorter>(() => new FirebaseKeySorter());
        public int Compare(JObject x, JObject y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            var xp = new FirebasePriority((JValue)x[".priority"]);
            var yp = new FirebasePriority((JValue)y[".priority"]);

            int result = xp.CompareTo(yp);

            if (result == 0)
            {
                result = _keySort.Value.Compare(x.Path, y.Path);
            }

            return result;
        }
    }
}
