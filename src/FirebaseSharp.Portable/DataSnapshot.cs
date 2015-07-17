using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseSharp.Portable.Interfaces;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable
{
    class DataSnapshot : IDataSnapshot
    {
        private readonly JToken _token;
        internal DataSnapshot(JToken token)
        {
            _token = token != null ? token.DeepClone() : null;
        }

        internal JToken Token
        {
            get { return _token; }
        }

        public bool Exists
        {
            get { return _token != null; }
        }
        public IDataSnapshot Child(string path)
        {
            JToken child = null;

            if (_token != null)
            {
                child =_token[path];
            }

            return new DataSnapshot(child);
        }

        public IEnumerable<IDataSnapshot> Children
        {
            get
            {
                if (_token == null)
                {
                    return new List<IDataSnapshot>();
                }

                return _token.Children().Select(t => new DataSnapshot(t));
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

        public string Value
        {
            get { return (_token == null) ? string.Empty : _token.ToString(); }
        }
        public string ExportVal { get; private set; }
    }
}
