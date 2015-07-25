using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable
{
    class DataSnapshot : IDataSnapshot
    {
        private readonly JToken _token;
        private readonly FirebaseApp _app;
        private readonly FirebasePath _path;
        internal DataSnapshot(FirebaseApp app, FirebasePath path, JToken token)
        {
            _token = token != null ? token.DeepClone() : null;
            _app = app;
            _path = path;
        }

        internal JToken Token
        {
            get { return _token; }
        }

        public bool Exists
        {
            get { return _token != null; }
        }
        public IDataSnapshot Child(string childName)
        {
            JToken child = _token;

            if (_token != null)
            {
                foreach (string childPath in new FirebasePath(childName).Segments)
                {
                    if (child == null)
                    {
                        break;
                    }

                    switch (child.Type)
                    {
                        case JTokenType.Property:
                            JProperty prop = (JProperty) child;
                            child = prop.Name == childPath ? prop : null;
                            break;
                        case JTokenType.Object:
                            child = child[childPath];
                            break;
                    }
                }
            }

            return new DataSnapshot(_app, _path.Child(childName), child);
        }

        public IEnumerable<IDataSnapshot> Children
        {
            get
            {
                List<IDataSnapshot> snaps = new List<IDataSnapshot>();

                if (_token != null)
                {

                    if (_token.Type == JTokenType.Array)
                    {
                        // arrays are keyed as "[0]" but we want them keyed as "0"
                        // so int based look-ups are easier
                        JArray array = (JArray) _token;
                        snaps.AddRange(array.Select((t, i) => new DataSnapshot(_app, _path.Child(i.ToString()), t)).Cast<IDataSnapshot>());
                    }
                    else if(_token.Type == JTokenType.Property)
                    {
                        JProperty prop = (JProperty) _token;
                        snaps.AddRange(_token.Children().Select(t => new DataSnapshot(_app, _path.Child(t.Path), prop.Value)));
                    }
                    else
                    {
                        snaps.AddRange(_token.Children().Select(t =>
                        {
                            JProperty prop = (JProperty) t;
                            return new DataSnapshot(_app, _path.Child(prop.Name), prop.Value);
                        }));
                    }
                }

                return snaps;
            }
        }

        public bool HasChildren
        {
            get
            {
                return (_token != null) && _token.Children().Any();
            }
        }

        public int NumChildren
        {
            get
            {
                return (_token == null) ? 0 : _token.Children().Count();
            }
        }

        public IFirebase Ref()
        {
            return new Firebase(_app, _path);
        }

        public FirebasePriority GetPriority()
        {
            if (_token == null)
            {
                return null;
            }

            if (_token.Type == JTokenType.Object)
            {
                JObject obj = (JObject) _token;
                var pri = obj[".priority"];
                if (pri != null)
                {
                    return new FirebasePriority((JValue)pri);
                }
            }

            if (_token.Type == JTokenType.Property)
            {
                JProperty prop = (JProperty) _token;
                if (prop.Name == ".priority")
                {
                    if (prop.Value.Type is JValue)
                    {
                        return new FirebasePriority((JValue)prop.Value);
                    }
                    else
                    {
                        throw new Exception("priority was set to non-value type.");
                    }
                }
            }

            return null;
        }
        public string Key { get { return _path.Key; } }

        public T Value<T>() where T: struct
        {
            string value = GetValueString();

            return (T)Convert.ChangeType(value, typeof(T));
        }

        public string Value()
        {
            return GetValueString();
        }

        public IDataSnapshot this[string child]
        {
            get { return Child(child); }
        }

        public string ExportVal()
        {
            // TODO: trim priority
            return Value();
        }

        private string GetValueString()
        {
            JProperty jp = _token as JProperty;
            if (jp != null && jp.Type != JTokenType.Null)
            {
                return jp.Value.ToString();
            }

            JValue jv = _token as JValue;
            if (jv != null && jv.Type != JTokenType.Null)
            {
                return jv.Value.ToString();
            }

            if (_token == null)
            {
                return "null";
            }

            return _token.ToString();
        }
    }
}
