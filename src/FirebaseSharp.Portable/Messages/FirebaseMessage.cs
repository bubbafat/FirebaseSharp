using FirebaseSharp.Portable.Interfaces;

namespace FirebaseSharp.Portable.Messages
{
    internal enum WriteBehavior
    {
        None,
        Replace,
        Merge,
        Push,
    }

    internal enum MessageSouce
    {
        Local,
        Remote
    }

    internal class FirebaseMessage
    {
        public FirebaseMessage(WriteBehavior behavior, FirebasePath path, string value, FirebaseStatusCallback callback,
            MessageSouce source)
            : this(behavior, path, value, null, callback, source)
        {
        }

        public FirebaseMessage(WriteBehavior behavior, FirebasePath path, string value, FirebasePriority priority,
            FirebaseStatusCallback callback, MessageSouce source)
        {
            Behavior = behavior;
            Path = path;
            Value = value;
            Callback = callback;
            Priority = priority;
            Source = source;
        }

        public WriteBehavior Behavior { get; private set; }
        public FirebasePath Path { get; private set; }
        public string Value { get; private set; }
        public FirebaseStatusCallback Callback { get; private set; }
        public FirebasePriority Priority { get; private set; }

        public MessageSouce Source { get; private set; }
    }
}