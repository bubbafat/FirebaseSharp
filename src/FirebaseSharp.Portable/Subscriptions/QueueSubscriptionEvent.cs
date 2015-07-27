using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable.Subscriptions
{
    internal class QueueSubscriptionEvent
    {
        private readonly SnapshotCallback _callback;
        private readonly DataSnapshot _snap;
        private readonly object _context;
        private readonly object _lock = new object();

        public QueueSubscriptionEvent(SnapshotCallback callback, DataSnapshot snap, object context)
        {
            _callback = callback;
            _snap = snap;
            _context = context;
        }

        public void Execute()
        {
            lock (_lock)
            {
                if (_callback != null)
                {
                    _callback(_snap, null, _context);
                }
            }
        }
    }
}