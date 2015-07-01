using System;

namespace FirebaseSharp.Portable.Cache
{
    public class ValueAddedEventArgs : EventArgs
    {
        public ValueAddedEventArgs(string path, string data)
        {
            Path = path;
            Data = data;
        }

        public string Path { get; private set; }
        public string Data { get; private set; }
    }

    public class ValueChangedEventArgs : EventArgs
    {
        public ValueChangedEventArgs(string path, string data, string oldData)
        {
            Path = path;
            Data = data;
            OldData = oldData;
        }

        public string Path { get; private set; }
        public string Data { get; private set; }

        public string OldData { get; private set; }
    }

    public class ValueRemovedEventArgs : EventArgs
    {
        public ValueRemovedEventArgs(string path)
        {
            Path = path;
        }

        public string Path { get; private set; }
    }

    public delegate void ValueAddedEventHandler(object sender, ValueAddedEventArgs e);
    public delegate void ValueChangedEventHandler(object sender, ValueChangedEventArgs e);
    public delegate void ValueRemovedEventHandler(object sender, ValueRemovedEventArgs e);
}
