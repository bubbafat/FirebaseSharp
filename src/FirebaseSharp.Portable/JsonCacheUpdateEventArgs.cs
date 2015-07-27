using System;

namespace FirebaseSharp.Portable
{
    internal class JsonCacheUpdateEventArgs : EventArgs
    {
        public JsonCacheUpdateEventArgs(FirebasePath path)
        {
            Path = path;
        }

        public FirebasePath Path { get; private set; }
    }
}