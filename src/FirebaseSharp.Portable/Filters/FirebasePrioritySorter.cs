using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FirebaseSharp.Portable.Interfaces;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    class FirebasePrioritySorter : IComparer<JToken>
    {
        readonly Lazy<FirebaseKeySorter> _keySort = new Lazy<FirebaseKeySorter>(() => new FirebaseKeySorter()); 
        public int Compare(JToken x, JToken y)
        {
            var xp = new FirebasePriority(x);
            var yp = new FirebasePriority(y);

            int result = xp.CompareTo(yp);

            if (result == 0)
            {
                result = _keySort.Value.Compare(x.Path, y.Path);
            }

            return result;
        }
    }
}
