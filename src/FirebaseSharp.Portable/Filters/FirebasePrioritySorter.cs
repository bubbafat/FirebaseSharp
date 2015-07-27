using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    internal class FirebasePrioritySorter : IComparer<JObject>
    {
        private readonly Lazy<FirebaseKeySorter> _keySort = new Lazy<FirebaseKeySorter>(() => new FirebaseKeySorter());

        public int Compare(JObject x, JObject y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            var xp = new FirebasePriority((JValue) x[".priority"]);
            var yp = new FirebasePriority((JValue) y[".priority"]);

            int result = xp.CompareTo(yp);

            if (result == 0)
            {
                result = _keySort.Value.Compare(x.Path, y.Path);
            }

            return result;
        }
    }
}