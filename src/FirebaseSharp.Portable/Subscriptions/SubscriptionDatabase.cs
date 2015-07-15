using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable
{
    internal class Subscription
    {
        public string Event { get; internal set; }
        public object Context { get; internal set; }
        public SnapshotCallback Callback { get; internal set; }
        public bool Once { get; internal set; }
    }

    internal class SubscriptionTreeNode
    {
        private readonly Dictionary<string, SubscriptionTreeNode> _children = new Dictionary<string, SubscriptionTreeNode>();
        private readonly List<Subscription> _subscriptions = new List<Subscription>();
        public SubscriptionTreeNode(string name)
        {
            Name = name;
        }
        public string Name { get; private set; }

        public IEnumerable<Subscription> Subscriptions
        {
            get
            {
                return _subscriptions;
            }
        }

        public void Subscribe(Subscription subscription)
        {
            _subscriptions.Add(subscription);
        }

        public void Remove(Subscription subscription)
        {
            _subscriptions.Remove(subscription);
        }

        public SubscriptionTreeNode GetOrCreate(string path)
        {
            SubscriptionTreeNode child;
            if (!_children.TryGetValue(path, out child))
            {
                child = new SubscriptionTreeNode(path);
                _children.Add(path, child);
            }

            return child;
        }

        public bool TryGetChild(string path, out SubscriptionTreeNode node)
        {
            return _children.TryGetValue(path, out node);
        }
    }

    internal class SubscriptionDatabase
    {
        private readonly SubscriptionTreeNode _root = new SubscriptionTreeNode(string.Empty);
        private readonly List<Subscription> _removeList = new List<Subscription>(); 
        private readonly object _lock = new object();

        public void Fire(string path, IDataSnapshot snapshot)
        {
            lock (_lock)
            {
                SubscriptionTreeNode node;
                if (TryGetNodeAtPath(path, out node))
                {
                    foreach (var sub in node.Subscriptions)
                    {
                        sub.Callback(snapshot, null, sub.Context);

                        if (sub.Once)
                        {
                            _removeList.Add(sub);
                        }
                    }

                    foreach (var sub in _removeList)
                    {
                        node.Remove(sub);
                    }

                    _removeList.Clear();
                }
            }
        }

        public void Subscribe(string path, string eventName, SnapshotCallback callback, object context, bool once)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Cannot subscribe to an empty path");
            }

            lock (_lock)
            {
                SubscriptionTreeNode node = GetOrCreateNodeAtPath(path);
                node.Subscribe(new Subscription
                {
                    Event = eventName,
                    Callback = callback,
                    Context = context,
                    Once = once,
                });
            }
        }

        public void Unsubscribe(string path, string eventName, SnapshotCallback callback, object context)
        {
            lock (_lock)
            {
                SubscriptionTreeNode node;
                if (TryGetNodeAtPath(path, out node))
                {
                    _removeList.AddRange(node.Subscriptions.Where(s => s.Event == eventName &&
                                                                       s.Callback == callback &&
                                                                       s.Context == context));

                    foreach (var sub in _removeList)
                    {
                        node.Remove(sub);
                    }

                    _removeList.Clear();
                }
            }
        }

        private SubscriptionTreeNode GetOrCreateNodeAtPath(string path)
        {
            SubscriptionTreeNode current = _root;

            foreach (string segment in SegmentPath(path))
            {
                current = current.GetOrCreate(path);
            }

            return current;
        }

        private bool TryGetNodeAtPath(string path, out SubscriptionTreeNode node)
        {
            node = _root;

            foreach (string segment in SegmentPath(path))
            {
                if (!node.TryGetChild(segment, out node))
                {
                    return false;
                }
            }

            return true;
        }

        IEnumerable<string> SegmentPath(string path)
        {
            return path.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
