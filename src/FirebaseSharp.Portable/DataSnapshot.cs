using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable
{
    internal class DataSnapshot : IDataSnapshot
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
            JToken next = _token;

            if (next != null)
            {
                foreach (string childPath in new FirebasePath(childName).Segments)
                {
                    switch (next.Type)
                    {
                        case JTokenType.Property:
                            JProperty prop = (JProperty) next;
                            next = prop.Name == childPath ? prop : null;
                            break;
                        case JTokenType.Object:
                            next = next[childPath];
                            break;
                        default:
                            next = null;
                            break;
                    }

                    if (next == null)
                    {
                        break;
                    }
                }
            }

            return new DataSnapshot(_app, _path.Child(childName), next);
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
                        snaps.AddRange(
                            array.Select((t, i) => new DataSnapshot(_app, _path.Child(i.ToString()), t)));
                    }
                    else if (_token.Type == JTokenType.Property)
                    {
                        JProperty prop = (JProperty) _token;
                        snaps.AddRange(
                            _token.Children().Select(t => new DataSnapshot(_app, _path.Child(t.Path), prop.Value)));
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
            get { return (_token != null) && _token.Children().Any(); }
        }

        public int NumChildren
        {
            get { return (_token == null) ? 0 : _token.Children().Count(); }
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
                    return new FirebasePriority((JValue) pri);
                }
            }

            if (_token.Type == JTokenType.Property)
            {
                JProperty prop = (JProperty) _token;
                if (prop.Name == ".priority")
                {
                    var value = prop.Value as JValue;
                    if (value != null)
                    {
                        return new FirebasePriority(value);
                    }
                    else
                    {
                        throw new Exception("priority was set to non-value type.");
                    }
                }
            }

            return null;
        }

        public string Key
        {
            get { return _path.Key; }
        }

        public T Value<T>()
        {
            return JsonConvert.DeserializeObject<T>(GetValueString());
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