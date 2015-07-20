using System;
using System.Collections.Generic;
using System.Linq;
using FirebaseSharp.Portable.Interfaces;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable
{
    class DataSnapshot : IDataSnapshot
    {
        private readonly JToken _token;
        internal DataSnapshot(string key, JToken token)
        {
            _token = token != null ? token.DeepClone() : null;
            Key = (key != null) ? key.TrimStart(new char[] {'/'}) : string.Empty;
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
            JToken child = null;

            if (_token != null)
            {
                child =_token.First[childName];
            }

            return new DataSnapshot(childName, child);
        }

        public IEnumerable<IDataSnapshot> Children
        {
            get
            {
                if (_token == null)
                {
                    return new List<IDataSnapshot>();
                }

                return _token.Children().Select(t => new DataSnapshot(t.Path, t));
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
        public IFirebase Ref { get; private set; }
        public IFirebasePriority Priority { get; private set; }
        public string Key { get; private set; }

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

        public string ExportVal { get; private set; }

        private string GetValueString()
        {
            JProperty jp = _token as JProperty;
            if (jp != null)
            {
                return jp.Value.ToString();
            }

            JValue jv = _token as JValue;
            if (jv != null)
            {
                return jv.Value.ToString();
            }

            return _token.ToString();
        }
    }
}
