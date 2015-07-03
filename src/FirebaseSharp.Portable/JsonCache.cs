using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable
{
    public delegate void DataChangedHandler(object sender, DataChangedEventArgs e);

    public enum ChangeSource
    {
        Local,
        Remote
    }

    public class DataChangedEventArgs : EventArgs
    {
        public DataChangedEventArgs(ChangeSource source, EventType eventType, OriginalEvent httpMethod, string path, string data, string oldData = null)
        {
            Source = source;
            Path = path;
            Data = data;
            Event = eventType;
            OldData = oldData;
            HttpMethod = httpMethod;
        }

        public ChangeSource Source { get; private set; }

        public EventType Event { get; private set; }
        public string Path { get; private set; }
        public string Data { get; private set; }
        public string OldData { get; private set; }
        public OriginalEvent HttpMethod { get; private set; }
    }

    public enum OriginalEvent
    {
        Put,
        Patch,
        Delete
    }

    public enum EventType
    {
        Added,
        Changed,
        Removed
    }

    /// <summary>
    /// The JsonCache is a single JSON object that represents the state of the 
    /// data as we know it.  As changes come in the object is updated 
    /// and the update communicated via an event.
    /// 
    /// PUT changes the path exactly as directed
    /// PATCH merges the changes (over-writing specifically named children, leaving the rest untouched)
    /// DELETE deletes at the path
    /// </summary>
    class JsonCache
    {
        private JToken _root = null;
        private readonly object _lock = new object();

        public event DataChangedHandler Changed;

        public void Put(ChangeSource source, string path, string data)
        {
            DataChangedEventArgs eventArgs;

            if (data == null)
            {
                Delete(source, path);
                return;
            }

            JToken newData = data.Trim().StartsWith("{")
                ? JToken.Parse(data)
                : new JValue(data);

            lock (_lock)
            {

                JToken found;
                if (TryGetChild(path, out found))
                {
                    JToken old = found.DeepClone();
                    if (!UpdateValues(found, newData))
                    {
                        if (found.Parent != null)
                        {
                            found.Replace(newData);
                        }
                        else
                        {
                            _root = newData;
                        }
                    }

                    eventArgs = new DataChangedEventArgs(source, EventType.Changed, OriginalEvent.Put, path, newData.ToString(), old.ToString());
                }
                else
                {
                    var inserted = InsertAt(path, newData);
                    eventArgs = new DataChangedEventArgs(source, EventType.Added, OriginalEvent.Put, path, inserted.ToString());
                }
            }

            // fire event outside of the lock to avoid re-entrency issues
            OnChanged(eventArgs);
        }

        public void Patch(ChangeSource source, string path, string data)
        {
            DataChangedEventArgs eventArgs;

            if (data == null)
            {
                // PATCH of null is skipped
                return;
            }

            JToken newData = data.Trim().StartsWith("{")
                ? JToken.Parse(data)
                : new JValue(data);

            lock (_lock)
            {
                JToken found;
                if (TryGetChild(path, out found))
                {
                    JToken old = found.DeepClone();
                    if (data.Trim().StartsWith("{"))
                    {
                        Merge(found, newData);
                    }
                    else
                    {
                        if (!UpdateValues(found, newData))
                        {
                            if (found.Parent != null)
                            {
                                found.Replace(newData);
                            }
                            else
                            {
                                _root = newData;
                            }
                        }
                    }

                    eventArgs = new DataChangedEventArgs(source, EventType.Changed, OriginalEvent.Patch, path, found.ToString(), old.ToString());
                }
                else
                {
                    var inserted = InsertAt(path, newData);
                    eventArgs = new DataChangedEventArgs(source, EventType.Added, OriginalEvent.Patch, path, inserted.ToString());
                }
            }
            OnChanged(eventArgs);
        }

        private void Delete(ChangeSource source, string path)
        {
            DataChangedEventArgs eventArgs;

            lock (_lock)
            {
                JToken node;
                if (TryGetChild(path, out node))
                {
                    if (node.Parent != null)
                    {
                        node.Parent.Remove();
                    }
                    else
                    {
                        _root = new JObject();
                    }

                    eventArgs = new DataChangedEventArgs(source, EventType.Removed, OriginalEvent.Put, path, null);
                }
                else
                {
                    return;
                }
            }

            OnChanged(eventArgs);
        }

        internal string Dump()
        {
            lock (_lock)
            {
                return _root.ToString(Formatting.None);
            }
        }

        private bool UpdateValues(JToken oldToken, JToken newToken)
        {
            JValue oldVal = oldToken as JValue;
            JValue newVal = newToken as JValue;

            if (oldVal != null && newVal != null)
            {
                oldVal.Value = newVal.Value;
                return true;
            }

            return false;
        }
        private JToken InsertAt(string path, JToken newData)
        {
            string[] segments = NormalizePath(path).Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length > 0)
            {
                if (_root == null)
                {
                    _root = new JObject();
                }

                var node = _root;

                for (int i = 0; i < segments.Length - 1; i++)
                {
                    string segment = segments[i];
                    var child = node[segment];
                    if (child == null)
                    {
                        node[segment] = new JObject();
                        node = node[segment];
                    }
                    else
                    {
                        node = child;
                    }
                }

                node[segments[segments.Length - 1]] = newData;
                return node[segments[segments.Length - 1]];
            }
            else
            {
                _root = newData;
                return _root;
            }
        }

        private void Merge(JToken target, JToken newData)
        {
            foreach (var newChildPath in newData.Children())
            {
                var existingTarget = target[newChildPath.Path];
                var newChild = newData[newChildPath.Path];

                // a PATCH of a null object is skipped
                // use PUT to delete
                if (newChild.Type == JTokenType.Null)
                {
                    continue;
                }

                JValue existingValue = existingTarget as JValue;

                if (existingValue != null)
                {
                    JValue newValue = newChild as JValue;
                    if (newValue != null)
                    {
                        existingValue.Replace(newValue);
                        continue;
                    }
                }
                
                target[newChild.Path] = newChild;
            }
        }

        private bool TryGetChild(string path, out JToken node)
        {
            string[] segments = NormalizePath(path).Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);

            node = _root;

            if (node != null)
            {
                foreach (var segment in segments)
                {
                    node = node[segment];
                    if (node == null)
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private static string NormalizePath(string path)
        {
            return path.TrimStart(new char[] { '/' }).Trim().Replace('/', '.');
        }

        private void OnChanged(DataChangedEventArgs args)
        {
            var handler = Changed;
            if (handler != null)
            {
                handler(this, args);
            }
        }
    }
}
