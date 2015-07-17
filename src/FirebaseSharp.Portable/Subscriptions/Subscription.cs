using System;
using System.Collections.Generic;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Subscriptions;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable
{
    internal class Subscription
    {
        private JToken _lastRead = null;

        private readonly object _lock = new object();
        private readonly List<ISubscriptionFilter> _filters = new List<ISubscriptionFilter>();

        internal Subscription(IEnumerable<ISubscriptionFilter> filters)
        {
            if (filters != null)
            {
                _filters.AddRange(filters);
            }
        }

        public string Event { get; internal set; }
        public object Context { get; internal set; }
        public SnapshotCallback Callback { get; internal set; }
        public bool Once { get; internal set; }
        public string Path { get; internal set; }

        public void Process(SyncDatabase root)
        {
            lock (_lock)
            {
                JToken last = _lastRead;
                JToken snap = ApplyFilters(root.SnapFor(Path).Token);

                // store the state for next time
                _lastRead = snap != null ? snap.DeepClone() : null;

                FireEvents(snap, last);
            }
        }

        private void FireEvents(JToken snap, JToken last)
        {
            switch (Event)
            {
                case "value":
                    FireValue(snap, last);
                    break;
                case "child_added":
                    FireChildAdded(snap, last);
                    break;
                case "child_removed":
                    FireChildRemoved(snap, last);
                    break;
                case "child_changed":
                    FireChildChanged(snap, last);
                    break;
                case "child_moved":
                    FireChildMoved(snap, last);
                    break;
            }
        }

        private void FireChildMoved(JToken snap, JToken last)
        {
            throw new NotImplementedException();
        }

        private void FireChildChanged(JToken snap, JToken last)
        {
            if (snap == null || last == null)
            {
                return;
            }

            foreach (var child in snap)
            {
                var previous = last[child.Path];
                if (!JToken.DeepEquals(child, previous))
                {
                    Fire(child);
                }
            }
        }

        private void FireChildRemoved(JToken snap, JToken last)
        {
            if (last == null)
            {
                return;
            }

            foreach (var child in last)
            {
                if (snap == null)
                {
                    Fire(child);
                }
                else
                {
                    if (snap[child.Path] == null)
                    {
                        Fire(child);
                    }
                }
            }
        }

        private void FireChildAdded(JToken snap, JToken last)
        {
            if (snap == null)
            {
                return;
            }

            foreach (var child in snap)
            {
                if (last == null)
                {
                    Fire(child);
                }
                else
                {
                    if (last[child.Path] == null)
                    {
                        Fire(child);
                    }
                }
            }
        }

        private void FireValue(JToken snap, JToken last)
        {
            if (JToken.DeepEquals(snap, last))
            {
                return;
            }

            Fire(snap);
        }

        private JToken ApplyFilters(JToken jToken)
        {
            JToken filtered = jToken;
            foreach (var filter in _filters)
            {
                filtered = filter.Apply(filtered);
            }

            return filtered;
        }

        private void Fire(JToken state)
        {
            SnapshotCallback callback;
            
            lock (_lock)
            {
                callback = Callback;

                if (callback == null)
                {
                    return;
                }

                if (Once)
                {
                    Callback = null;
                }
            }

            callback(new DataSnapshot(state), null, Context);
        }
    }
}