using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data.OleDb;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.SqlServer.Server;
using Newtonsoft.Json.Linq;

namespace JsonCache
{
    internal delegate void DataChangedHandler(object sender, DataChangedEventArgs e);

    enum ChangeSource
    {
        Local,
        Remote
    }

    internal class DataChangedEventArgs : EventArgs
    {
        public DataChangedEventArgs(ChangeSource source, EventType eventType, string path, string data)
        {
            Source = source;
            Path = path;
            Data = data;
            Event = eventType;
        }

        public ChangeSource Source { get; private set; }

        public EventType Event { get; private set; }
        public string Path { get; private set; }
        public string Data { get; private set; }
    }

    enum EventType
    {
        Added,
        Changed,
        Removed
    }

    class JsonCache
    {
        private JToken _root = null;

        public event DataChangedHandler Changed;

        public void Put(ChangeSource source, string path, string data)
        {
            if (data == null)
            {
                Delete(source, path);
                return;
            }

            JToken newData = data.Trim().StartsWith("{")
                ? JToken.Parse(data)
                : new JValue(data);

            JToken found;
            if (TryGetChild(path, out found))
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

                OnChanged(new DataChangedEventArgs(source, EventType.Changed, path, newData.ToString()));
            }
            else
            {
                var inserted = InsertAt(path, newData);
                OnChanged(new DataChangedEventArgs(source, EventType.Added, path, inserted.ToString()));
            }
        }

        public void Patch(ChangeSource source, string path, string data)
        {
            if (data == null)
            {
                Delete(source, path);
                return;
            }

            JToken newData = data.Trim().StartsWith("{")
                ? JToken.Parse(data)
                : new JValue(data);

            JToken found;
            if (TryGetChild(path, out found))
            {
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

                OnChanged(new DataChangedEventArgs(source, EventType.Changed, path, found.ToString()));
            }
            else
            {
                var inserted = InsertAt(path, newData);
                OnChanged(new DataChangedEventArgs(source, EventType.Added, path, inserted.ToString()));
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

                if (existingTarget != null)
                {
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
                }

                target[newChild.Path] = newChild;
            }
        }

        public void Delete(ChangeSource source, string path)
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
                    _root = JToken.Parse("{}");
                }

                OnChanged(new DataChangedEventArgs(source, EventType.Removed, path, null));
            }
        }

        private bool TryGetChild(string path, out JToken node)
        {
            string[] segments = NormalizePath(path).Split(new[] {'.'}, StringSplitOptions.RemoveEmptyEntries);

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
            return path.TrimStart(new char[] {'/'}).Trim().Replace('/', '.');
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


    

    class Program
    {
        static void Main(string[] args)
        {
            JsonCache cache = new JsonCache();

            cache.Changed += (sender, eventArgs) =>
            {
                Console.WriteLine("{0} {1}: {2}", eventArgs.Path, eventArgs.Event, eventArgs.Data);
            };

            cache.Put(ChangeSource.Remote, "/", "{\"people\": { \"me\": { \"name\": \"Robert Horvick\", \"age\": 38, \"home\": true }, \"you\": { \"name\": \"Susan Horvick\", \"age\": 39, \"home\": true }}}");
            cache.Patch(ChangeSource.Local, "/people/me", "{\"name\": \"Robert Pry Horvick\"}");
            cache.Patch(ChangeSource.Local, "/people/me/name", "Robert P. Horvick");
            cache.Patch(ChangeSource.Local, "/people/me", null);
            cache.Patch(ChangeSource.Local, "/people/you/name", null);


            // hold a master copy
            // fire events when data changes
            // understand the difference between local and server changes
        }
    }
}
