using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FirebaseSharp.Portable
{
    internal class CacheItem
    {
        public string Name;
        public string Value;
        public CacheItem Parent;
        public List<CacheItem> Children = new List<CacheItem>();
        public bool Created;
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
            string[] segments = path.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

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
                            string oldData = root.Value.ToString();
                            root.Value = reader.Value.ToString();
                            OnUpdated(new FirebaseValueChangedEventArgs(PathFromRoot(root), root.Value.ToString(),
                                oldData));
                        }

                        return;
                    case JsonToken.Null:
                        if (root.Parent != null)
                        {
                            if (RemoveChildFromParent(root))
                            {
                                OnRemoved(new FirebaseValueRemovedEventArgs(PathFromRoot(root)));
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

        private string PathFromRoot(CacheItem root)
        {
            LinkedList<CacheItem> inOrder = new LinkedList<CacheItem>();

            while(root.Name != null)
            {
                inOrder.AddFirst(root);
                root = root.Parent;

            }

            if (inOrder.Count == 0)
            {
                return "/";
            }

            StringBuilder sb = new StringBuilder();
            foreach (CacheItem d in inOrder)
            {
                sb.AppendFormat("/{0}", d.Name);
            }

            return sb.ToString();
        }

        private void ReadValue(CacheItem expando, JsonReader reader)
        {
            expando.Value = reader.ReadAsString();
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
