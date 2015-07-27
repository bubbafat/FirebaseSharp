using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Set changes the path exactly as directed
    /// Update merges the changes (over-writing specifically named children, leaving the rest untouched)
    /// Delete by Set or Update with a NULL data member.
    /// 
    /// Thoughts on the future...
    /// 
    /// What's the deal with syncing the tree.  Should we just apply deltas as they come in,
    /// in the order they arrive, or should we look at the tree as a whole and perform diffing
    /// against another tree?  This would require things like dirty flags, etc - but would make synching
    /// from a saved state possible (but is it a good idea?)
    /// Would JSON Patch make sense?  It's basically what the REST APIs are sending anyway.
    /// How should conflicts be handled?
    /// </summary>
    internal class SyncDatabase : IDisposable
    {
        private JObject _root;
        private bool _initialReceive;
        private readonly object _lock = new object();
        private readonly IFirebaseNetworkConnection _connection;
        private readonly FirebasePushIdGenerator _idGenerator = new FirebasePushIdGenerator();
        private readonly Queue<Subscription> _initialSubscriptions = new Queue<Subscription>();
        private readonly FirebaseApp _app;

        public SyncDatabase(FirebaseApp app, IFirebaseNetworkConnection connection)
        {
            _app = app;
            _root = new JObject();

            _connection = connection;
            _connection.Received += ConnectionOnReceived;
        }

        public EventHandler<JsonCacheUpdateEventArgs> Changed;

        internal DataSnapshot SnapFor(FirebasePath path)
        {
            lock (_lock)
            {
                JToken token;

                if (TryGetChild(path, out token))
                {
                    return new DataSnapshot(_app, path, token);
                }

                return new DataSnapshot(_app, null, null);
            }
        }

        private void ConnectionOnReceived(object sender, FirebaseEventReceivedEventArgs e)
        {
            UpdateLocal(e.Message);
            DrainInitialQueue();
        }

        private void DrainInitialQueue()
        {
            bool runDrain = false;
            if (false == _initialReceive)
            {
                lock (_lock)
                {
                    if (false == _initialReceive)
                    {
                        _initialReceive = true;
                        runDrain = true;
                    }
                }
            }

            if (runDrain)
            {
                while (_initialSubscriptions.Any())
                {
                    var sub = _initialSubscriptions.Dequeue();
                    sub.Process(this);
                }
            }
        }

        private JToken CreateToken(string value)
        {
            return value.Trim().StartsWith("{")
                ? JToken.Parse(value)
                : new JValue(value);
        }

        public void Set(FirebasePath path, object data, FirebaseStatusCallback callback)
        {
            string strData = JToken.FromObject(data).ToString(Formatting.None);
            Set(path, strData, callback);
        }

        public void Set(FirebasePath path, string data, FirebaseStatusCallback callback)
        {
            Set(path, data, null, callback, MessageSouce.Local);
        }

        private void Set(FirebasePath path, string data, FirebasePriority priority, FirebaseStatusCallback callback,
            MessageSouce source)
        {
            var message = new FirebaseMessage(WriteBehavior.Replace, path, data, priority, callback, source);

            UpdateLocal(message);
        }

        private void UpdateLocal(FirebaseMessage message)
        {
            if (message.Value == null)
            {
                Delete(message.Path);
            }
            else
            {
                JToken newData = CreateToken(message.Value);


                if (message.Behavior == WriteBehavior.Merge)
                {
                    Merge(message.Path, newData);
                }
                else
                {
                    if (message.Behavior == WriteBehavior.Replace)
                    {
                        if (message.Priority != null)
                        {
                            newData[".priority"] = new JValue(message.Priority.JsonValue);
                        }
                    }

                    InsertAt(message.Path, newData);
                }
            }

            OnChanged(message);

            if (message.Source == MessageSouce.Local)
            {
                QueueUpdate(message);
            }
        }

        private void QueueUpdate(FirebaseMessage firebaseMessage)
        {
            _connection.Send(firebaseMessage);
        }

        public string Push(FirebasePath path, object data, FirebaseStatusCallback callback)
        {
            string json = null;
            if (data != null)
            {
                json = JToken.FromObject(data).ToString();
            }

            return Push(path, json, callback);
        }


        public string Push(FirebasePath path, string data, FirebaseStatusCallback callback)
        {
            string childPath = _idGenerator.Next();

            if (data != null)
            {
                Set(path.Child(childPath), data, callback);
            }

            return childPath;
        }

        public void Update(FirebasePath path, string data, FirebaseStatusCallback callback)
        {
            var message = new FirebaseMessage(WriteBehavior.Merge, path, data, callback, MessageSouce.Local);

            UpdateLocal(message);
        }

        private void OnChanged(FirebaseMessage message)
        {
            var callback = Changed;
            if (callback != null)
            {
                callback(this, new JsonCacheUpdateEventArgs(message.Path));
            }
        }

        private void Delete(FirebasePath path)
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

        private void InsertAt(FirebasePath path, JToken newData)
        {
            // if there is aleady a node at the path, delete it
            Delete(path);

            string[] segments = path.Segments.ToArray();

            if (segments.Length > 0)
            {
                JToken node = _root;

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

                string newPath = segments[segments.Length - 1];
                JObject newObj = node as JObject;
                if (newObj == null)
                {
                    throw new Exception("Node object must be a JSON object type - illegal type: " + newData.Type);
                }

                if (newData is JProperty)
                {
                    newObj.Add(newData);
                }
                else
                {
                    newObj[newPath] = newData;
                }
            }
            else
            {
                JObject newObj = newData as JObject;
                if (newObj == null)
                {
                    throw new Exception("Root object must be a JSON object type - illegal type: " + newData.Type);
                }

                _root = newObj;
            }
        }

        private void Merge(FirebasePath root, JToken newData)
        {
            JToken found;
            if (TryGetChild(root, out found))
            {
                if (!UpdateValues(found, newData))
                {
                    foreach (var newChild in newData.Children())
                    {
                        if (DeleteFromTarget(root, newData, newChild.Path))
                        {
                            continue;
                        }

                        InsertAt(root.Child(newChild.Path), newChild);
                    }
                }
            }
            else
            {
                InsertAt(root, newData);
            }
        }

        private bool DeleteFromTarget(FirebasePath targetPath, JToken newData, string path)
        {
            if (newData[path].Type == JTokenType.Null)
            {
                Delete(targetPath.Child(path));
                return true;
            }

            return false;
        }

        private bool TryGetChild(FirebasePath path, out JToken node)
        {
            node = _root;

            if (node != null)
            {
                foreach (var segment in path.Segments)
                {
                    if (!node.Children().Any())
                    {
                        return false;
                    }

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

        internal void GoOnline()
        {
            _connection.Connect();
        }

        internal void GoOffline()
        {
            _connection.Disconnect();
        }

        public void Dispose()
        {
            using (_connection)
            {
            }
        }

        internal void ExecuteInitial(Subscription sub)
        {
            lock (_lock)
            {
                if (!_initialReceive)
                {
                    _initialSubscriptions.Enqueue(sub);
                    return;
                }
            }

            sub.Process(this);
        }

        internal void SetPriority(FirebasePath path, FirebasePriority priority, FirebaseStatusCallback callback)
        {
            Set(path.Child(".priority"), priority.JsonValue, callback);
        }

        internal void SetWithPriority(FirebasePath path, string value, FirebasePriority priority,
            FirebaseStatusCallback callback)
        {
            Set(path, value, priority, callback, MessageSouce.Local);
        }
    }
}