using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable.Subscriptions
{
    class TakeLast : ISubscriptionFilter
    {
        private LinkedList<IDataSnapshot> _snaps;
        private readonly int _count;
        public TakeLast(int count)
        {
            _count = count;
        }

        public void Begin()
        {
            _snaps = new LinkedList<IDataSnapshot>();
        }

        public void Take(IDataSnapshot snap)
        {
            _snaps.AddLast(snap);

            if (_snaps.Count >= _count)
            {
                _snaps.RemoveFirst();
            }
        }

        public IEnumerable<IDataSnapshot> Filter(IEnumerable<IDataSnapshot> snapshots)
        {
            return _snaps.Intersect(snapshots);
        }
    }
}
