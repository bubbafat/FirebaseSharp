using System;

namespace FirebaseSharp.Portable
{
    class JsonCacheUpdateEventArgs : EventArgs
    {
        public JsonCacheUpdateEventArgs(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }
    }
}