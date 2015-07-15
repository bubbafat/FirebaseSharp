using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable.Subscriptions
{
    class TakeFirst : ISubscriptionFilter
    {
        private List<IDataSnapshot> _snaps;
        private readonly int _count;
        public TakeFirst(int count)
        {
            _count = count;
        }

        public void Begin()
        {
            _snaps = new List<IDataSnapshot>();
        }

        public void Take(IDataSnapshot snap)
        {
            if (_snaps.Count < _count)
            {
                _snaps.Add(snap);
            }
        }

        public IEnumerable<IDataSnapshot> Filter(IEnumerable<IDataSnapshot> snapshots)
        {
            return _snaps.Intersect(snapshots);
        }
    }
}
