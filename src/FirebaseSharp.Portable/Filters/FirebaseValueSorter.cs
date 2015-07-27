using System;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json.Linq;

namespace FirebaseSharp.Portable.Filters
{
    /// <summary>
    /// Implements the orderByChild sorting rules described here:
    /// https://www.firebase.com/docs/web/guide/retrieving-data.html#section-ordered-data
    /// </summary>
    internal class FirebaseValueSorter : Comparer<JToken>
    {
        public override int Compare(JToken x, JToken y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            // the order is really important here
            // by the time we're checking strings we need to know that
            // neither is a null, boolean or numeric - each successive
            // test becomes easier because the possible implicit conversions
            // (e.g., string to int) was resolved by the target type (int)
            // handler.

            int result;
            if (TryNullTests(x, y, out result))
            {
                Debug.Assert(result != int.MinValue);
                return result;
            }

            // use paths, not values, for object equality
            // since we might be dealing with cloned objects
            if (x.Path == y.Path)
            {
                return 0;
            }

            if (TryBooleanTests(x, y, out result))
            {
                Debug.Assert(result != int.MinValue);
                return result;
            }

            if (TryNumericTests(x, y, out result))
            {
                Debug.Assert(result != int.MinValue);
                return result;
            }

            if (TryStringTests(x, y, out result))
            {
                Debug.Assert(result != int.MinValue);
                return result;
            }

            result = ParentKeyTest(x, y);
            Debug.Assert(result != int.MinValue);

            return result;
        }

        private string KeyName(JToken x)
        {
            if (x.Parent != null)
            {
                return x.Parent.Path;
            }

            return null;
        }

        private int ParentKeyTest(JToken x, JToken y)
        {
            string xkey = KeyName(x);
            string ykey = KeyName(y);

            if (xkey == null)
            {
                return ykey == null ? 0 : 1;
            }

            if (ykey == null)
            {
                return -1;
            }

            return String.Compare(xkey, ykey, StringComparison.Ordinal);
        }

        private bool TryStringTests(JToken x, JToken y, out int result)
        {
            if (x.Type == JTokenType.String)
            {
                if (y.Type != JTokenType.String)
                {
                    result = -1;
                    return true;
                }

                int order = String.Compare(x.Value<string>(), y.Value<string>(), StringComparison.Ordinal);
                result = order == 0 ? ParentKeyTest(x, y) : order;

                return true;
            }

            if (y.Type == JTokenType.String)
            {
                result = 1;
                return true;
            }

            result = int.MinValue;
            return false;
        }

        private bool IsNumeric(JToken x)
        {
            return x.Type == JTokenType.Integer || x.Type == JTokenType.Float;
        }

        private int SortNumeric(JToken x, JToken y)
        {
            if (x.Type == JTokenType.Integer && x.Type == JTokenType.Integer)
            {
                return x.Value<int>().CompareTo(y.Value<int>());
            }

            // if one is a float, compare them both as floats
            return x.Value<float>().CompareTo(y.Value<float>());
        }

        private bool TryNumericTests(JToken x, JToken y, out int result)
        {
            if (IsNumeric(x))
            {
                if (!IsNumeric(y))
                {
                    result = -1;
                    return true;
                }

                int sorted = SortNumeric(x, y);
                result = sorted == 0 ? ParentKeyTest(x, y) : sorted;
                return true;
            }

            if (IsNumeric(y))
            {
                result = 1;
                return true;
            }

            result = int.MinValue;
            return false;
        }

        private bool TryBooleanTests(JToken x, JToken y, out int result)
        {
            if (x.Type == JTokenType.Boolean)
            {
                if (y.Type != JTokenType.Boolean)
                {
                    result = -1;
                    return true;
                }

                if (x.Value<bool>() == y.Value<bool>())
                {
                    result = ParentKeyTest(x, y);
                }
                else
                {
                    result = x.Value<bool>() ? 1 : -1;
                }

                return true;
            }

            if (y.Type == JTokenType.Boolean)
            {
                result = 1;
                return true;
            }

            result = int.MinValue;
            return false;
        }

        private bool TryNullTests(JToken x, JToken y, out int result)
        {
            if (x == null && y == null)
            {
                result = 0;
                return true;
            }

            if (x == null || y == null)
            {
                result = (y == null) ? -1 : 1;
                return true;
            }

            if (x.Type == JTokenType.Null)
            {
                if (y.Type != JTokenType.Null)
                {
                    result = -1;
                }
                else
                {
                    result = ParentKeyTest(x, y);
                }
                return true;
            }

            if (y.Type == JTokenType.Null)
            {
                result = 1;
                return true;
            }

            result = int.MinValue;
            return false;
        }
    }
}