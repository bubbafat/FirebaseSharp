using System;
using System.Collections.Generic;
using System.Linq;

namespace FirebaseSharp.Portable
{
    internal class FirebasePath : IComparable<FirebasePath>
    {
        private readonly string[] _segments;
        private readonly string _normalized;

        public FirebasePath(string path = null)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                _normalized = string.Empty;
                _segments = new string[0];
            }
            else
            {
                _segments = path.Split(new[] {'/'}, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray();

                _normalized = String.Join("/", _segments);
            }
        }

        public string Path
        {
            get { return "/" + _normalized; }
        }

        public Uri RelativeUri
        {
            get { return new Uri(_normalized, UriKind.Relative); }
        }


        public FirebasePath Child(string child)
        {
            return new FirebasePath(_normalized + "/" + child);
        }

        public FirebasePath Parent()
        {
            if (IsRoot)
            {
                return new FirebasePath();
            }

            return new FirebasePath(string.Join("/", _segments.Take(_segments.Length - 1)));
        }

        public IEnumerable<string> Segments
        {
            get { return _segments; }
        }

        public bool IsRoot
        {
            get { return _segments.Length == 0; }
        }

        public string Key
        {
            get { return _segments.LastOrDefault() ?? "/"; }
        }

        public int CompareTo(FirebasePath other)
        {
            return String.Compare(Path, other.Path, StringComparison.Ordinal);
        }

        protected bool Equals(FirebasePath other)
        {
            return string.Equals(Path, other.Path);
        }

        public override int GetHashCode()
        {
            return Path.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Path == ((FirebasePath) obj).Path;
        }

        public override string ToString()
        {
            return Path;
        }
    }
}