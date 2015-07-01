using System.Collections.Generic;

namespace FirebaseSharp.Portable.Cache
{
    internal class CacheItem
    {
        private List<CacheItem> _children;

        public string Name { get; set; }
        public string Value { get; set; }
        public CacheItem Parent { get; set; }
        public bool Created { get; set; }

        public List<CacheItem> Children
        {
            get
            {
                // we don't need a lock here because the tree is already 
                // synchronized so there will never be concurrent requests
                if (_children == null)
                {
                    _children = new List<CacheItem>();
                }

                return _children;
            }
        }
    }
}