using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FirebaseSharp.Portable
{
    internal class CacheItem
    {
        private List<CacheItem> _children;

        public string Name { get; set; }
        public string Value { get; set; }
        public CacheItem Parent { get; set; }
        public bool Created { get; set; }

        public List<CacheItem> Children
        {
            get
            {
                // we don't need a lock here because the tree is already 
                // synchronized so there will never be concurrent requests
                if (_children == null)
                {
                    _children = new List<CacheItem>();
                }

                return _children;
            }
        }
    }
    public class FirebaseValueAddedEventArgs : EventArgs
    {
        public FirebaseValueAddedEventArgs(string path, string data)
        {
            Path = path;
            Data = data;
        }

        public string Path { get; private set; }
        public string Data { get; private set; }
    }

    public class FirebaseValueChangedEventArgs : EventArgs
    {
        public FirebaseValueChangedEventArgs(string path, string data, string oldData)
        {
            Path = path;
            Data = data;
            OldData = oldData;
        }

        public string Path { get; private set; }
        public string Data { get; private set; }

        public string OldData { get; private set; }
    }

    public class FirebaseValueRemovedEventArgs : EventArgs
    {
        public FirebaseValueRemovedEventArgs(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }
    }

    public delegate void ValueAddedEventHandler(object sender, FirebaseValueAddedEventArgs args);
    public delegate void ValueChangedEventHandler(object sender, FirebaseValueChangedEventArgs args);
    public delegate void ValueRemovedEventHandler(object sender, FirebaseValueRemovedEventArgs args);


    internal sealed class FirebaseCache
    {
        private readonly CacheItem _tree = new CacheItem();
        private readonly object _treeLock = new object();

        private readonly char[] _seperator = {'/'};

        public FirebaseCache()
        {
            _tree.Name = string.Empty;
            _tree.Created = false;
            _tree.Parent = null;
            _tree.Name = null;
        }

        public void Update(string path, JsonReader data)
        {
            lock (_treeLock)
            {
                CacheItem root = FindRoot(path);
                Update(root, data);
            }
        }

        private CacheItem FindRoot(string path)
        {
            string[] segments = path.Split(_seperator, StringSplitOptions.RemoveEmptyEntries);

            CacheItem root = _tree;

            foreach (string segment in segments)
            {
                root = GetNamedChild(root, segment);
            }

            return root;
        }
    

        private static CacheItem GetNamedChild(CacheItem root, string segment)
        {
            CacheItem newRoot = root.Children.FirstOrDefault(c => c.Name == segment);

            if (newRoot == null)
            {
                newRoot = new CacheItem {Name = segment, Parent = root, Created = true};
                root.Children.Add(newRoot);
            }

            return newRoot;
        }

        private void Update(CacheItem root, JsonReader reader)
        {
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        CacheItem expando = GetNamedChild(root, reader.Value.ToString());
                        Update(expando, reader);
                        break;
                    case JsonToken.Boolean:
                    case JsonToken.Bytes:
                    case JsonToken.Date:
                    case JsonToken.Float:
                    case JsonToken.Integer:
                    case JsonToken.String:
                        if (root.Created)
                        {
                            root.Value = reader.Value.ToString();
                            OnAdded(new FirebaseValueAddedEventArgs(PathFromRoot(root), reader.Value.ToString()));
                            root.Created = false;
                        }
                        else
                        {
                            string oldData = root.Value;
                            root.Value = reader.Value.ToString();
                            OnUpdated(new FirebaseValueChangedEventArgs(PathFromRoot(root), root.Value, oldData));
                        }

                        return;
                    case JsonToken.Null:
                        // if we're not the root, delete this from the parent
                        if (root.Parent != null)
                        {
                            if (RemoveChildFromParent(root))
                            {
                                OnRemoved(new FirebaseValueRemovedEventArgs(PathFromRoot(root)));
                            }
                        }
                        else
                        {
                            // we just cleared out the root - so delete all
                            // the children one-by-one (so events fire in proper order)
                            // we're modifying the collection, so ToArray
                            foreach (var child in root.Children.ToArray())
                            {
                                RemoveChildFromParent(child);
                                OnRemoved(new FirebaseValueRemovedEventArgs(PathFromRoot(child)));
                            }
                        }
                        return;
                    default:
                        // do nothing
                        break;
                }
            } 
        }

        private bool RemoveChildFromParent(CacheItem child)
        {
            if (child.Parent != null)
            {
                return child.Parent.Children.Remove(child);
            }

            return false;
        }

        // dont' need a lock since access is serialized
        readonly LinkedList<CacheItem> _pathFromRootList = new LinkedList<CacheItem>();
        private string PathFromRoot(CacheItem root)
        {
            // track the sizeso when we allocate our builder we get the right size up front
            int size = 1;

            while(root.Name != null)
            {
                size += root.Name.Length + 1;
                _pathFromRootList.AddFirst(root);
                root = root.Parent;

            }

            if (_pathFromRootList.Count == 0)
            {
                return "/";
            }

            StringBuilder sb = new StringBuilder(size);
            foreach (CacheItem d in _pathFromRootList)
            {
                sb.AppendFormat("/{0}", d.Name);
            }

            _pathFromRootList.Clear();

            return sb.ToString();
        }

        private void OnAdded(FirebaseValueAddedEventArgs args)
        {
            var added = Added;
            if (added != null)
            {
                added(this, args);
            }
        }

        private void OnUpdated(FirebaseValueChangedEventArgs args)
        {
            var updated = Changed;
            if (updated != null)
            {
                updated(this, args);
            }
        }


        private void OnRemoved(FirebaseValueRemovedEventArgs args)
        {
            var removed = Removed;
            if (removed != null)
            {
                removed(this, args);
            }
        }


        public event ValueAddedEventHandler Added;
        public event ValueChangedEventHandler Changed;
        public event ValueRemovedEventHandler Removed;
    }
}
