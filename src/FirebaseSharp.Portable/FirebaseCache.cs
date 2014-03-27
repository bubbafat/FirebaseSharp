using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FirebaseSharp.Portable
{
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
        private readonly dynamic _tree = new ExpandoObject();
        private readonly object _treeLock = new object();

        public FirebaseCache()
        {
            _tree.name = string.Empty;
            _tree.deleted = false;
            _tree.created = false;
            _tree.parent = null;
            _tree.name = null;
        }

        public void Update(string path, JsonReader data)
        {
            lock (_treeLock)
            {
                dynamic root = FindRoot(path);
                Update(root, data);
            }
        }

        private ExpandoObject FindRoot(string path)
        {
            string[] segments = path.Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);

            dynamic root = _tree;

            foreach (string segment in segments)
            {
                root = GetNamedChild(root, segment);
            }

            return root;
        }
    

        private static dynamic GetNamedChild(dynamic root, string segment)
        {
            IDictionary<string, object> dict = (IDictionary<string, object>) root;

            dynamic newRoot;
            object child;

            if (!dict.TryGetValue(segment, out child))
            {
                newRoot = new ExpandoObject();
                newRoot.name = segment;
                newRoot.parent = root;
                newRoot.created = true;
                dict.Add(segment, newRoot);
            }
            else
            {
                newRoot = child as ExpandoObject;
            }

            return newRoot;
        }

        private void Update(dynamic root, JsonReader reader)
        {
            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        dynamic expando = GetNamedChild(root, reader.Value.ToString());
                        Update(expando, reader);
                        break;
                    case JsonToken.Boolean:
                    case JsonToken.Bytes:
                    case JsonToken.Date:
                    case JsonToken.Float:
                    case JsonToken.Integer:
                    case JsonToken.String:
                        if (root.created)
                        {
                            root.value = reader.Value;
                            OnAdded(new FirebaseValueAddedEventArgs(PathFromRoot(root), reader.Value.ToString()));
                            root.created = false;
                        }
                        else
                        {
                            string oldData = root.value.ToString();
                            root.value = reader.Value;
                            OnUpdated(new FirebaseValueChangedEventArgs(PathFromRoot(root), root.value.ToString(),
                                oldData));
                        }

                        return;
                    case JsonToken.Null:
                        if (root.parent != null)
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

        private bool RemoveChildFromParent(dynamic child)
        {
            if (child.parent != null)
            {
                return ((IDictionary<string, object>)child.parent).Remove(child.name);
            }

            return false;
        }

        private string PathFromRoot(dynamic root)
        {
            LinkedList<dynamic> inOrder = new LinkedList<dynamic>();

            while(root.name != null)
            {
                inOrder.AddFirst(root);
                root = root.parent;

            }

            if (inOrder.Count == 0)
            {
                return "/";
            }

            StringBuilder sb = new StringBuilder();
            foreach (dynamic d in inOrder)
            {
                sb.AppendFormat("/{0}", d.name);
            }

            return sb.ToString();
        }

        private void ReadValue(dynamic expando, JsonReader reader)
        {
            expando.value = reader.ReadAsString();
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
