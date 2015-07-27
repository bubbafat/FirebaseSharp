using System;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable
{
    public enum PriorityType
    {
        None,
        String,
        Numeric
    }

    public class FirebasePriority
    {
        public PriorityType Type { get; private set; }
        private readonly float? _fp;
        private readonly string _sp;

        internal FirebasePriority(JValue priority)
        {
            if (priority == null || priority.Type == JTokenType.Null)
            {
                Type = PriorityType.None;
                return;
            }

            switch (priority.Type)
            {
                case JTokenType.None:
                    Type = PriorityType.None;
                    return;
                case JTokenType.Integer:
                case JTokenType.Float:
                    Type = PriorityType.Numeric;
                    _fp = priority.Value<float>();
                    return;
                case JTokenType.String:
                    int value;
                    if (int.TryParse(priority.Value<string>(), out value))
                    {
                        Type = PriorityType.Numeric;
                        _fp = value;
                    }
                    else
                    {
                        Type = PriorityType.String;
                        _sp = priority.Value<string>();
                    }
                    return;
                default:
                    throw new Exception(string.Format("Unable to load priority of type: {0}", priority.Type));
            }
        }

        public FirebasePriority(string priority)
        {
            if (priority == null)
            {
                Type = PriorityType.None;
            }
            else
            {
                Type = PriorityType.String;
                _sp = priority;
            }
        }

        public FirebasePriority(float priority)
        {
            Type = PriorityType.Numeric;
            _fp = priority;
        }

        public FirebasePriority(long priority)
        {
            Type = PriorityType.Numeric;
            _fp = priority;
        }


        /*
         * Children with no priority (the default) come first.
         * Children with a number as their priority come next. 
         *  They are sorted numerically by priority, small to large.
         * Children with a string as their priority come last. 
         *  They are sorted lexicographically by priority.
         * Whenever two children have the same priority (including no priority), 
         *  they are sorted by key. Numeric keys come first (sorted numerically), 
         *  followed by the remaining keys (sorted lexicographically).
         */

        public int CompareTo(FirebasePriority other)
        {
            // Children with no priority (the default) come first.
            if (Type == PriorityType.None)
            {
                if (other.Type == PriorityType.None)
                {
                    return 0;
                }

                return -1;
            }
            else if (other.Type == PriorityType.None)
            {
                return 1;
            }

            // Children with a number as their priority come next. 
            if (Type == PriorityType.Numeric)
            {
                if (other.Type == PriorityType.Numeric)
                {
                    if (_fp.HasValue && other._fp.HasValue)
                    {
                        return _fp.Value.CompareTo(other._fp.Value);
                    }

                    throw new Exception("Priority is a numeric but value not set");
                }

                return -1;
            }
            else if (other.Type == PriorityType.Numeric)
            {
                return 1;
            }

            // Children with a string as their priority come last.
            if (Type == PriorityType.String)
            {
                if (other.Type == PriorityType.String)
                {
                    if (_sp != null && other._sp != null)
                    {
                        return String.Compare(_sp, other._sp, StringComparison.Ordinal);
                    }

                    throw new Exception("Priority is a string but value not set");
                }

                // will throw in a moment in release builds
                Debug.Assert(false, "It should not be possible for both to not be strings at this point");
            }
            else if (other.Type == PriorityType.String)
            {
                return 1;
            }

            throw new Exception("Priority sorting did not detect a valid state");
        }

        public string JsonValue
        {
            get
            {
                switch (Type)
                {
                    case PriorityType.None:
                        return "null";
                    case PriorityType.Numeric:
                        return _fp.ToString();
                    case PriorityType.String:
                        return string.Format("\"{0}\"", _sp);
                    default:
                        throw new InvalidOperationException("Unknown format type: {0}" + Type);
                }
            }
        }

        public string Value
        {
            get
            {
                switch (Type)
                {
                    case PriorityType.None:
                        return null;
                    case PriorityType.Numeric:
                        return _fp.ToString();
                    case PriorityType.String:
                        return _sp;
                    default:
                        throw new InvalidOperationException("Unknown format type: {0}" + Type);
                }
            }
        }

        public override string ToString()
        {
            return Value;
        }
    }
}