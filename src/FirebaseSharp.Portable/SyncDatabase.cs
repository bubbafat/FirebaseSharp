using System;
using System.Diagnostics;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;
using FirebaseSharp.Portable.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable
{
    /// <summary>
    /// The SyncDatabase is a single JSON object that represents the state of the 
    /// data as we know it.  As changes come in the object is updated 
    /// and the update communicated via an event.
    /// 
    /// PUT changes the path exactly as directed
    /// PATCH merges the changes (over-writing specifically named children, leaving the rest untouched)
    /// DELETE deletes at the path
    /// </summary>
    class SyncDatabase
    {
        private JToken _root;
        private readonly object _lock = new object();
        private readonly IFirebaseNetworkConnection _connection;
        private readonly FirebasePushIdGenerator _idGenerator = new FirebasePushIdGenerator();

        public SyncDatabase(IFirebaseNetworkConnection connection)
        {
            _root = new JObject();

            _connection = connection;
            _connection.Received += ConnectionOnReceived;
        }

        private void ConnectionOnReceived(object sender, FirebaseEventReceivedEventArgs e)
        {
            switch (e.Message.Behavior)
            {
                case WriteBehavior.Replace:
                    Set(e.Message);
                    break;
                case WriteBehavior.Merge:
                    Update(e.Message);
                    break;
            }
        }


        private JToken CreateToken(string value)
        {
            return value.Trim().StartsWith("{")
                ? JToken.Parse(value)
                : new JValue(value);
        }

        public void Set(string path, string data, FirebaseStatusCallback callback)
        {
            var message = new FirebaseMessage(WriteBehavior.Replace, path, data, callback);
            Set(message);
            QueueUpdate(message);
        }

        private void Set(FirebaseMessage message)
        {
            if (message.Value == null)
            {
                Delete(message.Path);
            }
            else
            {
                JToken newData = CreateToken(message.Value);
                lock (_lock)
                {
                    JToken found;
                    if (TryGetChild(message.Path, out found))
                    {
                        Replace(found, newData);
                    }
                    else
                    {
                        InsertAt(message.Path, newData);
                    }
                }
            }
            
        }

        private async void QueueUpdate(FirebaseMessage firebaseMessage)
        {
            await _connection.Send(firebaseMessage).ContinueWith((t) =>
            {
                if (firebaseMessage.Callback != null)
                {
                    FirebaseError error = null;
                    if (t.Exception != null)
                    {
                        error = new FirebaseError(t.Exception.Message);
                    }

                    firebaseMessage.Callback(error);
                }
            });
        }

        private void Replace(JToken found, JToken newData)
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

        public string Push(string path, string data, FirebaseStatusCallback callback)
        {
            string newPath = CreatePushPath(path);
            Set(newPath, data, callback);

            return newPath;
        }

        private string CreatePushPath(string path)
        {
            string id = _idGenerator.Next();
            return path + "/" + id;
        }

        public void Update(string path, string data, FirebaseStatusCallback callback)
        {
            var message = new FirebaseMessage(WriteBehavior.Merge, path, data, callback);
            Update(message);
            QueueUpdate(message);
        }

        private void Update(FirebaseMessage message)
        {
            if (message.Value == null)
            {
                Delete(message.Path);
            }
            else
            {
                JToken newData = CreateToken(message.Value);

                lock (_lock)
                {
                    JToken found;
                    if (TryGetChild(message.Path, out found))
                    {
                        Merge(found, newData);
                    }
                    else
                    {
                        InsertAt(message.Path, newData);
                    }
                }
            }            
        }

        private void Delete(string path)
        {
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
                }
            }
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
            if (!UpdateValues(target, newData))
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
    }
}
